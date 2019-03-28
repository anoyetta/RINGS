using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Media;
using aframe;
using aframe.Updater;
using RINGS.Common;
using RINGS.Models;

namespace RINGS
{
    public partial class Config
    {
        private readonly static string DefaultUpdateSourceUri = "https://raw.githubusercontent.com/anoyetta/RINGS/master/RELEASE_NOTES.xml";

        public override Dictionary<string, object> DefaultValues => new Dictionary<string, object>()
        {
            { nameof(UpdateSourceUri), DefaultUpdateSourceUri },
            { nameof(UpdateChannel), ReleaseChannels.Stable },

            { nameof(Scale), 1.0d },
            { nameof(X), 200 },
            { nameof(Y), 100 },
            { nameof(W), 1024 },
            { nameof(H), 640 },
            { nameof(IsStartupWithWindows), false },
            { nameof(IsMinimizeStartup), false },
            { nameof(IsUseBuiltInBrowser), false },
            { nameof(BuiltinBrowserSize), 80.0d },
            { nameof(ChatLogPollingInterval), 20.0d },
            { nameof(DuplicateLogDue), 300.0d },
            { nameof(ChatLogSubscriberThreadPriority), ThreadPriority.BelowNormal },
            { nameof(ChatLogScrollBarWidth), 6.0d },
            { nameof(FileDirectory), Environment.GetFolderPath(Environment.SpecialFolder.Desktop) },

            { nameof(ChatOverlaySettings), CreateDefaultChatOverlaySettings() },
            { nameof(ChatChannelsSettings), CreateDefaultChatChannelsSettings() },
        };

        public readonly static string DefaultChatOverlayName = "Default";

        private static ChatOverlaySettingsModel[] CreateDefaultChatOverlaySettings()
        {
            var overlay = new ChatOverlaySettingsModel()
            {
                Name = DefaultChatOverlayName,
                X = 20,
                Y = 20,
                W = 640,
                H = 480,
                IsLock = false,
                Scale = 1.0d,
                BackgroundColor = Colors.Black,
                Opacity = 0.6d,
                PCNameStyle = PCNameStyles.FullName,
            };

            var chatPages = new SuspendableObservableCollection<ChatPageSettingsModel>()
            {
                new ChatPageSettingsModel()
                {
                    Name = "ALL",
                    ParentOverlaySettings = overlay,
                    HandledChannels = HandledChatChannelModel.CreateDefaultHandledChannels(true),
                    IgnoreFilters = FilterModel.CreateDefualtIgnoreFilters(),
                }
            };

            overlay.ChatPages = chatPages;

            return new[] { overlay };
        }

        private static ChatChannelSettingsModel[] CreateDefaultChatChannelsSettings()
        {
            var defaultSettings = new[]
            {
                new ChatChannelSettingsModel() { ChatCode = ChatCodes.Say, Color = WaColors.ぞうげいろ, },
                new ChatChannelSettingsModel() { ChatCode = ChatCodes.Yell, Color = WaColors.こがね, },
                new ChatChannelSettingsModel() { ChatCode = ChatCodes.Shout, Color = WaColors.しょうじょうひ, },
                new ChatChannelSettingsModel() { ChatCode = ChatCodes.TellIn, Color = WaColors.なでしこいろ, },
                new ChatChannelSettingsModel() { ChatCode = ChatCodes.TellOut, Color = WaColors.なでしこいろ, },
                new ChatChannelSettingsModel() { ChatCode = ChatCodes.Party, Color = WaColors.わすれなぐさいろ, },
                new ChatChannelSettingsModel() { ChatCode = ChatCodes.Alliance, Color = WaColors.あかだいだい, },
                new ChatChannelSettingsModel() { ChatCode = ChatCodes.Linkshell1, Color = WaColors.もえぎ, },
                new ChatChannelSettingsModel() { ChatCode = ChatCodes.Linkshell2, Color = WaColors.もえぎ, },
                new ChatChannelSettingsModel() { ChatCode = ChatCodes.Linkshell3, Color = WaColors.もえぎ, },
                new ChatChannelSettingsModel() { ChatCode = ChatCodes.Linkshell4, Color = WaColors.もえぎ, },
                new ChatChannelSettingsModel() { ChatCode = ChatCodes.Linkshell5, Color = WaColors.もえぎ, },
                new ChatChannelSettingsModel() { ChatCode = ChatCodes.Linkshell6, Color = WaColors.もえぎ, },
                new ChatChannelSettingsModel() { ChatCode = ChatCodes.Linkshell7, Color = WaColors.もえぎ, },
                new ChatChannelSettingsModel() { ChatCode = ChatCodes.Linkshell8, Color = WaColors.もえぎ, },
                new ChatChannelSettingsModel() { ChatCode = ChatCodes.CrossWorldLinkshell, Color = WaColors.かんぞういろ, },
                new ChatChannelSettingsModel() { ChatCode = ChatCodes.FreeCompany, Color = WaColors.きみどり, },
                new ChatChannelSettingsModel() { ChatCode = ChatCodes.FreeCompanyAnnounce, Color = WaColors.せいじねず, },
                new ChatChannelSettingsModel() { ChatCode = ChatCodes.FreeCompanyLogInOut, Color = WaColors.せいじねず, },
                new ChatChannelSettingsModel() { ChatCode = ChatCodes.NPCAnnounce, Color = WaColors.あかむらさき, },
                new ChatChannelSettingsModel() { ChatCode = ChatCodes.SystemMessage, Color = WaColors.しらうめねず, },
                new ChatChannelSettingsModel() { ChatCode = ChatCodes.PartyRecruiting, Color = WaColors.しらうめねず, },
            };

            var settings = ChatCodes.All.Select(x => new ChatChannelSettingsModel()
            {
                ChatCode = x
            }).ToList();

            foreach (var item in settings)
            {
                var s = defaultSettings
                    .FirstOrDefault(x => x.ChatCode == item.ChatCode)
                    ?? ChatChannelSettingsModel.DefaultChannelSettings;

                item.Color = s.Color;
                item.IsEnabledShadow = s.IsEnabledShadow;
                item.ShadowColor = s.ShadowColor;
                item.BlurRadius = s.BlurRadius;
                item.ShadowOpacity = s.ShadowOpacity;
            }

            return settings.ToArray();
        }

        public static readonly string EmptyChannelID = "* Your Channel ID *";
        public static readonly string EmptyBotToken = "* Your Bot Token *";

        private static SuspendableObservableCollection<CharacterProfileModel> CreateDefaultCharacterProfile() =>
            new SuspendableObservableCollection<CharacterProfileModel>()
            {
                new CharacterProfileModel()
                {
                    CharacterName = "Taro Yamada",
                },
            };

        private static SuspendableObservableCollection<DiscordChannelModel> CreateDefaultDiscordChannels() =>
            new SuspendableObservableCollection<DiscordChannelModel>()
            {
                new DiscordChannelModel()
                {
                    Name = "Default Channel",
                    ID = EmptyChannelID,
                    HelperBotName = "Default Channel Bot",
                },
            };

        private static SuspendableObservableCollection<DiscordBotModel> CreateDefaultDiscordBots() =>
            new SuspendableObservableCollection<DiscordBotModel>()
            {
                new DiscordBotModel()
                {
                    Name = "Default Channel Bot",
                    Token = EmptyBotToken
                },
            };

        public static readonly string TempDirectory = @".\temp";
    }
}
