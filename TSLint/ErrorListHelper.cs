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
            var toRemove = new List<ErrorTask>();

            foreach (ErrorTask task in ErrorListHelper.provider.Tasks)
            {
                if (task.Document == name)
                {
                    toRemove.Add(task);
                }
            }

            foreach (var task in toRemove)
            {
                ErrorListHelper.provider.Tasks.Remove(task);
            }
        }

        internal static void Add(TsLintTag tag)
        {
            var error = new ErrorTask();

            error.ErrorCategory =
                tag.ErrorType ==
                    PredefinedErrorTypeNames.SyntaxError
                        ? TaskErrorCategory.Error
                        : TaskErrorCategory.Warning;

            error.Document = tag.DocumentName;
            error.Line = tag.Line;
            error.Text = tag.ToolTipContent.ToString();

            error.Navigate += (s, e) => {
                var task = new Task();
                task.Document = error.Document;
                task.Line = error.Line + 1;

                ErrorListHelper.provider.Navigate(task, new Guid());
            };

            ErrorListHelper.provider.Tasks.Add(error);
        }
    }
}
