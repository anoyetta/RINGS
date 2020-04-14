using System;
using aframe;
using NLog;
using static aframe.AppLogger;

namespace RINGS.Common
{
    public static class ChatLogger
    {
        private static readonly Lazy<Logger> LazyLogger = new Lazy<Logger>(() => LogManager.GetLogger("RINGSChatLogger"));

        public static void WriteRaw(
            string raw)
        {
            LazyLogger.Value.Trace(raw);
        }

        public static void Write(
            string channel,
            string speaker,
            string speakerAlias,
            string message)
        {
            if (!string.IsNullOrEmpty(speaker))
            {
                if (!string.IsNullOrEmpty(speakerAlias))
                {
                    LazyLogger.Value.Info(
                        $"[{channel}]:{speaker}({speakerAlias}):{message}");
                }
                else
                {
                    LazyLogger.Value.Info(
                        $"[{channel}]:{speaker}:{message}");
                }
            }
            else
            {
                LazyLogger.Value.Info(
                    $"[{channel}]::{message}");
            }
        }

        public static void Flush() => LogManager.Flush();

        public static OnWriteEventHandler OnWrite;

        public static void WriteLogCallback(
            string dateTime,
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
                    DateTimeShort = DateTime.Parse(dateTime).ToString("HH:mm:ss"),
                    Message = message,
                };

                OnWrite.Invoke(
                    LazyLogger.Value,
                    arg);
            }
            catch (Exception)
            {
            }
        }
    }
}
