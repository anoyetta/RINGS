using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using aframe;
using Discord;
using MahApps.Metro.Controls.Dialogs;
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

        public System.Windows.Media.Color ActiveColor => (System.Windows.Media.Color)ColorConverter.ConvertFromString("#c3d825");

        public System.Windows.Media.Color InactiveColor => (System.Windows.Media.Color)ColorConverter.ConvertFromString("#949495");

        private static readonly string InactiveStatus = "inactive";

        private void RefreshTimer_Tick(object sender, EventArgs e)
        {
            var shar = SharlayanController.Instance;
            this.SharlayanStatus = shar.IsAttached ?
                "Attached" :
                InactiveStatus;

            this.CurrentPlayerName = shar.CurrentPlayer != null ?
                $"{shar.CurrentPlayer.Name} ({shar.CurrentPlayer.Job})" :
                InactiveStatus;

            var prof = this.Config.ActiveProfile;
            if (prof == null)
            {
                this.ActiveProfileName = InactiveStatus;
            }
            else
            {
                var links = prof.ChannelLinkerList
                    .Where(x => x.IsEnabled)
                    .Select(x => x.ChannelShortName);

                var fix = prof.IsFixedActivate ?
                    " [FIXED]" :
                    string.Empty;

                this.ActiveProfileName = links.Any() ?
                    $"{prof.CharacterName}{fix} - {string.Join(",", links)}" :
                    $"{prof.CharacterName}{fix} - NO LINK";
            }

            var bots = DiscordBotController.Instance.GetBots();
            this.DiscordBotStatus =
                bots.Any() && bots.All(x => x.ConnectionState == ConnectionState.Connected) ?
                "Ready" :
                InactiveStatus;
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

        private string sharlayanStatus = InactiveStatus;

        public string SharlayanStatus
        {
            get => this.sharlayanStatus;
            set => this.SetProperty(ref this.sharlayanStatus, value);
        }

        private string currentPlayerName = InactiveStatus;

        public string CurrentPlayerName
        {
            get => this.currentPlayerName;
            set => this.SetProperty(ref this.currentPlayerName, value);
        }

        private string activeProfileName = InactiveStatus;

        public string ActiveProfileName
        {
            get => this.activeProfileName;
            set => this.SetProperty(ref this.activeProfileName, value);
        }

        private string discordBotStatus = InactiveStatus;

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

        private DelegateCommand resetCommand;

        public DelegateCommand ResetCommand =>
            this.resetCommand ?? (this.resetCommand = new DelegateCommand(this.ExecuteResetCommand));

        private async void ExecuteResetCommand()
        {
            var result = await MessageBoxHelper.ShowMessageAsync(
                "RESET SUBSCRIBERS",
                "Sharlayan, DISCORD の監視スレッドをリセットしますか？\n" +
                "すべての接続が解除されアプリケーションの起動直後の状態に戻ります。",
                MessageDialogStyle.AffirmativeAndNegative);

            if (result != MessageDialogResult.Affirmative)
            {
                return;
            }

            this.SharlayanStatus = string.Empty;
            this.CurrentPlayerName = string.Empty;
            this.ActiveProfileName = string.Empty;
            this.DiscordBotStatus = string.Empty;

            var t1 = Task.Run(() => SharlayanController.Instance.StartAsync());
            var t2 = Task.Run(() => DiscordBotController.Instance.StartAsync());

            await Task.WhenAll(t1, t2);

            MessageBoxHelper.EnqueueSnackMessage("Subscribers restarted.");
        }

        private DelegateCommand<string> submitTestMessageCommand;

        public DelegateCommand<string> SubmitTestMessageCommand =>
            this.submitTestMessageCommand ?? (this.submitTestMessageCommand = new DelegateCommand<string>(this.ExecuteSubmitTestMessageCommand));

        private void ExecuteSubmitTestMessageCommand(
            string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                return;
            }

            var speaker = SharlayanController.Instance.CurrentPlayer?.Name ?? "RINGS";

            var model = new ChatLogModel()
            {
                ChatCode = this.TestChatCode,
                OriginalSpeaker = speaker,
                SpeakerType = SpeakerTypes.XIVPlayer,
                Message = message,
            };

            var alias = Config.Instance.ActiveProfile?.Alias ?? string.Empty;

            ChatLogsModel.AddToBuffers(model);
            DiscordBotController.Instance.SendMessage(
                model.ChatCode,
                speaker,
                alias,
                model.Message);

            ChatLogger.Write(
                model.ChannelShortName,
                speaker,
                alias,
                model.Message);
            ChatLogger.Flush();

            this.TestMessage = string.Empty;
            this.RaisePropertyChanged(nameof(this.TestMessage));
        }
    }
}
