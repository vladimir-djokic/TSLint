using EnvDTE80;
using Microsoft.VisualStudio.Threading;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace TSLint
{
    internal class TsLint
    {
        private static DTE2 dte2;

        private readonly static string tslintCmdSubpath =
            "\\node_modules\\.bin\\tslint.cmd";

        private readonly static KeyValuePair<string, string> defKeyValuePair =
            default(KeyValuePair<string, string>);

        // Key: project/solution path, value: tslint.cmd path.
        private readonly static Dictionary<string, string> cache =
            new Dictionary<string, string>();

        public static void Init(DTE2 dte2)
        {
            TsLint.dte2 = dte2;
        }

        public static async Task<string> Run(string tsFilename)
        {
            var existingPath = TsLint.cache.SingleOrDefault(
                p => tsFilename.Contains(p.Key)
            );

            var potentialPath = TsLint.defKeyValuePair;

            if (existingPath.Equals(TsLint.defKeyValuePair))
            {
                // First, check if the project for this file has local installation of tslint.
                potentialPath = TsLint.TryGetProjectTsLint(tsFilename);

                if (potentialPath.Equals(TsLint.defKeyValuePair))
                {
                    // Now, check if the solution has local installation of tslint.
                    potentialPath = TsLint.TryGetSolutionTsLint(tsFilename);
                }

                if (!potentialPath.Equals(TsLint.defKeyValuePair))
                {
                    existingPath = potentialPath;
                    TsLint.cache.Add(existingPath.Key, existingPath.Value);
                }
            }

            if (!existingPath.Equals(TsLint.defKeyValuePair))
            {
                var procInfo = new ProcessStartInfo()
                {
                    FileName = existingPath.Value,
                    Arguments = $"-t JSON {tsFilename}",
                    RedirectStandardOutput = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true,
                    UseShellExecute = false
                };

                var proc = Process.Start(procInfo);
                var reader = proc.StandardOutput;

                await proc.WaitForExitAsync();
                return await reader.ReadToEndAsync();
            }

            return null;
        }

        private static KeyValuePair<string, string> TryGetProjectTsLint(string tsFilename)
        {
            var item = dte2.Solution.FindProjectItem(tsFilename);
            var project = item.ContainingProject;
            var projectPath = Path.GetDirectoryName(project.FullName);
            var tsLintCmdPath = $"{projectPath}{tslintCmdSubpath}";

            return File.Exists(tsLintCmdPath)
                ? new KeyValuePair<string, string>(projectPath, tsLintCmdPath)
                : TsLint.defKeyValuePair;
        }

        private static KeyValuePair<string, string> TryGetSolutionTsLint(string tsFilename)
        {
            var solutionPath = Path.GetDirectoryName(dte2.Solution.FullName);
            var tsLintCmdPath = $"{solutionPath}{tslintCmdSubpath}";

            return File.Exists(tsLintCmdPath)
                ? new KeyValuePair<string, string>(solutionPath, tsLintCmdPath)
                : TsLint.defKeyValuePair;
        }
    }
}
