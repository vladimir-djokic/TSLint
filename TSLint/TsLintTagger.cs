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
        private readonly ITextView _view;
        private readonly ITextDocument _document;
        private readonly ITextStructureNavigator _textStructureNavigator;

        private readonly TsLintQueue _queue;
        private readonly List<TsLintTag> _collectedTags;

        internal TsLintTagger(
            ITextDocument document,
            ITextStructureNavigatorSelectorService textStructureNavigatorSelector,
            ITextView textView,
            ITextBuffer buffer
        )
        {
            this._view = textView;

            this._textStructureNavigator =
                textStructureNavigatorSelector.GetTextStructureNavigator(
                    this._view.TextBuffer
                );

            this._queue = new TsLintQueue(this.CollectTags);
            this._collectedTags = new List<TsLintTag>();

            this._document = document;

            this._document.FileActionOccurred += this.OnDocumentFileActionOccured;
            this._view.Closed += OnViewClosed;

            this._queue.Enqueue(this._document.FilePath)
                .ContinueWith(_ => this.UpdateErrorsList())
                .ContinueWith(_ => this.TriggerTagsChanged());
        }

        public IEnumerable<ITagSpan<IErrorTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            if (this._collectedTags.Count <= 0)
                yield break;

            foreach (var span in spans)
            {
                foreach (var tag in this._collectedTags)
                {
                    var snapshot = span.Snapshot;
                    var trackingSpanSpan = tag.TrackingSpan.GetSpan(snapshot);

                    if (!trackingSpanSpan.IntersectsWith(span))
                        continue;

                    var tagSpan = new TagSpan<IErrorTag>(
                        new SnapshotSpan(snapshot, trackingSpanSpan),
                        tag
                    );

                    yield return tagSpan;
                }
            }
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        private async void OnDocumentFileActionOccured(object sender, TextDocumentFileActionEventArgs e)
        {
            if (e.FileActionType != FileActionTypes.ContentSavedToDisk)
                return;

            await this._queue.Enqueue(e.FilePath);
            this.UpdateErrorsList();

            this.TriggerTagsChanged();
        }

        private void OnViewClosed(object sender, EventArgs e)
        {
            ErrorListHelper.Suspend();
            ErrorListHelper.RemoveAllForDocument(this._document.FilePath);
            ErrorListHelper.Resume();
        }

        private async System.Threading.Tasks.Task CollectTags(string tsFilename)
        {
            this._collectedTags.Clear();

            var output = await TsLint.Run(tsFilename);

            if (string.IsNullOrEmpty(output))
                return;

            var jArray = JArray.Parse(output);
            var result = jArray.ToObject<List<TsLintResult>>();

            foreach (var entry in result)
            {
                // If the document has been closed
                if (this._document.TextBuffer == null)
                    continue;

                var startLine = this._document.TextBuffer.CurrentSnapshot.GetLineFromLineNumber(entry.StartPosition.Line);
                var endLine = this._document.TextBuffer.CurrentSnapshot.GetLineFromLineNumber(entry.EndPosition.Line);

                var start = entry.StartPosition.Character;
                var end = entry.EndPosition.Character;

                SnapshotSpan span;

                if (start == end)
                {
                    // Handle some special cases. If the number of special cases increases,
                    // I should refactor this.
                    if (entry.RuleName == "semicolon")
                    {
                        // "Move" left by one character so that we can pin-point the text with the missing semicolon
                        start -= 1;
                    }

                    var extent = this._textStructureNavigator.GetExtentOfWord(endLine.Start + start);
                    span = extent.Span;
                }
                else
                {
                    span = new SnapshotSpan(
                        this._view.TextSnapshot,
                        Span.FromBounds(startLine.Start + start, endLine.Start + end)
                    );
                }

                var trackingSpan =
                    this._view.TextSnapshot.CreateTrackingSpan(
                        span,
                        SpanTrackingMode.EdgeInclusive
                    );

                var type = PredefinedErrorTypeNames.Warning;

                if (entry.RuleSeverity != null && entry.RuleSeverity == "ERROR")
                {
                    type = PredefinedErrorTypeNames.SyntaxError;
                }

                this._collectedTags.Add(
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
            ErrorListHelper.RemoveAllForDocument(this._document.FilePath);
            ErrorListHelper.Resume();

            foreach (var tag in this._collectedTags)
            {
                ErrorListHelper.Add(tag);
            }
        }

        private void TriggerTagsChanged()
        {
            this.TagsChanged?.Invoke(
                this,
                new SnapshotSpanEventArgs(
                    new SnapshotSpan(
                        this._view.TextSnapshot,
                        0,
                        this._view.TextSnapshot.Length
                    )
                )
            );
        }
    }
}
