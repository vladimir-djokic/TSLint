using Microsoft.VisualStudio.Threading;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace TSLint
{
    internal class TsLint
    {
        private static string tsLintCmdPath;

        public static void Init(string solutionDir)
        {
            var cmd = Path.Combine(solutionDir, "node_modules\\.bin\\tslint.cmd");

            if (File.Exists(cmd))
            {
                tsLintCmdPath = cmd;
            }
        }

        public static async Task<string> Run(string tsFilename)
        {
            if (tsLintCmdPath == null)
            {
                return null;
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

            var proc = Process.Start(procInfo);
            var reader = proc.StandardOutput;

            await proc.WaitForExitAsync();
            return await reader.ReadToEndAsync();
        }
    }
}
