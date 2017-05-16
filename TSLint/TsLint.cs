using Microsoft.VisualStudio.Threading;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace TSLint
{
    internal class TsLint
    {
        private static string tsLintCmd;

        public static void Init(string solutionDir)
        {
            var cmd = Path.Combine(solutionDir, "node_modules\\.bin\\tslint.cmd");

            if (File.Exists(cmd))
            {
                tsLintCmd = cmd;
            }
        }

        public static async Task<string> Run(string tsFilename)
        {
            if (tsLintCmd == null)
            {
                return null;
            }

            var procInfo = new ProcessStartInfo()
            {
                FileName = tsLintCmd,
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
