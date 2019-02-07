using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using aframe;
using Newtonsoft.Json;
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
            set => this.SetProperty(ref this.scale, Math.Round(value, 2));
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

        private readonly Dictionary<string, ChatPageSettingsModel> chatPages = new Dictionary<string, ChatPageSettingsModel>();

        [JsonProperty(PropertyName = "chat_pages", DefaultValueHandling = DefaultValueHandling.Include)]
        public ChatPageSettingsModel[] ChatPages
        {
            get => this.chatPages.Values.ToArray();
            set
            {
                this.chatPages.Clear();
                foreach (var item in value)
                {
                    item.ParentOverlaySettings = this;
                    this.chatPages[item.Name] = item;
                }

                this.RaisePropertyChanged();
            }
        }

        public ChatPageSettingsModel GetChatPages(
            string name)
        {
            if (string.IsNullOrEmpty(name) ||
                !this.chatPages.ContainsKey(name))
            {
                return null;
            }

            return this.chatPages[name];
        }

        public void AddChatPages(
            ChatPageSettingsModel page)
        {
            page.ParentOverlaySettings = this;
            this.chatPages[page.Name] = page;
            this.RaisePropertyChanged(nameof(this.ChatPages));
        }

        public void RemoveChatPages(
            string name)
        {
            if (!string.IsNullOrEmpty(name) &&
                this.chatPages.ContainsKey(name))
            {
                this.chatPages.Remove(name);
                this.RaisePropertyChanged(nameof(this.ChatPages));
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
                this.handledChatChannels.ContainsKey(chatLog.ChatCode ?? string.Empty) &&
                this.handledChatChannels[chatLog.ChatCode ?? string.Empty].IsEnabled;

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
            set => this.SetProperty(ref this.chatCode, value);
        }

        private bool isEnabled;

        [JsonProperty(PropertyName = "enabled")]
        public bool IsEnabled
        {
            get => this.isEnabled;
            set => this.SetProperty(ref this.isEnabled, value);
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
            set => this.SetProperty(ref this.chatCode, value);
        }

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

        private double shadowOpacity = 0;

        [JsonProperty(PropertyName = "shadow_opacity")]
        public double ShadowOpacity
        {
            get => this.shadowOpacity;
            set => this.SetProperty(ref this.shadowOpacity, value);
        }

        #endregion Shadow Color
    }
}
