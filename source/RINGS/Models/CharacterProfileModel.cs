using System.Collections.Generic;
using System.Collections.ObjectModel;
using Newtonsoft.Json;
using Prism.Mvvm;
using RINGS.Common;

namespace RINGS.Models
{
    public class CharacterProfileModel :
        BindableBase
    {
        private bool isEnabled;

        [JsonProperty(PropertyName = "enabled")]
        public bool IsEnabled
        {
            get => this.isEnabled;
            set => this.SetProperty(ref this.isEnabled, value);
        }

        private string characterName;

        [JsonProperty(PropertyName = "character_name")]
        public string CharacterName
        {
            get => this.characterName;
            set => this.SetProperty(ref this.characterName, value);
        }

        private string server;

        [JsonProperty(PropertyName = "server")]
        public string Server
        {
            get => this.server;
            set => this.SetProperty(ref this.server, value);
        }

        [JsonProperty(PropertyName = "channel_linker_settings")]
        public ObservableCollection<ChannelLinkerModel> ChannelLinkerList
        {
            get;
            private set;
        } = new ObservableCollection<ChannelLinkerModel>();

        private bool isActive;

        [JsonIgnore]
        public bool IsActive
        {
            get => this.isActive;
            set => this.SetProperty(ref this.isActive, value);
        }

        public static ObservableCollection<ChannelLinkerModel> CreateDefaultChannelLinkers()
        {
            var result = new List<ChannelLinkerModel>();

            foreach (var code in ChatCodes.All)
            {
                result.Add(new ChannelLinkerModel()
                {
                    ChatCode = code,
                    IsEnabled = false,
                });
            }

            return new ObservableCollection<ChannelLinkerModel>(result);
        }
    }
}
