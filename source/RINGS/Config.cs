using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using aframe;
using aframe.Updater;
using aframe.ViewModels;
using Microsoft.Win32;
using Newtonsoft.Json;
using RINGS.Common;
using RINGS.Models;

namespace RINGS
{
    public partial class Config : JsonConfigBase
    {
        #region Lazy Singleton

        private readonly static Lazy<Config> instance = new Lazy<Config>(Load);

        public static Config Instance => instance.Value;

        public Config()
        {
            this.DiscordChannelList.CollectionChanged += (_, __) => this.RaisePropertyChanged(nameof(this.DiscordChannelItemsSource));
            this.DiscordBotList.CollectionChanged += (_, __) => this.RaisePropertyChanged(nameof(this.DiscordBotItemsSource));
        }

        #endregion Lazy Singleton

        public static string FileName => Path.Combine(
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
            "RINGS.config.json");

        public static Config Load()
        {
            MigrateConfig(FileName);

            var config = Config.Load<Config>(
                FileName,
                out bool isFirstLoad);

            // チャットページに親オブジェクトを設定する
            foreach (var overlay in config.ChatOverlaySettings)
            {
                foreach (var page in overlay.ChatPages)
                {
                    page.ParentOverlaySettings = overlay;
                }
            }

            if (isFirstLoad)
            {
                config.CharacterProfileList = CreateDefaultCharacterProfile();
                config.CharacterProfileList.First().ChannelLinkerList.AddRange(
                    CharacterProfileModel.CreateDefaultChannelLinkers());

                config.DiscordBotList = CreateDefaultDiscordBots();
                config.DiscordChannelList = CreateDefaultDiscordChannels();
            }

            return config;
        }

        public void Save() => this.Save(FileName);

        #region Migration

        /// <summary>
        /// バージョンアップ等による設定ファイルの追加、変更を反映する
        /// </summary>
        private static void MigrateConfig(
            string fileName)
        {
            fileName = SwitchFileName(fileName);

            if (!File.Exists(fileName))
            {
                return;
            }

            var config = Config.Load<Config>(
                fileName);

            var i = 0;

            // チャットページのチャンネルを整備する
            i = 0;
            var channels = ChatCodes.All
                .Select(x => new
                {
                    ChatCode = x,
                    Order = i++,
                })
                .ToArray();

            foreach (var overlay in config.ChatOverlaySettings)
            {
                foreach (var page in overlay.ChatPages)
                {
                    var handledChannels = new List<HandledChatChannelModel>(page.HandledChannels);

                    handledChannels
                        .Where(x => !ChatCodes.All.Contains(x.ChatCode))
                        .ToArray()
                        .Walk(x => handledChannels.Remove(x));

                    handledChannels.AddRange(ChatCodes.All
                        .Where(x => !handledChannels.Any(y => y.ChatCode == x))
                        .Select(x => new HandledChatChannelModel()
                        {
                            ChatCode = x,
                            IsEnabled = true,
                        }));

                    handledChannels.Sort((x, y) =>
                    {
                        var orderX = channels.FirstOrDefault(z => z.ChatCode == x.ChatCode)?.Order ?? int.MaxValue;
                        var orderY = channels.FirstOrDefault(z => z.ChatCode == y.ChatCode)?.Order ?? int.MaxValue;
                        return orderX - orderY;
                    });

                    page.HandledChannels = handledChannels.ToArray();

                    // 除外フィルタを設定する
                    if (page.IgnoreFilters == null)
                    {
                        page.IgnoreFilters = FilterModel.CreateDefualtIgnoreFilters();
                    }
                    else
                    {
                        for (int j = 0; j < page.IgnoreFilters.Length; j++)
                        {
                            if (page.IgnoreFilters[j] == null)
                            {
                                page.IgnoreFilters[j] = new FilterModel();
                            }
                        }
                    }
                }
            }

            // ログカラー設定を整備する
            var colors = new List<ChatChannelSettingsModel>(config.ChatChannelsSettings);

            colors
                .Where(x => !ChatCodes.All.Contains(x.ChatCode))
                .ToArray()
                .Walk(x => colors.Remove(x));

            colors.AddRange(ChatCodes.All
                .Where(x => !colors.Any(y => y.ChatCode == x))
                .Select(x => new ChatChannelSettingsModel()
                {
                    ChatCode = x,
                }));

            colors.Sort((x, y) =>
            {
                var orderX = channels.FirstOrDefault(z => z.ChatCode == x.ChatCode)?.Order ?? int.MaxValue;
                var orderY = channels.FirstOrDefault(z => z.ChatCode == y.ChatCode)?.Order ?? int.MaxValue;
                return orderX - orderY;
            });

            config.ChatChannelsSettings = colors.ToArray();

            // キャラクタープロファイルを整備する
            i = 0;
            var linkers = ChatCodes.LinkableChannels
                .Select(x => new
                {
                    ChatCode = x,
                    Order = i++,
                })
                .ToArray();

            foreach (var prof in config.CharacterProfileList)
            {
                prof.ChannelLinkerList
                    .Where(x => !ChatCodes.LinkableChannels.Contains(x.ChatCode))
                    .ToArray()
                    .Walk(x => prof.ChannelLinkerList.Remove(x));

                prof.ChannelLinkerList.AddRange(ChatCodes.LinkableChannels
                    .Where(x => !prof.ChannelLinkerList.Any(y => y.ChatCode == x))
                    .Select(x => new ChannelLinkerModel()
                    {
                        ChatCode = x,
                    }));

                prof.ChannelLinkerList.Sort((x, y) =>
                {
                    var orderX = linkers.FirstOrDefault(z => z.ChatCode == x.ChatCode)?.Order ?? int.MaxValue;
                    var orderY = linkers.FirstOrDefault(z => z.ChatCode == y.ChatCode)?.Order ?? int.MaxValue;
                    return orderX - orderY;
                });
            }

            // 保存する
            config.Save();
        }

        #endregion Migration

        #region Update Checker

        private string updateSourceUri = DefaultUpdateSourceUri;

        [JsonProperty(PropertyName = "update_source_uri")]
        public string UpdateSourceUri
        {
            get => this.updateSourceUri;
            set => this.SetProperty(ref this.updateSourceUri, value);
        }

        private ReleaseChannels updateChannel = ReleaseChannels.Stable;

        [JsonProperty(PropertyName = "update_channel")]
        public ReleaseChannels UpdateChannel
        {
            get => this.updateChannel;
            set
            {
                if (this.SetProperty(ref this.updateChannel, value))
                {
                    HelpViewModel.Instance.RaiseCurrentReleaseChannelChanged();
                }
            }
        }

        private DateTimeOffset lastUpdateTimestamp = DateTimeOffset.MinValue;

        [JsonIgnore]
        public DateTimeOffset LastUpdateTimestamp
        {
            get => this.lastUpdateTimestamp;
            set => this.SetProperty(ref this.lastUpdateTimestamp, value);
        }

        [JsonProperty(PropertyName = "last_update_timestamp")]
        public string LastUpdateTimestampCrypted
        {
            get => Crypter.EncryptString(this.lastUpdateTimestamp.ToString("o"));
            set
            {
                DateTime d;
                if (DateTime.TryParse(value, out d))
                {
                    if (d > DateTime.Now)
                    {
                        d = DateTime.Now;
                    }

                    this.lastUpdateTimestamp = d;
                    return;
                }

                try
                {
                    var decrypt = Crypter.DecryptString(value);
                    if (DateTime.TryParse(decrypt, out d))
                    {
                        if (d > DateTime.Now)
                        {
                            d = DateTime.Now;
                        }

                        this.lastUpdateTimestamp = d;
                        return;
                    }
                }
                catch (Exception)
                {
                }

                this.lastUpdateTimestamp = DateTime.MinValue;
            }
        }

        #endregion Update Checker

        #region Data

        [JsonIgnore]
        public string AppName => Assembly.GetExecutingAssembly().GetTitle();

        [JsonIgnore]
        public string AppNameWithVersion => $"{this.AppName} - {this.AppVersionString}";

        [JsonIgnore]
        public Version AppVersion => Assembly.GetExecutingAssembly().GetVersion();

        [JsonIgnore]
        public string AppVersionString => $"v{this.AppVersion.ToString()}";

        private double scale = 1.0;

        [JsonProperty(PropertyName = "scale")]
        public double Scale
        {
            get => this.scale;
            set => this.SetProperty(ref this.scale, Math.Round(value, 2));
        }

        private double x;

        [JsonProperty(PropertyName = "X")]
        public double X
        {
            get => this.x;
            set => this.SetProperty(ref this.x, Math.Round(value, 1));
        }

        private double y;

        [JsonProperty(PropertyName = "Y")]
        public double Y
        {
            get => this.y;
            set => this.SetProperty(ref this.y, Math.Round(value, 1));
        }

        private double w;

        [JsonProperty(PropertyName = "W")]
        public double W
        {
            get => this.w;
            set => this.SetProperty(ref this.w, Math.Round(value, 1));
        }

        private double h;

        [JsonProperty(PropertyName = "H")]
        public double H
        {
            get => this.h;
            set => this.SetProperty(ref this.h, Math.Round(value, 1));
        }

        private bool isStartupWithWindows;

        [JsonProperty(PropertyName = "is_startup_with_windows")]
        public bool IsStartupWithWindows
        {
            get => this.isStartupWithWindows;
            set
            {
                if (this.SetProperty(ref this.isStartupWithWindows, value))
                {
                    this.SetStartup(value);
                }
            }
        }

        public async void SetStartup(
            bool isStartup) =>
            await Task.Run(() =>
            {
                using (var regkey = Registry.CurrentUser.OpenSubKey(
                    @"Software\Microsoft\Windows\CurrentVersion\Run",
                    true))
                {
                    if (isStartup)
                    {
                        regkey.SetValue(
                            Assembly.GetExecutingAssembly().GetProduct(),
                            $"\"{Assembly.GetExecutingAssembly().Location}\"");
                    }
                    else
                    {
                        regkey.DeleteValue(
                            Assembly.GetExecutingAssembly().GetProduct(),
                            false);
                    }
                }
            });

        private bool isMinimizeStartup;

        [JsonProperty(PropertyName = "is_minimize_startup")]
        public bool IsMinimizeStartup
        {
            get => this.isMinimizeStartup;
            set => this.SetProperty(ref this.isMinimizeStartup, value);
        }

        private double chatLogPollingInterval = 10.0d;

        [JsonProperty(PropertyName = "chatlog_polling_interval")]
        public double ChatLogPollingInterval
        {
            get => this.chatLogPollingInterval;
            set => this.SetProperty(ref this.chatLogPollingInterval, value);
        }

        private double chatLogScrollBarWidth = 6.0d;

        [JsonProperty(PropertyName = "chatlog_scrollbar_width")]
        public double ChatLogScrollBarWidth
        {
            get => this.chatLogScrollBarWidth;
            set => this.SetProperty(ref this.chatLogScrollBarWidth, value);
        }

        private readonly Dictionary<string, ChatOverlaySettingsModel> chatOverlaySettings = new Dictionary<string, ChatOverlaySettingsModel>();

        [JsonProperty(PropertyName = "chat_overlays", DefaultValueHandling = DefaultValueHandling.Include)]
        public ChatOverlaySettingsModel[] ChatOverlaySettings
        {
            get => this.chatOverlaySettings.Values.ToArray();
            set
            {
                this.chatOverlaySettings.Clear();

                if (value != null)
                {
                    foreach (var item in value)
                    {
                        this.chatOverlaySettings[item.Name] = item;
                    }
                }

                this.RaisePropertyChanged();
            }
        }

        public void AddChatOverlaySettings(
            ChatOverlaySettingsModel settings)
        {
            this.chatOverlaySettings[settings.Name] = settings;
            this.RaisePropertyChanged(nameof(this.ChatOverlaySettings));
        }

        public void RemoveChatOverlaySettings(
            ChatOverlaySettingsModel settings)
        {
            if (this.chatOverlaySettings.ContainsKey(settings.Name))
            {
                foreach (var page in settings.ChatPages)
                {
                    page.DisposeLogBuffer();
                }

                this.chatOverlaySettings.Remove(settings.Name);
                settings = null;

                this.RaisePropertyChanged(nameof(this.ChatOverlaySettings));
            }
        }

        private readonly Dictionary<string, ChatChannelSettingsModel> chatChannelsSettings = new Dictionary<string, ChatChannelSettingsModel>();

        [JsonProperty(PropertyName = "chat_channels", DefaultValueHandling = DefaultValueHandling.Include)]
        public ChatChannelSettingsModel[] ChatChannelsSettings
        {
            get => this.chatChannelsSettings.Values.ToArray();
            set
            {
                this.chatChannelsSettings.Clear();

                if (value != null)
                {
                    foreach (var item in value)
                    {
                        this.chatChannelsSettings[item.ChatCode] = item;
                    }
                }

                this.RaisePropertyChanged();
            }
        }

        public ChatOverlaySettingsModel GetChatOverlaySettings(
            string name)
        {
            if (!string.IsNullOrEmpty(name) &&
                this.chatOverlaySettings.ContainsKey(name))
            {
                return this.chatOverlaySettings[name];
            }

            return null;
        }

        public ChatChannelSettingsModel GetChatChannelsSettings(
            string chatCode)
        {
            if (!string.IsNullOrEmpty(chatCode) &&
                this.chatChannelsSettings.ContainsKey(chatCode))
            {
                return this.chatChannelsSettings[chatCode];
            }

            return null;
        }

        [JsonProperty(PropertyName = "character_profiles", DefaultValueHandling = DefaultValueHandling.Include)]
        public SuspendableObservableCollection<CharacterProfileModel> CharacterProfileList
        {
            get;
            private set;
        } = new SuspendableObservableCollection<CharacterProfileModel>();

        [JsonIgnore]
        public CharacterProfileModel ActiveProfile
        {
            get
            {
                var fixedProf = this.CharacterProfileList.FirstOrDefault(x =>
                    x.IsEnabled &&
                    x.IsFixedActivate);
                if (fixedProf != null)
                {
                    return fixedProf;
                }

                return this.CharacterProfileList.FirstOrDefault(x =>
                    x.IsEnabled &&
                    x.IsActive);
            }
        }

        [JsonProperty(PropertyName = "discord_channels", DefaultValueHandling = DefaultValueHandling.Include)]
        public SuspendableObservableCollection<DiscordChannelModel> DiscordChannelList
        {
            get;
            private set;
        } = new SuspendableObservableCollection<DiscordChannelModel>();

        [JsonProperty(PropertyName = "discord_bots", DefaultValueHandling = DefaultValueHandling.Include)]
        public SuspendableObservableCollection<DiscordBotModel> DiscordBotList
        {
            get;
            private set;
        } = new SuspendableObservableCollection<DiscordBotModel>();

        [JsonIgnore]
        public IEnumerable<DiscordChannelModel> DiscordChannelItemsSource
            => new[]
            {
                new DiscordChannelModel()
                {
                    ID = string.Empty,
                    Name = "NO LINKED",
                },
            }.Concat(this.DiscordChannelList);

        [JsonIgnore]
        public static readonly string EmptyBotName = "NO ASSIGNED";

        [JsonIgnore]
        public IEnumerable<DiscordBotModel> DiscordBotItemsSource
            => new[]
            {
                new DiscordBotModel()
                {
                    Name = EmptyBotName,
                    Token = string.Empty,
                },
            }.Concat(this.DiscordBotList);

        #endregion Data
    }
}
