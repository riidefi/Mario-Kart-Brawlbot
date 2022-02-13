using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CTTB.Commands
{
    public class TextCommands : BaseCommandModule
    {
        [Command("help")]
        //[RequireRoles(RoleCheckMode.Any, "Pack & Bot Dev", "Admin")]
        public async Task Help(CommandContext ctx)
        {
            string description = "**c!help**\n*This command displays this message.*" +
                "\n**c!cttp**\n*This command gives you links to tutorials and information about the Custom Track Test Pack.*" +
                "\n**c!source**\n*This command gives you the link to the github page of the bot.*" +
                "\n**c!staff [name of track]**\n*This command displays the staff ghosts for the first found track depending on the input.*" +
                "\n**c!getissues [name of track]**\n*This command displays the reported issues of the first found track depending on the input.*" +
                "\n**c!bkt [rts/cts/rts200/cts200] [name of track]**\n*This command displays the best time for the track inputted.*" +
                "\n**c!wwpop [rts/cts] [range(1-32/218)]**\n*This command displays the leaderboard for worldwide popularity of tracks.*" +
                "\n**c!ttpop [rts/cts] [range(1-32/218)]**\n*This command displays the leaderboard for time trial popularity of tracks.*" +
                "\n**c!wwpopsearch [rts/cts] [name of track]**\n*This command displays the worldwide popularity of tracks containing the string inputted.*" +
                "\n**c!ttpopsearch [rts/cts] [name of track]**\n*This command displays the time trial popularity of tracks containing the string inputted.*";

            foreach (var role in ctx.Member.Roles)
            {
                if (role.Name == "Pack & Bot Dev" || role.Name == "Admin")
                {
                    description = "**c!help**\n*This command displays this message.*" +
                        "\n**c!cttp**\n*This command gives you links to tutorials and information about the Custom Track Test Pack.*" +
                        "\n**c!source**\n*This command gives you the link to the github page of the bot.*" +
                        "\n**c!staff [name of track]**\n*This command displays the staff ghosts for the first found track depending on the input.*" +
                        "\n**c!getissues [name of track]**\n*This command displays the reported issues of the first found track depending on the input.*" +
                        "\n**c!bkt [rts/cts/rts200/cts200] [name of track]**\n*This command displays the best time for the track inputted.*" +
                        "\n**c!wwpop [rts/cts] [range(1-32/218)]**\n*This command displays the leaderboard for worldwide popularity of tracks.*" +
                        "\n**c!ttpop [rts/cts] [range(1-32/218)]**\n*This command displays the leaderboard for time trial popularity of tracks.*" +
                        "\n**c!wwpopsearch [rts/cts] [name of track]**\n*This command displays the worldwide popularity of tracks containing the string inputted.*" +
                        "\n**c!ttpopsearch [rts/cts] [name of track]**\n*This command displays the time trial popularity of tracks containing the string inputted.*" +
                        "\n\n__**Admin Commands:**__" +
                        "\n**c!update**\n*This command updates the database with data from Chadsoft and Wiimmfi.*" +
                        "\n**c!reportissue [issue category] [name of track (in quotes)] [issue]**\n*This command allows you to add an issue to the track inputted on the spreadsheet.*" +
                        "\n**c!clearissues [name of track]**\n*This command allows you to clear the issues of the track inputted.*" +
                        "\n**c!replaceissues [track] [new track] [author] [version] [slot] [laps]**\n*The command allows you to replace a track on the issues spreadsheet with a new one.*";
                }
            }

            var embed = new DiscordEmbedBuilder
            {
                Color = new DiscordColor("#FF0000"),
                Title = "__**Help**__",
                Description = description,
                Timestamp = DateTime.UtcNow
            };

            await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
        }

        [Command("cttp")]
        //[RequireRoles(RoleCheckMode.Any, "Pack & Bot Dev", "Admin")]
        public async Task CTTP(CommandContext ctx)
        {
            var embed = new DiscordEmbedBuilder
            {
                Color = new DiscordColor("#FF0000"),
                Title = "__**Custom Track Test Pack Links**__",
                Description = "**Base Pack:** *https://drive.google.com/file/d/1tzhaBxycHEvY5G2k5jTzpONBhmq6g08g/view?usp=sharing*" +
                "\n**Installation Tutorial:** *https://youtu.be/Vh3GXTbxHLU*" +
                "\n**Test Pack Creation Tutorial:** *https://youtu.be/igtfmZAyG3g*",
                Timestamp = DateTime.UtcNow
            };

            await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
        }

        [Command("source")]
        //[RequireRoles(RoleCheckMode.Any, "Pack & Bot Dev", "Admin")]
        public async Task SourceCode(CommandContext ctx)
        {
            var embed = new DiscordEmbedBuilder
            {
                Color = new DiscordColor("#FF0000"),
                Title = "__**Source Code:**__",
                Description = "**Github:** *https://github.com/Brawlboxgaming/Custom-Track-Testing-Bot*",
                Timestamp = DateTime.UtcNow
            };

            await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
        }
    }
}
