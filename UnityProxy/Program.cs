using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UnityProxy
{
    class Program
    {
        static string outputFile;
        static Process process;
        static long lastLength = 0;

        static void Main(string[] args)
        {
            var processInfo = new ProcessStartInfo(args[0], string.Join(" ", args.Skip(1).ToArray()));
            process = Process.Start(processInfo);
            process.Exited += Process_Exited;

            for (int i = 1; i < args.Length; i++)
            {
                if (args[i] == "-logFile")
                    outputFile = args[i + 1];
            }

            var watcher = new Thread(Watcher);
            watcher.Start();

            process.WaitForExit();
        }

        private static void Watcher()
        {
            while (!process.HasExited)
            {
                Thread.Sleep(250);
                try
                {
                    var file = File.Open(outputFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    if (file.Length == lastLength) continue;
                    var buffer = new byte[file.Length - lastLength];
                    file.Seek(lastLength, SeekOrigin.Begin);
                    file.Read(buffer, 0, buffer.Length);
                    Console.Write(System.Text.Encoding.UTF8.GetString(buffer));
                    lastLength = file.Length;
                } catch { }
            }

        }

        private static void Process_Exited(object sender, EventArgs e)
        {
            Environment.Exit(process.ExitCode);
        }
    }
}
