using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Text.Tagging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace TSLint
{
    internal class TsLintTagger : ITagger<IErrorTag>
    {
        private readonly ITextView view;
        private readonly ITextDocument document;
        private readonly ITextStructureNavigator textStructureNavigator;

        private readonly TsLintQueue queue;
        private readonly List<TsLintTag> collectedTags;

        internal TsLintTagger(
            ITextDocumentFactoryService textDocumentFactory,
            ITextStructureNavigatorSelectorService textStructureNavigatorSelector,
            ITextView textView,
            ITextBuffer buffer
        )
        {
            this.view = textView;
            this.textStructureNavigator = textStructureNavigatorSelector.GetTextStructureNavigator(this.view.TextBuffer);

            this.queue = new TsLintQueue(this.CollectTags);
            this.collectedTags = new List<TsLintTag>();

            var success =
                textDocumentFactory.TryGetTextDocument(
                    buffer,
                    out this.document
                );

            if (success)
            {
                this.document.FileActionOccurred += this.OnDocumentFileActionOccured;
                this.view.Closed += OnViewClosed;

                this.queue.Enqueue(this.document.FilePath)
                    .ContinueWith(_ => this.UpdateErrorsList())
                    .ContinueWith(_ => this.TriggerTagsChanged());
            }
        }

        public IEnumerable<ITagSpan<IErrorTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            if (this.collectedTags.Count > 0)
            {
                foreach (var span in spans)
                {
                    foreach (var tag in this.collectedTags)
                    {
                        var snapshot = span.Snapshot;
                        var trackingSpanSpan = tag.TrackingSpan.GetSpan(snapshot);

                        if (trackingSpanSpan.IntersectsWith(span))
                        {
                            var tagSpan = new TagSpan<IErrorTag>(new SnapshotSpan(snapshot, trackingSpanSpan), tag);
                            yield return tagSpan;
                        }
                    }
                }
            }
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        private async void OnDocumentFileActionOccured(object sender, TextDocumentFileActionEventArgs e)
        {
            if (e.FileActionType == FileActionTypes.ContentSavedToDisk)
            {
                await this.queue.Enqueue(e.FilePath);
                this.UpdateErrorsList();

                this.TriggerTagsChanged();
            }
        }

        private void OnViewClosed(object sender, EventArgs e)
        {
            ErrorListHelper.Suspend();
            ErrorListHelper.RemoveAllForDocument(this.document.FilePath);
            ErrorListHelper.Resume();
        }

        private async System.Threading.Tasks.Task CollectTags(string tsFilename)
        {
            this.collectedTags.Clear();

            var output = await TsLint.Run(tsFilename);

            if (string.IsNullOrEmpty(output))
            {
                return;
            }

            var jArray = JArray.Parse(output);
            var result = jArray.ToObject<List<TsLintResult>>();

            foreach (var entry in result)
            {
                var line = this.document.TextBuffer.CurrentSnapshot.GetLineFromLineNumber(entry.EndPosition.Line);

                var start = entry.StartPosition.Character;
                var end = entry.EndPosition.Character;

                SnapshotSpan span;

                if (start == end)
                {
                    // Handle some special cases. If the number of special casess increases,
                    // I should refactor this.
                    if (entry.RuleName == "semicolon")
                    {
                        // "Move" left by one character so that we can pin-point the text with the missing semicolon
                        start -= 1;
                    }

                    var extent = this.textStructureNavigator.GetExtentOfWord(line.Start + start);
                    span = extent.Span;
                }
                else
                {
                    span = new SnapshotSpan(this.view.TextSnapshot, Span.FromBounds(line.Start + start, line.Start + end));
                }

                var trackingSpan = this.view.TextSnapshot.CreateTrackingSpan(span, SpanTrackingMode.EdgeInclusive);

                var type = PredefinedErrorTypeNames.Warning;

                if (entry.RuleSeverity != null && entry.RuleSeverity == "ERROR")
                {
                    type = PredefinedErrorTypeNames.SyntaxError;
                }

                this.collectedTags.Add(
                    new TsLintTag(
                        trackingSpan,
                        type,
                        $"[tslint] {entry.Failure} ({entry.RuleName})",
                        tsFilename,
                        entry.StartPosition.Line,
                        entry.StartPosition.Character
                    )
                );
            }
        }

        private void UpdateErrorsList()
        {
            ErrorListHelper.Suspend();
            ErrorListHelper.RemoveAllForDocument(this.document.FilePath);
            ErrorListHelper.Resume();

            foreach (var tag in this.collectedTags)
            {
                ErrorListHelper.Add(tag);
            }
        }

        private void TriggerTagsChanged()
        {
            this.TagsChanged(
                this,
                new SnapshotSpanEventArgs(
                    new SnapshotSpan(
                        this.view.TextSnapshot,
                        0,
                        this.view.TextSnapshot.Length
                    )
                )
            );
        }
    }
}
