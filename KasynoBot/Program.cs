
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.Lavalink;
using KasynoBot.Commands.Prefix;
using KasynoBot.Commands.Slash;
using KasynoBot.ConfigHandler;
using DSharpPlus.Net;


namespace KasynoBot
{
    public sealed class Program
    {
        private static DiscordClient client { get; set; }
        private static CommandsNextExtension commands { get; set; }
        private static SlashCommandsExtension slash { get; set; }

        public static async Task Main(string[] args)
        {
            Console.WriteLine("Konfigurowanie bota!");

            var config = new JsonBotConfig();
            await config.ReadJSON();
            var clientConfiguration = new DiscordConfiguration
            {
                Intents = DiscordIntents.All,
                ReconnectIndefinitely = true,
                ShardId = 0,
                ShardCount = 1,
                AutoReconnect = true,
                TokenType = TokenType.Bot,
                Token = $"{config.DiscordBotToken}"
            };

            client = new DiscordClient(clientConfiguration);

            client.Ready += Client_Ready;

            var commandsConfiguration = new CommandsNextConfiguration
            {
                StringPrefixes = new string[] { $"{config.DiscordBotPrefix}" },
                EnableDms = false,
                EnableMentionPrefix = true,
                EnableDefaultHelp = false
            };

            commands = client.UseCommandsNext(commandsConfiguration);

            commands.RegisterCommands<Fun>();
            commands.RegisterCommands<Economy>();

            slash = client.UseSlashCommands();

            slash.RegisterCommands<Basic.User>();
            slash.RegisterCommands<Funs>();
            slash.RegisterCommands<RandomCommands.Losuj>();
            slash.RegisterCommands<EconomySlash>();
            slash.RegisterCommands<MusicCommands>();

            client.UseInteractivity(new InteractivityConfiguration
            {
                Timeout = TimeSpan.FromMinutes(2)
            });


            
            await client.ConnectAsync();
            await Task.Delay(-1);
        }
        private static async Task Client_Ready(DiscordClient sender, ReadyEventArgs args)
        {
            Console.WriteLine("Bot jest gotowy do zabaw!");
            var endpoint = new ConnectionEndpoint
            {
                Hostname = "192.168.1.13",
                Port = 2333 
            };

            var lavalink = client.UseLavalink();
            var lavalinkConfig = new LavalinkConfiguration
            {
                Password = "Dupa123",
                RestEndpoint = endpoint,
                SocketEndpoint = endpoint
            };

            await lavalink.ConnectAsync(lavalinkConfig);
        }

    }

}
