using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Microsoft.EntityFrameworkCore;
using MKBB.Class;
using MKBB.Data;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;

namespace MKBB.Commands
{
    public class Testing : ApplicationCommandModule
    {
        [SlashCommand("test", "this is a test")]
        [SlashRequireOwner]
        public static async Task Test(InteractionContext ctx)
        {
            try
            {
                DiscordChannel channel = ctx.Client.GetGuildAsync(180306609233330176).Result.GetChannel(638870517251571712);
                IReadOnlyList<DiscordMessage> pinnedMessages = await channel.GetPinnedMessagesAsync();
                DateTime timeCreated = pinnedMessages[0].CreationTimestamp.Date;
                Debugger.Break();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}