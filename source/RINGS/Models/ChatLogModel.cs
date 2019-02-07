using System;
using System.ComponentModel;
using Discord.WebSocket;
using Prism.Mvvm;
using RINGS.Common;
using Sharlayan.Core;

namespace RINGS.Models
{
    public class ChatLogModel :
        BindableBase
    {
        private static volatile int CurrentID = 1;

        public ChatLogModel()
        {
            this.ID = CurrentID++;
            this.PropertyChanged += this.ChatLogModel_PropertyChanged;
        }

        private void ChatLogModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(this.ChatCode):
                    this.ChannelSettings =
                        Config.Instance.GetChatChannelsSettings(this.chatCode)
                        ?? ChatChannelSettingsModel.DefaultChannelSettings;
                    break;
            }
        }

        public int ID { get; private set; }

        public DateTime Timestamp { get; private set; } = DateTime.Now;

        private string chatCode = string.Empty;

        public string ChatCode
        {
            get => this.chatCode;
            set
            {
                if (this.SetProperty(ref this.chatCode, value))
                {
                    this.RaisePropertyChanged(nameof(this.ChatCodeIndicator));
                }
            }
        }

        public string ChatCodeIndicator =>
            ChatCodes.DisplayNames.ContainsKey(this.chatCode) && !string.IsNullOrEmpty(ChatCodes.DisplayNames[this.chatCode].ShortName) ?
            $"[{ChatCodes.DisplayNames[this.chatCode].ShortName}] " :
            string.Empty;

        private string speaker;

        public string Speaker
        {
            get => this.speaker;
            set => this.SetProperty(ref this.speaker, value);
        }

        private string message;

        public string Message
        {
            get => this.message;
            set => this.SetProperty(ref this.message, value);
        }

        private bool isMe;

        public bool IsMe
        {
            get => this.isMe;
            set => this.SetProperty(ref this.isMe, value);
        }

        private SocketMessage discordLog;

        public SocketMessage DiscordLog
        {
            get => this.discordLog;
            set => this.SetProperty(ref this.discordLog, value);
        }

        private ChatLogItem xivLog;

        public ChatLogItem XIVLog
        {
            get => this.xivLog;
            set => this.SetProperty(ref this.xivLog, value);
        }

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

        private ChatChannelSettingsModel channelSettings;

        public ChatChannelSettingsModel ChannelSettings
        {
            get => this.channelSettings;
            private set => this.SetProperty(ref this.channelSettings, value);
        }

        public override string ToString() =>
            $"{this.chatCode}:{this.speaker}:{this.message}";

        public static ChatLogModel FromXIVLog(
            ChatLogItem xivLog)
        {
            var log = new ChatLogModel()
            {
                XIVLog = xivLog
            };

            log.ChatCode = xivLog.Code;

            var i = xivLog.Line.IndexOf(":");
            if (i >= 0)
            {
                log.Speaker = xivLog.Line.Substring(0, i);
                log.Message = xivLog.Line.Substring(i + 1);
            }
            else
            {
                log.Message = xivLog.Line;
            }

            return log;
        }

        public static ChatLogModel FromDiscordLog(
            SocketMessage dicordLog)
        {
            var log = new ChatLogModel()
            {
                DiscordLog = dicordLog
            };

            var i = dicordLog.Content.IndexOf(":");
            if (i >= 0)
            {
                log.Speaker = dicordLog.Content.Substring(0, i);
                log.Message = dicordLog.Content.Substring(i + 1);
            }
            else
            {
                log.Speaker = dicordLog.Author.Username;
                log.Message = dicordLog.Content;
            }

            return log;
        }
    }
}
