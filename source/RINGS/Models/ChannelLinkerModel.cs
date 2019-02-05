using Newtonsoft.Json;
using Prism.Mvvm;

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
            set => this.SetProperty(ref this.chatCode, value);
        }

        private uint discordChannelID;

        [JsonProperty(PropertyName = "discord_channel_id")]
        public uint DiscordChannelID
        {
            get => this.discordChannelID;
            set => this.SetProperty(ref this.discordChannelID, value);
        }
    }
}
