using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using aframe;
using Discord;
using Prism.Mvvm;
using RINGS.Common;
using RINGS.Controllers;

namespace RINGS.ViewModels
{
    public class HomeViewModel : BindableBase
    {
        public Config Config => Config.Instance;

        public HomeViewModel()
        {
            this.refreshTimer.Tick += this.RefreshTimer_Tick;
            this.refreshTimer.Start();

            ChatLogger.OnWrite += (_, e) => this.AddLog(e);
        }

        private DispatcherTimer refreshTimer = new DispatcherTimer(DispatcherPriority.ContextIdle)
        {
            Interval = TimeSpan.FromSeconds(3),
        };

        private void RefreshTimer_Tick(object sender, EventArgs e)
        {
            var shar = SharlayanController.Instance;
            this.SharlayanStatus = shar.IsAttached ?
                "Attached" :
                string.Empty;

            this.CurrentPlayerName = shar.CurrentPlayer?.Name;

            this.ActiveProfileName = Config.Instance.CharacterProfileList
                .FirstOrDefault(x => x.IsEnabled && x.IsActive)?
                .CharacterName ?? string.Empty;

            var disco = DiscordBotController.Instance;
            var bots = disco.GetBots();
            this.DiscordBotStatus =
                bots.All(x => x.ConnectionState == ConnectionState.Connected) ?
                "Ready" :
                string.Empty;
        }

        private async void AddLog(
            AppLogOnWriteEventArgs args)
            => await Application.Current.Dispatcher.InvokeAsync(
                () => this.ChatLogs.Add(args),
                DispatcherPriority.ContextIdle);

        public ObservableCollection<AppLogOnWriteEventArgs> ChatLogs { get; }
            = new ObservableCollection<AppLogOnWriteEventArgs>();

        private string sharlayanStatus;

        public string SharlayanStatus
        {
            get => this.sharlayanStatus;
            set => this.SetProperty(ref this.sharlayanStatus, value);
        }

        private string currentPlayerName;

        public string CurrentPlayerName
        {
            get => this.currentPlayerName;
            set => this.SetProperty(ref this.currentPlayerName, value);
        }

        private string activeProfileName;

        public string ActiveProfileName
        {
            get => this.activeProfileName;
            set => this.SetProperty(ref this.activeProfileName, value);
        }

        private string discordBotStatus;

        public string DiscordBotStatus
        {
            get => this.discordBotStatus;
            set => this.SetProperty(ref this.discordBotStatus, value);
        }
    }
}
