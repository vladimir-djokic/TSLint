using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;

namespace TSLint
{
    internal class TsLintTag : IErrorTag
    {
        public TsLintTag(ITrackingSpan trackingSpan, string errorType, string toolTip, string documentName, int line)
        {
            this.TrackingSpan = trackingSpan;
            this.ErrorType = errorType;
            this.ToolTipContent = toolTip;
            this.DocumentName = documentName;
            this.Line = line;
        }

        public string ErrorType { get; private set; }
        public object ToolTipContent { get; private set; }

        public string DocumentName { get; set; }
        public int Line { get; set; }

        public ITrackingSpan TrackingSpan { get; private set; }
    }
}
