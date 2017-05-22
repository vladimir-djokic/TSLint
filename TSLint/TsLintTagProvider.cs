using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;

namespace TSLint
{
    [Export(typeof(IViewTaggerProvider))]
    [ContentType("TypeScript")]
    [TagType(typeof(ErrorTag))]
    internal class TsLintTagProvider : IViewTaggerProvider
    {
        [Import]
        internal ITextDocumentFactoryService TextDocumentFactoryService
        { get; set; }

        [Import]
        internal ITextStructureNavigatorSelectorService TextStructureNavigatorSelectorService
        { get; set; }

        public ITagger<T> CreateTagger<T>(ITextView textView, ITextBuffer buffer) where T : ITag
        {
            // Make sure we are only tagging the top buffer
            if (buffer != textView.TextBuffer)
                return null;

            return textView.Properties.GetOrCreateSingletonProperty(
                () =>
                    new TsLintTagger(
                        this.TextDocumentFactoryService,
                        this.TextStructureNavigatorSelectorService,
                        textView,
                        buffer
                    )
            ) as ITagger<T>;
        }
    }
}
