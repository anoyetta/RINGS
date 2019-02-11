using Newtonsoft.Json;
using Prism.Mvvm;
using RINGS.Common;

namespace RINGS.Models
{
    public class ChannelLinkerModel :
        BindableBase
    {
        private bool isEnabled;

        [JsonProperty(PropertyName = "enabled")]
        public bool IsEnabled
        {
            get => this.isEnabled;
            set => this.SetProperty(ref this.isEnabled, value);
        }

        private string chatCode;

        [JsonProperty(PropertyName = "chat_code")]
        public string ChatCode
        {
            get => this.chatCode;
            set
            {
                if (this.SetProperty(ref this.chatCode, value))
                {
                    this.RaisePropertyChanged(nameof(this.ChannelName));
                    this.RaisePropertyChanged(nameof(this.ChannelShortName));
                }
            }
        }

        [JsonIgnore]
        public string ChannelName => ChatCodes.DisplayNames.ContainsKey(this.chatCode) ?
            ChatCodes.DisplayNames[this.chatCode].DisplayName :
            string.Empty;

        [JsonIgnore]
        public string ChannelShortName => ChatCodes.DisplayNames.ContainsKey(this.chatCode) ?
            ChatCodes.DisplayNames[this.chatCode].ShortName :
            string.Empty;

        private uint discordChannelID;

        [JsonProperty(PropertyName = "discord_channel_id")]
        public uint DiscordChannelID
        {
            get => this.discordChannelID;
            set => this.SetProperty(ref this.discordChannelID, value);
        }
    }
}
