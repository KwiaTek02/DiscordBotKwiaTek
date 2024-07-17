using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.Lavalink;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KasynoBot.Commands.Slash
{
    public class MusicCommands : ApplicationCommandModule
    {
        private static List<LavalinkTrack> _queue = new List<LavalinkTrack>();

        [SlashCommand("play", "Odtwarza muzykę z YouTube.")]
        public async Task Play(InteractionContext ctx, [Option("url", "Link do utworu na YouTube.")] string url)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Odtwarzanie..."));

            if (!Uri.TryCreate(url, UriKind.Absolute, out var uriResult) || (uriResult.Scheme != Uri.UriSchemeHttp && uriResult.Scheme != Uri.UriSchemeHttps))
            {
                var emoji2 = DiscordEmoji.FromName(ctx.Client, ":warning:");
                var embed2 = new DiscordEmbedBuilder
                {
                    Title = $"{emoji2} Błąd",
                    Description = $"Podany link nie jest prawidłowym URL.",
                    Color = DiscordColor.Red
                };

                await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed2));
                return;
            }

            var lava = ctx.Client.GetLavalink();
            if (!lava.ConnectedNodes.Any())
            {
                var emoji2 = DiscordEmoji.FromName(ctx.Client, ":notes:");
                var embed3 = new DiscordEmbedBuilder
                {
                    Title = $"{emoji2} Odtwarzanie",
                    Description = $"Lavalink nie jest podłączony prawidłowo.",
                    Color = DiscordColor.Red
                };

                await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed3));
                return;
            }

            var node = lava.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Guild);

            if (conn == null)
            {
                var channel = ctx.Member?.VoiceState?.Channel;
                if (channel == null)
                {
                    var emoji2 = DiscordEmoji.FromName(ctx.Client, ":notes:");
                    var embed3 = new DiscordEmbedBuilder
                    {
                        Title = $"{emoji2} Odtwarzanie",
                        Description = $"Musisz być na kanale głosowym!",
                        Color = DiscordColor.Red
                    };

                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed3));
                    return;
                }
                conn = await node.ConnectAsync(channel);
            }

            var loadResult = await node.Rest.GetTracksAsync(uriResult);
            if (loadResult.LoadResultType == LavalinkLoadResultType.LoadFailed || loadResult.LoadResultType == LavalinkLoadResultType.NoMatches)
            {
                var emoji2 = DiscordEmoji.FromName(ctx.Client, ":notes:");
                var embed3 = new DiscordEmbedBuilder
                {
                    Title = $"{emoji2} Odtwarzanie",
                    Description = $"Nie można załadować utworu.",
                    Color = DiscordColor.Red
                };

                await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed3));
                return;
            }

            foreach (var track in loadResult.Tracks)
            {
                _queue.Add(track);
            }

            if (conn.CurrentState.CurrentTrack == null)
            {
                await PlayNextTrack(ctx, conn);
            }
            else
            {
                var emoji = DiscordEmoji.FromName(ctx.Client, ":notes:");
                var embed = new DiscordEmbedBuilder
                {
                    Title = $"{emoji} Dodano do kolejki",
                    Description = $"{loadResult.Tracks.First().Title}",
                    Color = DiscordColor.Gold
                };

                await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
            }
        }

        private async Task PlayNextTrack(InteractionContext ctx, LavalinkGuildConnection conn)
        {
            if (_queue.Any())
            {
                var track = _queue.First();
                _queue.RemoveAt(0);
                await conn.PlayAsync(track);

                var emoji = DiscordEmoji.FromName(ctx.Client, ":notes:");
                var embed = new DiscordEmbedBuilder
                {
                    Title = $"{emoji} Odtwarzanie",
                    Description = $"{track.Title}",
                    Url = track.Uri.ToString(),
                    Color = DiscordColor.Gold
                };

                await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
            }
        }

        [SlashCommand("skip", "Pomija aktualnie odtwarzany utwór.")]
        public async Task Skip(InteractionContext ctx)
        {
            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Guild);

            if (conn == null || conn.CurrentState.CurrentTrack == null)
            {
                var emoji2 = DiscordEmoji.FromName(ctx.Client, ":notes:");
                var embed = new DiscordEmbedBuilder
                {
                    Title = $"{emoji2} Odtwarzanie",
                    Description = $"Bot nie odtwarza żadnego utworu!",
                    Color = DiscordColor.Red
                };

                await ctx.CreateResponseAsync(new DiscordInteractionResponseBuilder().AddEmbed(embed));
                return;
            }

            await conn.StopAsync();
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Pomijanie..."));
            await PlayNextTrack(ctx, conn);
        }

        [SlashCommand("pause", "Zatrzymuje aktualnie odtwarzany utwór.")]
        public async Task Pause(InteractionContext ctx)
        {
            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Guild);

            if (conn == null || conn.CurrentState.CurrentTrack == null)
            {
                var emoji2 = DiscordEmoji.FromName(ctx.Client, ":notes:");
                var embed = new DiscordEmbedBuilder
                {
                    Title = $"{emoji2} Odtwarzanie",
                    Description = $"Bot nie odtwarza żadnego utworu!",
                    Color = DiscordColor.Red
                };

                await ctx.CreateResponseAsync(new DiscordInteractionResponseBuilder().AddEmbed(embed));
                return;
            }

            await conn.PauseAsync();
            var emoji3 = DiscordEmoji.FromName(ctx.Client, ":notes:");
            var embed2 = new DiscordEmbedBuilder
            {
                Title = $"{emoji3} Odtwarzanie",
                Description = $"Odtwarzanie zatrzymane.",
                Color = DiscordColor.Gold
            };

            await ctx.CreateResponseAsync(new DiscordInteractionResponseBuilder().AddEmbed(embed2));
        }

        [SlashCommand("resume", "Wznawia zatrzymany utwór.")]
        public async Task Resume(InteractionContext ctx)
        {
            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Guild);

            if (conn == null || conn.CurrentState.CurrentTrack == null)
            {
                var emoji2 = DiscordEmoji.FromName(ctx.Client, ":notes:");
                var embed = new DiscordEmbedBuilder
                {
                    Title = $"{emoji2} Odtwarzanie",
                    Description = $"Bot nie odtwarza żadnego utworu!",
                    Color = DiscordColor.Red
                };

                await ctx.CreateResponseAsync(new DiscordInteractionResponseBuilder().AddEmbed(embed));
                return;
            }

            await conn.ResumeAsync();
            var emoji3 = DiscordEmoji.FromName(ctx.Client, ":notes:");
            var embed2 = new DiscordEmbedBuilder
            {
                Title = $"{emoji3} Odtwarzanie",
                Description = $"Odtwarzanie wznowione.",
                Color = DiscordColor.Gold
            };

            await ctx.CreateResponseAsync(new DiscordInteractionResponseBuilder().AddEmbed(embed2));
        }

        [SlashCommand("stop", "Zatrzymuje odtwarzanie i opuszcza kanał głosowy.")]
        public async Task Stop(InteractionContext ctx)
        {
            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Guild);

            if (conn == null || conn.CurrentState.CurrentTrack == null)
            {
                var emoji2 = DiscordEmoji.FromName(ctx.Client, ":notes:");
                var embed = new DiscordEmbedBuilder
                {
                    Title = $"{emoji2} Odtwarzanie",
                    Description = $"Bot nie odtwarza żadnego utworu!",
                    Color = DiscordColor.Red
                };

                await ctx.CreateResponseAsync(new DiscordInteractionResponseBuilder().AddEmbed(embed));
                return;
            }

            await conn.StopAsync();
            await conn.DisconnectAsync();
            var emoji3 = DiscordEmoji.FromName(ctx.Client, ":notes:");
            var embed2 = new DiscordEmbedBuilder
            {
                Title = $"{emoji3} Odtwarzanie",
                Description = $"Odtwarzanie zatrzymane. Bot opuścił kanał głosowy.",
                Color = DiscordColor.Gold
            };

            await ctx.CreateResponseAsync(new DiscordInteractionResponseBuilder().AddEmbed(embed2));
        }
    }
}
