using System.Threading;
using System.Threading.Tasks;
using aframe;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using Prism.Commands;
using Prism.Mvvm;

namespace RINGS.Models
{
    public class DiscordBotModel :
        BindableBase
    {
        [JsonIgnore]
        public Config Config => Config.Instance;

        private string name;

        [JsonProperty(PropertyName = "name")]
        public string Name
        {
            get => this.name;
            set => this.SetProperty(ref this.name, value);
        }

        private string token;

        [JsonProperty(PropertyName = "token")]
        public string Token
        {
            get => this.token;
            set => this.SetProperty(ref this.token, value);
        }

        private DiscordSocketClient testBot;
        private volatile bool isReady;

        private DelegateCommand testCommand;

        [JsonIgnore]
        public DelegateCommand TestCommand =>
            this.testCommand ?? (this.testCommand = new DelegateCommand(this.ExecuteTestCommand));

        private async void ExecuteTestCommand()
        {
            this.StartTestBot();
            await this.WaitReadyAsync();

            if (!this.isReady)
            {
                MessageBoxHelper.EnqueueSnackMessage("ERROR! Connection failed. Please, you check Help and Log.");
            }

            await this.testBot.LogoutAsync();
            this.testBot.Dispose();
            this.testBot = null;
        }

        private DelegateCommand<string> pingCommand;

        [JsonIgnore]
        public DelegateCommand<string> PingCommand =>
            this.pingCommand ?? (this.pingCommand = new DelegateCommand<string>(this.ExecutePingCommand));

        private async void ExecutePingCommand(
            string channelID)
        {
            if (!ulong.TryParse(channelID, out ulong uid))
            {
                return;
            }

            this.StartTestBot();
            await this.WaitReadyAsync();

            if (!this.isReady)
            {
                MessageBoxHelper.EnqueueSnackMessage("ERROR! Failed to send ping. Please, you check Help and Log.");
                return;
            }

            var ch = this.testBot.GetChannel(uid) as ISocketMessageChannel;
            if (ch == null)
            {
                MessageBoxHelper.EnqueueSnackMessage("ERROR! Failed to send ping, channel not found. Please, you check Help and Log.");
                return;
            }

            await ch.SendMessageAsync("ping!");
            MessageBoxHelper.EnqueueSnackMessage("SUCCESS! Your bot said ping!");

            await this.testBot.LogoutAsync();
            this.testBot.Dispose();
            this.testBot = null;
        }

        private async void StartTestBot()
        {
            this.isReady = false;
            this.testBot = new DiscordSocketClient();

            this.testBot.Log += this.TestBot_Log;
            this.testBot.Ready += this.TestBot_Ready;

            await this.testBot.LoginAsync(TokenType.Bot, this.Token);
            await this.testBot.StartAsync();
        }

        private async Task WaitReadyAsync() => await Task.Run(() =>
        {
            for (int i = 0; i < 200; i++)
            {
                Thread.Sleep(50);

                if (this.isReady)
                {
                    break;
                }
            }
        });

        private Task TestBot_Log(
            LogMessage arg)
        {
            if (arg.Exception == null)
            {
                AppLogger.Write($"DISCORD Bot Tester - [{this.name}] {arg.Source}:{arg.Message}");
            }
            else
            {
                WPFHelper.Dispatcher.Invoke(() =>
                    MessageBoxHelper.EnqueueSnackMessage("FATAL ERROR! Please, you check Help and Log."));
                AppLogger.Error($"DISCORD Bot Tester - [{this.name}] {arg.Source}:{arg.Message}", arg.Exception);
            }

            return Task.CompletedTask;
        }

        private Task TestBot_Ready()
        {
            this.isReady = true;
            WPFHelper.Dispatcher.Invoke(() =>
                MessageBoxHelper.EnqueueSnackMessage("SUCCESS! Test bot is Ready!"));
            return Task.CompletedTask;
        }
    }
}
