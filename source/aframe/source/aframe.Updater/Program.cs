using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading;
using Microsoft.VisualBasic.FileIO;

namespace aframe.Updater
{
    public class Program
    {
        private static void Main(
            string[] args)
        {
            try
            {
                if (args == null ||
                    args.Length < 3)
                {
                    Console.WriteLine("Usage:");
                    Console.WriteLine("aframe.Updater [対象のプロセス名] [アップデート元フォルダパス] [アップデート先フォルダパス]");
                    return;
                }

                var targetProcessName = args[0];
                var source = args[1];
                var destination = args[2];

                Update(targetProcessName, source, destination);
            }
            catch (Exception ex)
            {
                WriteLogLine("Update Failed. Unhandled exception.");
                WriteLogLine("予期しない例外が発生しました。アップデート作業を中止します。");
                WriteLogLine(ex.ToString());
            }
        }

        private static void Update(
            string targetProcessName,
            string source,
            string destination)
        {
            const int Interval = 100;

            WriteLogLine("Shutdown Application...");

            if (targetProcessName.ToLower().Contains(".exe"))
            {
                targetProcessName = Path.GetFileNameWithoutExtension(targetProcessName);
            }

            for (int i = 0; i < (10000 / Interval); i++)
            {
                Thread.Sleep(Interval);

                var ps = Process.GetProcessesByName(targetProcessName);
                if (ps == null ||
                    ps.Length < 1)
                {
                    break;
                }

                foreach (var p in ps)
                {
                    p.CloseMainWindow();
                    p.WaitForExit();
                    p.Dispose();
                }
            }

            WriteLogLine("Shutdown Completed.");
            Thread.Sleep(Interval * 30);

            WriteLogLine("Update Application...");

            if (Directory.Exists(destination))
            {
                var backupFileName = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    @"aframe\Updater",
                    $"backup{DateTime.Now:yyMMddHHmmss}.zip");

                var folder = Path.GetDirectoryName(backupFileName);
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }

                ZipFile.CreateFromDirectory(
                    destination,
                    backupFileName,
                    CompressionLevel.Optimal,
                    false);

                foreach (var f in Directory.GetFiles(
                    destination,
                    "*",
                    System.IO.SearchOption.AllDirectories))
                {
                    File.Delete(f);
                }
            }

            if (!Directory.Exists(source))
            {
                WriteLogLine("Update Failed. Missing source folder.");
                return;
            }

            FileSystem.CopyDirectory(
                source,
                destination,
                true);

            Thread.Sleep(Interval / 10);

            WriteLogLine("Update Completed.");
            Thread.Sleep(Interval);

            WriteLogLine("Reboot Application...");
            Process.Start(new ProcessStartInfo(targetProcessName)
            {
                WorkingDirectory = Path.GetDirectoryName(targetProcessName)
            });

            WriteLogLine("Update all finished. bye!");
        }

        private static string logFileName;

        private static string LogFileName =>
            logFileName ?? (logFileName = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                @"aframe\logs\aframe.Updater.log"));

        private static void WriteLogLine(
            string message,
            params object[] args)
        {
            Console.WriteLine(message, args);

            var dir = Path.GetDirectoryName(LogFileName);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            File.AppendAllText(
                LogFileName,
                $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {message}\n",
                new UTF8Encoding(false));
        }
    }
}
