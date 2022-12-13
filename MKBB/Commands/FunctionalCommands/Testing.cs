using DSharpPlus;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using IronPython.Runtime;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

namespace MKBB.Commands
{
    public class Testing : ApplicationCommandModule
    {
        [SlashCommand("test", "this is a test")]
        public async Task Test(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder() { IsEphemeral = !(ctx.Channel.Id == 908709951411716166) });

            var embed = new DiscordEmbedBuilder
            {
                Color = new DiscordColor("#FF0000"),
                Title = $"__**Notice:**__",
                Description = $"{Convert.ToInt32(DateTime.Parse(File.ReadAllText("lastUpdated.txt")).Subtract(DateTime.ParseExact("31/12/1899", "dd/MM/yyyy", CultureInfo.InvariantCulture)).TotalDays)}",
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = $"Server Time: {DateTime.Now}"
                }
            };

            var message = await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
        }
    }
}
