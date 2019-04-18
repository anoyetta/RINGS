using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using aframe;
using Discord.WebSocket;
using Prism.Commands;
using Prism.Mvvm;
using RINGS.Common;
using RINGS.Overlays;
using Sharlayan.Core;

namespace RINGS.Models
{
    public class ChatLogModel :
        BindableBase
    {
        private static readonly Thickness ZeroMargin = new Thickness();
        private static readonly double AttachmentImageWidthRatio = 0.9d;

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
                para1.Inlines.Add(CreateSpeakerElement());

                if (this.IsExistSpeakerAlias)
                {
                    para1.Inlines.Add(new Run($"( {this.SpeakerAlias} )" + " "));
                }

                para1.Inlines.Add(new Run(": "));
            }

            para1.Inlines.Add(new Run($"{this.Message}"));
            doc.Blocks.Add(para1);

            if (this.discordLog == null)
            {
                return doc;
            }

            var headerBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#fff1cf"));
            var hyperLinkBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f8e58c"));
            headerBrush.Freeze();
            hyperLinkBrush.Freeze();

            var fontSize = this.ParentOverlaySettings != null ?
                this.ParentOverlaySettings.Font.Size * 1.0 :
                15;

            var contentWidth = this.ParentOverlaySettings != null ?
                this.ParentOverlaySettings.W * 0.8 :
                350;

            var files = this.discordLog.Attachments.Where(x => !x.Url.IsImage());
            var imagesAtta = this.discordLog.Attachments.Where(x => x.Url.IsImage());

            // Attachments
            foreach (var atta in files)
            {
                var para = CreateSubParagraph();
                AddHyperLink(para, "File", atta.Filename, atta.Url);
                doc.Blocks.Add(para);
            }

            var imagesEmbeds = this.discordLog.Embeds
                .Where(x => x.Image.HasValue)
                .Select(x => x.Image.Value);

            var videos = this.discordLog.Embeds
                .Where(x => x.Video.HasValue)
                .Select(x => x.Video.Value);

            var links = this.discordLog.Embeds
                .Where(x =>
                    x.Url != null &&
                    !imagesEmbeds.Any(image => image.Url == x.Url) &&
                    !videos.Any(video => video.Url == x.Url))
                .Select(x => x.Url);

            // Attachments Images
            if (imagesAtta.Any())
            {
                var panel = new WrapPanel()
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(0, 5, 0, 2),
                };

                foreach (var image in imagesAtta)
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
                        MaxWidth = this.ParentOverlaySettings != null ?
                            this.ParentOverlaySettings.W * AttachmentImageWidthRatio :
                            250.0d,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Margin = new Thickness(0, 2, 4, 2)
                    };

                    panel.Children.Add(view);
                }

                doc.Blocks.Add(new BlockUIContainer(panel));
            }

            // Embeds Images
            if (imagesEmbeds.Any())
            {
                var panel = new WrapPanel()
                {
                    Orientation = Orientation.Horizontal
                };

                foreach (var image in imagesEmbeds)
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
                var headerElement = new Run($" {header}: ")
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

            Inline CreateSpeakerElement()
            {
                if (this.discordLog != null ||
                    string.IsNullOrEmpty(this.SpeakerCharacterName) ||
                    this.ChatCode == ChatCodes.NPC ||
                    this.ChatCode == ChatCodes.NPCAnnounce ||
                    this.ChatCode == ChatCodes.CustomEmotes ||
                    this.ChatCode == ChatCodes.StandardEmotes)
                {
                    return new Run(this.Speaker + " ");
                }

                var url = string.Empty;
                var server = Uri.EscapeDataString(this.SpeakerServer);
                var name = this.SpeakerCharacterName;

                var first = string.Empty;
                var family = string.Empty;

                if (name.Contains(" "))
                {
                    var parts = name.Split(' ');
                    first = parts[0].Replace(".", string.Empty);
                    family = parts[1].Replace(".", string.Empty);
                }
                else
                {
                    return new Run(this.Speaker + " ");
                }

                var isInitialized = first.Length <= 1 || family.Length <= 1;

                if (!string.IsNullOrEmpty(this.SpeakerCharacterName) &&
                    !string.IsNullOrEmpty(server) &&
                    !isInitialized)
                {
                    var nameArgument = Uri.EscapeDataString($"{first} {family}");
                    url = $@"https://ja.fflogs.com/character/jp/{server}/{nameArgument}";
                }
                else
                {
                    url = $@"https://ja.fflogs.com/search/?term=";

                    if (!isInitialized)
                    {
                        url += Uri.EscapeDataString($"{first} {family}");
                    }
                    else
                    {
                        if (first.Length > 1)
                        {
                            url += Uri.EscapeDataString(first);
                        }
                        else
                        {
                            return new Run(this.Speaker + " ");
                        }
                    }
                }

                var text = new Run()
                {
                    Text = this.Speaker,
                    Cursor = Cursors.Arrow,
                    TextDecorations = TextDecorations.Underline,
                    ToolTip = url,
                    Tag = new Uri(url),
                };

                text.MouseLeftButtonDown += this.Speaker_MouseLeftButtonDown;

                var span = new Span();
                span.Inlines.Add(text);
                span.Inlines.Add(new Run(" "));

                return span;
            }
        }

        private void HyperlinkElement_RequestNavigate(
            object sender,
            RequestNavigateEventArgs e)
        {
            if (!Config.Instance.IsUseBuiltInBrowser)
            {
                Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            }
            else
            {
                var hyperlink = sender as Hyperlink;
                var window = Window.GetWindow(hyperlink);
                WebViewOverlay.Instance.ShowUrl(
                    window,
                    e.Uri.AbsoluteUri);
            }

            e.Handled = true;
        }

        private void Speaker_MouseLeftButtonDown(
            object sender,
            MouseButtonEventArgs e)
        {
            var uri = (sender as Run).Tag as Uri;

            if (uri != null)
            {
                if (!Config.Instance.IsUseBuiltInBrowser)
                {
                    Process.Start(new ProcessStartInfo(uri.AbsoluteUri));
                }
                else
                {
                    var window = Window.GetWindow(sender as Run);
                    WebViewOverlay.Instance.ShowUrl(
                        window,
                        uri.AbsoluteUri,
                        new Size(1200d, 780d));
                }
            }
        }

        private void SpeakerLink_RequestNavigate(
            object sender,
            RequestNavigateEventArgs e)
        {
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
                if (this.SetProperty(ref this.chatCode, value.ToUpper()))
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

        private string speakerCharacterName;

        public string SpeakerCharacterName
        {
            get => this.speakerCharacterName;
            set => this.SetProperty(ref this.speakerCharacterName, value);
        }

        private string speakerServer;

        public string SpeakerServer
        {
            get => this.speakerServer;
            set => this.SetProperty(ref this.speakerServer, value);
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

            var currentProfName = Config.Instance.ActiveProfile?.CharacterName;

            var i = xivLog.Line.IndexOf(":");
            if (i >= 0)
            {
                // 話者の部分を取り出す
                var speakerPart = xivLog.Line.Substring(0, i);

                // 話者から特殊文字を除去する
                speakerPart = RemoveSpecialChar(speakerPart);

                // サーバ名部分を取り出して書式を整える
                var match = CharacterNameWithServerRegex.Match(speakerPart);
                if (match.Success)
                {
                    var server = match.Groups["server"];
                    log.SpeakerServer = server.ToString();
                    log.SpeakerCharacterName = speakerPart.Remove(server.Index, server.Length);

                    speakerPart = $"{log.SpeakerCharacterName}@{log.SpeakerServer}";
                }
                else
                {
                    log.SpeakerServer = string.Empty;
                    log.SpeakerCharacterName = speakerPart;
                }

                log.OriginalSpeaker = speakerPart;
                log.Message = xivLog.Line.Substring(i + 1);
            }
            else
            {
                log.OriginalSpeaker = string.Empty;
                log.Message = xivLog.Line;
            }

            if (currentPlayerNames != null)
            {
                log.IsMe = currentPlayerNames.Any(x =>
                    log.OriginalSpeaker.Contains(x));
            }
            else
            {
                if (!string.IsNullOrEmpty(currentProfName))
                {
                    log.IsMe = log.OriginalSpeaker.Contains(currentProfName);
                }
            }

            log.Message = FomartCharacterNames(log.message);

            return log;
        }

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
            log.message = FomartCharacterNames(log.message);

            return log;
        }

        private static readonly Regex SpeakerRegex = new Regex(
            @"(?<name>[a-zA-Z\-'\.]+ [a-zA-Z\-'\.]+@?[a-zA-Z]*) \((?<alias>.+)\)",
            RegexOptions.Compiled);

        private static readonly Regex CharacterNameWithServerRegex = new Regex(
            $@"(?<name>[a-zA-Z\-'\.]+ [a-zA-Z\-'\.]+)(?<server>{string.Join("|", Servers.Names)})",
            RegexOptions.Compiled);

        private static string FomartCharacterNames(
            string message)
        {
            var matches = CharacterNameWithServerRegex.Matches(message);
            foreach (Match match in matches)
            {
                var replacement = $"{match.Groups["name"].Value}";
                if (match.Groups["server"].Success)
                {
                    replacement += $"@{match.Groups["server"]}";
                }

                message.Replace(match.Value, replacement);
            }

            return message;
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

        #region Context Menu Commands

        private DelegateCommand<ChatLogModel> copyLogCommand;

        public DelegateCommand<ChatLogModel> CopyLogCommand =>
            this.copyLogCommand ?? (this.copyLogCommand = new DelegateCommand<ChatLogModel>(this.ExecuteCopyLogCommand));

        private async void ExecuteCopyLogCommand(
            ChatLogModel model)
        {
            await WPFHelper.Dispatcher.InvokeAsync(() =>
            {
                var sb = new StringBuilder();

                var block = model.ChatDocument.Blocks.FirstOrDefault();
                if (block != null &&
                    block is Paragraph para)
                {
                    foreach (var i in para.Inlines)
                    {
                        if (i is Run run)
                        {
                            sb.Append(run.Text);
                        }
                    }
                }

                if (sb.Length > 0)
                {
                    Clipboard.SetDataObject(sb.ToString());
                }
            });
        }

        private DelegateCommand<ChatLogModel> copyMessageCommand;

        public DelegateCommand<ChatLogModel> CopyMessageCommand =>
            this.copyMessageCommand ?? (this.copyMessageCommand = new DelegateCommand<ChatLogModel>(this.ExecuteCopyMessageCommand));

        private void ExecuteCopyMessageCommand(
            ChatLogModel model)
        {
            if (model == null ||
                string.IsNullOrEmpty(model.Message))
            {
                return;
            }

            Clipboard.SetDataObject(model.Message);
        }

        private DelegateCommand<ChatLogModel> copySpeakerCommand;

        public DelegateCommand<ChatLogModel> CopySpeakerCommand =>
            this.copySpeakerCommand ?? (this.copySpeakerCommand = new DelegateCommand<ChatLogModel>(this.ExecuteCopySpeakerCommand));

        private void ExecuteCopySpeakerCommand(
            ChatLogModel model)
        {
            if (model == null ||
                string.IsNullOrEmpty(model.OriginalSpeaker))
            {
                return;
            }

            Clipboard.SetDataObject(model.OriginalSpeaker);
        }

        private DelegateCommand<ChatLogModel> invitePartyCommand;

        public DelegateCommand<ChatLogModel> InvitePartyCommand =>
            this.invitePartyCommand ?? (this.invitePartyCommand = new DelegateCommand<ChatLogModel>(this.ExecuteInvitePartyCommand));

        private void ExecuteInvitePartyCommand(
            ChatLogModel model)
        {
            if (!string.IsNullOrEmpty(model.OriginalSpeaker))
            {
                Clipboard.SetDataObject($"/pcmd add \"{model.OriginalSpeaker}\"");
            }
        }

        private DelegateCommand<ChatLogModel> kickPartyCommand;

        public DelegateCommand<ChatLogModel> KickPartyCommand =>
            this.kickPartyCommand ?? (this.kickPartyCommand = new DelegateCommand<ChatLogModel>(this.ExecuteKickPartyCommand));

        private void ExecuteKickPartyCommand(
            ChatLogModel model)
        {
            if (!string.IsNullOrEmpty(model.OriginalSpeaker))
            {
                Clipboard.SetDataObject($"/pcmd kick \"{model.OriginalSpeaker}\"");
            }
        }

        #endregion Context Menu Commands
    }

    public static class UriExtensions
    {
        private static IEnumerable<string> GetFileNameExtensions(
            ImageCodecInfo ici)
        {
            foreach (var s in ici.FilenameExtension.Split(';'))
            {
                yield return s
                    .Replace("*", string.Empty)
                    .Replace(".", string.Empty);
            }
        }

        public static string GetImageFormat(
            string ext)
        {
            ext = ext.Replace(".", string.Empty);

            foreach (var ici in ImageCodecInfo.GetImageDecoders())
            {
                foreach (var s in GetFileNameExtensions(ici))
                {
                    if (s.ToUpper() == ext.ToUpper())
                    {
                        return ici.FormatDescription;
                    }
                }
            }

            return null;
        }

        public static bool IsImage(
            this string uri)
        {
            var ext = Path.GetExtension(uri);
            if (string.IsNullOrEmpty(ext))
            {
                return false;
            }

            var format = GetImageFormat(ext);
            return !string.IsNullOrEmpty(format);
        }
    }
}
