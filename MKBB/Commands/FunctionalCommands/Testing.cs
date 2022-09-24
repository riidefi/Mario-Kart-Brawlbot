using DSharpPlus;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using IronPython.Runtime;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace MKBB.Commands
{
    public class Testing : ApplicationCommandModule
    {
        [SlashCommand("test", "this is a test")]
        public async Task Test(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder() { IsEphemeral = ctx.Channel.Id == 908709951411716166 ? false : true });

            //string playerList = JsonConvert.SerializeObject(players);
            //File.WriteAllText("council.json", playerList);



            var embed = new DiscordEmbedBuilder
            {
                Color = new DiscordColor("#FF0000"),
                Title = $"__**Notice:**__",
                Description = $"Created json.",
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = $"Server Time: {DateTime.Now}"
                }
            };

            var message = await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed).AddComponents(Util.GeneratePageArrows(ctx)));
        }
    }
}
