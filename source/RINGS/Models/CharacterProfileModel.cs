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

        private string alias;

        [JsonProperty(PropertyName = "alias")]
        public string Alias
        {
            get => this.alias;
            set => this.SetProperty(ref this.alias, NameFilter(value));
        }

        private static string NameFilter(
            string name)
        {
            var result = string.Empty;

            var chars = name.ToCharArray();
            foreach (var c in chars)
            {
                if ((c >= 'a' && c <= 'z') ||
                    (c >= 'A' && c <= 'Z') ||
                    c == '\'' ||
                    c == '-' ||
                    c == ' ' ||
                    c == '.')
                {
                    result += c;
                }
            }

            return result;
        }

#if false
        private string server;

        [JsonProperty(PropertyName = "server")]
        public string Server
        {
            get => this.server;
            set => this.SetProperty(ref this.server, value);
        }
#endif

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

            foreach (var code in ChatCodes.LinkableChannels)
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
