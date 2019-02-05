using System;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Sharlayan;
using Sharlayan.Models;

namespace RINGS.Experience
{
    public class Program
    {
        private const string BotToken = "** Your BOT Token **";
        private const ulong TestChannelID = 492187710572724244;

        public static class FFXIVChatLogCodes
        {
            public const string System = "0039";
            public const string NPC = "0044";
            public const string Alliance = "";
            public const string Say = "";
            public const string LS1 = "0010";
            public const string CWLS = "0025";
            public const string FCBoard = "0245";
            public const string FCLogout = "2246";
            public const string PartyBoshu = "0048";
        }

        private SocketChannel ch;

        public static void Main(string[] args)
        {
            ServicePointManager.SecurityProtocol &= ~SecurityProtocolType.Tls;
            ServicePointManager.SecurityProtocol &= ~SecurityProtocolType.Tls11;
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;

            var processes = Process.GetProcessesByName("ffxiv_dx11");
            if (processes.Length > 0)
            {
                var gameLanguage = "Japanese";
                var useLocalCache = true;
                var patchVersion = "latest";

                var process = processes[0];
                var processModel = new ProcessModel
                {
                    Process = process,
                    IsWin64 = true
                };

                MemoryHandler.Instance.SetProcess(
                    processModel,
                    gameLanguage,
                    patchVersion,
                    useLocalCache);
            }

            var previousArrayIndex = 0;
            var previousOffset = 0;

            var instance = new Program();

            Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(100);

                    var result = Reader.GetChatLog(previousArrayIndex, previousOffset);

                    previousArrayIndex = result.PreviousArrayIndex;
                    previousOffset = result.PreviousOffset;

                    foreach (var chatLog in result.ChatLogItems)
                    {
                        /*
                         * Line 話者:メッセージ
                         * Code 0000形式
                         * Conbined 0000:話者:メッセージ
                        */
                        Console.WriteLine($"{chatLog.Code}:{chatLog.Line}");

                        if (chatLog.Code == "0025")
                        {
                            var textCh = instance.ch as ISocketMessageChannel;
                            await textCh?.SendMessageAsync(chatLog.Line);
                        }
                    }
                }
            });

            instance.Start().GetAwaiter().GetResult();
        }

        private DiscordSocketClient client;

        private async Task Start()
        {
            client = new DiscordSocketClient();

            client.Ready += Client_Ready;
            client.Log += Client_Log;
            client.MessageReceived += Client_MessageReceived;

            await client.LoginAsync(TokenType.Bot, BotToken);
            await client.StartAsync();

            await Task.Delay(-1);
        }

        private Task Client_MessageReceived(SocketMessage arg)
        {
            Console.WriteLine(arg.Content);

            if (arg.Content.Contains("ping"))
            {
                var ch = arg.Channel;
                ch.SendMessageAsync("pong");
            }

            return Task.CompletedTask;
        }

        private Task Client_Log(LogMessage arg)
        {
            Console.WriteLine(arg.Message);

            return Task.CompletedTask;
        }

        private Task Client_Ready()
        {
            Console.WriteLine("Ready.");
            var servers = client.Guilds;

            this.ch = this.client.GetChannel(TestChannelID);

            return Task.CompletedTask;
        }
    }
}
