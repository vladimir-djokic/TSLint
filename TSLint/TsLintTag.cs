using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;

namespace TSLint
{
    internal class TsLintTag : IErrorTag
    {
        public TsLintTag(ITrackingSpan trackingSpan, string errorType, string toolTip)
        {
            this.TrackingSpan = trackingSpan;
            this.ErrorType = errorType;
            this.ToolTipContent = toolTip;
        }

        public string ErrorType { get; private set; }

        public object ToolTipContent { get; private set; }

        public ITrackingSpan TrackingSpan { get; private set; }
    }
}
