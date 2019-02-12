using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using aframe;
using Discord;
using Prism.Commands;
using Prism.Mvvm;
using RINGS.Common;
using RINGS.Controllers;
using RINGS.Models;
using RINGS.Views;

namespace RINGS.ViewModels
{
    public class HomeViewModel : BindableBase
    {
        public Config Config => Config.Instance;

        public HomeViewModel()
        {
            this.refreshTimer.Tick += this.RefreshTimer_Tick;
            this.refreshTimer.Start();

            this.ChatLogs.CollectionChanged += (_, __) => HomeView.SendScrollToEndLog();
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

            var bots = DiscordBotController.Instance.GetBots();
            this.DiscordBotStatus =
                bots.Any() && bots.All(x => x.ConnectionState == ConnectionState.Connected) ?
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

        public IEnumerable<ChatCodeContainer> ChatCodeList =>
            ChatCodes.Linkshells.Select(x => new ChatCodeContainer()
            {
                ChatCode = x
            });

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

        private string testChatCode = ChatCodes.Linkshell1;

        public string TestChatCode
        {
            get => this.testChatCode;
            set => this.SetProperty(ref this.testChatCode, value);
        }

        private string testMessage;

        public string TestMessage
        {
            get => this.testMessage;
            set => this.SetProperty(ref this.testMessage, value);
        }

        private DelegateCommand submitTestMessageCommand;

        public DelegateCommand SubmitTestMessageCommand =>
            this.submitTestMessageCommand ?? (this.submitTestMessageCommand = new DelegateCommand(this.ExecuteSubmitTestMessageCommand));

        private void ExecuteSubmitTestMessageCommand()
        {
            if (string.IsNullOrEmpty(this.TestMessage))
            {
                return;
            }

            var model = new ChatLogModel()
            {
                ChatCode = this.TestChatCode,
                OriginalSpeaker = SharlayanController.Instance.CurrentPlayer?.Name ?? "RINGS",
                SpeakerType = SpeakerTypes.XIVPlayer,
                Message = this.TestMessage,
            };

            var alias = Config.Instance.CharacterProfileList
                .FirstOrDefault(x => x.IsEnabled && x.IsActive)?
                .CharacterName ?? string.Empty;

            ChatLogsModel.AddToBuffers(model);
            DiscordBotController.Instance.SendMessage(
                model.ChatCode,
                SharlayanController.Instance.CurrentPlayer?.Name ?? "RINGS",
                alias,
                model.Message);
            ChatLogger.Write(model.ChannelShortName, model.OriginalSpeaker, model.Speaker, model.Message);
        }
    }
}
