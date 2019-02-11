using Newtonsoft.Json;
using Prism.Mvvm;

namespace RINGS.Models
{
    public class DiscordBotModel :
        BindableBase
    {
        public DiscordBotModel()
        {
            /*
            this.PropertyChanged += (_, e) =>
            {
                this.Config.RefreshDiscordBotList();
            };
            */
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
    }
}
