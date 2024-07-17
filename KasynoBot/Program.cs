
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
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Net;
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
            client.ComponentInteractionCreated += Client_Button_Interaction_Created;

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

        private static async Task Client_Button_Interaction_Created(DiscordClient sender, ComponentInteractionCreateEventArgs args)
        {
            switch (args.Interaction.Data.CustomId)
            {
                case "info":
                    var user = args.Interaction.User;
                    var embed = new DiscordEmbedBuilder()
                        .WithTitle("Informacje o użytkowniku")
                        .WithDescription($"Nazwa: {user.Mention}\nID: {user.Id}\nData założenia konta: {user.CreationTimestamp}")
                        .WithThumbnail(user.AvatarUrl)
                        .WithColor(DiscordColor.Gold)
                        .AddField("user", user.Mention, true);
                    await args.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));
                    break;
            }
        }
        public async Task Ping(InteractionContext ctx)
        {
            var embed = new DiscordEmbedBuilder()
                .WithTitle("Twój ping!")
                .WithDescription($"{ctx.Client.Ping}ms")
                .WithColor(DiscordColor.Gold);
            var button = new DiscordButtonComponent(ButtonStyle.Primary, "ping", "Sprawdź ponownie");
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed).AddComponents(button));
        }

        private static async Task Client_Ready(DiscordClient sender, ReadyEventArgs args)
        {
            Console.WriteLine("Bot jest gotowy do zabaw!");
            var endpoint = new ConnectionEndpoint
            {
                Hostname = "192.168.1.13", // Adres Lavalink
                Port = 2333 // Port Lavalink
            };

            var lavalink = client.UseLavalink();
            var lavalinkConfig = new LavalinkConfiguration
            {
                Password = "Dupa123", // Hasło Lavalink
                RestEndpoint = endpoint,
                SocketEndpoint = endpoint
            };

            await lavalink.ConnectAsync(lavalinkConfig);
        }

    }

}
