using Newtonsoft.Json;
using Prism.Mvvm;

namespace RINGS.Models
{
    public class DiscordChannelModel :
        BindableBase
    {
        public DiscordChannelModel()
        {
            /*
            this.PropertyChanged += (_, e) =>
            {
                this.Config.RefreshDiscordChannelList();
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

        private string id;

        [JsonProperty(PropertyName = "id")]
        public string ID
        {
            get => this.id;
            set => this.SetProperty(ref this.id, value);
        }

        private string helperBotName;

        [JsonProperty(PropertyName = "helper_bot")]
        public string HelperBotName
        {
            get => this.helperBotName;
            set => this.SetProperty(ref this.helperBotName, value);
        }
    }
}
