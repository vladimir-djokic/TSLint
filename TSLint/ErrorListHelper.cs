using Microsoft.VisualStudio.Shell;
using System;
using Microsoft.VisualStudio.Text.Adornments;

namespace TSLint
{
    internal static class ErrorListHelper
    {
        private static ErrorListProvider _provider;

        internal static void Init(IServiceProvider serviceProvider)
        {
            ErrorListHelper._provider = new ErrorListProvider(serviceProvider)
            {
                ProviderName = "TSLint",
                ProviderGuid = new Guid("901887BA-2855-47A8-929D-AAD7FB6E8709")
            };

            ErrorListHelper._provider.Show();
        }

        internal static void Suspend()
        {
            if (ErrorListHelper._provider != null)
                ErrorListHelper._provider.SuspendRefresh();
        }

        internal static void Resume()
        {
            if (ErrorListHelper._provider != null)
                ErrorListHelper._provider.ResumeRefresh();
        }

        internal static void RemoveAllForDocument(string name)
        {
            if (ErrorListHelper._provider == null)
                return;

            for (var i = ErrorListHelper._provider.Tasks.Count - 1; i >= 0; i--)
            {
                var task = ErrorListHelper._provider.Tasks[i];

                if (task.Document == name)
                    ErrorListHelper._provider.Tasks.RemoveAt(i);
            }
        }

        internal static void Add(TsLintTag tag)
        {
            if (ErrorListHelper._provider == null)
                return;

            var error = new ErrorTask()
            {
                ErrorCategory =
                tag.ErrorType ==
                    PredefinedErrorTypeNames.SyntaxError
                        ? TaskErrorCategory.Error
                        : TaskErrorCategory.Warning,

                Document = tag.DocumentName,
                Line = tag.Line,
                Column = tag.Column,
                Text = tag.ToolTipContent.ToString(),
            };

            error.Navigate += (s, e) =>
            {
                var task = new Task()
                {
                    Document = error.Document,
                    Line = error.Line + 1,
                    Column = error.Column + 1
                };

                ErrorListHelper._provider.Navigate(task, new Guid());
            };

            ErrorListHelper._provider.Tasks.Add(error);
        }
    }
}
