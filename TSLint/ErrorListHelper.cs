using Microsoft.VisualStudio.Shell;
using System;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Text.Adornments;
using System.Collections.Generic;

namespace TSLint
{
    internal static class ErrorListHelper
    {
        private static ErrorListProvider provider;

        internal static void Init(IServiceProvider serviceProvider)
        {
            ErrorListHelper.provider = new ErrorListProvider(serviceProvider);
            provider.ProviderName = "TSLint";
            provider.ProviderGuid = new Guid("901887BA-2855-47A8-929D-AAD7FB6E8709");
            provider.Show();
        }

        internal static void Suspend()
        {
            ErrorListHelper.provider.SuspendRefresh();
        }

        internal static void Resume()
        {
            ErrorListHelper.provider.ResumeRefresh();
        }

        internal static void RemoveAllForDocument(string name)
        {
            for (var i = ErrorListHelper.provider.Tasks.Count - 1; i >= 0; i--)
            {
                var task = ErrorListHelper.provider.Tasks[i];

                if (task.Document == name)
                {
                    ErrorListHelper.provider.Tasks.RemoveAt(i);
                }
            }
        }

        internal static void Add(TsLintTag tag)
        {
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

            error.Navigate += (s, e) => {
                var task = new Task()
                {
                    Document = error.Document,
                    Line = error.Line + 1,
                    Column = error.Column + 1
                };

                ErrorListHelper.provider.Navigate(task, new Guid());
            };

            ErrorListHelper.provider.Tasks.Add(error);
        }
    }
}
