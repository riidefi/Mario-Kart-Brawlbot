using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System.IO;
using System.Threading.Tasks;

namespace CTTB.Commands
{
    public class TextCommands : BaseCommandModule
    {

        [Command("help")]
        public async Task Help(CommandContext ctx)
        {
            string description = "__**Standard Commands:**__" +
                "\nc!help" +
                "\nc!cttp" +
                "\nc!source" +
                "\nc!staff track" +
                "\nc!issues track" +
                "\nc!subissues track" +
                "\nc!info track" +
                "\nc!bkt track" +
                "\nc!pop rts/cts/track" +
                "\nc!ttpop rts/cts/track" +
                "\nc!summary track" +
                "\nc!nextupdate" +
                "\nc!rating track";

            foreach (var role in ctx.Member.Roles)
            {
                if (role.Name == "Track Council")
                {
                    description += "\n\n__**Council Commands:**__" +
                        "\nc!hw" +
                        "\nc!gethw track/all" +
                        "\nc!submithw yes/fixes/neutral/no \"track\" feedback" +
                        "\nc!assignedthreads" +
                        "\nc!missedhw";
                }
            }

            foreach (var role in ctx.Member.Roles)
            {
                if (role.Name == "Admin")
                {
                    {
                        description += "\n\n__**Admin Commands:**__" +
                            "\nc!update" +
                            "\nc!lastupdated" +
                            "\nc!reportissue major/minor \"track\" -Issue" +
                            "\nc!reportsubissue \"track\" -Issue" +
                            "\nc!clearissues track" +
                            "\nc!clearsubissues track" +
                            "\nc!replaceissues \"old track\" \"new track\" \"author\" \"version\" \"slot\" \"laps\"" +
                            "\nc!gethw \"track\"/all member" +
                            "\nc!addhw \"track\" \"author\" \"version\" \"download link\" \"slot-filename\" \"speed/lap modifiers\" notes" +
                            "\nc!delhw track" +
                            "\nc!createtest" +
                            "\nc!starttimers" +
                            "\nc!missedhw member" +
                            "\nc!resetmissedhw member" +
                            "\nc!checkmissedhw" +
                            "\nc!removemissedhw member" +
                            "\nc!addmissedhw member" +
                            "\nc!randomassign (reset)" +
                            "\nc!assignedthreads member" +
                            "\nc!removeassignedthread thread id/all" +
                            "\nc!assign member thread id" +
                            "\nc!unassign member thread id" +
                            "\nc!dmrole role message";
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

            await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
        }

        [Command("cttp")]
        public async Task CTTP(CommandContext ctx)
        {
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

            await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
        }

        [Command("sheets")]
        public async Task Sheets(CommandContext ctx)
        {
            string description = "**[Backroom Page:](https://docs.google.com/spreadsheets/d/1xwhKoyypCWq5tCRTI69ijJoDiaoAVsvYAxz-q4UBNqM/edit#gid=1751905284)** *This page has all of the information on our finalized updates, and is viewable to the public. The first tab outlines what the next few updates will look like, and the second tab contains all the accepted tracks, with the fixes needed (if any).*" +
                "\n\n**[Testing Notes Page:](https://docs.google.com/spreadsheets/d/19CF8UP1ubGfkM31uHha6bxYFENnQ-k7L7AEtozENnvk/edit#gid=1807035740)** *This page is what we fill when doing the online tests. It also contains the downloads for all the tracks in the test.*";
            ulong[] channelIds = new ulong[] {
                217126063803727872,
                946835035372257320,
                750123394237726847,
                818728462352646155,
                957878480354295819,
                751534710068477953,
                935200150710808626
            };
            for (int i = 0; i < channelIds.Length; i++)
            {
                if (ctx.Channel.Id == channelIds[i])
                {
                    description += "\n\n**[Council Page:](https://docs.google.com/spreadsheets/d/1I9yFsomTcvFT4hp6eN2azsfv6MsIy1897tBFX_gmtss/edit#gid=906385082)** *This page has multiple important things for the Council to keep track of; this is where members review tracks that are submitted. This is also where we vote for members when they apply.*";
                }
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

            await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
        }

        [Command("source")]
        public async Task SourceCode(CommandContext ctx)
        {
            var embed = new DiscordEmbedBuilder
            {
                Color = new DiscordColor("#FF0000"),
                Title = "__**Source Code:**__",
                Description = "**Github:** *https://github.com/Brawlboxgaming/Custom-Track-Testing-Bot*",
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                }
            };

            await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
        }
    }
}
