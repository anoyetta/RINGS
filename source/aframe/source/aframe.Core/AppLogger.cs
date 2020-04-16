using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.VisualBasic.FileIO;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace aframe
{
    public enum AppLogLevel
    {
        Off = 0,
        Trace,
        Debug,
        Info,
        Warn,
        Error,
        Fatal
    }

    public static partial class AppLogger
    {
        private static Logger defaultLogger;

        private static LogLevel ConvetLogLevel(
            AppLogLevel level)
            => new[]
            {
                LogLevel.Off,
                LogLevel.Trace,
                LogLevel.Debug,
                LogLevel.Info,
                LogLevel.Warn,
                LogLevel.Error,
                LogLevel.Fatal,
            }[(int)level];

        public static void Init(
            string defaultLoggerName,
            string configFileName = null)
        {
            LoadConfiguration(configFileName, Assembly.GetCallingAssembly());
            defaultLogger = LogManager.GetLogger(defaultLoggerName);

            Write($"{defaultLogger.Name} Init.");
        }

        public static string GetCurrentLogFileName()
        {
            var fts = LogManager.Configuration.AllTargets
                .Where(x => x is FileTarget)
                .Cast<FileTarget>();
            foreach (var ft in fts)
            {
                var file = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    ft.FileName.Render(
                        new LogEventInfo
                        {
                            Level = LogLevel.Trace
                        }));

                if (File.Exists(file))
                {
                    return file;
                }
            }

            return null;
        }

        public static async Task SaveArchiveLogDirectoryAsync(
            string archiveName) => await Task.Run(() =>
        {
            AppLogger.Flush();

            var source = Path.GetDirectoryName(AppLogger.GetCurrentLogFileName());

            var temp = Path.GetTempFileName();
            File.Delete(temp);
            Directory.CreateDirectory(temp);

            FileSystem.CopyDirectory(
                source,
                temp,
                true);

            if (File.Exists(archiveName))
            {
                File.Delete(archiveName);
            }

            ZipFile.CreateFromDirectory(
                temp,
                archiveName,
                CompressionLevel.Optimal,
                false);

            Directory.Delete(temp, true);
        });

        public static void Write(
            string message,
            AppLogLevel level = AppLogLevel.Trace,
            Exception exception = null)
        {
            try
            {
                // メッセージの改行コードを置換する
                message = message.Replace(Environment.NewLine, "<br />");
                message = message.Replace("\n", "<br />");
                message = message.Replace("\r", "<br />");

                defaultLogger?.Log(ConvetLogLevel(level), exception, message);

                Trace.WriteLine(
                    $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [{level.ToString().ToUpper().PadRight(5)}] {message}");
                if (exception != null)
                {
                    Trace.WriteLine(exception.ToFormat());
                }
            }
            catch (Exception)
            {
            }
        }

        public static void Error(
            string message,
            Exception exception = null)
        {
            Write(message, AppLogLevel.Error, exception);
            Flush();
        }

        public static void Fatal(
            string message,
            Exception exception = null)
        {
            Write(message, AppLogLevel.Fatal, exception);
            Flush();
        }

        public static void Flush() => LogManager.Flush();

        public static void Shutdown()
        {
            Write($"{defaultLogger.Name} Shutdown.");

            LogManager.Flush();
            LogManager.Shutdown();
        }

        public delegate void OnWriteEventHandler(object sender, AppLogOnWriteEventArgs e);

        public static OnWriteEventHandler OnWrite;

        public static void WriteLogCallback(
            string dateTime,
            string level,
            string message)
        {
            try
            {
                if (OnWrite == null)
                {
                    return;
                }

                var arg = new AppLogOnWriteEventArgs()
                {
                    DateTime = dateTime,
                    Level = level,
                    Message = message,
                };

                OnWrite.Invoke(
                    defaultLogger,
                    arg);
            }
            catch (Exception)
            {
            }
        }

        private static void LoadConfiguration(
            string configFileName,
            Assembly callingAssembly = null)
        {
            if (string.IsNullOrEmpty(configFileName))
            {
                return;
            }

            if (File.Exists(configFileName))
            {
                LogManager.Configuration = new XmlLoggingConfiguration(configFileName);
                return;
            }

            var asms = new[]
            {
                callingAssembly,
                Assembly.GetEntryAssembly(),
                Assembly.GetExecutingAssembly(),
            };

            foreach (var asm in asms)
            {
                if (asm == null)
                {
                    continue;
                }

                var dir = Path.GetDirectoryName(asm.Location);
                var file = Path.Combine(dir, Path.GetFileName(configFileName));
                if (File.Exists(file))
                {
                    LogManager.Configuration = new XmlLoggingConfiguration(file);
                    return;
                }
            }
        }
    }

    public class AppLogOnWriteEventArgs : EventArgs
    {
        public string DateTime { get; set; }

        public string DateTimeShort { get; set; }

        public string Level { get; set; }

        public string CallSite { get; set; }

        public string Message { get; set; }

        public string ToShortString()
            => string.Join(
                " ",
                new[]
                {
                    this.DateTime,
                    $"[{this.Level}]",
                    this.Message,
                }.Where(x => !string.IsNullOrEmpty(x)));

        public override string ToString()
            => string.Join(
                " ",
                new[]
                {
                    this.DateTime,
                    $"[{this.Level}]",
                    this.Message,
                    this.CallSite
                }.Where(x => !string.IsNullOrEmpty(x)));
    }
}
