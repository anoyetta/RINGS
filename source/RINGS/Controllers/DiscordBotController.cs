using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using aframe;
using Discord;
using Discord.WebSocket;
using RINGS.Common;
using RINGS.Models;

namespace RINGS.Controllers
{
    public class DiscordBotController
    {
        #region Singleton

        private static readonly Lazy<DiscordBotController> instance = new Lazy<DiscordBotController>(() => new DiscordBotController());

        public static DiscordBotController Instance => instance.Value;

        private DiscordBotController()
        {
        }

        #endregion Singleton

        private readonly ConcurrentDictionary<string, DiscordSocketClient> Bots = new ConcurrentDictionary<string, DiscordSocketClient>();

        private static readonly double DetectBotInterval = 1.0d;

        private Thread initializeBotThread;

        public DiscordSocketClient[] GetBots() => this.Bots.Values.ToArray();

        public async Task StartAsync() => await Task.Run(() =>
        {
            if (this.initializeBotThread != null)
            {
                if (this.initializeBotThread.IsAlive)
                {
                    this.initializeBotThread.Abort();
                }

                this.initializeBotThread = null;
            }

            this.ClearBots();

            this.initializeBotThread = new Thread(new ThreadStart(this.InitializeBot))
            {
                IsBackground = true,
                Priority = ThreadPriority.Lowest,
            };

            this.initializeBotThread.Start();
        });

        private void InitializeBot()
        {
            Thread.Sleep(TimeSpan.FromSeconds(5));
            AppLogger.Write("DISCORD Bot initializer started.");

            while (true)
            {
                Thread.Sleep(TimeSpan.FromSeconds(DetectBotInterval));

                try
                {
                    var activeProfile = Config.Instance.ActiveProfile;
                    if (activeProfile == null)
                    {
                        this.ClearBots();
                        Thread.Sleep(TimeSpan.FromSeconds(5));
                        continue;
                    }

                    var task = Task.Run(async () =>
                    {
                        var activeBotSettings = activeProfile.ChannelLinkerList
                            .Where(x =>
                                x.IsEnabled &&
                                !string.IsNullOrEmpty(x.DiscordChannelID))
                            .Select(x => this.GetBotByChannelID(x.DiscordChannelID))
                            .ToArray();

                        // 新しいBOTを生成する
                        var newBots = activeBotSettings
                            .Where(x =>
                                x != null &&
                                !this.Bots.ContainsKey(x?.Name))
                            .ToArray();

                        foreach (var config in newBots)
                        {
                            var bot = new DiscordSocketClient();
                            this.Bots.AddOrUpdate(
                                config.Name,
                                (_) => bot,
                                (_, old) => old = bot);

                            bot.Log += this.Bot_Log;
                            bot.MessageReceived += this.Bot_MessageReceived;

                            await bot.LoginAsync(TokenType.Bot, config.Token);
                            await bot.StartAsync();
                        }

                        // 不要になったBOTを始末する
                        var keys = this.Bots
                            .Where(x => !activeBotSettings.Any(y => y.Name == x.Key))
                            .Select(x => x.Key)
                            .ToArray();

                        this.ClearBots(keys);
                    });

                    task.Wait();
                }
                catch (ThreadAbortException)
                {
                    return;
                }
                catch (Exception ex)
                {
                    AppLogger.Error("Happened exception from DISCORD Bot initializer.", ex);
                    Thread.Sleep(TimeSpan.FromSeconds(10));
                }
            }
        }

        public async void ClearBots(
            IEnumerable<string> keys = null)
        {
            if (keys == null)
            {
                keys = this.Bots.Keys;
            }

            foreach (var key in keys)
            {
                if (this.Bots.TryRemove(key, out DiscordSocketClient bot) &&
                    bot != null)
                {
                    await bot.LogoutAsync();
                    bot.Log -= this.Bot_Log;
                    bot.MessageReceived -= this.Bot_MessageReceived;
                    bot.Dispose();
                    bot = null;
                }
            }
        }

        public string LastSendChatCode { get; private set; }

        public async void SendMessage(
            string chatCode,
            string speaker,
            string speakerAlias,
            string message,
            string fileName = null)
        {
            var bot = this.GetBotByChatCode(chatCode);
            if (bot == null ||
                bot.ConnectionState != ConnectionState.Connected)
            {
                return;
            }

            var linker = this.GetActiveChannels()?.FirstOrDefault(x => x.ChatCode == chatCode);
            if (linker == null)
            {
                return;
            }

            if (ulong.TryParse(linker.DiscordChannelID, out ulong uid))
            {
                if (uid == 0)
                {
                    return;
                }

                var ch = bot.GetChannel(uid) as ISocketMessageChannel;
                if (ch == null)
                {
                    return;
                }

                var text = !string.IsNullOrEmpty(speakerAlias) ?
                    $"{speaker} ({speakerAlias}) :{message}" :
                    $"{speaker} :{message}";

                if (string.IsNullOrEmpty(fileName) ||
                    !File.Exists(fileName))
                {
                    await ch.SendMessageAsync(text);
                }
                else
                {
                    await ch.SendFileAsync(fileName, text);
                }

                this.LastSendChatCode = chatCode;
            }
        }

        private Task Bot_Log(
            LogMessage arg)
        {
            if (arg.Exception == null)
            {
                AppLogger.Write($"DISCORD Bot - {arg.Source}:{arg.Message}");
            }
            else
            {
                AppLogger.Error($"DISCORD Bot - {arg.Source}:{arg.Message}", arg.Exception);
            }

            return Task.CompletedTask;
        }

        private readonly List<ulong> LogIDHistory = new List<ulong>(5120);

        private Task Bot_MessageReceived(
            SocketMessage arg)
        {
            lock (this.LogIDHistory)
            {
                if (this.LogIDHistory.Contains(arg.Id))
                {
                    return Task.CompletedTask;
                }

                this.LogIDHistory.Add(arg.Id);
            }

            var activeChannels = this.GetActiveChannels();
            if (activeChannels == null)
            {
                return Task.CompletedTask;
            }

            var ch = activeChannels
                .FirstOrDefault(x =>
                    x.DiscordChannelID == arg.Channel.Id.ToString());
            if (ch == null)
            {
                return Task.CompletedTask;
            }

            var model = ChatLogModel.FromDiscordLog(arg);
            model.ChatCode = ch.ChatCode;
            model.IsMe = model.OriginalSpeaker == SharlayanController.Instance.CurrentPlayer?.Name;

            if (!model.IsMe ||
                model.DiscordLog.Attachments.Any())
            {
                WPFHelper.Dispatcher.Invoke(() =>
                {
                    ChatLogsModel.AddToBuffers(model);
                });

                var chName = !string.IsNullOrEmpty(ch.ChannelShortName) ?
                    ch.ChannelShortName :
                    ch.ChannelName;

                ChatLogger.Write(
                    chName,
                    model.Speaker,
                    model.SpeakerAlias,
                    model.Message);
            }

            return Task.CompletedTask;
        }

        private ChannelLinkerModel[] cachedActiveChannels;
        private DateTime cachedActiveChannelsTimestamp = DateTime.MinValue;

        public ChannelLinkerModel[] GetActiveChannels()
        {
            if (this.cachedActiveChannels != null &&
                (DateTime.Now - this.cachedActiveChannelsTimestamp).TotalSeconds <= 1.0d)
            {
                return this.cachedActiveChannels;
            }

            var activeProfile = Config.Instance.ActiveProfile;
            if (activeProfile == null)
            {
                this.cachedActiveChannels = null;
                this.cachedActiveChannelsTimestamp = DateTime.Now;
                return null;
            }

            var activeChannels = new List<ChannelLinkerModel>();
            foreach (var chatCode in ChatCodes.LinkableChannels)
            {
                var linker = activeProfile.ChannelLinkerList.FirstOrDefault(x =>
                    x.IsEnabled &&
                    !string.IsNullOrEmpty(x.DiscordChannelID) &&
                    x.ChatCode == chatCode);

                if (linker != null)
                {
                    activeChannels.Add(linker);
                }
            }

            this.cachedActiveChannels = activeChannels.ToArray();
            this.cachedActiveChannelsTimestamp = DateTime.Now;
            return this.cachedActiveChannels;
        }

        private readonly Dictionary<string, DiscordSocketClient> cashedBots = new Dictionary<string, DiscordSocketClient>();
        private DateTime cachedBotsTimestamp = DateTime.MinValue;

        private DiscordSocketClient GetBotByChatCode(
            string chatCode)
        {
            lock (this.cashedBots)
            {
                if ((DateTime.Now - this.cachedBotsTimestamp).TotalSeconds <= DetectBotInterval)
                {
                    return this.cashedBots.ContainsKey(chatCode) ?
                        this.cashedBots[chatCode] :
                        null;
                }

                this.cashedBots.Clear();

                var activeChannels = this.GetActiveChannels();
                if (activeChannels == null)
                {
                    return null;
                }

                foreach (var ch in activeChannels)
                {
                    var botModel = this.GetBotByChannelID(ch.DiscordChannelID);
                    if (botModel != null)
                    {
                        if (this.Bots.TryGetValue(botModel.Name, out DiscordSocketClient bot))
                        {
                            this.cashedBots[ch.ChatCode] = bot;
                        }
                    }
                }

                this.cachedBotsTimestamp = DateTime.Now;

                return this.cashedBots.ContainsKey(chatCode) ?
                    this.cashedBots[chatCode] :
                    null;
            }
        }

        private DiscordBotModel GetBotByChannelID(
            string channelID)
        {
            var ch = Config.Instance.DiscordChannelList.FirstOrDefault(x => x.ID == channelID);
            return Config.Instance.DiscordBotList.FirstOrDefault(x =>
                !string.IsNullOrEmpty(x.Token) &&
                x.Name == ch?.HelperBotName);
        }
    }
}
