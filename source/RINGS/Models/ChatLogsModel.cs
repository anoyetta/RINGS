using System.Collections.Generic;
using aframe;

namespace RINGS.Models
{
    public class ChatLogsModel
    {
        private static readonly int BufferSize = 5120;

        private readonly List<ChatLogModel> buffer = WPFHelper.IsDesignMode ?
            CreateDesigntimeChatLogs() :
            new List<ChatLogModel>(BufferSize + (BufferSize / 10));

        public IReadOnlyList<ChatLogModel> Buffer => this.buffer;

        public void Add(
            ChatLogModel log)
        {
            lock (this.buffer)
            {
                this.buffer.Add(log);
            }
        }

        public void AddRange(
            IEnumerable<ChatLogModel> logs)
        {
            lock (this.buffer)
            {
                this.buffer.AddRange(logs);
            }
        }

        public void Clear()
        {
            lock (this.buffer)
            {
                this.buffer.Clear();
            }
        }

        public void Garbage()
        {
            lock (this.buffer)
            {
                if (this.buffer.Count <= BufferSize)
                {
                    return;
                }

                for (int i = 0; i < BufferSize; i++)
                {
                    this.buffer.RemoveAt(0);
                }
            }
        }

        private static List<ChatLogModel> CreateDesigntimeChatLogs()
        {
            var logs = new List<ChatLogModel>();

            return logs;
        }
    }
}
