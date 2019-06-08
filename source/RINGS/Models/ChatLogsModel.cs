using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
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

                if (WPFHelper.IsDesignMode)
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
                if (this.IsDuplicate(log))
                {
                    return;
                }

                log.ParentOverlaySettings = this.ParentOverlaySettings;
                log.ParentPageSettings = this.ParentPageSettings;
                this.Buffer.Add(log);

                if ((this.Buffer.Count % 128) == 0)
                {
                    try
                    {
                        this.Buffer.IsSuppressNotification = true;
                        this.Garbage();
                    }
                    finally
                    {
                        this.Buffer.IsSuppressNotification = false;
                    }
                }
            }
        }

        public void AddRange(
            IEnumerable<ChatLogModel> logs)
        {
            lock (this.Buffer)
            {
                foreach (var log in logs)
                {
                    if (this.IsDuplicate(log))
                    {
                        return;
                    }

                    log.ParentOverlaySettings = this.ParentOverlaySettings;
                    log.ParentPageSettings = this.ParentPageSettings;
                    this.Buffer.Add(log);
                }

                if (this.Buffer.Count > Config.Instance.ChatLogBufferSize &&
                    (this.Buffer.Count % 128) == 0)
                {
                    try
                    {
                        this.Buffer.IsSuppressNotification = true;
                        this.Garbage();
                    }
                    finally
                    {
                        this.Buffer.IsSuppressNotification = false;
                    }
                }
            }
        }

        public void Clear()
        {
            lock (this.Buffer)
            {
                this.Buffer.Clear();
            }
        }

        private string[] duplicateCheckBuffer = new string[8];
        private volatile int duplicateCheckIndex = 0;
        private DateTime duplicateCheckTimestamp = DateTime.MinValue;

        private bool IsDuplicate(
            ChatLogModel chatLog)
        {
            if ((DateTime.Now - this.duplicateCheckTimestamp).TotalMilliseconds > Config.Instance.DuplicateLogDue)
            {
                for (int i = 0; i < this.duplicateCheckBuffer.Length; i++)
                {
                    this.duplicateCheckBuffer[i] = string.Empty;
                }
            }

            this.duplicateCheckTimestamp = DateTime.Now;

            /*
            var key = $"{chatLog.ChatCode}-{NormalizeSpeaker(chatLog.Speaker)}-{chatLog.Message}";
            */
            var key = $"{NormalizeSpeaker(chatLog.Speaker)}-{chatLog.Message}";

            var result = this.duplicateCheckBuffer.Any(x => x == key);

            if (!result)
            {
                if (this.duplicateCheckIndex > this.duplicateCheckBuffer.GetUpperBound(0))
                {
                    this.duplicateCheckIndex = 0;
                }

                this.duplicateCheckBuffer[this.duplicateCheckIndex] = key;
                this.duplicateCheckIndex++;
            }

            return result;
        }

        private static string NormalizeSpeaker(
            string speaker)
        {
            var normal = speaker;
            if (string.IsNullOrEmpty(normal))
            {
                return normal;
            }

            var delimiterIndex = normal.IndexOf("@");
            if (delimiterIndex > 0)
            {
                normal = normal.Substring(0, delimiterIndex);
            }

            var parts = normal.Split(' ');
            if (parts.Length >= 2)
            {
                normal = $"{parts[0].Substring(0, 1)}{parts[1].Substring(0, 1)}";
            }

            return normal;
        }

        public void Garbage()
        {
            lock (this.Buffer)
            {
                var bufferSize = Config.Instance.ChatLogBufferSize;

                if (this.Buffer.Count <= bufferSize)
                {
                    return;
                }

                var garbageCount = (this.Buffer.Count - bufferSize) + (bufferSize / 10);
                for (int i = 0; i < garbageCount; i++)
                {
                    this.Buffer.RemoveAt(0);
                    Thread.Yield();
                }
            }
        }

        public void LoadDummyLogs()
        {
            this.RemoveDummyLogs();

            lock (this.Buffer)
            {
                this.CreateDesigntimeChatLogs(this.Buffer);
            }
        }

        public void RemoveDummyLogs()
        {
            lock (this.Buffer)
            {
                var dummys = this.Buffer.Where(x => x.IsDummy).ToArray();
                foreach (var item in dummys)
                {
                    this.Buffer.Remove(item);
                }
            }
        }

        private void CreateDesigntimeChatLogs(
            SuspendableObservableCollection<ChatLogModel> buffer)
        {
            buffer.Add(new ChatLogModel()
            {
                IsDummy = true,
                ParentOverlaySettings = this.ParentOverlaySettings,
                ParentPageSettings = this.ParentPageSettings,
                ChatCode = ChatCodes.SystemMessage,
                OriginalSpeaker = "SYSTEM",
                SpeakerType = SpeakerTypes.XIVPlayer,
                Message = $"ダミーログ {this.ParentPageSettings.Name}"
            });

            buffer.Add(new ChatLogModel()
            {
                IsDummy = true,
                ParentOverlaySettings = this.ParentOverlaySettings,
                ParentPageSettings = this.ParentPageSettings,
                ChatCode = ChatCodes.Say,
                OriginalSpeaker = "Naoki Yoshida",
                SpeakerCharacterName = "Naoki Yoshida",
                SpeakerServer = "Chocobo",
                SpeakerAlias = "Yoshi-P",
                SpeakerType = SpeakerTypes.XIVPlayer,
                Message = "本日は晴天なり。"
            });

            buffer.Add(new ChatLogModel()
            {
                IsDummy = true,
                ParentOverlaySettings = this.ParentOverlaySettings,
                ParentPageSettings = this.ParentPageSettings,
                ChatCode = ChatCodes.Say,
                OriginalSpeaker = "Naoki Yoshida",
                SpeakerCharacterName = "Naoki Yoshida",
                SpeakerServer = "Chocobo",
                SpeakerAlias = "Yoshi-P",
                SpeakerType = SpeakerTypes.XIVPlayer,
                Message = "明日も晴天かな？"
            });

            buffer.Add(new ChatLogModel()
            {
                IsDummy = true,
                ParentOverlaySettings = this.ParentOverlaySettings,
                ParentPageSettings = this.ParentPageSettings,
                ChatCode = ChatCodes.Say,
                OriginalSpeaker = "Naoki Yoshida",
                SpeakerCharacterName = "Naoki Yoshida",
                SpeakerServer = "Chocobo",
                SpeakerAlias = "Yoshi-P",
                SpeakerType = SpeakerTypes.XIVPlayer,
                Message = "あのイーハトーヴォのすきとおった風、夏でも底に冷たさをもつ青いそら、うつくしい森で飾られたモリーオ市、郊外のぎらぎらひかる草の波。"
            });

            buffer.Add(new ChatLogModel()
            {
                IsDummy = true,
                ParentOverlaySettings = this.ParentOverlaySettings,
                ParentPageSettings = this.ParentPageSettings,
                ChatCode = ChatCodes.Party,
                OriginalSpeaker = "Naoki Yoshida",
                SpeakerCharacterName = "Naoki Yoshida",
                SpeakerServer = "Chocobo",
                SpeakerAlias = "Yoshi-P",
                SpeakerType = SpeakerTypes.XIVPlayer,
                Message = "よろしくおねがいします～ ＞＜"
            });

            buffer.Add(new ChatLogModel()
            {
                IsDummy = true,
                ParentOverlaySettings = this.ParentOverlaySettings,
                ParentPageSettings = this.ParentPageSettings,
                ChatCode = ChatCodes.Linkshell1,
                OriginalSpeaker = "Naoki Yoshida",
                SpeakerCharacterName = "Naoki Yoshida",
                SpeakerServer = "Chocobo",
                SpeakerAlias = "Yoshi-P",
                SpeakerType = SpeakerTypes.XIVPlayer,
                Message = "リンクシェル1の皆さん、こんにちは。"
            });

            buffer.Add(new ChatLogModel()
            {
                IsDummy = true,
                ParentOverlaySettings = this.ParentOverlaySettings,
                ParentPageSettings = this.ParentPageSettings,
                ChatCode = ChatCodes.Linkshell2,
                OriginalSpeaker = "Naoki Yoshida",
                SpeakerCharacterName = "Naoki Yoshida",
                SpeakerServer = "Chocobo",
                SpeakerAlias = "Yoshi-P",
                SpeakerType = SpeakerTypes.XIVPlayer,
                Message = "リンクシェル2の皆さん、こんにちは。"
            });

            buffer.Add(new ChatLogModel()
            {
                IsDummy = true,
                ParentOverlaySettings = this.ParentOverlaySettings,
                ParentPageSettings = this.ParentPageSettings,
                ChatCode = ChatCodes.Linkshell3,
                OriginalSpeaker = "Naoki Yoshida",
                SpeakerCharacterName = "Naoki Yoshida",
                SpeakerServer = "Chocobo",
                SpeakerAlias = "Yoshi-P",
                SpeakerType = SpeakerTypes.XIVPlayer,
                Message = "リンクシェル3の皆さん、こんにちは。"
            });

            buffer.Add(new ChatLogModel()
            {
                IsDummy = true,
                ParentOverlaySettings = this.ParentOverlaySettings,
                ParentPageSettings = this.ParentPageSettings,
                ChatCode = ChatCodes.Linkshell4,
                OriginalSpeaker = "Naoki Yoshida",
                SpeakerCharacterName = "Naoki Yoshida",
                SpeakerServer = "Chocobo",
                SpeakerAlias = "Yoshi-P",
                SpeakerType = SpeakerTypes.XIVPlayer,
                Message = "リンクシェル4の皆さん、こんにちは。"
            });

            buffer.Add(new ChatLogModel()
            {
                IsDummy = true,
                ParentOverlaySettings = this.ParentOverlaySettings,
                ParentPageSettings = this.ParentPageSettings,
                ChatCode = ChatCodes.Linkshell5,
                OriginalSpeaker = "Naoki Yoshida",
                SpeakerCharacterName = "Naoki Yoshida",
                SpeakerServer = "Chocobo",
                SpeakerAlias = "Yoshi-P",
                SpeakerType = SpeakerTypes.XIVPlayer,
                Message = "リンクシェル5の皆さん、こんにちは。"
            });

            buffer.Add(new ChatLogModel()
            {
                IsDummy = true,
                ParentOverlaySettings = this.ParentOverlaySettings,
                ParentPageSettings = this.ParentPageSettings,
                ChatCode = ChatCodes.Linkshell6,
                OriginalSpeaker = "Naoki Yoshida",
                SpeakerCharacterName = "Naoki Yoshida",
                SpeakerServer = "Chocobo",
                SpeakerAlias = "Yoshi-P",
                SpeakerType = SpeakerTypes.XIVPlayer,
                Message = "リンクシェル6の皆さん、こんにちは。"
            });

            buffer.Add(new ChatLogModel()
            {
                IsDummy = true,
                ParentOverlaySettings = this.ParentOverlaySettings,
                ParentPageSettings = this.ParentPageSettings,
                ChatCode = ChatCodes.Linkshell7,
                OriginalSpeaker = "Naoki Yoshida",
                SpeakerCharacterName = "Naoki Yoshida",
                SpeakerServer = "Chocobo",
                SpeakerAlias = "Yoshi-P",
                SpeakerType = SpeakerTypes.XIVPlayer,
                Message = "リンクシェル7の皆さん、こんにちは。"
            });

            buffer.Add(new ChatLogModel()
            {
                IsDummy = true,
                ParentOverlaySettings = this.ParentOverlaySettings,
                ParentPageSettings = this.ParentPageSettings,
                ChatCode = ChatCodes.Linkshell8,
                OriginalSpeaker = "Naoki Yoshida",
                SpeakerCharacterName = "Naoki Yoshida",
                SpeakerServer = "Chocobo",
                SpeakerAlias = "Yoshi-P",
                SpeakerType = SpeakerTypes.XIVPlayer,
                Message = "リンクシェル8の皆さん、こんにちは。"
            });

            buffer.Add(new ChatLogModel()
            {
                IsDummy = true,
                ParentOverlaySettings = this.ParentOverlaySettings,
                ParentPageSettings = this.ParentPageSettings,
                ChatCode = ChatCodes.CrossWorldLinkshell,
                OriginalSpeaker = "Naoki Yoshida",
                SpeakerCharacterName = "Naoki Yoshida",
                SpeakerServer = "Chocobo",
                SpeakerAlias = "Yoshi-P",
                SpeakerType = SpeakerTypes.XIVPlayer,
                Message = "CWLSの皆さん、こんにちは。"
            });

            buffer.Add(new ChatLogModel()
            {
                IsDummy = true,
                ParentOverlaySettings = this.ParentOverlaySettings,
                ParentPageSettings = this.ParentPageSettings,
                ChatCode = ChatCodes.FreeCompany,
                OriginalSpeaker = "Naoki Yoshida",
                SpeakerCharacterName = "Naoki Yoshida",
                SpeakerServer = "Chocobo",
                SpeakerAlias = "Yoshi-P",
                SpeakerType = SpeakerTypes.XIVPlayer,
                Message = "フリーカンパニーの皆さん、こんにちは。"
            });

            buffer.Add(new ChatLogModel()
            {
                IsDummy = true,
                ParentOverlaySettings = this.ParentOverlaySettings,
                ParentPageSettings = this.ParentPageSettings,
                ChatCode = ChatCodes.NPCAnnounce,
                OriginalSpeaker = "ネール・デウス・ダーナス",
                SpeakerType = SpeakerTypes.XIVPlayer,
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
