using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
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

        public async Task SendAsync(
            string tts)
            => await Task.Run(() => this.Send(tts));

        public void Send(
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
                using (var tcp = new TcpClient(server, port))
                using (var ns = tcp.GetStream())
                using (var bw = new BinaryWriter(ns))
                {
                    var messageAsBytes = Encoding.UTF8.GetBytes(tts);

                    bw.Write(BoyomiCommand);
                    bw.Write(BoyomiSpeed);
                    bw.Write(BoyomiTone);
                    bw.Write(BoyomiVolume);
                    bw.Write(BoyomiVoice);
                    bw.Write(BoyomiTextEncoding);
                    bw.Write(messageAsBytes.Length);
                    bw.Write(messageAsBytes);

                    bw.Flush();
                }
            }
            catch (Exception ex)
            {
                AppLogger.Error("Exception occurred when sending to the TTS server.", ex);
            }
        }
    }
}
