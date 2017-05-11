using System.Diagnostics;
using System.IO;

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

        public static string Run(string tsFilename)
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

            proc.WaitForExit();
            return reader.ReadToEnd();
        }
    }
}
