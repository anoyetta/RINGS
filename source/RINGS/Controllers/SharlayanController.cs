using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using aframe;
using Gma.System.MouseKeyHook;
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
        private static readonly TimeSpan ChatIdleInterval = TimeSpan.FromMilliseconds(100);
        private static readonly TimeSpan ChatIdelThreshold = TimeSpan.FromSeconds(30);

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
                Priority = Config.Instance.ChatLogSubscriberThreadPriority,
            };

            this.subscribeChatLogThread.Start();
        });

        private volatile bool isWorking = false;

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
                    if (this.isWorking)
                    {
                        continue;
                    }

                    this.isWorking = true;

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
                        ClearLocalCaches();
                        MemoryHandler.Instance.SetProcess(
                            new ProcessModel
                            {
                                Process = ffxiv,
                                IsWin64 = true,
                            },
                            gameLanguage: language,
                            useLocalCache: true);

                        this.handledProcessID = ffxiv.Id;
                        this.previousArrayIndex = 0;
                        this.previousOffset = 0;
                        this.currentPlayer = null;

                        AppLogger.Write("Attached to FFXIV.");
                    }

                    this.RefreshActiveProfile();
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
                finally
                {
                    this.isWorking = false;
                }
            }
        }

        private static readonly string[] LocalCacheFiles = new[]
        {
            "actions.json",
            "signatures-x64.json",
            "statuses.json",
            "structures-x64.json",
            "zones.json"
        };

        private static void ClearLocalCaches()
        {
            var dir = Directory.GetCurrentDirectory();
            foreach (var f in LocalCacheFiles)
            {
                var file = Path.Combine(dir, f);
                if (File.Exists(file))
                {
                    File.Delete(file);
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
            }

            lock (Config.Instance.CharacterProfileList)
            {
                var active = Config.Instance.ActiveProfile;
                if (active == null ||
                    (!active.IsFixedActivate &&
                    active.CharacterName != this.currentPlayer?.Name))
                {
                    DiscordBotController.Instance.ClearBots();

                    var prof = Config.Instance.CharacterProfileList?.FirstOrDefault(x =>
                        x.IsEnabled &&
                        string.Equals(
                            x.CharacterName,
                            this.currentPlayer?.Name,
                            StringComparison.OrdinalIgnoreCase));

                    if (prof != null)
                    {
                        Config.Instance.CharacterProfileList.Walk(x => x.IsActive = false);
                        prof.IsActive = true;
                        AppLogger.Write($"\"{prof.CharacterName}\"'s chat link profile activated.");
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

        private DateTime lastChatLogReceivedTimestamp = DateTime.MinValue;

        private void SubscribeChatLog()
        {
            Thread.Sleep(TimeSpan.FromSeconds(DetectProcessInterval));
            AppLogger.Write("FFXIV chat log subscriber started.");

            var previousPlayerName = string.Empty;

            while (true)
            {
                var interval = TimeSpan.FromMilliseconds(Config.Instance.ChatLogPollingInterval);
                var isExistLogs = false;

                try
                {
                    // スレッドプライオリティを更新する
                    if (Thread.CurrentThread.Priority != Config.Instance.ChatLogSubscriberThreadPriority)
                    {
                        Thread.CurrentThread.Priority = Config.Instance.ChatLogSubscriberThreadPriority;
                    }

                    if (!this.IsAttached ||
                        !Reader.CanGetChatLog())
                    {
                        interval = TimeSpan.FromSeconds(DetectProcessInterval);
                        continue;
                    }

                    var targetLogs = default(IEnumerable<ChatLogItem>);

                    try
                    {
                        if (this.isWorking)
                        {
                            continue;
                        }

                        this.isWorking = true;

                        if (this.currentPlayer != null &&
                            !string.IsNullOrEmpty(this.currentPlayer.Name))
                        {
                            if (!string.IsNullOrEmpty(previousPlayerName) &&
                                !string.Equals(
                                    previousPlayerName,
                                    this.currentPlayer.Name,
                                    StringComparison.OrdinalIgnoreCase))
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

                        if (!result.ChatLogItems.Any())
                        {
                            continue;
                        }

                        targetLogs = result.ChatLogItems
                            .Where(x => ChatCodes.All.Contains(x.Code));

                        isExistLogs = targetLogs.Any();
                    }
                    finally
                    {
                        this.isWorking = false;
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
                                var playerName = this.currentPlayer?.Name;
                                if (string.IsNullOrEmpty(playerName))
                                {
                                    playerName = previousPlayerName;

                                    if (string.IsNullOrEmpty(playerName))
                                    {
                                        playerName = Config.Instance.ActiveProfile?.CharacterName;
                                    }
                                }

                                DiscordBotController.Instance.SendMessage(
                                    model.ChatCode,
                                    playerName,
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
                    interval = TimeSpan.FromSeconds(DetectProcessInterval * 2);
                }
                finally
                {
                    var now = DateTime.Now;

                    if (isExistLogs)
                    {
                        this.lastChatLogReceivedTimestamp = now;
                        Thread.Yield();
                    }
                    else
                    {
                        if ((now - this.lastChatLogReceivedTimestamp) > ChatIdelThreshold)
                        {
                            interval = ChatIdleInterval;
                        }

                        Thread.Sleep(interval);
                    }
                }
            }
        }

        private static readonly object GlobalHookLock = new object();
        private static IKeyboardMouseEvents globalHook;

        public static void SubscribeKeyHook()
        {
            lock (GlobalHookLock)
            {
                if (globalHook == null)
                {
                    globalHook = Hook.GlobalEvents();
                    globalHook.KeyPress += GlobalHook_KeyPress;
                }
            }
        }

        public static void UnsubscribeKeyHook()
        {
            lock (GlobalHookLock)
            {
                if (globalHook != null)
                {
                    globalHook.KeyPress -= GlobalHook_KeyPress;
                    globalHook.Dispose();
                    globalHook = null;
                }
            }
        }

        private static readonly char[] ChatInputingKeys = new[]
        {
            (char)System.Windows.Forms.Keys.Enter,
            (char)System.Windows.Forms.Keys.Space,
        };

        private static void GlobalHook_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
        {
            if (ChatInputingKeys.Contains(e.KeyChar))
            {
                SharlayanController.Instance.lastChatLogReceivedTimestamp = DateTime.Now;
            }
        }
    }
}
