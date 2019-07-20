using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using aframe;

namespace RINGS.Controllers
{
    public class BoyomiClient
    {
        #region Lazy Singleton

        private static readonly Lazy<BoyomiClient> LazyInstance = new Lazy<BoyomiClient>(() => new BoyomiClient());

        public static BoyomiClient Instance => LazyInstance.Value;

        private BoyomiClient()
        {
        }

        #endregion Lazy Singleton

        #region Constants

        private const short BoyomiCommand = 0x0001;
        private const short BoyomiSpeed = -1;
        private const byte BoyomiTextEncoding = 0;
        private const short BoyomiTone = -1;
        private const short BoyomiVoice = 0;
        private const short BoyomiVolume = -1;

        #endregion Constants

        private readonly ConcurrentQueue<string> TTSQueue = new ConcurrentQueue<string>();

        public void Enqueue(string tts)
        {
            this.StartSendTTSTask();
            this.TTSQueue.Enqueue(tts);
        }

        private volatile Thread sendTTSThread;

        private void StartSendTTSTask()
        {
            if (this.sendTTSThread != null)
            {
                return;
            }

            this.sendTTSThread = new Thread(() =>
            {
                while (true)
                {
                    while (this.TTSQueue.TryDequeue(out string tts))
                    {
                        this.Send(tts);
                        Thread.Yield();
                    }

                    Thread.Sleep(100);
                }
            })
            {
                Priority = ThreadPriority.Lowest,
                IsBackground = true,
            };

            this.sendTTSThread.Start();
            Thread.Sleep(10);
        }

        private void Send(
            string tts)
        {
            var server = Config.Instance.TTSServerAddress;
            var port = Config.Instance.TTSServerPort;

            if (string.IsNullOrEmpty(server))
            {
                AppLogger.Error("TTS Server address is empty.");
                return;
            }

            if (port > 65535 ||
                port < 1)
            {
                AppLogger.Error("TTS Server port is invalid.");
                return;
            }

            if (server.ToLower() == "localhost")
            {
                server = "127.0.0.1";
            }

            try
            {
                lock (this)
                {
                    using (var tcp = new TcpClient(server, port))
                    using (var ns = tcp.GetStream())
                    using (var buffer = new MemoryStream())
                    using (var bw = new BinaryWriter(buffer))
                    {
                        var messageAsBytes = Encoding.UTF8.GetBytes(tts);

                        bw.Write(BoyomiCommand);
                        bw.Write((short)Config.Instance.TTSSpeed);
                        bw.Write(BoyomiTone);
                        bw.Write((short)Config.Instance.TTSVolume);
                        bw.Write(BoyomiVoice);
                        bw.Write(BoyomiTextEncoding);
                        bw.Write(messageAsBytes.Length);
                        bw.Write(messageAsBytes);
                        bw.Flush();

                        ns.Write(buffer.ToArray(), 0, (int)buffer.Length);
                        ns.Flush();
                    }
                }
            }
            catch (Exception ex)
            {
                AppLogger.Error("Exception occurred when sending to the TTS server.", ex);
            }
        }
    }
}
