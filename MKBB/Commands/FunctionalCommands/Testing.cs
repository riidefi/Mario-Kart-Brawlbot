using DSharpPlus;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using IronPython.Runtime;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MKBB.Commands
{
    public class Testing : ApplicationCommandModule
    {
        [SlashCommand("test", "this is a test")]
        public async Task Test(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder() { IsEphemeral = true });

            var embeds = new List<DiscordEmbedBuilder>() {

            new DiscordEmbedBuilder
            {
                Color = new DiscordColor("#FF0000"),
                Title = $"__**Test:**__",
                Description = "This is a test page 1.",
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = $"Server Time: {DateTime.Now}"
                }
            },

            new DiscordEmbedBuilder
            {
                Color = new DiscordColor("#FF0000"),
                Title = $"__**Test:**__",
                Description = "This is a test page 2.",
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = $"Server Time: {DateTime.Now}"
                }
            },

            new DiscordEmbedBuilder
            {
                Color = new DiscordColor("#FF0000"),
                Title = $"__**Test:**__",
                Description = "This is a test page 3.",
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = $"Server Time: {DateTime.Now}"
                }
            },
            };

            var message = await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embeds[0]).AddComponents(Util.GeneratePageArrows(ctx)));

            PendingPaginator pending = new PendingPaginator() { CurrentPage = 0, MessageId = message.Id, Context = ctx, Pages = embeds };

            Util.PendingInteractions.Add(pending);
        }
    }
}
