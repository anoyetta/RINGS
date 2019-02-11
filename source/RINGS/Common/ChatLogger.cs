using System;
using aframe;
using NLog;
using static aframe.AppLogger;

namespace RINGS.Common
{
    public static class ChatLogger
    {
        private static readonly Lazy<Logger> LazyLogger = new Lazy<Logger>(() => LogManager.GetLogger("RINGSChatLogger"));

        public static void Write(
            string channel,
            string originalSpeaker,
            string speaker,
            string message)
        {
            LazyLogger.Value.Info(
                $"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}] {channel}:{speaker}({originalSpeaker}):{message}");
        }

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
