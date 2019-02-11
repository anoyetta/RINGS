using System;
using System.ComponentModel;
using System.Linq;
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
                case nameof(this.OriginalSpeaker):
                case nameof(this.SpeakerType):
                case nameof(this.ParentOverlaySettings):
                    this.SetSpeaker();
                    break;

                case nameof(this.ChatCode):
                    this.ChannelSettings =
                        Config.Instance.GetChatChannelsSettings(this.chatCode)
                        ?? ChatChannelSettingsModel.DefaultChannelSettings;
                    break;
            }
        }

        public int ID { get; private set; }

        public DateTime Timestamp { get; private set; } = DateTime.Now;

        public bool IsDummy { get; set; } = false;

        private string chatCode = string.Empty;

        public string ChatCode
        {
            get => this.chatCode;
            set
            {
                if (this.SetProperty(ref this.chatCode, value))
                {
                    this.RaisePropertyChanged(nameof(this.ChannelName));
                    this.RaisePropertyChanged(nameof(this.ChannelShortName));
                    this.RaisePropertyChanged(nameof(this.ChatCodeIndicator));
                }
            }
        }

        public string ChannelName => ChatCodes.DisplayNames.ContainsKey(this.chatCode) ?
            ChatCodes.DisplayNames[this.chatCode].DisplayName :
            string.Empty;

        public string ChannelShortName => ChatCodes.DisplayNames.ContainsKey(this.chatCode) ?
            ChatCodes.DisplayNames[this.chatCode].ShortName :
            string.Empty;

        public string ChatCodeIndicator =>
            ChatCodes.DisplayNames.ContainsKey(this.chatCode) && !string.IsNullOrEmpty(ChatCodes.DisplayNames[this.chatCode].ShortName) ?
            $"[{ChatCodes.DisplayNames[this.chatCode].ShortName}] " :
            string.Empty;

        private string speaker;

        public string Speaker
        {
            get => this.speaker;
            private set => this.SetProperty(ref this.speaker, value);
        }

        private string originalSpeaker;

        public string OriginalSpeaker
        {
            get => this.originalSpeaker;
            set => this.SetProperty(ref this.originalSpeaker, value);
        }

        private SpeakerTypes speakerType = SpeakerTypes.XIVPlayer;

        public SpeakerTypes SpeakerType
        {
            get => this.speakerType;
            set => this.SetProperty(ref this.speakerType, value);
        }

        public void SetSpeaker()
        {
            var speaker = string.Empty;

            switch (this.SpeakerType)
            {
                case SpeakerTypes.XIVPlayer:
                case SpeakerTypes.DiscordBot:
                    speaker = this.ParentOverlaySettings?.PCNameStyle.FormatName(this.OriginalSpeaker);
                    break;

                case SpeakerTypes.DiscordUser:
                    speaker = this.OriginalSpeaker;
                    break;
            }

            this.Speaker = speaker;
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
            ChatLogItem xivLog,
            string[] currentPlayerNames)
        {
            var log = new ChatLogModel()
            {
                XIVLog = xivLog,
                SpeakerType = SpeakerTypes.XIVPlayer,
            };

            log.ChatCode = xivLog.Code;

            var i = xivLog.Line.IndexOf(":");
            if (i >= 0)
            {
                log.OriginalSpeaker = xivLog.Line.Substring(0, i);
                log.Message = xivLog.Line.Substring(i + 1);
            }
            else
            {
                log.Message = xivLog.Line;
            }

            if (currentPlayerNames != null)
            {
                log.IsMe = currentPlayerNames.Contains(log.OriginalSpeaker);
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

            if (!dicordLog.Author.IsBot)
            {
                log.SpeakerType = SpeakerTypes.DiscordUser;
                log.OriginalSpeaker = dicordLog.Author.Username;
                log.Message = dicordLog.Content;
            }
            else
            {
                var i = dicordLog.Content.IndexOf(":");
                if (i >= 0)
                {
                    log.SpeakerType = SpeakerTypes.DiscordBot;
                    log.OriginalSpeaker = dicordLog.Content.Substring(0, i);
                    log.Message = dicordLog.Content.Substring(i + 1);
                }
                else
                {
                    log.SpeakerType = SpeakerTypes.DiscordUser;
                    log.OriginalSpeaker = dicordLog.Author.Username;
                    log.Message = dicordLog.Content;
                }
            }

            return log;
        }
    }
}
