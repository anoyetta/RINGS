using System.Collections.Generic;
using aframe;

namespace RINGS.Models
{
    public class ChatLogsModel
    {
        #region Singleton

        private static ChatLogsModel instance;

        public static ChatLogsModel Instance => instance ?? (instance = new ChatLogsModel());

        private ChatLogsModel()
        {
        }

        #endregion Singleton

        private readonly List<ChatLogModel> buffer = WPFHelper.IsDesignMode ?
            CreateDesigntimeChatLogs() :
            new List<ChatLogModel>(10240);

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

        private static List<ChatLogModel> CreateDesigntimeChatLogs()
        {
            var logs = new List<ChatLogModel>();

            return logs;
        }
    }
}
