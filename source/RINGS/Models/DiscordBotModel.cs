using System;
using Discord.WebSocket;
using Newtonsoft.Json;
using Prism.Mvvm;

namespace RINGS.Models
{
    public class DiscordBotModel :
        BindableBase,
        IDisposable

    {
        public void Dispose()
        {
            this.discordClient?.Dispose();
        }

        [JsonIgnore]
        public Config Config => Config.Instance;

        private string name;

        [JsonProperty(PropertyName = "name")]
        public string Name
        {
            get => this.name;
            set => this.SetProperty(ref this.name, value);
        }

        private string token;

        [JsonProperty(PropertyName = "token")]
        public string Token
        {
            get => this.token;
            set => this.SetProperty(ref this.token, value);
        }

        private DiscordSocketClient discordClient;
    }
}
