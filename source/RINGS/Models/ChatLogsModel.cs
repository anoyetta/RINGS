using System;
using System.Collections.Generic;
using System.Linq;
using aframe;
using Prism.Mvvm;

namespace RINGS.Models
{
    public class ChatLogsModel :
        BindableBase,
        IDisposable
    {
        public static List<ChatLogsModel> ActiveBuffers { get; private set; } = new List<ChatLogsModel>();

        public static void AddToBuffers(
            ChatLogModel log)
            => AddToBuffers(new[] { log });

        public static void AddToBuffers(
            IEnumerable<ChatLogModel> logs)
        {
            var buffers = default(IEnumerable<ChatLogsModel>);

            lock (ActiveBuffers)
            {
                buffers = ActiveBuffers.ToArray();
            }

            foreach (var buffer in buffers)
            {
                var targets = logs.Where(x => buffer?.FilterCallback(x) ?? false);
                buffer.AddRange(targets);
            }
        }

        public ChatLogsModel()
        {
            lock (ActiveBuffers)
            {
                ActiveBuffers.Add(this);
            }
        }

        public void Dispose()
        {
            lock (ActiveBuffers)
            {
                this.Buffer.Clear();
                ActiveBuffers.Remove(this);
            }
        }

        private static readonly int BufferSize = 5120;

        private static List<ChatLogModel> InnerList => new List<ChatLogModel>(BufferSize + (BufferSize / 10));

        public SuspendableObservableCollection<ChatLogModel> Buffer { get; private set; } = WPFHelper.IsDesignMode ?
            CreateDesigntimeChatLogs() :
            new SuspendableObservableCollection<ChatLogModel>(InnerList);

        public Predicate<ChatLogModel> FilterCallback { get; set; }

        private ChatPageSettingsModel parentPageSettings;

        public ChatPageSettingsModel ParentPageSettings
        {
            get => this.parentPageSettings;
            set => this.SetProperty(ref this.parentPageSettings, value);
        }

        public void Add(
            ChatLogModel log)
        {
            lock (this.Buffer)
            {
                log.ParentPageSettings = ParentPageSettings;
                this.Buffer.Add(log);
            }
        }

        public void AddRange(
            IEnumerable<ChatLogModel> logs)
        {
            foreach (var log in logs)
            {
                log.ParentPageSettings = ParentPageSettings;
            }

            lock (this.Buffer)
            {
                this.Buffer.AddRange(logs);
            }
        }

        public void Clear()
        {
            lock (this.Buffer)
            {
                this.Buffer.Clear();
            }
        }

        public void Garbage()
        {
            lock (this.Buffer)
            {
                if (this.Buffer.Count <= BufferSize)
                {
                    return;
                }

                for (int i = 0; i < BufferSize; i++)
                {
                    this.Buffer.RemoveAt(0);
                }
            }
        }

        private static SuspendableObservableCollection<ChatLogModel> CreateDesigntimeChatLogs()
        {
            var logs = new SuspendableObservableCollection<ChatLogModel>();

            return logs;
        }
    }
}
