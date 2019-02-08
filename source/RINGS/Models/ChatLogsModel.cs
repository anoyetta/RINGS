using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using aframe;
using Prism.Mvvm;
using RINGS.Common;

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

            this.buffer = new Lazy<SuspendableObservableCollection<ChatLogModel>>(() =>
            {
                var b = new SuspendableObservableCollection<ChatLogModel>(InnerList);

                if (WPFHelper.IsDesignMode || WPFHelper.IsDebugMode)
                {
                    this.CreateDesigntimeChatLogs(b);
                }

                b.CollectionChanged += this.Buffer_CollectionChanged;

                return b;
            });
        }

        public void Dispose()
        {
            lock (ActiveBuffers)
            {
                this.Buffer.CollectionChanged -= this.Buffer_CollectionChanged;
                this.Buffer.Clear();
                ActiveBuffers.Remove(this);
            }
        }

        public SuspendableObservableCollection<ChatLogModel> Buffer => this.buffer.Value;

        public event EventHandler<ChatLogAddedEventArgs> ChatLogAdded;

        protected void OnChatLogAdded(
            ChatLogAddedEventArgs e)
            => this.ChatLogAdded?.Invoke(this, e);

        public Predicate<ChatLogModel> FilterCallback { get; set; }

        private void Buffer_CollectionChanged(
            object sender, 
            NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null &&
                e.NewItems.Count > 0)
            {
                foreach (var item in e.NewItems)
                {
                    this.OnChatLogAdded(new ChatLogAddedEventArgs(
                        this.parentPageSettings,
                        item as ChatLogModel));
                }
            }
        }

        private static readonly int BufferSize = 5120;

        private static List<ChatLogModel> InnerList => new List<ChatLogModel>(BufferSize + (BufferSize / 10));

        private Lazy<SuspendableObservableCollection<ChatLogModel>> buffer;

        private ChatOverlaySettingsModel parentOverlaySettings;

        public ChatOverlaySettingsModel ParentOverlaySettings
        {
            get => this.parentOverlaySettings;
            set => this.SetProperty(ref this.parentOverlaySettings, value);
        }

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
                log.ParentOverlaySettings = this.ParentOverlaySettings;
                log.ParentPageSettings = this.ParentPageSettings;
                this.Buffer.Add(log);
            }
        }

        public void AddRange(
            IEnumerable<ChatLogModel> logs)
        {
            foreach (var log in logs)
            {
                log.ParentOverlaySettings = this.ParentOverlaySettings;
                log.ParentPageSettings = this.ParentPageSettings;
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

        private void CreateDesigntimeChatLogs(
            SuspendableObservableCollection<ChatLogModel> buffer)
        {
            buffer.Add(new ChatLogModel()
            {
                ParentOverlaySettings = this.ParentOverlaySettings,
                ParentPageSettings = this.ParentPageSettings,
                ChatCode = ChatCodes.Say,
                Speaker = "Naoki Y.",
                Message = "本日は晴天なり。"
            });

            buffer.Add(new ChatLogModel()
            {
                ParentOverlaySettings = this.ParentOverlaySettings,
                ParentPageSettings = this.ParentPageSettings,
                ChatCode = ChatCodes.Say,
                Speaker = "Naoki Y.",
                Message = "明日も晴天かな？"
            });

            buffer.Add(new ChatLogModel()
            {
                ParentOverlaySettings = this.ParentOverlaySettings,
                ParentPageSettings = this.ParentPageSettings,
                ChatCode = ChatCodes.Say,
                Speaker = "Naoki Y.",
                Message = "あのイーハトーヴォのすきとおった風、夏でも底に冷たさをもつ青いそら、うつくしい森で飾られたモリーオ市、郊外のぎらぎらひかる草の波。"
            });

            buffer.Add(new ChatLogModel()
            {
                ParentOverlaySettings = this.ParentOverlaySettings,
                ParentPageSettings = this.ParentPageSettings,
                ChatCode = ChatCodes.Party,
                Speaker = "Naoki Y.",
                Message = "よろしくおねがいします～ ＞＜"
            });

            buffer.Add(new ChatLogModel()
            {
                ParentOverlaySettings = this.ParentOverlaySettings,
                ParentPageSettings = this.ParentPageSettings,
                ChatCode = ChatCodes.Linkshell1,
                Speaker = "Naoki Y.",
                Message = "リンクシェル1の皆さん、こんにちは。"
            });

            buffer.Add(new ChatLogModel()
            {
                ParentOverlaySettings = this.ParentOverlaySettings,
                ParentPageSettings = this.ParentPageSettings,
                ChatCode = ChatCodes.Linkshell2,
                Speaker = "Naoki Y.",
                Message = "リンクシェル2の皆さん、こんにちは。"
            });

            buffer.Add(new ChatLogModel()
            {
                ParentOverlaySettings = this.ParentOverlaySettings,
                ParentPageSettings = this.ParentPageSettings,
                ChatCode = ChatCodes.Linkshell3,
                Speaker = "Naoki Y.",
                Message = "リンクシェル3の皆さん、こんにちは。"
            });

            buffer.Add(new ChatLogModel()
            {
                ParentOverlaySettings = this.ParentOverlaySettings,
                ParentPageSettings = this.ParentPageSettings,
                ChatCode = ChatCodes.Linkshell4,
                Speaker = "Naoki Y.",
                Message = "リンクシェル4の皆さん、こんにちは。"
            });

            buffer.Add(new ChatLogModel()
            {
                ParentOverlaySettings = this.ParentOverlaySettings,
                ParentPageSettings = this.ParentPageSettings,
                ChatCode = ChatCodes.Linkshell5,
                Speaker = "Naoki Y.",
                Message = "リンクシェル5の皆さん、こんにちは。"
            });

            buffer.Add(new ChatLogModel()
            {
                ParentOverlaySettings = this.ParentOverlaySettings,
                ParentPageSettings = this.ParentPageSettings,
                ChatCode = ChatCodes.Linkshell6,
                Speaker = "Naoki Y.",
                Message = "リンクシェル6の皆さん、こんにちは。"
            });

            buffer.Add(new ChatLogModel()
            {
                ParentOverlaySettings = this.ParentOverlaySettings,
                ParentPageSettings = this.ParentPageSettings,
                ChatCode = ChatCodes.Linkshell7,
                Speaker = "Naoki Y.",
                Message = "リンクシェル7の皆さん、こんにちは。"
            });

            buffer.Add(new ChatLogModel()
            {
                ParentOverlaySettings = this.ParentOverlaySettings,
                ParentPageSettings = this.ParentPageSettings,
                ChatCode = ChatCodes.Linkshell8,
                Speaker = "Naoki Y.",
                Message = "リンクシェル8の皆さん、こんにちは。"
            });

            buffer.Add(new ChatLogModel()
            {
                ParentOverlaySettings = this.ParentOverlaySettings,
                ParentPageSettings = this.ParentPageSettings,
                ChatCode = ChatCodes.CrossWorldLinkshell,
                Speaker = "Naoki Y.",
                Message = "CWLSの皆さん、こんにちは。"
            });

            buffer.Add(new ChatLogModel()
            {
                ParentOverlaySettings = this.ParentOverlaySettings,
                ParentPageSettings = this.ParentPageSettings,
                ChatCode = ChatCodes.FreeCompany,
                Speaker = "Naoki Y.",
                Message = "フリーカンパニーの皆さん、こんにちは。"
            });

            buffer.Add(new ChatLogModel()
            {
                ParentOverlaySettings = this.ParentOverlaySettings,
                ParentPageSettings = this.ParentPageSettings,
                ChatCode = ChatCodes.NPCAnnounce,
                Speaker = "ネール・デウス・ダーナス",
                Message = "チャリオッツいくおー ^ ^"
            });
        }
    }

    public class ChatLogAddedEventArgs : EventArgs
    {
        public ChatLogAddedEventArgs()
        {
        }
        public ChatLogAddedEventArgs(
            ChatPageSettingsModel parentPage,
            ChatLogModel addedLog)
        {
            this.ParentPage = parentPage;
            this.AddedLog = addedLog;
        }

        public ChatPageSettingsModel ParentPage { get; set; }

        public ChatLogModel AddedLog { get; set; }
    }
}
