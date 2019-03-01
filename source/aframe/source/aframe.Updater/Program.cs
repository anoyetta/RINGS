using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;
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

                WriteLogLine("Updater start.");
                Update(targetProcessName, source, destination);
            }
            catch (Exception ex)
            {
                WriteLogLine("Update Failed. Unhandled exception.");
                WriteLogLine("予期しない例外が発生しました。アップデート作業を中止します。");
                WriteLogLine(ex.ToString());
            }
            finally
            {
                WriteLogLine("Updater end.");
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
                    if (!p.HasExited)
                    {
                        p.WaitForExit(200);
                    }

                    if (!p.HasExited)
                    {
                        p.CloseMainWindow();
                        p.WaitForExit(500);
                    }

                    if (!p.HasExited)
                    {
                        p.Kill();
                        p.WaitForExit(500);
                    }

                    p.Dispose();
                }
            }

            WriteLogLine("Shutdown Completed.");
            Thread.Sleep(Interval * 30);

            WriteLogLine("Update Application...");

            if (!Directory.Exists(source))
            {
                WriteLogLine("Update Failed. Missing source folder.");
                return;
            }

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

                var ignores = GetIgnoreList(destination);

                foreach (var f in Directory.GetFiles(
                    destination,
                    "*",
                    System.IO.SearchOption.AllDirectories))
                {
                    if (!ignores.Any(ignore =>
                        f.IndexOf(ignore, StringComparison.OrdinalIgnoreCase) > -1))
                    {
                        File.Delete(f);
                    }
                    else
                    {
                        WriteLogLine($"ignore: {Path.GetFileName(f)}");
                    }
                }
            }

            FileSystem.CopyDirectory(
                source,
                destination,
                true);

            Thread.Sleep(Interval / 10);

            WriteLogLine("Update Completed.");
            Thread.Sleep(Interval);

#if false
            WriteLogLine("Reboot Application...");
            Process.Start(new ProcessStartInfo(targetProcessName)
            {
                WorkingDirectory = Path.GetDirectoryName(targetProcessName)
            });
#else
            MessageBox.Show(
                $"Update Completed. Please restart {targetProcessName} yourself." + Environment.NewLine +
                $"アップデートが完了しました。{targetProcessName} を再起動してください。",
                "Updater",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
#endif
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

        private static IEnumerable<string> GetIgnoreList(
            string destinationDirectory)
        {
            var list = new List<string>();

            var location = Assembly.GetExecutingAssembly().Location;
            var directory = Path.GetDirectoryName(location);

            var destinationIgnoreFileName = Path.Combine(
                destinationDirectory,
                $"{Path.GetFileNameWithoutExtension(location)}.ignore.txt");

            var sourceIgnoreFileName = Path.Combine(
                directory,
                $"{Path.GetFileNameWithoutExtension(location)}.ignore.txt");

            if (File.Exists(destinationIgnoreFileName))
            {
                var lines = File.ReadAllLines(destinationIgnoreFileName, new UTF8Encoding(false));
                if (lines.Length > 0)
                {
                    list.AddRange(lines.Where(x => !x.StartsWith("#")));
                }
            }
            else
            {
                if (File.Exists(sourceIgnoreFileName))
                {
                    var lines = File.ReadAllLines(sourceIgnoreFileName, new UTF8Encoding(false));
                    if (lines.Length > 0)
                    {
                        list.AddRange(lines.Where(x => !x.StartsWith("#")));
                    }
                }
            }

            return list;
        }
    }
}
