using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using System.IO;
using System.Threading.Tasks;

namespace MKBB.Commands
{
    public class TextCommands : ApplicationCommandModule
    {
        [SlashCommand("help", "Gives a list of commands available.")]
        public async Task Help(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder() { IsEphemeral = ctx.Channel.Id == 908709951411716166 ? false : true });
            string description = "__**Standard Commands:**__" +
                "\n/bkt track" +
                "\n/cttp" +
                "\n/help" +
                "\n/info track" +
                "\n/issues track" +
                "\n/nextupdate" +
                "\n/pop rts/cts/track stat-duration online/tts" +
                "\n/rating track" +
                "\n/source" +
                "\n/staff track" +
                "\n/subissues track" +
                "\n/summary track" +
                "\n/tools name";

            if (ctx.Guild.Id == 180306609233330176)
            {
                foreach (var role in ctx.Member.Roles)
                {
                    if (role.Name == "Track Council" || role.Name == "Admin")
                    {
                        description += "\n\n__**Council Commands:**__" +
                            "\n/assignedthreads" +
                            "\n/gethw track member" +
                            "\n/hw" +
                            "\n/strikes member" +
                            "\n/submithw yes/fixes/neutral/no track feedback";
                    }
                }

                foreach (var role in ctx.Member.Roles)
                {
                    if (role.Name == "Admin")
                    {
                        {
                            description += "\n\n__**Admin Commands:**__" +
                                "\n/addhw track author version download link slot-filename speed/lap modifiers notes" +
                                "\n/addstrike member" +
                                "\n/addtool name creators description download" +
                                "\n/assign member thread id" +
                                "\n/assignedthreads member" +
                                "\n/clearissues track" +
                                "\n/createtest" +
                                "\n/delhw track" +
                                "\n/deltool name" +
                                "\n/dmrole role message" +
                                "\n/edittool oldname name creators description download" +
                                "\n/lastupdated" +
                                "\n/randomassign (reset)" +
                                "\n/removeassignedthread thread id/all" +
                                "\n/removestrike member" +
                                "\n/replaceissues old track new track author version slot laps" +
                                "\n/reportissue major/minor track -Issue" +
                                "\n/resetstrikes member" +
                                "\n/starttimers" +
                                "\n/strikes member/all" +
                                "\n/unassign member thread id" +
                                "\n/update";
                        }
                    }
                }
            }

            var embed = new DiscordEmbedBuilder
            {
                Color = new DiscordColor("#FF0000"),
                Title = "__**Help**__",
                Description = description,
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                }
            };

            var trackTestersInvite = new DiscordLinkButtonComponent("https://discord.gg/sjPzuJ7PwD", "CTGP Track Testers Invite");

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed).AddComponents(trackTestersInvite));
        }

        [SlashCommand("cttp", "Gives links relating to the Custom Track Test Pack.")]
        public async Task CTTP(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder() { IsEphemeral = ctx.Channel.Id == 908709951411716166 ? false : true });
            var embed = new DiscordEmbedBuilder
            {
                Color = new DiscordColor("#FF0000"),
                Title = "__**Custom Track Test Pack Links**__",
                Description = "**Base Pack:** *https://drive.google.com/file/d/1tzhaBxycHEvY5G2k5jTzpONBhmq6g08g/view?usp=sharing*" +
                "\n**Installation Tutorial:** *https://youtu.be/Vh3GXTbxHLU*" +
                "\n**Test Pack Creation Tutorial:** *https://youtu.be/igtfmZAyG3g*",
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                }
            };

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
        }

        [SlashCommand("sheets", "Gives the links to useful spreadsheets.")]
        public async Task Sheets(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder() { IsEphemeral = ctx.Channel.Id == 908709951411716166 ? false : true });
            string description = "**Backroom Page:** *This page has all of the information on our finalized updates, and is viewable to the public. The first tab outlines what the next few updates will look like, and the second tab contains all the accepted tracks, with the fixes needed (if any).*" +
                "\n\n**Testing Notes Page:** *This page is what we fill when doing the online tests. It also contains the downloads for all the tracks in the test.*";
            var message = new DiscordWebhookBuilder();
            bool councilMember = false;
            foreach (var role in ctx.Member.Roles)
            {
                if (role.Id == 608386209655554058 || role.Id == 228909597090512896 && ctx.Channel.Id != 908709951411716166)
                {
                    councilMember = true;
                    break;
                }
            }

            if (councilMember)
            {
                description += $"\n\n**Council Page:** *This page has multiple important things for the Council to keep track of; this is where members review tracks that are submitted. This is also where we vote for members when they apply.*";
                message.AddComponents(Util.GetBackroomLinkButton(), Util.GetTestingNotesLinkButton(), Util.GetCouncilLinkButton());
            }
            else
            {
                message.AddComponents(Util.GetBackroomLinkButton(), Util.GetTestingNotesLinkButton());
            }

            var embed = new DiscordEmbedBuilder
            {
                Color = new DiscordColor("#FF0000"),
                Title = "__**Useful Google Sheets Pages**__",
                Description = description,
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                }
            };

            await ctx.EditResponseAsync(message.AddEmbed(embed));
        }

        [SlashCommand("source", "Gives a link to the source code of this bot.")]
        public async Task SourceCode(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder() { IsEphemeral = ctx.Channel.Id == 908709951411716166 ? false : true });
            var embed = new DiscordEmbedBuilder
            {
                Color = new DiscordColor("#FF0000"),
                Title = "__**Source Code:**__",
                Description = "**Github:** *https://github.com/Brawlboxgaming/Mario-Kart-Brawlbot*",
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                }
            };

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
        }
    }
}
