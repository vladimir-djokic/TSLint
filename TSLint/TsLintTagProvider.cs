using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;

namespace TSLint
{
    [Export(typeof(IViewTaggerProvider))]
    [ContentType("TypeScript")]
    [TagType(typeof(ErrorTag))]
    internal class TsLintTagProvider : IViewTaggerProvider
    {
        private readonly string[] allowedExtensions = { ".ts", ".tsx" };

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

            ITextDocument document;

            var success =
                this.TextDocumentFactoryService.TryGetTextDocument(
                    buffer,
                    out document
                );

            if (!success)
                return null;

            var ext = Path.GetExtension(document.FilePath);

            if (!this.allowedExtensions.Any(e => e == ext))
                return null;

            return textView.Properties.GetOrCreateSingletonProperty(
                () =>
                    new TsLintTagger(
                        document,
                        this.TextStructureNavigatorSelectorService,
                        textView,
                        buffer
                    )
            ) as ITagger<T>;
        }
    }
}
