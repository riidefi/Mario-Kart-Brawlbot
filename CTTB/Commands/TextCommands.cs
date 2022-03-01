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
            string description = "__**Standard Commands:**__" +
                "\nc!help" +
                "\nc!cttp" +
                "\nc!source" +
                "\nc!staff [name of track]" +
                "\nc!getissues [name of track]" +
                "\nc!getinfo [name of track]" +
                "\nc!bkt [rts/cts/rts200/cts200] [name of track]" +
                "\nc!wwpop [rts/cts] [range(1-32/218)]" +
                "\nc!ttpop [rts/cts] [range(1-32/218)]" +
                "\nc!wwpopsearch [rts/cts] [name of track]" +
                "\nc!ttpopsearch [rts/cts] [name of track]" +
                "\nc!getsummary [name of track]" +
                "\nc!nextupdate";

            foreach (var role in ctx.Member.Roles)
            {
                if (role.Name == "Track Council")
                {
                    description += "\n\n__**Council Commands:**__" +
                        "\nc!hw" +
                        "\nc!gethw [name of track]" +
                        "\nc!submithw [yes/fixes/neutral/no] [name of track] [feedback]";
                }
                if (role.Name == "Pack & Bot Dev" || role.Name == "Admin")
                {
                    description += "\n\n__**Admin Commands:**__" +
                        "\nc!update" +
                        "\nc!reportissue [issue category] [name of track] [issue]" +
                        "\nc!clearissues [name of track]" +
                        "\nc!replaceissues [track] [new track] [author] [version] [slot] [laps]" +
                        "\nc!gethw [name of track] [mention/name]" +
                        "\nc!addhw [name of track] [author] [version] [download link] [slot (e.g. Luigi Circuit - beginner_course)] [speed/ lap modifiers] [notes]";
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
