using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using IronPython.Runtime.Operations;
using Newtonsoft.Json;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CTTB.Commands
{
    public class Threads : BaseCommandModule
    {
        public Util Utility = new Util();

        [Command("removeassignedthread")]
        [RequireRoles(RoleCheckMode.Any, "Admin")]
        public async Task RemoveThreadAssignment(CommandContext ctx, [RemainingText] string threadId = "")
        {
            if (ulong.Parse(threadId) == 0)
            {
                var embed = new DiscordEmbedBuilder
                {
                    Color = new DiscordColor("#FF0000"),
                    Title = "__**Error:**__",
                    Description = $"*Thread ID was not inputted.*" +
                            "\n**c!removeassignedthread thread id/all**",
                    Footer = new DiscordEmbedBuilder.EmbedFooter
                    {
                        Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                    }
                };
                await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
            }
            else
            {
                try
                {
                    if (ctx.Channel.Id == 217126063803727872 || ctx.Channel.Id == 750123394237726847 || ctx.Channel.Id == 935200150710808626 || ctx.Channel.Id == 946835035372257320 || ctx.Channel.Id == 751534710068477953)
                    {
                        string json;
                        using (var fs = File.OpenRead("council.json"))
                        using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                            json = await sr.ReadToEndAsync().ConfigureAwait(false);
                        List<CouncilMember> councilJson = JsonConvert.DeserializeObject<List<CouncilMember>>(json);

                        if (threadId == "all")
                        {
                            foreach (var m in councilJson)
                            {
                                m.AssignedThreadIds.Clear();
                            }

                            string council = JsonConvert.SerializeObject(councilJson);
                            File.WriteAllText("council.json", council);

                            var embed = new DiscordEmbedBuilder
                            {
                                Color = new DiscordColor("#FF0000"),
                                Title = "__**Success:**__",
                                Description = "The council json has been reset.",
                                Footer = new DiscordEmbedBuilder.EmbedFooter
                                {
                                    Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                                }
                            };
                            await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
                        }
                        else
                        {
                            string tName = "";
                            for (int i = 0; i < councilJson.Count; i++)
                            {
                                int ix = councilJson.FindIndex(x => x.AssignedThreadIds.Contains(ulong.Parse(threadId)));
                                if (ix == -1)
                                {
                                    break;
                                }
                                else
                                {
                                    int idIx = councilJson[ix].AssignedThreadIds.FindIndex(x => x == ulong.Parse(threadId));

                                    DiscordChannel wipFeedbackChannel;
                                    ctx.Guild.Channels.TryGetValue(369281592407097345, out wipFeedbackChannel);
                                    List<DiscordChannel> threads = new List<DiscordChannel>((await wipFeedbackChannel.ListPublicArchivedThreadsAsync()).Threads);
                                    foreach (var thread in wipFeedbackChannel.Threads)
                                    {
                                        threads.Add(thread);
                                    }
                                    foreach (var thread in threads)
                                    {
                                        if (idIx != -1 && thread.Id == councilJson[ix].AssignedThreadIds[idIx])
                                        {
                                            tName = thread.Name;
                                            break;
                                        }
                                    }

                                    councilJson[ix].AssignedThreadIds.RemoveAt(idIx);
                                }
                            }
                            if (tName == "")
                            {
                                var embed = new DiscordEmbedBuilder
                                {
                                    Color = new DiscordColor("#FF0000"),
                                    Title = "__**Error:**__",
                                    Description = $"*The thread ID {threadId} could not be found.*" +
                                    $"\n**c!removeassignedthread thread id/all**",
                                    Footer = new DiscordEmbedBuilder.EmbedFooter
                                    {
                                        Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                                    }
                                };
                                await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
                            }
                            else
                            {
                                string council = JsonConvert.SerializeObject(councilJson);
                                File.WriteAllText("council.json", council);
                                var embed = new DiscordEmbedBuilder
                                {
                                    Color = new DiscordColor("#FF0000"),
                                    Title = "__**Success:**__",
                                    Description = $"*[{tName}](https://discord.com/channels/180306609233330176/{threadId}) has been unassigned from all council members.*",
                                    Footer = new DiscordEmbedBuilder.EmbedFooter
                                    {
                                        Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                                    }
                                };
                                await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    var embed = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = "__**Error:**__",
                        Description = $"*{ex.Message}*" +
                            "\n**c!removeassignedthread thread id/all**",
                        Footer = new DiscordEmbedBuilder.EmbedFooter
                        {
                            Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                        }
                    };
                    await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);

                    Console.WriteLine(ex.ToString());
                }
            }
        }

        [Command("assignedthreads")]
        [RequireRoles(RoleCheckMode.Any, "Track Council", "Admin")]
        public async Task CheckAssignedThreads(CommandContext ctx, [RemainingText] string member = "")
        {
            try
            {
                if (ctx.Channel.Id == 217126063803727872 || ctx.Channel.Id == 750123394237726847 || ctx.Channel.Id == 935200150710808626 || ctx.Channel.Id == 946835035372257320 || ctx.Channel.Id == 751534710068477953)
                {
                    await ctx.TriggerTypingAsync();
                    string json;
                    using (var fs = File.OpenRead("council.json"))
                    using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                        json = await sr.ReadToEndAsync().ConfigureAwait(false);
                    List<CouncilMember> councilJson = JsonConvert.DeserializeObject<List<CouncilMember>>(json);
                    if (member == "reset" && ctx.Member.Roles.Select(x => x.Name == "Admin").Count() > 0)
                    {
                        foreach (var councilMember in councilJson)
                        {
                            councilMember.AssignedThreadIds = new List<ulong>();
                        }
                    }
                    else
                    {
                        string description = string.Empty;

                        int j = 0;
                        foreach (var role in ctx.Member.Roles)
                        {
                            if (role.Name == "Admin")
                            {
                                j++;
                                break;
                            }
                        }
                        int ix = -1;

                        if (j == 0)
                        {
                            ix = councilJson.FindIndex(x => x.DiscordId == ctx.Member.Id);
                        }
                        else
                        {
                            if (member == "")
                            {
                                ix = councilJson.FindIndex(x => x.DiscordId == ctx.Member.Id);
                            }
                            else
                            {
                                ix = councilJson.FindIndex(x => Utility.CompareIncompleteStrings(x.SheetName, member) || Utility.CompareStringsLevenshteinDistance(x.SheetName, member));
                            }
                        }

                        for (int i = 0; i < councilJson[ix].AssignedThreadIds.Count; i++)
                        {
                            string tName = string.Empty;
                            DiscordChannel wipFeedbackChannel;
                            ctx.Guild.Channels.TryGetValue(369281592407097345, out wipFeedbackChannel);
                            List<DiscordChannel> threads = new List<DiscordChannel>((await wipFeedbackChannel.ListPublicArchivedThreadsAsync()).Threads);
                            foreach (var thread in wipFeedbackChannel.Threads)
                            {
                                threads.Add(thread);
                            }
                            foreach (var thread in threads)
                            {
                                if (thread.Id == councilJson[ix].AssignedThreadIds[i])
                                {
                                    tName = thread.Name;
                                }
                            }
                            description += $"*[{tName}](https://discord.com/channels/180306609233330176/{councilJson[ix].AssignedThreadIds[i]})*\n";
                        }

                        if (councilJson[ix].AssignedThreadIds.Count == 0)
                        {
                            description = "*No assigned threads.*";
                        }

                        var embed = new DiscordEmbedBuilder
                        {
                            Color = new DiscordColor("#FF0000"),
                            Title = $"__**Assigned Threads of {councilJson[ix].SheetName}:**__",
                            Description = description,
                            Footer = new DiscordEmbedBuilder.EmbedFooter
                            {
                                Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                            }
                        };
                        await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
                    }
                }
            }
            catch (Exception ex)
            {
                var embed = new DiscordEmbedBuilder
                {
                    Color = new DiscordColor("#FF0000"),
                    Title = "__**Error:**__",
                    Description = $"*{ex.Message}*" +
                        "\n**c!assignedthreads member**",
                    Footer = new DiscordEmbedBuilder.EmbedFooter
                    {
                        Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                    }
                };
                await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);

                Console.WriteLine(ex.ToString());
            }
        }

        [Command("threadassign")]
        [RequireRoles(RoleCheckMode.Any, "Admin")]
        public async Task AssignCouncilMemberToThread(CommandContext ctx, [RemainingText] string arg = "")
        {
            if (ctx.Guild.Id == 180306609233330176 && ctx.Channel.IsThread && ctx.Channel.ParentId == 369281592407097345)
            {
                var embed = new DiscordEmbedBuilder();
                try
                {
                    string json = string.Empty;
                    await ctx.TriggerTypingAsync();

                    if (arg == "reset")
                    {
                        List<CouncilMember> recentlyAssigned = new List<CouncilMember>();
                        List<CouncilMember> remove = new List<CouncilMember>();

                        string assigned = JsonConvert.SerializeObject(recentlyAssigned);
                        string removals = JsonConvert.SerializeObject(remove);
                        File.WriteAllText("assigned.json", assigned);

                        embed = new DiscordEmbedBuilder
                        {
                            Color = new DiscordColor("#FF0000"),
                            Title = "__**Success:**__",
                            Description = "The thread json has been reset.",
                            Footer = new DiscordEmbedBuilder.EmbedFooter
                            {
                                Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                            }
                        };
                        await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
                    }

                    else
                    {
                        Random rng = new Random();

                        using (var fs = File.OpenRead("council.json"))
                        using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                            json = await sr.ReadToEndAsync().ConfigureAwait(false);
                        List<CouncilMember> councilJson = JsonConvert.DeserializeObject<List<CouncilMember>>(json);

                        List<CouncilMember> compJson = new List<CouncilMember>();
                        List<CouncilMember> nonCompJson = new List<CouncilMember>();
                        List<CouncilMember> assignedMembers = new List<CouncilMember>();

                        using (var fs = File.OpenRead("assigned.json"))
                        using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                            json = await sr.ReadToEndAsync().ConfigureAwait(false);
                        List<CouncilMember> recentlyAssigned = JsonConvert.DeserializeObject<List<CouncilMember>>(json);

                        using (var fs = File.OpenRead("council.json"))
                        using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                            json = await sr.ReadToEndAsync().ConfigureAwait(false);
                        List<CouncilMember> defaultCouncil = JsonConvert.DeserializeObject<List<CouncilMember>>(json);

                        for (int i = 0; i < councilJson.Count; i++)
                        {
                            if (councilJson[i].CompPlayer)
                            {
                                compJson.Add(councilJson[i]);
                            }
                            else
                            {
                                nonCompJson.Add(councilJson[i]);
                            }
                        }

                        for (int i = 0; i < ctx.Channel.Name.Split(',').Length; i++)
                        {
                            int ix = councilJson.FindIndex(x => ctx.Channel.Name.ToLowerInvariant().Contains(x.SheetName.ToLowerInvariant()));
                            if (ix != -1)
                            {
                                councilJson.RemoveAt(ix);
                            }
                        }

                        for (int i = 0; i < recentlyAssigned.Count; i++)
                        {
                            int ix = councilJson.FindIndex(x => x.DiscordId == recentlyAssigned[i].DiscordId);
                            if (ix != -1)
                            {
                                councilJson.RemoveAt(ix);
                            }
                        }

                        if (councilJson.Count < 5)
                        {
                            for (int i = 0; i < councilJson.Count; i++)
                            {
                                assignedMembers.Add(councilJson[i]);
                                int ix = defaultCouncil.FindIndex(x => x.DiscordId == assignedMembers[assignedMembers.Count - 1].DiscordId);
                                if (defaultCouncil[ix].AssignedThreadIds.Count == 3)
                                {
                                    defaultCouncil[ix].AssignedThreadIds.RemoveAt(0);
                                }
                                defaultCouncil[ix].AssignedThreadIds.Add(ctx.Channel.Id);
                            }

                            using (var fs = File.OpenRead("council.json"))
                            using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                                json = await sr.ReadToEndAsync().ConfigureAwait(false);
                            councilJson = JsonConvert.DeserializeObject<List<CouncilMember>>(json);
                            recentlyAssigned = new List<CouncilMember>();
                        }

                        int[] randomNums = new int[councilJson.Count];

                        for (int i = 0; i < councilJson.Count; i++)
                        {
                            randomNums[i] = i;
                        }

                        randomNums = randomNums.OrderBy(x => rng.Next()).ToArray();

                        for (int i = 0; i < 3; i++)
                        {
                            if (assignedMembers.Count == 3)
                            {
                                break;
                            }
                            assignedMembers.Add(councilJson[randomNums[i]]);
                            int ix = defaultCouncil.FindIndex(x => x.DiscordId == assignedMembers[assignedMembers.Count - 1].DiscordId);
                            if (defaultCouncil[ix].AssignedThreadIds.Count == 3)
                            {
                                defaultCouncil[ix].AssignedThreadIds.RemoveAt(0);
                            }
                            defaultCouncil[ix].AssignedThreadIds.Add(ctx.Channel.Id);
                        }

                        if (assignedMembers.Count(x => x.CompPlayer == true) > 1)
                        {
                            for (int i = 4; i < councilJson.Count; i++)
                            {
                                if (assignedMembers.Count > 4)
                                {
                                    break;
                                }
                                if (!councilJson[randomNums[i]].CompPlayer)
                                {
                                    assignedMembers.Add(councilJson[randomNums[i]]);
                                    int ix = defaultCouncil.FindIndex(x => x.DiscordId == assignedMembers[assignedMembers.Count - 1].DiscordId);
                                    if (defaultCouncil[ix].AssignedThreadIds.Count == 3)
                                    {
                                        defaultCouncil[ix].AssignedThreadIds.RemoveAt(0);
                                    }
                                    defaultCouncil[ix].AssignedThreadIds.Add(ctx.Channel.Id);
                                }
                            }
                        }
                        else if (assignedMembers.Count(x => x.CompPlayer == false) > 1)
                        {
                            for (int i = 4; i < councilJson.Count; i++)
                            {
                                if (assignedMembers.Count > 4)
                                {
                                    break;
                                }
                                if (councilJson[randomNums[i]].CompPlayer)
                                {
                                    assignedMembers.Add(councilJson[randomNums[i]]);
                                    int ix = defaultCouncil.FindIndex(x => x.DiscordId == assignedMembers[assignedMembers.Count - 1].DiscordId);
                                    if (defaultCouncil[ix].AssignedThreadIds.Count == 3)
                                    {
                                        defaultCouncil[ix].AssignedThreadIds.RemoveAt(0);
                                    }
                                    defaultCouncil[ix].AssignedThreadIds.Add(ctx.Channel.Id);
                                }
                            }
                        }
                        else
                        {
                            assignedMembers.Add(councilJson[randomNums[4]]);
                            int ix = defaultCouncil.FindIndex(x => x.DiscordId == assignedMembers[assignedMembers.Count - 1].DiscordId);
                            if (defaultCouncil[ix].AssignedThreadIds.Count == 3)
                            {
                                defaultCouncil[ix].AssignedThreadIds.RemoveAt(0);
                            }
                            defaultCouncil[ix].AssignedThreadIds.Add(ctx.Channel.Id);
                        }

                        for (int i = 5; i < randomNums.Count(); i++)
                        {
                            if (assignedMembers.Count < 5)
                            {
                                if (!assignedMembers.Select(x => x.DiscordId == councilJson[randomNums[i]].DiscordId).Any())
                                {
                                    assignedMembers.Add(councilJson[randomNums[i]]);
                                    int ix = defaultCouncil.FindIndex(x => x.DiscordId == assignedMembers[assignedMembers.Count - 1].DiscordId);
                                    if (defaultCouncil[ix].AssignedThreadIds.Count == 3)
                                    {
                                        defaultCouncil[ix].AssignedThreadIds.RemoveAt(0);
                                    }
                                    defaultCouncil[ix].AssignedThreadIds.Add(ctx.Channel.Id);
                                }
                            }
                            else
                            {
                                break;
                            }
                        }

                        foreach (var m in assignedMembers)
                        {
                            recentlyAssigned.Add(m);
                        }

                        await ctx.Channel.SendMessageAsync(
                        $"<@{assignedMembers[0].DiscordId}>, " +
                        $"<@{assignedMembers[1].DiscordId}>, " +
                        $"<@{assignedMembers[2].DiscordId}>, " +
                        $"<@{assignedMembers[3].DiscordId}>, and " +
                        $"<@{assignedMembers[4].DiscordId}> have been assigned to this thread.");

                        string assigned = JsonConvert.SerializeObject(recentlyAssigned);
                        File.WriteAllText("assigned.json", assigned);
                        string council = JsonConvert.SerializeObject(defaultCouncil);
                        File.WriteAllText("council.json", council);
                    }
                }
                catch (Exception ex)
                {
                    embed = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = "__**Error:**__",
                        Description = $"*{ex.Message}*" +
                            "\n**c!threadassign**",
                        Footer = new DiscordEmbedBuilder.EmbedFooter
                        {
                            Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                        }
                    };
                    await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);

                    Console.WriteLine(ex.ToString());
                }
            }
        }
    }
}