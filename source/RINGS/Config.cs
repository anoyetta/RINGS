using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using aframe;
using Microsoft.Win32;
using Newtonsoft.Json;
using RINGS.Models;

namespace RINGS
{
    public partial class Config : JsonConfigBase
    {
        #region Singleton

        private static Config instance;

        public static Config Instance => instance ?? (instance = new Lazy<Config>(() => Config.Load<Config>(FileName)).Value);

        public Config()
        {
        }

        #endregion Singleton

        public static string FileName => Path.Combine(
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
            "RINGS.config.json");

        public void Save() => this.Save(FileName);

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

        private ChatOverlaySettingsModel chatOverlaySettings = new ChatOverlaySettingsModel();

        [JsonProperty(PropertyName = "chat_overlay")]
        public ChatOverlaySettingsModel ChatOverlaySettings
        {
            get => this.chatOverlaySettings;
            set => this.SetProperty(ref this.chatOverlaySettings, value);
        }

        #endregion Data
    }
}
