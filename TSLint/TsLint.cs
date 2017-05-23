using EnvDTE80;
using Microsoft.VisualStudio.Threading;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace TSLint
{
    internal class TsLint
    {
        private static DTE2 dte2;

        public static void Init(DTE2 dte2)
        {
            TsLint.dte2 = dte2;
        }

        public static async Task<string> Run(string tsFilename)
        {
            // First, try to find out if tslint is local to the file's project.
            var item = dte2.Solution.FindProjectItem(tsFilename);
            var project = item.ContainingProject;
            var tsLintCmdPath = $"{Path.GetDirectoryName(project.FullName)}\\node_modules\\.bin\\tslint.cmd";

            if (!File.Exists(tsLintCmdPath))
            {
                // Now, try to find out if tslint is local to the solution
                tsLintCmdPath = $"{Path.GetDirectoryName(dte2.Solution.FullName)}\\node_modules\\.bin\\tslint.cmd";

                if (!File.Exists(tsLintCmdPath))
                {
                    return null;
                }
            }

            var procInfo = new ProcessStartInfo()
            {
                FileName = tsLintCmdPath,
                Arguments = $"-t JSON {tsFilename}",
                RedirectStandardOutput = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                UseShellExecute = false
            };

            var proc = System.Diagnostics.Process.Start(procInfo);
            var reader = proc.StandardOutput;

            await proc.WaitForExitAsync();
            return await reader.ReadToEndAsync();
        }
    }
}
