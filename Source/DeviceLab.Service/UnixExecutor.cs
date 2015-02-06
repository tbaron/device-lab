using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace InfoSpace.DeviceLab.Service
{
    internal class UnixExecutor : IExecutor
    {
        public string Output { get; private set; }

        public string Error { get; private set; }

        public string Command { get; private set; }

        public async Task Execute(string command)
        {
            Command = command;

            var startInfo = new ProcessStartInfo("bash")
            {
                Arguments = "-lc \"" + command.Replace("\"", "\\\"") + "\"",
                WindowStyle = ProcessWindowStyle.Hidden,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                UseShellExecute = false,
                LoadUserProfile = true,
            };

            using (Process cmdProcess = Process.Start(startInfo))
            using (StreamReader outStream = cmdProcess.StandardOutput)
            using (StreamReader errStream = cmdProcess.StandardError)
            {
                var results = await Task.WhenAll(
                    outStream.ReadToEndAsync(),
                    errStream.ReadToEndAsync());

                Output = results[0];
                Error = results[1];
            }
        }
    }
}
