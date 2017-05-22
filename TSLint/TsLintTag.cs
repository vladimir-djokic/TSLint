using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;

namespace TSLint
{
    internal class TsLintTag : IErrorTag
    {
        public TsLintTag(
            ITrackingSpan trackingSpan,
            string errorType,
            string toolTip,
            string documentName,
            int line,
            int column
        )
        {
            this.TrackingSpan = trackingSpan;
            this.ErrorType = errorType;
            this.ToolTipContent = toolTip;
            this.DocumentName = documentName;
            this.Line = line;
            this.Column = column;
        }

        public string ErrorType
        { get; private set; }

        public object ToolTipContent
        { get; private set; }

        public string DocumentName
        { get; private set; }

        public int Line
        { get; private set; }

        public int Column
        { get; private set; }

        public ITrackingSpan TrackingSpan
        { get; private set; }
    }
}
