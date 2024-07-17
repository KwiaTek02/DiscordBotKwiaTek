using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Microsoft.VisualBasic.FileIO;
using Newtonsoft.Json;

namespace KasynoBot.Commands.Slash
{
    
    public sealed class EconomySlash : ApplicationCommandModule
    {
        public static Dictionary<ulong, Dictionary<ulong, int>> serverUserBalances = new Dictionary<ulong, Dictionary<ulong, int>>();
        private static Dictionary<ulong, DateTime> lastWorkTimes = new Dictionary<ulong, DateTime>();
        private static readonly string filePath = $"{AppDomain.CurrentDomain.BaseDirectory}/balances.json";
        private static readonly string lastWorkTimesFilePath = $"{AppDomain.CurrentDomain.BaseDirectory}/lastWorkTimes.json";
        private static readonly string rouletteImagePath = $"{AppDomain.CurrentDomain.BaseDirectory}/Images/ruletka.png";

        public EconomySlash()
        {
            LoadBalances();
            LoadLastWorkTimes();
        }

        [SlashCommand("stankonta", "Pokazuje stan konta użytkownika.")]
        public async Task Balance(InteractionContext ctx)
        {
            var kasa = DiscordEmoji.FromName(ctx.Client, ":coin:");
            ulong serverId = ctx.Guild.Id;
            ulong userId = ctx.User.Id;

            int balance = serverUserBalances.ContainsKey(serverId) && serverUserBalances[serverId].ContainsKey(userId)
                ? serverUserBalances[serverId][userId]
                : 0;

            var embed = new DiscordEmbedBuilder()
            
                .WithTitle($"Stan konta gracza")
                .WithDescription( $"{ctx.User.Mention}\nMasz {balance} {kasa}.")
                .WithColor(DiscordColor.Gold);

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));
        }

        [SlashCommand("pracuj", "Umożliwia użytkownikowi zarobienie żetonów.")]
        public async Task Work(InteractionContext ctx)
        {
            ulong serverId = ctx.Guild.Id;
            ulong userId = ctx.User.Id;

            if (lastWorkTimes.ContainsKey(userId))
            {
                var lastWorkTime = lastWorkTimes[userId];
                var timeSinceLastWork = DateTime.UtcNow - lastWorkTime;

                if (timeSinceLastWork.TotalMinutes < 10)
                {
                    var timeLeft = TimeSpan.FromMinutes(10) - timeSinceLastWork;
                    var embed2 = new DiscordEmbedBuilder()
                        .WithTitle("Praca")
                        .WithDescription($"Jesteś wyczerpany po pracy. Musisz poczekać {timeLeft.Minutes} minut i {timeLeft.Seconds} sekund przed ponownym pójściem do niej!")
                        .WithColor(DiscordColor.Red);
                    
                    await ctx.CreateResponseAsync(embed: embed2);
                    return;
                }
            }

            Random random = new Random();
            int earnedAmount = random.Next(50, 101);

            if (!serverUserBalances.ContainsKey(serverId))
            {
                serverUserBalances[serverId] = new Dictionary<ulong, int>();
            }

            if (serverUserBalances[serverId].ContainsKey(userId))
            {
                serverUserBalances[serverId][userId] += earnedAmount;
            }
            else
            {
                serverUserBalances[serverId][userId] = earnedAmount;
            }

            var kasa = DiscordEmoji.FromName(ctx.Client, ":coin:");
            lastWorkTimes[userId] = DateTime.UtcNow;
            SaveBalances();
            SaveLastWorkTimes();

            var embed = new DiscordEmbedBuilder()
                .WithTitle("Praca")
                .WithDescription ($"Pracowałeś ciężko za najniższą krajową i zdobyłeś {earnedAmount} {kasa}!")
                .WithColor(DiscordColor.Green);
            

            await ctx.CreateResponseAsync(embed: embed);
        }

        [SlashCommand("ruletka", "Pozwala użytkownikowi grać w ruletkę.")]
        public async Task Roulette(InteractionContext ctx, [Option("bet", "Kwota zakładu")] long bet, [Option("option", "Opcja zakładu")] string option)
        {
            var kasa = DiscordEmoji.FromName(ctx.Client, ":coin:");
            ulong serverId = ctx.Guild.Id;
            ulong userId = ctx.User.Id;

            if (!serverUserBalances.ContainsKey(serverId) || !serverUserBalances[serverId].ContainsKey(userId) || serverUserBalances[serverId][userId] < bet)
            {
                var embed = new DiscordEmbedBuilder()
                    .WithTitle("Ruletka")
                    .WithDescription("Nie masz wystarczającej liczby żetonów, aby postawić ten zakład.")
                    .WithColor(DiscordColor.Red);
                await ctx.CreateResponseAsync(embed: embed);
                return;
            }

            Random random = new Random();
            int result = random.Next(0, 37);
            bool isRed = new[] { 1, 3, 5, 7, 9, 12, 14, 16, 18, 19, 21, 23, 25, 27, 30, 32, 34, 36 }.Contains(result);
            bool isEven = result % 2 == 0;
            int payout = 0;

            switch (option.ToLower())
            {
                case "parzyste":
                    if (isEven && result != 0) payout = (int)(bet * 2);
                    break;
                case "nieparzyste":
                    if (!isEven && result != 0) payout = (int)(bet * 2);
                    break;
                case "czerwony":
                    if (isRed) payout = (int)(bet * 2);
                    break;
                case "czerwone":
                    if (isRed) payout = (int)(bet * 2);
                    break;
                case "czarny":
                    if (!isRed && result != 0) payout = (int)(bet * 2);
                    break;
                case "czarne":
                    if (!isRed && result != 0) payout = (int)(bet * 2);
                    break;
                case "1-18":
                    if (result >= 1 && result <= 18) payout = (int)(bet * 2);
                    break;
                case "19-36":
                    if (result >= 19 && result <= 36) payout = (int)(bet * 2);
                    break;
                case "1st":
                    if (result % 3 == 1 && result != 0) payout = (int)(bet * 3);
                    break;
                case "2nd":
                    if (result % 3 == 2) payout = (int)(bet * 3);
                    break;
                case "3rd":
                    if (result % 3 == 0 && result != 0) payout = (int)(bet * 3);
                    break;
                case "1-12":
                    if (result >= 1 && result <= 12) payout = (int)(bet * 3);
                    break;
                case "13-24":
                    if (result >= 13 && result <= 24) payout = (int)(bet * 3);
                    break;
                case "25-36":
                    if (result >= 25 && result <= 36) payout = (int)(bet * 3);
                    break;
                default:
                    if (int.TryParse(option, out int chosenNumber) && chosenNumber == result)
                    {
                        payout = (int)(bet * 36);
                    }
                    break;
            }

            serverUserBalances[serverId][userId] -= (int)bet;
            if (payout > 0)
            {
                serverUserBalances[serverId][userId] += payout;
                var embed = new DiscordEmbedBuilder()

                    .WithTitle("Ruletka")
                    .WithDescription($"Piłka wylądowana na {result}. Wygrałeś {payout} {kasa}! Farciarz jebany")
                    .WithColor(DiscordColor.Green);
                

                await ctx.CreateResponseAsync(embed: embed);
            }
            else
            {
                var embed = new DiscordEmbedBuilder()

                    .WithTitle("Ruletka")
                    .WithDescription($"Piłka wylądowana na {result}. Przegrałeś {bet} {kasa}. Haa! Frajer!")
                    .WithColor(DiscordColor.Red);
                
                await ctx.CreateResponseAsync(embed: embed);
            }

            SaveBalances();
        }

        [SlashCommand("jackpot", "We wtorki tylko automaty 🎰")]
        public async Task Spin(InteractionContext ctx, [Option("bet", "Kwota zakładu")] long bet)
        {
            ulong serverId = ctx.Guild.Id;
            ulong userId = ctx.User.Id;
            var emoji1 = DiscordEmoji.FromName(ctx.Client, ":hearts:");
            var emoji2 = DiscordEmoji.FromName(ctx.Client, ":gem:");
            var emoji3 = DiscordEmoji.FromName(ctx.Client, ":large_orange_diamond:");
            var emoji4 = DiscordEmoji.FromName(ctx.Client, ":cross:");
            var emoji5 = DiscordEmoji.FromName(ctx.Client, ":shamrock:");
            var emoji777 = DiscordEmoji.FromName(ctx.Client, ":rose:");
            var slot = DiscordEmoji.FromName(ctx.Client, ":slot_machine:");
            var wykrzyknik = DiscordEmoji.FromName(ctx.Client, ":exclamation:");
            var kasa = DiscordEmoji.FromName(ctx.Client, ":coin:");

            if (!serverUserBalances.ContainsKey(serverId) || !serverUserBalances[serverId].ContainsKey(userId) ||
                serverUserBalances[serverId][userId] < bet)
            {
                var embed = new DiscordEmbedBuilder()
                    .WithTitle("Jackpot")
                    .WithDescription("Nie masz wystarczającej liczby żetonów, aby zakręcić spinami .")
                    .WithColor(DiscordColor.Red);
                await ctx.CreateResponseAsync(embed: embed);
                return;
            }

            Dictionary<DiscordEmoji, int> emojiWeights = new Dictionary<DiscordEmoji, int>
            {
                { emoji1, 25 }, 
                { emoji2, 20 }, 
                { emoji3, 15 }, 
                { emoji4, 10 }, 
                { emoji5, 25 }, 
                { emoji777, 5 } 
            };

            Random random = new Random();
            DiscordEmoji[] emojis = { emoji1, emoji2, emoji3, emoji4, emoji5, emoji777 };

            DiscordEmoji GetRandomEmoji()
            {
                int totalWeight = emojiWeights.Values.Sum();
                int randomValue = random.Next(totalWeight);

                foreach (var emoji in emojiWeights)
                {
                    if (randomValue < emoji.Value)
                        return emoji.Key;

                    randomValue -= emoji.Value;
                }
                return null; 
            }

            DiscordEmoji result1 = GetRandomEmoji();
            DiscordEmoji result2 = GetRandomEmoji();
            DiscordEmoji result3 = GetRandomEmoji();

            int payout = 0;
            serverUserBalances[serverId][userId] -= (int)bet;

            if (result1 == emoji1 && result2 == emoji1 && result3 == emoji1) // 1 przypadek
            {
                payout = (int)(bet * 2);
                serverUserBalances[serverId][userId] += payout;
                var embed = new DiscordEmbedBuilder()

                    .WithTitle($"Jackpot {slot}")
                    .WithDescription($"Wylosowało:\n{result1} {result2} {result3}\nWygrałeś {payout} {kasa}! Farciarz jebany")
                    .WithColor(DiscordColor.Green);


                await ctx.CreateResponseAsync(embed: embed);
            }
            else if (result1 == emoji1 && result2 == emoji1 && result3 != emoji1) 
            {
                payout = (int)(bet * 1);
                serverUserBalances[serverId][userId] += payout;
                var embed = new DiscordEmbedBuilder()

                    .WithTitle($"Jackpot {slot}")
                    .WithDescription($"Wylosowało:\n{result1} {result2} {result3}\nWygrałeś {payout} {kasa}! Farciarz jebany")
                    .WithColor(DiscordColor.Green);


                await ctx.CreateResponseAsync(embed: embed);
            }
            else if (result1 != emoji1 && result2 == emoji1 && result3 == emoji1)
            {
                payout = (int)(bet * 1);
                serverUserBalances[serverId][userId] += payout;
                var embed = new DiscordEmbedBuilder()

                    .WithTitle($"Jackpot {slot}")
                    .WithDescription($"Wylosowało:\n{result1} {result2} {result3}\nWygrałeś {payout} {kasa}! Farciarz jebany")
                    .WithColor(DiscordColor.Green);


                await ctx.CreateResponseAsync(embed: embed);
            }

            else if (result1 == emoji2 && result2 == emoji2 && result3 == emoji2) // 2 przypadek
            {
                payout = (int)(bet * 3);
                serverUserBalances[serverId][userId] += payout;
                var embed = new DiscordEmbedBuilder()

                    .WithTitle($"Jackpot {slot}")
                    .WithDescription($"Wylosowało:\n{result1} {result2} {result3}\nWygrałeś {payout} {kasa}! Może już starczy?")
                    .WithColor(DiscordColor.Green);


                await ctx.CreateResponseAsync(embed: embed);
            }
            else if (result1 == emoji2 && result2 == emoji2 && result3 != emoji2) 
            {
                payout = (int)(bet * 2);
                serverUserBalances[serverId][userId] += payout;
                var embed = new DiscordEmbedBuilder()

                    .WithTitle($"Jackpot {slot}")
                    .WithDescription($"Wylosowało:\n{result1} {result2} {result3}\nWygrałeś {payout} {kasa}! Było blisko żeby 3 te same wylosowało!")
                    .WithColor(DiscordColor.Green);


                await ctx.CreateResponseAsync(embed: embed);
            }

            else if (result1 != emoji2 && result2 == emoji2 && result3 == emoji2) 
            {
                payout = (int)(bet * 2);
                serverUserBalances[serverId][userId] += payout;
                var embed = new DiscordEmbedBuilder()

                    .WithTitle($"Jackpot {slot}")
                    .WithDescription($"Wylosowało:\n{result1} {result2} {result3}\nWygrałeś {payout} {kasa}! Było blisko żeby 3 te same wylosowało!")
                    .WithColor(DiscordColor.Green);


                await ctx.CreateResponseAsync(embed: embed);
            }

            else if (result1 == emoji3 && result2 == emoji3 && result3 == emoji3) // 3 przypadek zostaje 
            {
                payout = (int)(bet * 3);
                serverUserBalances[serverId][userId] += payout;
                var embed = new DiscordEmbedBuilder()

                    .WithTitle($"Jackpot {slot}")
                    .WithDescription($"Wylosowało:\n{result1} {result2} {result3}\nWygrałeś {payout} {kasa}! Opłacało się zaryzykować!")
                    .WithColor(DiscordColor.Green);


                await ctx.CreateResponseAsync(embed: embed);

            }
            else if (result1 == emoji3 && result2 == emoji3 && result3 != emoji3) 
            {
                payout = (int)(bet * 2);
                serverUserBalances[serverId][userId] += payout;
                var embed = new DiscordEmbedBuilder()

                    .WithTitle($"Jackpot {slot}")
                    .WithDescription($"Wylosowało:\n{result1} {result2} {result3}\nWygrałeś {payout} {kasa}! Dobrze, że nie trafiłeś 3 tych samych!")
                    .WithColor(DiscordColor.Green);


                await ctx.CreateResponseAsync(embed: embed);
            }

            else if (result1 != emoji3 && result2 == emoji3 && result3 == emoji3) 
            {
                payout = (int)(bet * 2);
                serverUserBalances[serverId][userId] += payout;
                var embed = new DiscordEmbedBuilder()

                    .WithTitle($"Jackpot {slot}")
                    .WithDescription($"Wylosowało:\n{result1} {result2} {result3}\nWygrałeś {payout} {kasa}! Dobrze, że nie trafiłeś 3 tych samych!")
                    .WithColor(DiscordColor.Green);


                await ctx.CreateResponseAsync(embed: embed);
            }

            else if(result1 == emoji4 && result2 == emoji4 && result3 == emoji4) // 4 przypadek zostaje 
            {
                payout = (int)(bet * 2);
                serverUserBalances[serverId][userId] += payout;
                var embed = new DiscordEmbedBuilder()

                    .WithTitle($"Jackpot {slot}")
                    .WithDescription($"Wylosowało:\n{result1} {result2} {result3}\nWygrałeś {payout} {kasa}! Ciesze się że tylko tyle wygrałeś!")
                    .WithColor(DiscordColor.Green);


                await ctx.CreateResponseAsync(embed: embed);
            }

            else if (result1 == emoji4 && result2 == emoji4 && result3 != emoji4)
            {
                payout = (int)(bet * 1);
                serverUserBalances[serverId][userId] += payout;
                var embed = new DiscordEmbedBuilder()

                    .WithTitle($"Jackpot {slot}")
                    .WithDescription($"Wylosowało:\n{result1} {result2} {result3}\nWygrałeś {payout} {kasa}! Więcej nie wygrasz!")
                    .WithColor(DiscordColor.Green);


                await ctx.CreateResponseAsync(embed: embed);

            }

            else if (result1 != emoji4 && result2 == emoji4 && result3 == emoji4)
            {
                payout = (int)(bet * 1);
                serverUserBalances[serverId][userId] += payout;
                var embed = new DiscordEmbedBuilder()

                    .WithTitle($"Jackpot {slot}")
                    .WithDescription($"Wylosowało:\n{result1} {result2} {result3}\nWygrałeś {payout} {kasa}! Więcej nie wygrasz!")
                    .WithColor(DiscordColor.Green);


                await ctx.CreateResponseAsync(embed: embed);
            }

            else if(result1 == emoji5 && result2 == emoji5 && result3 == emoji5) // 5 przypadek
            {
                payout = (int)(bet * 5);
                serverUserBalances[serverId][userId] += payout;
                var embed = new DiscordEmbedBuilder()

                    .WithTitle($"Jackpot {slot}")
                    .WithDescription($"Wylosowało:\n{result1} {result2} {result3}\nOkradłeś bank!!! Wygrałeś {payout} {kasa}!")
                    .WithColor(DiscordColor.Green);


                await ctx.CreateResponseAsync(embed: embed);
            }
            else if (result1 == emoji5 && result2 == emoji5 && result3 != emoji5) 
            {
                payout = (int)(bet * 2);
                serverUserBalances[serverId][userId] += payout;
                var embed = new DiscordEmbedBuilder()

                    .WithTitle($"Jackpot {slot}")
                    .WithDescription($"Wylosowało:\n{result1} {result2} {result3}\nWygrałeś {payout} {kasa}! Chociaż banku nie okradłeś!")
                    .WithColor(DiscordColor.Green);


                await ctx.CreateResponseAsync(embed: embed);
            }
            else if (result1 != emoji5 && result2 == emoji5 && result3 == emoji5) 
            {
                payout = (int)(bet * 2);
                serverUserBalances[serverId][userId] += payout;
                var embed = new DiscordEmbedBuilder()

                    .WithTitle($"Jackpot {slot}")
                    .WithDescription($"Wylosowało:\n{result1} {result2} {result3}\nWygrałeś {payout} {kasa}! Odpuść już!")
                    .WithColor(DiscordColor.Green);


                await ctx.CreateResponseAsync(embed: embed);
            }
            else if(result1 == emoji777 && result2 == emoji777 && result3 == emoji777) // 6 przypadek 777
            {
                payout = (int)(bet * 25);
                serverUserBalances[serverId][userId] += payout;
                var embed = new DiscordEmbedBuilder()

                    .WithTitle($"Jackpot {slot}")
                    .WithDescription($"\n      JACKPOT WIN{wykrzyknik}{wykrzyknik}{wykrzyknik}Wylosowało:\n{result1} {result2} {result3}\n. Wygrałeś {payout} {kasa}!")
                    .WithColor(DiscordColor.Green);


                await ctx.CreateResponseAsync(embed: embed);
            }
            else if (result1 == emoji777 && result2 == emoji777 && result3 != emoji777) 
            {
                payout = (int)(bet * 5);
                serverUserBalances[serverId][userId] += payout;
                var embed = new DiscordEmbedBuilder()

                    .WithTitle($"Jackpot {slot}")
                    .WithDescription($"Wylosowało:\n{result1} {result2} {result3}\nWygrałeś {payout} {kasa}! Uffff! Było blisko Jackpotu!")
                    .WithColor(DiscordColor.Green);


                await ctx.CreateResponseAsync(embed: embed);
            }
            else if (result1 != emoji777 && result2 == emoji777 && result3 == emoji777) 
            {
                payout = (int)(bet * 5);
                serverUserBalances[serverId][userId] += payout;
                var embed = new DiscordEmbedBuilder()

                    .WithTitle($"Jackpot {slot}")
                    .WithDescription($"Wylosowało:\n{result1} {result2} {result3}\nWygrałeś {payout} {kasa}! Uffff! Było blisko Jackpotu!")
                    .WithColor(DiscordColor.Green);


                await ctx.CreateResponseAsync(embed: embed);
            }
            else
            {
                payout = 0;
                var embed = new DiscordEmbedBuilder()

                    .WithTitle($"Jackpot {slot}")
                    .WithDescription($"Wylosowało:\n{result1} {result2} {result3}\n\nPrzegrałeś {bet} {kasa}. Haa! Frajer!")
                    .WithColor(DiscordColor.Red);

                await ctx.CreateResponseAsync(embed: embed);
            }


            SaveBalances();

        }

        [SlashCommand("ruletka-info", "Wyświetla informacje o grze w ruletkę.")]
        public async Task RouletteInfo(InteractionContext ctx)
        {
            var embed = new DiscordEmbedBuilder()
            
                .WithTitle("Ruletka Informacje")
                .WithDescription("W ruletce możesz obstawiać wiele zakładów.\nZastosowanie: /ruletka <bet> <option>\n\nMnożniki wypłat:\r\n[x36] Pojedynczy numer\r\n[x3] Tuziny (1-12, 13-24, 25-36)\r\n[x3] Kolumny (1st, 2nd, 3rd)\r\n[x2] Połówki (1-18, 19-36)\r\n[x2] Nieparzyste/parzyste\r\n[x2] Kolory (czerwony, czarny)\n\nPrzykłady:\r\n/ruletka 200 nieparzyste\r\n/ruletka 600 2\r\n/ruletka 40 13-24")
                .WithColor(DiscordColor.Gold);

            embed.WithImageUrl($"attachment://ruletka.png");

            using (var stream = new FileStream(rouletteImagePath, FileMode.Open, FileAccess.Read))
            {
                var messageBuilder = new DiscordMessageBuilder()
                    .WithContent("")
                    .AddFile("ruletka.png", stream)
                    .WithEmbed(embed);

                await ctx.Channel.SendMessageAsync(messageBuilder);
            }
        }

        [SlashCommand("top", "Wyświetla top 5 najbogatszych osób na serwerze.")]
        public async Task Top(InteractionContext ctx)
        {
            ulong serverId = ctx.Guild.Id;
            ulong userId = ctx.User.Id;

            if (!serverUserBalances.ContainsKey(serverId) || serverUserBalances[serverId].Count == 0)
            {
                var embed = new DiscordEmbedBuilder()
                    .WithTitle("Top 5 najbogatszych osób na serwerze")
                    .WithDescription("Brak danych o użytkownikach.")
                    .WithColor(DiscordColor.Red);

                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));
                return;
            }

            // Pobierz top 3 najbogatszych osób na serwerze
            var topUsers = serverUserBalances[serverId]
                .OrderByDescending(kv => kv.Value)
                .Take(5)
                .ToList();

            var descriptionBuilder = new StringBuilder();
            for (int i = 0; i < topUsers.Count; i++)
            {
                var user = await ctx.Guild.GetMemberAsync(topUsers[i].Key);
                if (user != null)
                {
                    descriptionBuilder.AppendLine($"{i + 1}. {user.Mention} - {topUsers[i].Value} żetonów.");
                }
            }

            // Znajdź pozycję bieżącego użytkownika
            var allUsersSorted = serverUserBalances[serverId]
                .OrderByDescending(kv => kv.Value)
                .ToList();

            int userRank = allUsersSorted.FindIndex(kv => kv.Key == userId) + 1;

            if (userRank <= 5)
            {
                descriptionBuilder.AppendLine($"\nZajmujesz {userRank} miejsce w rankingu!");
            }
            else
            {
                descriptionBuilder.AppendLine($"\nZajmujesz {userRank} miejsce w rankingu, poza top 3.");
            }

            var embed2 = new DiscordEmbedBuilder()
                .WithTitle("Top 5 najbogatszych osób na serwerze")
                .WithDescription(descriptionBuilder.ToString())
                .WithColor(DiscordColor.Gold);

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed2));
        }

        private void LoadBalances()
        {
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                serverUserBalances = JsonConvert.DeserializeObject<Dictionary<ulong, Dictionary<ulong, int>>>(json) ?? new Dictionary<ulong, Dictionary<ulong, int>>();
            }
            else
            {
                serverUserBalances = new Dictionary<ulong, Dictionary<ulong, int>>();
            }
        }

        private void SaveBalances()
        {
            string json = JsonConvert.SerializeObject(serverUserBalances, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }

        private void LoadLastWorkTimes()
        {
            if (File.Exists(lastWorkTimesFilePath))
            {
                string json = File.ReadAllText(lastWorkTimesFilePath);
                lastWorkTimes = JsonConvert.DeserializeObject<Dictionary<ulong, DateTime>>(json) ?? new Dictionary<ulong, DateTime>();
            }
            else
            {
                lastWorkTimes = new Dictionary<ulong, DateTime>();
            }
        }

        private void SaveLastWorkTimes()
        {
            string json = JsonConvert.SerializeObject(lastWorkTimes, Formatting.Indented);
            File.WriteAllText(lastWorkTimesFilePath, json);
        }
    }
}
