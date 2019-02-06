using System;
using System.Collections.Generic;
using System.Linq;
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
                var brush = new SolidColorBrush(Color.FromScRgb((float)this.opacity, this.backgroundColor.ScR, this.backgroundColor.ScG, this.backgroundColor.ScB));
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

        private double opacity = 0.95d;

        [JsonProperty(PropertyName = "opacity")]
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

        private PCNameStyles pcNameStyle = PCNameStyles.FullName;

        [JsonProperty(PropertyName = "pcname_style")]
        public PCNameStyles PCNameStyle
        {
            get => this.pcNameStyle;
            set => this.SetProperty(ref this.pcNameStyle, value);
        }

        private readonly Dictionary<string, ChatPageSettingsModel> chatPages = new Dictionary<string, ChatPageSettingsModel>();

        [JsonProperty(PropertyName = "chat_pages")]
        public IEnumerable<ChatPageSettingsModel> ChatPages
        {
            get => this.chatPages.Values;
            set
            {
                this.chatPages.Clear();
                foreach (var item in value)
                {
                    this.chatPages[item.Name] = item;
                }

                this.RaisePropertyChanged();
            }
        }

        public ChatPageSettingsModel GetChatPages(
            string name)
        {
            if (this.chatPages.ContainsKey(name))
            {
                return null;
            }

            return this.chatPages[name];
        }

        public void AddChatPages(
            ChatPageSettingsModel page)
        {
            this.chatPages[page.Name] = page;
            this.RaisePropertyChanged(nameof(this.ChatPages));
        }

        public void RemoveChatPages(
            string name)
        {
            if (this.chatPages.ContainsKey(name))
            {
                this.chatPages.Remove(name);
                this.RaisePropertyChanged(nameof(this.ChatPages));
            }
        }
    }

    public class ChatPageSettingsModel :
        BindableBase
    {
        private string name;

        [JsonProperty(PropertyName = "name")]
        public string Name
        {
            get => this.name;
            set => this.SetProperty(ref this.name, value);
        }

        private readonly Dictionary<string, ChatChannelSettingsModel> channelSettings =
            ChatChannelSettingsModel.CreateDefaultChannels(false)
            .ToDictionary(x => x.ChatCode);

        [JsonProperty(PropertyName = "channels")]
        public IEnumerable<ChatChannelSettingsModel> ChannelSettings
        {
            get => this.channelSettings.Values;
            set
            {
                this.channelSettings.Clear();
                foreach (var item in value)
                {
                    this.channelSettings[item.ChatCode] = item;
                }

                this.RaisePropertyChanged();
            }
        }

        public ChatChannelSettingsModel GetChatChannelSettings(
            string chatCode)
        {
            if (this.channelSettings.ContainsKey(chatCode))
            {
                return null;
            }

            return this.channelSettings[chatCode];
        }
    }

    public class ChatChannelSettingsModel :
        BindableBase
    {
        public static IEnumerable<ChatChannelSettingsModel> CreateDefaultChannels(
            bool defaultVisibility = false)
            => ChatCodes.All.Select(code => new ChatChannelSettingsModel()
            {
                ChatCode = code,
                IsVisible = defaultVisibility,
            });

        private string chatCode;

        [JsonProperty(PropertyName = "chat_code")]
        public string ChatCode
        {
            get => this.chatCode;
            set => this.SetProperty(ref this.chatCode, value);
        }

        private bool isVisible;

        [JsonProperty(PropertyName = "visible")]
        public bool IsVisible
        {
            get => this.isVisible;
            set => this.SetProperty(ref this.isVisible, value);
        }

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

                var brush = WPFHelper.Dispatcher.InvokeAsync(() =>
                {
                    var b = new SolidColorBrush(this.color);
                    b.Freeze();
                    return b;
                }).Result;

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
    }
}
