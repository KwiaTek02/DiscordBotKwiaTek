using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;

namespace KasynoBot.Commands.Slash
{
    [SlashCommandGroup("basic", "Basic commands")]
    public sealed class Basic : ApplicationCommandModule
    {
        [SlashCommandGroup("user", "Komendy na użytkownikach")]
        public sealed class User : ApplicationCommandModule
        {
            [SlashCommand("info", "Pokazuje informacje o użytkowniku")]
            [SlashCooldown(1, 5, SlashCooldownBucketType.User)]
            public async Task UserInfo(InteractionContext ctx,
                [Option("user", "Użytkownik o którym mają być wyświetlane informacje")] DiscordUser user = null)
            {
                user ??= ctx.User;
                var embed = new DiscordEmbedBuilder()
                    .WithTitle("Informacje o użytkowniku")
                    .WithDescription($"Nazwa: {user.Mention}\nID: {user.Id}\nData założenia konta: {user.CreationTimestamp}")
                    .WithThumbnail(user.AvatarUrl)
                    .WithColor(DiscordColor.Gold);
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));
            }
        }
    }

    [SlashCommandGroup("fun", "Fun commands")]
    public sealed class Funs : ApplicationCommandModule
    {
        [SlashCommand("ping", "Pokazuje ping użytkownika")]
        public async Task Ping(InteractionContext ctx)
        {
            var embed = new DiscordEmbedBuilder()
                .WithTitle("Twój ping!")
                .WithDescription($"{ctx.Client.Ping}ms")
                .WithColor(DiscordColor.Gold);
            
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));
        }

        [SlashCommand("pkn", "Gra w papier, kamień, nożyce")]
        public async Task KPN(InteractionContext ctx,
            [Option("wybór", "Twój wybór")] string choice)
        {
            string[] choices = new string[] { "Kamien", "Papier", "Nozyce" };
            Random random = new Random();
            int randomNumber = random.Next(0, 3);
            string botChoice = choices[randomNumber];
            string result = "";
            if (choice == botChoice)
            {
                result = "Remis!";
                var embed = new DiscordEmbedBuilder()

                    .WithTitle("Papier, Kamień, Nożyce!")
                    .WithDescription($"Wybrałeś: {choice}\nBot wybrał: {botChoice}\n\n{result}")
                    .WithColor(DiscordColor.Gold);
                
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));
            }
            else if (choice == "Kamien" && botChoice == "Nozyce" || choice == "Papier" && botChoice == "Kamien" || choice == "Nozyce" && botChoice == "Papier")
            {
                result = "Wygrałeś! Fart!";
                var embed = new DiscordEmbedBuilder()

                    .WithTitle("Papier, Kamień, Nożyce!")
                    .WithDescription($"Wybrałeś: {choice}\nBot wybrał: {botChoice}\n\n{result}")
                    .WithColor(DiscordColor.Green);

                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));
            }
            else
            {
                result = "Przegrałeś lamusie";
                var embed = new DiscordEmbedBuilder()

                    .WithTitle("Papier, Kamień, Nożyce!")
                    .WithDescription($"Wybrałeś: {choice}\nBot wybrał: {botChoice}\n\n{result}")
                    .WithColor(DiscordColor.Red);

                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));
            }
        }

        [SlashCommand("powiedz", "Bot wysyła za ciebie wiadomość")]
        public async Task Say(InteractionContext ctx,
            [Option("wiadomość", "Wiadomość do wysłania")] string message)
        {
            var embed = new DiscordEmbedBuilder()

                .WithTitle("Wiadomość")
                .WithDescription(message)
                .WithColor(DiscordColor.Gold);
            
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));
        }
    }

    [SlashCommandGroup("random", "Random commands")]
    public sealed class RandomCommands : ApplicationCommandModule
    {
        [SlashCommandGroup("losuj", "Komendy na użytkownikach")]
        public sealed class Losuj : ApplicationCommandModule
        {
            [SlashCommand("graczy", "Losuje kolejność graczy na kanale głosowym")]
            public async Task RandomUser(InteractionContext ctx)
            {
                var voiceState = ctx.Member?.VoiceState;
                if (voiceState?.Channel == null || voiceState == null)
                {
                    var embed2 = new DiscordEmbedBuilder()
                    
                        .WithTitle("Losowa kolejność graczy")
                        .WithDescription("Musisz być na kanale głosowym, aby użyć tej komendy.")
                        .WithColor(DiscordColor.Red);
                    
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed2));
                    return;
                }

                var members = voiceState.Channel.Users?.ToList();
                if (members == null || members.Count == 0)
                {
                    var embed3 = new DiscordEmbedBuilder()

                        .WithTitle("Losowa kolejność graczy")
                        .WithDescription("Na kanale głosowym nie ma innych użytkowników.")
                        .WithColor(DiscordColor.Red);
                    
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed3));
                    return;
                }

                var random = new System.Random();
                var randomizedMembers = members.OrderBy(x => random.Next()).ToList();

                var embed = new DiscordEmbedBuilder
                {
                    Title = "Losowa kolejność graczy",
                    Color = DiscordColor.Green
                };

                for (int i = 0; i < randomizedMembers.Count; i++)
                {
                    embed.AddField($"{randomizedMembers[i].DisplayName}", $"{i + 1}", inline: true);
                }

                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));
            }

            [SlashCommand("liczbe", "Losuje liczbę")]
            public async Task RandomNumber(InteractionContext ctx,
                [Option("min", "Minimalna liczba")] long min,
                [Option("max", "Maksymalna liczba")] long max)
            {
                System.Random random = new System.Random();
                int randomNumber = random.Next((int)min, (int)max+1);
                var embed = new DiscordEmbedBuilder()

                    .WithTitle("Wylosowana liczba")
                    .WithDescription(randomNumber.ToString())
                    .WithColor(DiscordColor.Green);
                
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));
            }

        }

    }
}
