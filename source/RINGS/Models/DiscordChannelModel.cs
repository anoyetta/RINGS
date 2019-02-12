using System.Linq;
using Newtonsoft.Json;
using Prism.Mvvm;

namespace RINGS.Models
{
    public class DiscordChannelModel :
        BindableBase
    {
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
            set
            {
                if (this.SetProperty(ref this.helperBotName, value))
                {
                    this.RaisePropertyChanged(nameof(this.HelperBot));
                }
            }
        }

        [JsonIgnore]
        public DiscordBotModel HelperBot => this.Config.DiscordBotList.FirstOrDefault(x => x.Name == this.HelperBotName);
    }
}
