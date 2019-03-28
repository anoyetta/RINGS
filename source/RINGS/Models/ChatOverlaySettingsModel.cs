using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using aframe;
using Newtonsoft.Json;
using Prism.Commands;
using Prism.Mvvm;
using RINGS.Common;

namespace RINGS.Models
{
    public class ChatOverlaySettingsModel :
        BindableBase
    {
        private string name;

        [JsonProperty(PropertyName = "name")]
        public string Name

        {
            get => this.name;
            set => this.SetProperty(ref this.name, value);
        }

        private bool isEnabled = true;

        [JsonProperty(PropertyName = "enabled", DefaultValueHandling = DefaultValueHandling.Include)]
        public bool IsEnabled
        {
            get => this.isEnabled;
            set => this.SetProperty(ref this.isEnabled, value);
        }

        private double x = 20;

        [JsonProperty(PropertyName = "X")]
        public double X
        {
            get => this.x;
            set => this.SetProperty(ref this.x, Math.Round(value, 1));
        }

        private double y = 20;

        [JsonProperty(PropertyName = "Y")]
        public double Y
        {
            get => this.y;
            set => this.SetProperty(ref this.y, Math.Round(value, 1));
        }

        private double w = 640;

        [JsonProperty(PropertyName = "W")]
        public double W
        {
            get => this.w;
            set => this.SetProperty(ref this.w, Math.Round(value, 1));
        }

        private double h = 480;

        [JsonProperty(PropertyName = "H")]
        public double H
        {
            get => this.h;
            set => this.SetProperty(ref this.h, Math.Round(value, 1));
        }

        private bool isLock;

        [JsonProperty(PropertyName = "is_lock")]
        public bool IsLock
        {
            get => this.isLock;
            set => this.SetProperty(ref this.isLock, value);
        }

        private double scale = 1.0d;

        [JsonProperty(PropertyName = "scale")]
        public double Scale
        {
            get => this.scale;
            set => this.SetProperty(ref this.scale, value);
        }

        private Color backgroundColor = Colors.Black;

        [JsonIgnore]
        public Color BackgroundColor
        {
            get => this.backgroundColor;
            set
            {
                if (this.SetProperty(ref this.backgroundColor, value))
                {
                    this.RaisePropertyChanged(nameof(this.BackgroundBrush));
                }
            }
        }

        [JsonIgnore]
        public SolidColorBrush BackgroundBrush
        {
            get
            {
                var brush = new SolidColorBrush(this.backgroundColor);
                brush.Opacity = this.opacity;
                brush.Freeze();
                return brush;
            }
        }

        [JsonProperty(PropertyName = "backgroud")]
        public string BackgroupColorString
        {
            get => this.backgroundColor.ToString();
            set => this.BackgroundColor = (Color)ColorConverter.ConvertFromString(value);
        }

        private double opacity = 0.60d;

        [JsonProperty(PropertyName = "opacity", DefaultValueHandling = DefaultValueHandling.Include)]
        public double Opacity
        {
            get => this.opacity;
            set
            {
                if (this.SetProperty(ref this.opacity, value))
                {
                    this.RaisePropertyChanged(nameof(this.BackgroundBrush));
                }
            }
        }

        private FontInfo font = (FontInfo)FontInfo.DefaultFont.Clone();

        [JsonProperty(PropertyName = "font", DefaultValueHandling = DefaultValueHandling.Include)]
        public FontInfo Font
        {
            get => this.font;
            set => this.SetProperty(ref this.font, value);
        }

        private double lineMargin = 1;

        [JsonProperty(PropertyName = "line_margin", DefaultValueHandling = DefaultValueHandling.Include)]
        public double LineMargin
        {
            get => this.lineMargin;
            set
            {
                if (this.SetProperty(ref this.lineMargin, value))
                {
                    this.RaisePropertyChanged(nameof(this.LineMarginThickness));
                }
            }
        }

        [JsonIgnore]
        public Thickness LineMarginThickness => new Thickness(0, 0, 0, this.lineMargin);

        private PCNameStyles pcNameStyle = PCNameStyles.FullName;

        [JsonProperty(PropertyName = "pcname_style", DefaultValueHandling = DefaultValueHandling.Include)]
        public PCNameStyles PCNameStyle
        {
            get => this.pcNameStyle;
            set => this.SetProperty(ref this.pcNameStyle, value);
        }

        private bool isAutoHide = false;

        [JsonProperty(PropertyName = "auto_hide")]
        public bool IsAutoHide
        {
            get => this.isAutoHide;
            set => this.SetProperty(ref this.isAutoHide, value);
        }

        private double timeToHide = 10.0d;

        [JsonProperty(PropertyName = "time_to_hide")]
        public double TimeToHide
        {
            get => this.timeToHide;
            set => this.SetProperty(ref this.timeToHide, value);
        }

        private bool isAutoActivatePage = false;

        [JsonProperty(PropertyName = "auto_activate_page")]
        public bool IsAutoActivatePage
        {
            get => this.isAutoActivatePage;
            set => this.SetProperty(ref this.isAutoActivatePage, value);
        }

        private readonly SuspendableObservableCollection<ChatPageSettingsModel> chatPages = new SuspendableObservableCollection<ChatPageSettingsModel>();

        [JsonProperty(PropertyName = "chat_pages", DefaultValueHandling = DefaultValueHandling.Include)]
        public SuspendableObservableCollection<ChatPageSettingsModel> ChatPages
        {
            get => this.chatPages;
            set
            {
                this.chatPages.Clear();

                foreach (var page in value)
                {
                    page.ParentOverlaySettings = this;
                }

                this.chatPages.AddRange(value, true);
            }
        }

        public ChatPageSettingsModel GetChatPage(
            string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }

            return this.chatPages.FirstOrDefault(x => x.Name == name);
        }

        public void AddChatPage(
            ChatPageSettingsModel page)
        {
            page.ParentOverlaySettings = this;
            this.chatPages.Add(page);
        }

        public void RemoveChatPage(
            string name)
        {
            var page = this.GetChatPage(name);
            if (page != null)
            {
                this.chatPages.Remove(page);
            }
        }
    }

    public class ChatPageSettingsModel :
        BindableBase
    {
        public ChatPageSettingsModel()
        {
        }

        private ChatOverlaySettingsModel parentOverlaySettings;

        [JsonIgnore]
        public ChatOverlaySettingsModel ParentOverlaySettings
        {
            get => this.parentOverlaySettings;
            set => this.SetProperty(ref this.parentOverlaySettings, value);
        }

        private string name;

        [JsonProperty(PropertyName = "name")]
        public string Name
        {
            get => this.name;
            set => this.SetProperty(ref this.name, value);
        }

        private readonly Dictionary<string, HandledChatChannelModel> handledChatChannels =
            HandledChatChannelModel.CreateDefaultHandledChannels()
            .ToDictionary(x => x.ChatCode);

        [JsonProperty(PropertyName = "handled_channels", DefaultValueHandling = DefaultValueHandling.Include)]
        public HandledChatChannelModel[] HandledChannels
        {
            get => this.handledChatChannels.Values.ToArray();
            set
            {
                this.handledChatChannels.Clear();
                foreach (var item in value)
                {
                    this.handledChatChannels[item.ChatCode] = item;
                }

                this.RaisePropertyChanged();
            }
        }

        private bool isEnabledAllChannels;

        [JsonIgnore]
        public bool IsEnabledAllChannels
        {
            get => this.isEnabledAllChannels;
            set
            {
                if (this.SetProperty(ref this.isEnabledAllChannels, value))
                {
                    this.HandledChannels.Walk(x =>
                        x.IsEnabled = this.isEnabledAllChannels);
                }
            }
        }

        private FilterModel[] ignoreFilters = FilterModel.CreateDefualtIgnoreFilters();

        [JsonProperty(PropertyName = "ignores", DefaultValueHandling = DefaultValueHandling.Include)]
        public FilterModel[] IgnoreFilters
        {
            get => this.ignoreFilters;
            set => this.SetProperty(ref this.ignoreFilters, value);
        }

        [JsonIgnore]
        public ChatLogsModel LogBuffer
        {
            get;
            private set;
        }

        public void CreateLogBuffer()
        {
            if (this.LogBuffer != null)
            {
                this.LogBuffer.Dispose();
                this.LogBuffer = null;
            }

            this.LogBuffer = new ChatLogsModel()
            {
                ParentOverlaySettings = this.ParentOverlaySettings,
                ParentPageSettings = this,
            };

            this.LogBuffer.FilterCallback = (chatLog) =>
                this.parentOverlaySettings != null &&
                this.parentOverlaySettings.IsEnabled &&
                this.handledChatChannels.ContainsKey(chatLog.ChatCode ?? string.Empty) &&
                this.handledChatChannels[chatLog.ChatCode ?? string.Empty].IsEnabled &&
                !this.ignoreFilters.Any(x => x?.IsMatch(chatLog.Message) ?? false);

            this.RaisePropertyChanged(nameof(this.LogBuffer));
        }

        public void DisposeLogBuffer()
        {
            if (this.LogBuffer != null)
            {
                this.LogBuffer.Dispose();
                this.LogBuffer = null;
            }

            this.RaisePropertyChanged(nameof(this.LogBuffer));
        }
    }

    public class HandledChatChannelModel :
        BindableBase
    {
        public static HandledChatChannelModel[] CreateDefaultHandledChannels(
            bool defaultVisibility = false)
            => ChatCodes.All.Select(code => new HandledChatChannelModel()
            {
                ChatCode = code,
                IsEnabled = defaultVisibility,
            }).ToArray();

        private string chatCode;

        [JsonProperty(PropertyName = "chat_code")]
        public string ChatCode
        {
            get => this.chatCode;
            set
            {
                if (this.SetProperty(ref this.chatCode, value))
                {
                    this.RaisePropertyChanged(nameof(this.ChannelName));
                }
            }
        }

        [JsonIgnore]
        public string ChannelName => ChatCodes.DisplayNames.ContainsKey(this.chatCode) ?
            ChatCodes.DisplayNames[this.chatCode].DisplayName :
            string.Empty;

        private bool isEnabled;

        [JsonProperty(PropertyName = "enabled")]
        public bool IsEnabled
        {
            get => this.isEnabled;
            set => this.SetProperty(ref this.isEnabled, value);
        }
    }

    public class FilterModel :
        BindableBase
    {
        public static FilterModel[] CreateDefualtIgnoreFilters() =>
            new[]
            {
                new FilterModel(),
                new FilterModel(),
                new FilterModel(),
                new FilterModel(),
                new FilterModel(),
                new FilterModel(),
                new FilterModel(),
                new FilterModel(),
            };

        private string keyword = string.Empty;

        [JsonProperty(PropertyName = "keyword", DefaultValueHandling = DefaultValueHandling.Include)]
        public string Keyword
        {
            get => this.keyword;
            set
            {
                if (this.SetProperty(ref this.keyword, value))
                {
                    this.RefreshFilterRegex();
                }
            }
        }

        private FilterTypes filterType = FilterTypes.FullMatch;

        [JsonProperty(PropertyName = "type", DefaultValueHandling = DefaultValueHandling.Include)]
        public FilterTypes FilterType
        {
            get => this.filterType;
            set
            {
                if (this.SetProperty(ref this.filterType, value))
                {
                    this.RefreshFilterRegex();
                }
            }
        }

        [JsonIgnore]
        private static readonly IEnumerable<EnumContainer<FilterTypes>> FilterTypeMasterList =
            EnumConverter.ToEnumerableContainer<FilterTypes>();

        [JsonIgnore]
        public IEnumerable<EnumContainer<FilterTypes>> FilterTypeList => FilterTypeMasterList;

        private void RefreshFilterRegex()
        {
            if (!string.IsNullOrEmpty(this.Keyword) &&
                this.FilterType == FilterTypes.Regex)
            {
                this.filterRegex = new Regex(
                    this.keyword,
                    RegexOptions.Compiled |
                    RegexOptions.IgnoreCase);
            }
            else
            {
                this.filterRegex = null;
            }
        }

        private Regex filterRegex;

        public bool IsMatch(
            string logMessage)
        {
            if (string.IsNullOrEmpty(logMessage) ||
                string.IsNullOrEmpty(this.Keyword))
            {
                return false;
            }

            var result = false;

            switch (this.filterType)
            {
                case FilterTypes.FullMatch:
                    result = this.keyword.Equals(logMessage, StringComparison.OrdinalIgnoreCase);
                    break;

                case FilterTypes.Contains:
                    result = logMessage.IndexOf(this.keyword, StringComparison.OrdinalIgnoreCase) > -1;
                    break;

                case FilterTypes.StartWith:
                    result = logMessage.StartsWith(this.keyword, StringComparison.OrdinalIgnoreCase);
                    break;

                case FilterTypes.EndWith:
                    result = logMessage.EndsWith(this.keyword, StringComparison.OrdinalIgnoreCase);
                    break;

                case FilterTypes.Regex:
                    if (this.filterRegex != null)
                    {
                        result = this.filterRegex.IsMatch(logMessage);
                    }
                    break;
            }

            return result;
        }
    }

    public class ChatChannelSettingsModel :
        BindableBase
    {
        public static readonly ChatChannelSettingsModel DefaultChannelSettings = new ChatChannelSettingsModel();

        private string chatCode;

        [JsonProperty(PropertyName = "chat_code")]
        public string ChatCode
        {
            get => this.chatCode;
            set
            {
                if (this.SetProperty(ref this.chatCode, value))
                {
                    this.RaisePropertyChanged(nameof(this.ChannelName));
                }
            }
        }

        [JsonIgnore]
        public string ChannelName => ChatCodes.DisplayNames.ContainsKey(this.chatCode) ?
            ChatCodes.DisplayNames[this.chatCode].DisplayName :
            string.Empty;

        #region Main Color

        private Color color = Colors.White;

        [JsonIgnore]
        public Color Color
        {
            get => this.color;
            set
            {
                if (this.SetProperty(ref this.color, value))
                {
                    this.RaisePropertyChanged(nameof(this.ColorBrush));
                }
            }
        }

        private static readonly Dictionary<Color, SolidColorBrush> LogColorBrushes = new Dictionary<Color, SolidColorBrush>();

        [JsonIgnore]
        public SolidColorBrush ColorBrush
        {
            get
            {
                if (LogColorBrushes.ContainsKey(this.color))
                {
                    return LogColorBrushes[this.color];
                }

                var brush = new SolidColorBrush(this.color);
                brush.Freeze();

                LogColorBrushes[this.color] = brush;
                return brush;
            }
        }

        [JsonProperty(PropertyName = "color")]
        public string ColorString
        {
            get => this.Color.ToString();
            set => this.Color = (Color)ColorConverter.ConvertFromString(value);
        }

        #endregion Main Color

        #region Shadow Color

        private bool isEnabledShadow = false;

        [JsonProperty(PropertyName = "enabled_shadow", DefaultValueHandling = DefaultValueHandling.Include)]
        public bool IsEnabledShadow
        {
            get => this.isEnabledShadow;
            set => this.SetProperty(ref this.isEnabledShadow, value);
        }

        private Color shadowColor = Colors.White;

        [JsonIgnore]
        public Color ShadowColor
        {
            get => this.shadowColor;
            set => this.SetProperty(ref this.shadowColor, value);
        }

        [JsonProperty(PropertyName = "shadow_color")]
        public string ShadowColorString
        {
            get => this.ShadowColor.ToString();
            set => this.ShadowColor = (Color)ColorConverter.ConvertFromString(value);
        }

        private double blurRadius = 1.0;

        [JsonProperty(PropertyName = "shadow_radius")]
        public double BlurRadius
        {
            get => this.blurRadius;
            set => this.SetProperty(ref this.blurRadius, value);
        }

        private double shadowOpacity = 1.0;

        [JsonProperty(PropertyName = "shadow_opacity")]
        public double ShadowOpacity
        {
            get => this.shadowOpacity;
            set => this.SetProperty(ref this.shadowOpacity, value);
        }

        private DelegateCommand changeMainColorCommand;

        [JsonIgnore]
        public DelegateCommand ChangeMainColorCommand =>
            this.changeMainColorCommand ?? (this.changeMainColorCommand = new DelegateCommand(
                () => CommandHelper.ExecuteChangeColor(
                    () => this.Color,
                    color => this.Color = color)));

        private DelegateCommand changeShadowColorCommand;

        [JsonIgnore]
        public DelegateCommand ChangeShadowColorCommand =>
            this.changeShadowColorCommand ?? (this.changeShadowColorCommand = new DelegateCommand(
                () => CommandHelper.ExecuteChangeColor(
                    () => this.ShadowColor,
                    color => this.ShadowColor = color)));

        #endregion Shadow Color
    }
}
