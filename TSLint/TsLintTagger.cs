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

        private readonly IList<TsLintTag> collectedTags;

        internal TsLintTagger(
            ITextDocumentFactoryService textDocumentFactory,
            ITextStructureNavigatorSelectorService textStructureNavigatorSelector,
            ITextView textView,
            ITextBuffer buffer
        )
        {
            this.view = textView;
            this.textStructureNavigator = textStructureNavigatorSelector.GetTextStructureNavigator(this.view.TextBuffer);

            this.collectedTags = new List<TsLintTag>();

            var success =
                textDocumentFactory.TryGetTextDocument(
                    buffer,
                    out this.document
                );

            if (success)
            {
                this.document.FileActionOccurred += this.OnDocumentFileActionOccured;
                this.CollectTags(this.document.FilePath);
            }
        }

        private void OnDocumentFileActionOccured(object sender, TextDocumentFileActionEventArgs e)
        {
            if (e.FileActionType == FileActionTypes.ContentSavedToDisk)
            {
                this.CollectTags(e.FilePath);

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

        private void CollectTags(string tsFilename)
        {
            this.collectedTags.Clear();

            var output = TsLint.Run(tsFilename);

            if (output == null)
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

                this.collectedTags.Add(
                    new TsLintTag(
                        trackingSpan,
                        PredefinedErrorTypeNames.Warning,
                        $"[tslint] {entry.Failure} ({entry.RuleName})"
                    )
                );
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
    }
}
