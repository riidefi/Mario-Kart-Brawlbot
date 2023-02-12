using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using FluentScheduler;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static IronPython.Modules._ast;

namespace MKBB.Commands
{
    public class Config : ApplicationCommandModule
    {

        [SlashCommand("botchannel", "Configures the channel(s) in which commands will no longer be ephemeral (requires admin).")]
        [SlashRequireUserPermissions(Permissions.ManageGuild)]
        public async Task ConfigureBotChannel(InteractionContext ctx,
            [Choice("True", 1)]
            [Choice("False", 0)]
            [Option("no-channels", "If you would like no channels configured, set this to true.")] bool noChannels = false)
        {
            try
            {
                await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder() { IsEphemeral = Util.CheckEphemeral(ctx) });

                if (noChannels)
                {
                    List<Server> servers = JsonConvert.DeserializeObject<List<Server>>(File.ReadAllText("servers.json"));
                    foreach (var server in servers)
                    {
                        if (ctx.Guild.Id == server.Id)
                        {
                            server.BotChannelIds = new List<ulong>();
                            break;
                        }
                    }
                    File.WriteAllText("servers.json", JsonConvert.SerializeObject(servers));

                    var embed = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = $"__**Success:**__",
                        Description = $"*The server's bot channels have been set to none.*",
                        Footer = new DiscordEmbedBuilder.EmbedFooter
                        {
                            Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                        }
                    };

                    var message = await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
                }
                else
                {
                    var embed = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = $"__**Choose your channels:**__",
                        Description = $"*Please select one or more channels from the drop down menu below.*",
                        Footer = new DiscordEmbedBuilder.EmbedFooter
                        {
                            Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                        }
                    };

                    var message = await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed).AddComponents(Util.GenerateChannelConfigSelectMenu()));

                    PendingChannelConfigInteraction pending = new PendingChannelConfigInteraction() { Context = ctx, MessageId = message.Id };
                    Util.PendingChannelConfigInteractions.Add(pending);
                }
            }
            catch (Exception ex)
            {
                await Util.ThrowError(ctx, ex);
            }
        }
    }
}
