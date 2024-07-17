using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.CommandsNext.Attributes;

namespace KasynoBot.Commands.Prefix
{
    public sealed class Fun : BaseCommandModule 
    {
        [Command("ping")]
        public async Task Ping(CommandContext ctx, string? type = null)
        {
            var embed = new DiscordEmbedBuilder
            {
                Title = "Twój ping!",
                Description = $"{ctx.Client.Ping}ms",
                Color = DiscordColor.Green
            };
            
            await ctx.RespondAsync(embed: embed);

        }

        [Command("losujliczbe")]
        public async Task RandomNumber(CommandContext ctx, int min, int max)
        {
            Random random = new Random();
            int randomNumber = random.Next(min, max+1);
            var embed = new DiscordEmbedBuilder
            {
                Title = "Wylosowana liczba",
                Description = randomNumber.ToString(),
                Color = DiscordColor.Green
            };
            await ctx.RespondAsync(embed: embed);
        }

        [Command("pkn")]
        public async Task KPN(CommandContext ctx, string choice)
        {
            string[] choices = new string[] { "Kamien", "Papier", "Nozyce" };
            Random random = new Random();
            int randomNumber = random.Next(0, 3);
            string botChoice = choices[randomNumber];
            string result = "";
            if (choice == botChoice)
            {
                result = "Remis!";
            }else if (choice == "Kamien" && botChoice == "Nozyce" || choice == "Papier" && botChoice == "Kamien" || choice == "Nozyce" && botChoice == "Papier")
            {
                result = "Wygrałeś! Fart!";
            }

            else
            {
                result = "Przegrałeś lamusie";
            }
            var embed = new DiscordEmbedBuilder
            {
                Title = "Papier, Kamień, Nożyce!",
                Description = ($"Wybrałeś: {choice}\nBot wybrał: {botChoice}\n\n{result}"),
                Color = DiscordColor.Green
            };
            await ctx.RespondAsync(embed: embed);
        }

        [Command("powiedz")]
        public async Task Say(CommandContext ctx, string message)
        {
            var embed = new DiscordEmbedBuilder
            {
                Title = "Wiadomość",
                Description = message,
                Color = DiscordColor.Green
            };
            await ctx.RespondAsync(embed: embed);
        }

        [Command("losujgraczy")]
        public async Task RandomizePlayers(CommandContext ctx)
        {
            var voiceState = ctx.Member?.VoiceState;
            if (voiceState?.Channel == null || voiceState == null)
            {
                var embed2 = new DiscordEmbedBuilder
                {
                    Title = "Losowa kolejność graczy",
                    Description = "Musisz być na kanale głosowym, aby użyć tej komendy.",
                    Color = DiscordColor.Red
                };
                await ctx.RespondAsync(embed: embed2);
                return;
            }


            var members = voiceState.Channel.Users?.ToList();
            if (members == null || members.Count == 0)
            {
                var embed3 = new DiscordEmbedBuilder
                {
                    Title = "Losowa kolejność graczy",
                    Description = "Na kanale głosowym nie ma innych użytkowników.",
                    Color = DiscordColor.Red
                };
                await ctx.RespondAsync(embed: embed3);
                return;
            }


            var random = new Random();
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

            await ctx.RespondAsync(embed: embed);
        }
    }
}
