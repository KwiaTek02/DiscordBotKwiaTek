using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Newtonsoft.Json;

namespace KasynoBot.Commands.Prefix
{
    public sealed class Economy : BaseCommandModule
    {
        // Zmienione na Dictionary<ulong, Dictionary<ulong, int>> gdzie pierwszy klucz to ID serwera, drugi to ID użytkownika
        public static Dictionary<ulong, Dictionary<ulong, int>> serverUserBalances = new Dictionary<ulong, Dictionary<ulong, int>>();
        private static Dictionary<ulong, DateTime> lastWorkTimes = new Dictionary<ulong, DateTime>();
        private static readonly string filePath = $"{AppDomain.CurrentDomain.BaseDirectory}/balances.json";
        private static readonly string lastWorkTimesFilePath = $"{AppDomain.CurrentDomain.BaseDirectory}/lastWorkTimes.json";
        private static readonly string rouletteImagePath = $"{AppDomain.CurrentDomain.BaseDirectory}/Images/ruletka.png";

        public Economy()
        {
            LoadBalances();
            LoadLastWorkTimes();
        }

        [Command("balance")]
        public async Task Balance(CommandContext ctx)
        {
            ulong serverId = ctx.Guild.Id;
            ulong userId = ctx.User.Id;

            int balance = serverUserBalances.ContainsKey(serverId) && serverUserBalances[serverId].ContainsKey(userId)
                ? serverUserBalances[serverId][userId]
                : 0;

            var embed = new DiscordEmbedBuilder
            {
                Title = $"Stan konta gracza {ctx.User.Username}",
                Description = $"Masz {balance} żetonów.",
                Color = DiscordColor.Green
            };

            await ctx.RespondAsync(embed: embed);
        }

        [Command("work")]
        public async Task Work(CommandContext ctx)
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
                    var embed2 = new DiscordEmbedBuilder
                    {
                        Title = "Praca",
                        Description = $"Jesteś wyczerpany po pracy. Musisz poczekać {timeLeft.Minutes} minut i {timeLeft.Seconds} sekund przed ponownym pójściem do niej!",
                        Color = DiscordColor.Red
                    };
                    await ctx.RespondAsync(embed: embed2);
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

            lastWorkTimes[userId] = DateTime.UtcNow;
            SaveBalances();
            SaveLastWorkTimes();

            var embed = new DiscordEmbedBuilder
            {
                Title = "Praca",
                Description = $"Pracowałeś ciężko za najniższą krajową i zdobyłeś {earnedAmount} żetonów!",
                Color = DiscordColor.Green
            };

            await ctx.RespondAsync(embed: embed);
        }

        [Command("ruletka")]
        public async Task Roulette(CommandContext ctx, int bet, string option)
        {
            ulong serverId = ctx.Guild.Id;
            ulong userId = ctx.User.Id;

            if (!serverUserBalances.ContainsKey(serverId) || !serverUserBalances[serverId].ContainsKey(userId) || serverUserBalances[serverId][userId] < bet)
            {
                await ctx.RespondAsync("Nie masz wystarczającej liczby żetonów, aby postawić ten zakład.");
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
                    if (isEven && result != 0) payout = bet * 2;
                    break;
                case "nieparzyste":
                    if (!isEven && result != 0) payout = bet * 2;
                    break;
                case "czerwony":
                    if (isRed) payout = bet * 2;
                    break;
                case "czarny":
                    if (!isRed && result != 0) payout = bet * 2;
                    break;
                case "1-18":
                    if (result >= 1 && result <= 18) payout = bet * 2;
                    break;
                case "19-36":
                    if (result >= 19 && result <= 36) payout = bet * 2;
                    break;
                case "1st":
                    if (result % 3 == 1 && result != 0) payout = bet * 3;
                    break;
                case "2nd":
                    if (result % 3 == 2) payout = bet * 3;
                    break;
                case "3rd":
                    if (result % 3 == 0 && result != 0) payout = bet * 3;
                    break;
                case "1-12":
                    if (result >= 1 && result <= 12) payout = bet * 3;
                    break;
                case "13-24":
                    if (result >= 13 && result <= 24) payout = bet * 3;
                    break;
                case "25-36":
                    if (result >= 25 && result <= 36) payout = bet * 3;
                    break;
                default:
                    if (int.TryParse(option, out int chosenNumber) && chosenNumber == result)
                    {
                        payout = bet * 36;
                    }
                    break;
            }

            serverUserBalances[serverId][userId] -= bet;
            if (payout > 0)
            {
                serverUserBalances[serverId][userId] += payout;
                var embed = new DiscordEmbedBuilder
                {
                    Title = "Ruletka",
                    Description = $"Piłka wylądowana na {result}. Wygrałeś {payout} żetonów! Farciarz jebany",
                    Color = DiscordColor.Green
                };

                await ctx.RespondAsync(embed: embed);
            }
            else
            {
                var embed = new DiscordEmbedBuilder
                {
                    Title = "Ruletka",
                    Description = $"Piłka wylądowana na {result}. Przegrałeś {bet} żetonów. Haa! Frajer! ",
                    Color = DiscordColor.Red
                };
                await ctx.RespondAsync(embed: embed);
            }

            SaveBalances();
        }

        [Command("ruletka-info")]
        public async Task RouletteInfo(CommandContext ctx)
        {
            var embed = new DiscordEmbedBuilder
            {
                Title = "Ruletka Informacje",
                Description = "W ruletce możesz obstawiać wiele zakładów.\n Zastosowanie: !ruletka <zakład> <opcje>\n\nMnożniki wypłat:\r\n[x36] Pojedynczy numer\r\n[x 3] Tuziny (1-12, 13-24, 25-36)\r\n[x 3] Kolumny (1st., 2nd., 3rd. )\r\n[x 2] Połówki (1-18, 19-36)\r\n[x 2] nieparzyste/parzyste\r\n[x 2] Kolory (czerwony, czarny)\n\nPrzykłady:\r\n !ruletka 200 nieparzyste\r\n !ruletka 600 2\n !ruletka 40 13-24",
                Color = DiscordColor.Green
            };

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
