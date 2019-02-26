using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using aframe;
using RINGS.Common;
using RINGS.Models;
using Sharlayan;
using Sharlayan.Core;
using Sharlayan.Models;

namespace RINGS.Controllers
{
    public class SharlayanController
    {
        #region Singleton

        private static readonly Lazy<SharlayanController> instance = new Lazy<SharlayanController>(() => new SharlayanController());

        public static SharlayanController Instance => instance.Value;

        private SharlayanController()
        {
        }

        #endregion Singleton

        private static readonly double DetectProcessInterval = 5.0d;

        private Thread subscribeFFXIVProcessThread;
        private Thread subscribeChatLogThread;
        private int handledProcessID;
        private int previousArrayIndex = 0;
        private int previousOffset = 0;
        private CurrentPlayer currentPlayer;
        private string[] currentPlayerNames;

        public int HandledProcessID => this.handledProcessID;

        public bool IsAttached => this.handledProcessID != 0;

        public CurrentPlayer CurrentPlayer => this.currentPlayer;

        public async Task StartAsync() => await Task.Run(() =>
        {
            this.handledProcessID = 0;
            this.previousArrayIndex = 0;
            this.previousOffset = 0;
            this.currentPlayer = null;
            this.currentPlayerNames = null;

            if (this.subscribeFFXIVProcessThread != null)
            {
                if (this.subscribeFFXIVProcessThread.IsAlive)
                {
                    this.subscribeFFXIVProcessThread.Abort();
                }

                this.subscribeFFXIVProcessThread = null;
            }

            if (this.subscribeChatLogThread != null)
            {
                if (this.subscribeChatLogThread.IsAlive)
                {
                    this.subscribeChatLogThread.Abort();
                }

                this.subscribeChatLogThread = null;
            }

            this.ClearActiveProfile();

            this.subscribeFFXIVProcessThread = new Thread(new ThreadStart(this.SubscribeFFXIVProcess))
            {
                IsBackground = true,
                Priority = ThreadPriority.Lowest,
            };

            this.subscribeFFXIVProcessThread.Start();

            this.subscribeChatLogThread = new Thread(new ThreadStart(this.SubscribeChatLog))
            {
                IsBackground = true,
                Priority = ThreadPriority.Normal,
            };

            this.subscribeChatLogThread.Start();
        });

        private void SubscribeFFXIVProcess()
        {
            var language = "Japanese";

            Thread.Sleep(TimeSpan.FromSeconds(DetectProcessInterval));
            AppLogger.Write("FFXIV process subscriber started.");

            while (true)
            {
                Thread.Sleep(TimeSpan.FromSeconds(DetectProcessInterval));

                try
                {
                    lock (this)
                    {
                        var processes = Process.GetProcessesByName("ffxiv_dx11");
                        if (processes.Length < 1)
                        {
                            if (this.IsAttached)
                            {
                                this.handledProcessID = 0;
                            }

                            this.ClearActiveProfile();
                            continue;
                        }

                        var ffxiv = processes[0];

                        if (!MemoryHandler.Instance.IsAttached ||
                            this.handledProcessID != ffxiv.Id)
                        {
                            MemoryHandler.Instance.SetProcess(
                                new ProcessModel
                                {
                                    Process = ffxiv,
                                    IsWin64 = true
                                },
                                language);

                            this.handledProcessID = ffxiv.Id;
                            this.previousArrayIndex = 0;
                            this.previousOffset = 0;
                            this.currentPlayer = null;

                            AppLogger.Write("Attached to FFXIV.");
                        }

                        this.RefreshActiveProfile();
                    }
                }
                catch (ThreadAbortException)
                {
                    return;
                }
                catch (Exception ex)
                {
                    AppLogger.Error("Happened exception from FFXIV process subscriber.", ex);
                    Thread.Sleep(TimeSpan.FromSeconds(DetectProcessInterval * 2));
                }
            }
        }

        private void RefreshActiveProfile()
        {
            if (!Reader.CanGetPlayerInfo())
            {
                this.ClearActiveProfile();
                return;
            }

            var result = Reader.GetCurrentPlayer();
            if (result == null)
            {
                this.ClearActiveProfile();
                return;
            }

            var newPlayer = result.CurrentPlayer;
            if (newPlayer != null &&
                !string.IsNullOrEmpty(newPlayer.Name) &&
                this.currentPlayer?.Name != newPlayer.Name)
            {
                this.currentPlayer = newPlayer;
                this.currentPlayerNames = new[]
                {
                    PCNameStyles.FullName.FormatName(this.currentPlayer.Name),
                    PCNameStyles.FullInitial.FormatName(this.currentPlayer.Name),
                    PCNameStyles.InitialFull.FormatName(this.currentPlayer.Name),
                    PCNameStyles.Initial.FormatName(this.currentPlayer.Name),
                };

                AppLogger.Write($"Current player is {this.currentPlayer?.Name}");

                lock (Config.Instance.CharacterProfileList)
                {
                    if (!Config.Instance.CharacterProfileList.Any(x => x.IsFixedActivate))
                    {
                        DiscordBotController.Instance.ClearBots();

                        Config.Instance.CharacterProfileList.Walk(x => x.IsActive = false);
                        var prof = Config.Instance.CharacterProfileList.FirstOrDefault(x =>
                            x.IsEnabled &&
                            x.CharacterName == this.currentPlayer.Name);
                        if (prof != null)
                        {
                            prof.IsActive = true;
                            AppLogger.Write($"\"{prof.CharacterName}\"'s chat link profile activated.");
                        }
                    }
                }
            }
        }

        private void ClearActiveProfile()
        {
            lock (Config.Instance.CharacterProfileList)
            {
                Config.Instance.CharacterProfileList.Walk(x => x.IsActive = false);
                this.currentPlayer = null;
                this.currentPlayerNames = new string[0];
            }
        }

        private void SubscribeChatLog()
        {
            Thread.Sleep(TimeSpan.FromSeconds(DetectProcessInterval));
            AppLogger.Write("FFXIV chat log subscriber started.");

            var previousPlayerName = string.Empty;

            while (true)
            {
                var isExistLogs = false;

                try
                {
                    if (!this.IsAttached ||
                        !Reader.CanGetChatLog())
                    {
                        Thread.Sleep(TimeSpan.FromSeconds(DetectProcessInterval));
                        continue;
                    }

                    var targetLogs = default(IEnumerable<ChatLogItem>);

                    lock (this)
                    {
                        if (this.currentPlayer != null &&
                            !string.IsNullOrEmpty(this.currentPlayer.Name))
                        {
                            if (!string.IsNullOrEmpty(previousPlayerName) &&
                                previousPlayerName != this.currentPlayer.Name)
                            {
                                this.previousArrayIndex = 0;
                                this.previousOffset = 0;
                            }

                            previousPlayerName = this.CurrentPlayer.Name;
                        }

                        var result = Reader.GetChatLog(this.previousArrayIndex, this.previousOffset);
                        if (result == null)
                        {
                            continue;
                        }

                        this.previousArrayIndex = result.PreviousArrayIndex;
                        this.previousOffset = result.PreviousOffset;

                        targetLogs = result.ChatLogItems
                            .Where(x => ChatCodes.All.Contains(x.Code));

                        isExistLogs = targetLogs.Any();
                    }

                    if (isExistLogs)
                    {
                        var models = targetLogs
                            .Select(x => ChatLogModel.FromXIVLog(x, this.currentPlayerNames))
                            .ToArray();

                        WPFHelper.Dispatcher.Invoke(() =>
                        {
                            ChatLogsModel.AddToBuffers(models);
                        });

                        foreach (var model in models)
                        {
                            if (model.IsMe)
                            {
                                DiscordBotController.Instance.SendMessage(
                                    model.ChatCode,
                                    this.currentPlayer?.Name,
                                    Config.Instance.ActiveProfile?.Alias,
                                    model.Message);
                            }

                            var chName = !string.IsNullOrEmpty(model.ChannelShortName) ?
                                model.ChannelShortName :
                                model.ChannelName;

                            ChatLogger.Write(
                                chName,
                                model.Speaker,
                                model.SpeakerAlias,
                                model.Message);
                        }
                    }
                }
                catch (ThreadAbortException)
                {
                    return;
                }
                catch (Exception ex)
                {
                    AppLogger.Error("Happened exception from chat log subscriber.", ex);
                    Thread.Sleep(TimeSpan.FromSeconds(DetectProcessInterval * 2));
                }

                if (isExistLogs)
                {
                    Thread.Yield();
                }
                else
                {
                    Thread.Sleep(TimeSpan.FromMilliseconds(Config.Instance.ChatLogPollingInterval));
                }
            }
        }
    }
}
