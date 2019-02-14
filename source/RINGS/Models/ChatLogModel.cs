using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Discord.WebSocket;
using Prism.Mvvm;
using RINGS.Common;
using Sharlayan.Core;

namespace RINGS.Models
{
    public class ChatLogModel :
        BindableBase
    {
        private static readonly Thickness ZeroMargin = new Thickness();

        private FlowDocument CreateChatDocument()
        {
            var doc = new FlowDocument();

            var para1 = new Paragraph() { Margin = ZeroMargin };

            if (this.IsExistChatCodeIndicator)
            {
                para1.Inlines.Add(this.ParentOverlaySettings != null ?
                    new Run(this.ChatCodeIndicator + " ")
                    {
                        FontSize = this.ParentOverlaySettings.Font.Size * 0.9,
                        BaselineAlignment = BaselineAlignment.Center,
                    } :
                    new Run(this.ChatCodeIndicator + " "));
            }

            if (this.IsExistSpeaker)
            {
                para1.Inlines.Add(new Run(this.Speaker + " "));

                if (this.IsExistSpeakerAlias)
                {
                    para1.Inlines.Add(new Run($"( {this.SpeakerAlias} )" + " "));
                }

                para1.Inlines.Add(new Run(": "));
            }

            para1.Inlines.Add($"{this.Message}");
            doc.Blocks.Add(para1);

            if (this.discordLog == null)
            {
                return doc;
            }

            var headerBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#fff1cf"));
            var hyperLinkBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("##f8e58c"));
            headerBrush.Freeze();
            hyperLinkBrush.Freeze();

            var fontSize = this.ParentOverlaySettings != null ?
                this.ParentOverlaySettings.Font.Size * 0.9 :
                15;

            var contentWidth = this.ParentOverlaySettings != null ?
                this.ParentOverlaySettings.W * 0.8 :
                350;

            // Attachments
            foreach (var atta in this.discordLog.Attachments)
            {
                var para = CreateSubParagraph();
                AddHyperLink(para, "File", atta.Filename, atta.Url);
                doc.Blocks.Add(para);
            }

            var images = this.discordLog.Embeds
                .Where(x => x.Image.HasValue)
                .Select(x => x.Image.Value);

            var videos = this.discordLog.Embeds
                .Where(x => x.Video.HasValue)
                .Select(x => x.Video.Value);

            var links = this.discordLog.Embeds
                .Where(x =>
                    x.Url != null &&
                    !images.Any(image => image.Url == x.Url) &&
                    !videos.Any(video => video.Url == x.Url))
                .Select(x => x.Url);

            // Images
            if (images.Any())
            {
                var panel = new WrapPanel()
                {
                    Orientation = Orientation.Horizontal
                };

                foreach (var image in images)
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(image.Url);
                    bitmap.EndInit();

                    var inline = new InlineUIContainer(new Image()
                    {
                        Source = bitmap
                    });

                    var hyperlink = new Hyperlink(inline)
                    {
                        NavigateUri = new Uri(image.Url),
                    };

                    hyperlink.RequestNavigate += this.HyperlinkElement_RequestNavigate;

                    var view = new Viewbox()
                    {
                        Child = new TextBlock(hyperlink),
                        MaxWidth = 250,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Margin = new Thickness(0, 2, 4, 2)
                    };

                    panel.Children.Add(view);
                }

                doc.Blocks.Add(new BlockUIContainer(panel));
            }

            // Videos
            foreach (var video in videos)
            {
                var para = CreateSubParagraph();
                AddHyperLink(para, "Video", video.Url, video.Url);
                doc.Blocks.Add(para);
            }

            // Links
            foreach (var url in links)
            {
                var para = CreateSubParagraph();
                AddHyperLink(para, "Link", url, url);
                doc.Blocks.Add(para);
            }

            return doc;

            Paragraph CreateSubParagraph()
            {
                return new Paragraph()
                {
                    FontSize = fontSize,
                    Margin = ZeroMargin,
                    FontFamily = new FontFamily("Consolas")
                };
            }

            void AddHyperLink(
                Paragraph para,
                string header,
                string text,
                string url)
            {
                var headerElement = new Run(header + ": ")
                {
                    BaselineAlignment = BaselineAlignment.Subscript,
                    Foreground = headerBrush
                };

                var hyperlinkElement = new Hyperlink()
                {
                    Foreground = hyperLinkBrush
                };

                hyperlinkElement.Inlines.Add(new TextBlock()
                {
                    Text = text,
                    MaxWidth = contentWidth,
                    TextTrimming = TextTrimming.CharacterEllipsis,
                });

                hyperlinkElement.NavigateUri = new Uri(url);
                hyperlinkElement.RequestNavigate += this.HyperlinkElement_RequestNavigate;

                para.Inlines.Add(headerElement);
                para.Inlines.Add(hyperlinkElement);
            }
        }

        private void HyperlinkElement_RequestNavigate(
            object sender,
            RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private static volatile int CurrentID = 1;

        public ChatLogModel()
        {
            this.ID = CurrentID++;
            this.PropertyChanged += this.ChatLogModel_PropertyChanged;
        }

        private void ChatLogModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(this.OriginalSpeaker):
                case nameof(this.SpeakerType):
                case nameof(this.ParentOverlaySettings):
                    this.SetSpeaker();
                    break;

                case nameof(this.ChatCode):
                    this.ChannelSettings =
                        Config.Instance.GetChatChannelsSettings(this.chatCode)
                        ?? ChatChannelSettingsModel.DefaultChannelSettings;
                    break;
            }

            if (e.PropertyName != nameof(this.ChatDocument))
            {
                this.RaisePropertyChanged(nameof(this.ChatDocument));
            }
        }

        public FlowDocument ChatDocument => this.CreateChatDocument();

        public int ID { get; private set; }

        public DateTime Timestamp { get; private set; } = DateTime.Now;

        public bool IsDummy { get; set; } = false;

        private string chatCode = string.Empty;

        public string ChatCode
        {
            get => this.chatCode;
            set
            {
                if (this.SetProperty(ref this.chatCode, value))
                {
                    this.RaisePropertyChanged(nameof(this.ChannelName));
                    this.RaisePropertyChanged(nameof(this.ChannelShortName));
                    this.RaisePropertyChanged(nameof(this.ChatCodeIndicator));
                    this.RaisePropertyChanged(nameof(this.IsExistChatCodeIndicator));
                }
            }
        }

        public string ChannelName => ChatCodes.DisplayNames.ContainsKey(this.chatCode) ?
            ChatCodes.DisplayNames[this.chatCode].DisplayName :
            string.Empty;

        public string ChannelShortName => ChatCodes.DisplayNames.ContainsKey(this.chatCode) ?
            ChatCodes.DisplayNames[this.chatCode].ShortName :
            string.Empty;

        public string ChatCodeIndicator =>
            ChatCodes.DisplayNames.ContainsKey(this.chatCode) && !string.IsNullOrEmpty(ChatCodes.DisplayNames[this.chatCode].ShortName) ?
            $"[{ChatCodes.DisplayNames[this.chatCode].ShortName}{this.ChatCodeIndicatorPlus}]" :
            string.Empty;

        private string ChatCodeIndicatorPlus => this.IsFromDiscord ? "+" : string.Empty;

        public bool IsExistChatCodeIndicator => !string.IsNullOrEmpty(this.ChatCodeIndicator);

        private string speaker;

        public string Speaker
        {
            get => this.speaker;
            private set
            {
                if (this.SetProperty(ref this.speaker, value))
                {
                    this.RaisePropertyChanged(nameof(this.IsExistSpeaker));
                    this.RaisePropertyChanged(nameof(this.IsExistSpeakerAlias));
                }
            }
        }

        public bool IsExistSpeaker => !string.IsNullOrEmpty(this.Speaker);

        private string originalSpeaker;

        public string OriginalSpeaker
        {
            get => this.originalSpeaker;
            set => this.SetProperty(ref this.originalSpeaker, value);
        }

        private string speakerAlias;

        public string SpeakerAlias
        {
            get => this.speakerAlias;
            set
            {
                if (this.SetProperty(ref this.speakerAlias, value))
                {
                    this.RaisePropertyChanged(nameof(this.SpeakerAlias));
                }
            }
        }

        public bool IsExistSpeakerAlias => !string.IsNullOrEmpty(this.Speaker) && !string.IsNullOrEmpty(this.SpeakerAlias);

        private SpeakerTypes speakerType = SpeakerTypes.XIVPlayer;

        public SpeakerTypes SpeakerType
        {
            get => this.speakerType;
            set => this.SetProperty(ref this.speakerType, value);
        }

        public void SetSpeaker()
        {
            var speaker = string.Empty;

            switch (this.SpeakerType)
            {
                case SpeakerTypes.XIVPlayer:
                case SpeakerTypes.DiscordBot:
                    speaker = this.ParentOverlaySettings?.PCNameStyle.FormatName(this.OriginalSpeaker);
                    break;

                case SpeakerTypes.DiscordUser:
                    speaker = this.OriginalSpeaker;
                    break;
            }

            this.Speaker = speaker;
        }

        private string message;

        public string Message
        {
            get => this.message;
            set => this.SetProperty(ref this.message, value);
        }

        private bool isMe;

        public bool IsMe
        {
            get => this.isMe;
            set => this.SetProperty(ref this.isMe, value);
        }

        private SocketMessage discordLog;

        public SocketMessage DiscordLog
        {
            get => this.discordLog;
            set => this.SetProperty(ref this.discordLog, value);
        }

        public bool IsFromDiscord => this.discordLog != null;

        private ChatLogItem xivLog;

        public ChatLogItem XIVLog
        {
            get => this.xivLog;
            set => this.SetProperty(ref this.xivLog, value);
        }

        private ChatOverlaySettingsModel parentOverlaySettings;

        public ChatOverlaySettingsModel ParentOverlaySettings
        {
            get => this.parentOverlaySettings;
            set => this.SetProperty(ref this.parentOverlaySettings, value);
        }

        private ChatPageSettingsModel parentPageSettings;

        public ChatPageSettingsModel ParentPageSettings
        {
            get => this.parentPageSettings;
            set => this.SetProperty(ref this.parentPageSettings, value);
        }

        private ChatChannelSettingsModel channelSettings;

        public ChatChannelSettingsModel ChannelSettings
        {
            get => this.channelSettings;
            private set => this.SetProperty(ref this.channelSettings, value);
        }

        public override string ToString() =>
            $"{this.chatCode}:{this.speaker}:{this.message}";

        public static ChatLogModel FromXIVLog(
            ChatLogItem xivLog,
            string[] currentPlayerNames)
        {
            var log = new ChatLogModel()
            {
                XIVLog = xivLog,
                SpeakerType = SpeakerTypes.XIVPlayer,
            };

            log.ChatCode = xivLog.Code;

            var i = xivLog.Line.IndexOf(":");
            if (i >= 0)
            {
                // 話者の部分を取り出す
                var speakerPart = xivLog.Line.Substring(0, i);

                // 話者から特殊文字を除去する
                speakerPart = RemoveSpecialChar(speakerPart);

                // サーバ名部分を取り出して書式を整える
                var server = Servers.Names.FirstOrDefault(x =>
                    speakerPart.EndsWith(x));
                if (!string.IsNullOrEmpty(server))
                {
                    speakerPart = speakerPart.Replace(server, string.Empty);
                    speakerPart = $"{speakerPart}@{server}";
                }

                log.OriginalSpeaker = speakerPart;
                log.Message = xivLog.Line.Substring(i + 1);
            }
            else
            {
                log.Message = xivLog.Line;
            }

            if (currentPlayerNames != null)
            {
                log.IsMe = currentPlayerNames.Any(x =>
                    log.OriginalSpeaker.Contains(x));
            }

            return log;
        }

        private static readonly Regex SpeakerRegex = new Regex(
            @"(?<name>[a-zA-Z\-'\.]+ [a-zA-Z\-'\.]+@?[a-zA-Z]*) \((?<alias>.+)\)",
            RegexOptions.Compiled);

        public static ChatLogModel FromDiscordLog(
            SocketMessage discordLog)
        {
            var log = new ChatLogModel()
            {
                DiscordLog = discordLog
            };

            var message = new StringBuilder();

            if (!discordLog.Author.IsBot)
            {
                log.SpeakerType = SpeakerTypes.DiscordUser;
                log.OriginalSpeaker = discordLog.Author.Username;
                message.Append(discordLog.Content);
            }
            else
            {
                var i = discordLog.Content.IndexOf(":");
                if (i >= 0)
                {
                    log.SpeakerType = SpeakerTypes.DiscordBot;
                    message.Append(discordLog.Content.Substring(i + 1));

                    var speaker = discordLog.Content.Substring(0, i).Trim();
                    var match = SpeakerRegex.Match(speaker);
                    if (!match.Success)
                    {
                        log.OriginalSpeaker = speaker;
                        log.SpeakerAlias = string.Empty;
                    }
                    else
                    {
                        log.OriginalSpeaker = match.Groups["name"].ToString();
                        log.SpeakerAlias = match.Groups["alias"].ToString();
                    }
                }
                else
                {
                    log.SpeakerType = SpeakerTypes.DiscordUser;
                    log.OriginalSpeaker = discordLog.Author.Username;
                    message.Append(discordLog.Content);
                }
            }

            log.Message = message.ToString();

            return log;
        }

        private static readonly Regex[] SpecialCharRegexList = new[]
        {
            // Unicodeのその他の記号(Miscellaneous Symbols)
            new Regex("[\u2600-\u26FF]", RegexOptions.Compiled),

            // Unicodeのアルメニア文字(Armenian)
            new Regex("[\u0530-\u058F]", RegexOptions.Compiled),

            // Unicodeのグルムキー文字(Gurmukhi)
            new Regex("[\u0A00-\u0A7F]", RegexOptions.Compiled),

            // 私用領域(Private Use Area)
            new Regex("[\uE000-\uF8FF]", RegexOptions.Compiled),
        };

        /// <summary>
        /// 特殊文字を除去する
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string RemoveSpecialChar(
            string text)
        {
            foreach (var regex in SpecialCharRegexList)
            {
                text = regex.Replace(text, string.Empty);
            }

            return text;
        }
    }
}