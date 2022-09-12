using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using Newtonsoft.Json;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace MKBB.Commands
{
    public class Threads : ApplicationCommandModule
    {
        [SlashCommand("removeassignedthread", "Remove all council members from a specified thread.")]
        [SlashRequireUserPermissions(Permissions.Administrator)]
        public async Task RemoveThreadAssignment(InteractionContext ctx,
            [Option("thread-id", "The id of the thread you wish to remove all members from.")] string threadId)
        {
            try
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
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
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
                            Description = $"*The thread ID {threadId} could not be found.*",
                            Footer = new DiscordEmbedBuilder.EmbedFooter
                            {
                                Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                            }
                        };
                        await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
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
                        await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
                    }
                }
            }
            catch (Exception ex)
            {
                var embed = new DiscordEmbedBuilder
                {
                    Color = new DiscordColor("#FF0000"),
                    Title = "__**Error:**__",
                    Description = $"*{ex.Message}*",
                    Footer = new DiscordEmbedBuilder.EmbedFooter
                    {
                        Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                    }
                };
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));

                Console.WriteLine(ex.ToString());
            }
        }

        [SlashCommand("assignedthreads", "To view the threads you are assigned to.")]
        // Council
        public async Task CheckAssignedThreads(InteractionContext ctx,
            [Option("member", "The council member you want to view the threads of.")] string member)
        {
            try
            {
                await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder() { IsEphemeral = true });
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

                    int ix = -1;

                    if (member == "")
                    {
                        ix = councilJson.FindIndex(x => x.DiscordId == ctx.Member.Id);
                    }
                    else
                    {
                        ix = councilJson.FindIndex(x => Util.CompareStrings(x.SheetName, member));
                        if (ix == -1)
                        {
                            ix = councilJson.FindIndex(x => Util.CompareIncompleteStrings(x.SheetName, member) || Util.CompareStringsLevenshteinDistance(x.SheetName, member));
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
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
                }
            }
            catch (Exception ex)
            {
                var embed = new DiscordEmbedBuilder
                {
                    Color = new DiscordColor("#FF0000"),
                    Title = "__**Error:**__",
                    Description = $"*{ex.Message}*",
                    Footer = new DiscordEmbedBuilder.EmbedFooter
                    {
                        Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                    }
                };
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));

                Console.WriteLine(ex.ToString());
            }
        }

        [SlashCommand("randomassign", "Randomly assign council members to a thread.")]
        [SlashRequireUserPermissions(Permissions.Administrator)]
        public async Task AssignCouncilMembersToThread(InteractionContext ctx)
        {
            if (ctx.Channel.IsThread && ctx.Channel.ParentId == 369281592407097345)
            {
                try
                {
                    await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder() { IsEphemeral = true });

                    string json = string.Empty;

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

                catch (Exception ex)
                {
                    var embed = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = "__**Error:**__",
                        Description = $"*{ex.Message}*",
                        Footer = new DiscordEmbedBuilder.EmbedFooter
                        {
                            Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                        }
                    };
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));

                    Console.WriteLine(ex.ToString());
                }
            }
        }

        [SlashCommand("assign", "Assign a thread to a council member.")]
        [SlashRequireUserPermissions(Permissions.Administrator)]
        public async Task AssignCouncilMemberToThread(InteractionContext ctx,
            [Option("member", "The council member you want to remove the thread from.")] string member,
            [Option("thread-id", "The id of the thread you wish to remove the specified member from.")] string threadId)
        {
            if (ctx.Channel.IsThread && ctx.Channel.ParentId == 369281592407097345)
            {
                try
                {
                    await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder() { IsEphemeral = true });

                    string json = string.Empty;

                    using (var fs = File.OpenRead("council.json"))
                    using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                        json = await sr.ReadToEndAsync().ConfigureAwait(false);
                    List<CouncilMember> councilJson = JsonConvert.DeserializeObject<List<CouncilMember>>(json);

                    using (var fs = File.OpenRead("assigned.json"))
                    using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                        json = await sr.ReadToEndAsync().ConfigureAwait(false);
                    List<CouncilMember> recentlyAssigned = JsonConvert.DeserializeObject<List<CouncilMember>>(json);

                    int ix = -1;

                    foreach (var m in councilJson)
                    {
                        ix = councilJson.FindIndex(x => Util.CompareStrings(x.SheetName, member));
                        if (ix == -1)
                        {
                            ix = councilJson.FindIndex(x => Util.CompareIncompleteStrings(x.SheetName, member) || Util.CompareStringsLevenshteinDistance(x.SheetName, member));
                        }
                    }

                    if (ix == -1)
                    {
                        var embed = new DiscordEmbedBuilder
                        {
                            Color = new DiscordColor("#FF0000"),
                            Title = "__**Error:**__",
                            Description = $"*{member} could not be found on council.*",
                            Footer = new DiscordEmbedBuilder.EmbedFooter
                            {
                                Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                            }
                        };
                        await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
                    }

                    else
                    {
                        bool duplicate = false;
                        foreach (var thread in councilJson[ix].AssignedThreadIds)
                        {
                            if (thread == ulong.Parse(threadId))
                            {
                                duplicate = true;
                                break;
                            }
                        }
                        bool threadExists = false;
                        DiscordChannel wipFeedbackChannel;
                        ctx.Guild.Channels.TryGetValue(369281592407097345, out wipFeedbackChannel);
                        List<DiscordChannel> threads = new List<DiscordChannel>((await wipFeedbackChannel.ListPublicArchivedThreadsAsync()).Threads);
                        foreach (var thread in wipFeedbackChannel.Threads)
                        {
                            threads.Add(thread);
                        }
                        foreach (var assignedThread in councilJson[ix].AssignedThreadIds)
                        {
                            foreach (var thread in threads)
                            {
                                if (thread.Id == assignedThread)
                                {
                                    threadExists = true;
                                    break;
                                }
                            }
                        }
                        if (duplicate)
                        {
                            var embed = new DiscordEmbedBuilder
                            {
                                Color = new DiscordColor("#FF0000"),
                                Title = "__**Error:**__",
                                Description = $"*Thread has already been assigned to council member.*",
                                Footer = new DiscordEmbedBuilder.EmbedFooter
                                {
                                    Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                                }
                            };
                            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
                        }
                        else if (!threadExists)
                        {
                            var embed = new DiscordEmbedBuilder
                            {
                                Color = new DiscordColor("#FF0000"),
                                Title = "__**Error:**__",
                                Description = $"*No thread exists with id {threadId}.*",
                                Footer = new DiscordEmbedBuilder.EmbedFooter
                                {
                                    Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                                }
                            };
                            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
                        }
                        else
                        {
                            councilJson[ix].AssignedThreadIds.Add(ulong.Parse(threadId));

                            recentlyAssigned.Add(councilJson[ix]);

                            string assigned = JsonConvert.SerializeObject(recentlyAssigned);
                            File.WriteAllText("assigned.json", assigned);
                            string council = JsonConvert.SerializeObject(councilJson);
                            File.WriteAllText("council.json", council);

                            var embed = new DiscordEmbedBuilder
                            {
                                Color = new DiscordColor("#FF0000"),
                                Title = "__**Success:**__",
                                Description = $"*<#{threadId}> has been assigned to {councilJson[ix].SheetName}.*",
                                Footer = new DiscordEmbedBuilder.EmbedFooter
                                {
                                    Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                                }
                            };
                            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
                        }
                    }
                }
                catch (Exception ex)
                {
                    var embed = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = "__**Error:**__",
                        Description = $"*{ex.Message}*",
                        Footer = new DiscordEmbedBuilder.EmbedFooter
                        {
                            Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                        }
                    };
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));

                    Console.WriteLine(ex.ToString());
                }
            }
        }

        [SlashCommand("unassign", "Unassign a thread to a council member.")]
        [SlashRequireUserPermissions(Permissions.Administrator)]
        public async Task UnassignCouncilMemberToThread(InteractionContext ctx,
            [Option("member", "The council member you want to remove the thread from.")] string member,
            [Option("thread-id", "The id of the thread you wish to remove the specified member from.")] string threadId)
        {
            if (ctx.Channel.IsThread && ctx.Channel.ParentId == 369281592407097345)
            {
                try
                {
                    await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder() { IsEphemeral = true });

                    string json = string.Empty;

                    using (var fs = File.OpenRead("council.json"))
                    using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                        json = await sr.ReadToEndAsync().ConfigureAwait(false);
                    List<CouncilMember> councilJson = JsonConvert.DeserializeObject<List<CouncilMember>>(json);

                    using (var fs = File.OpenRead("assigned.json"))
                    using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                        json = await sr.ReadToEndAsync().ConfigureAwait(false);
                    List<CouncilMember> recentlyAssigned = JsonConvert.DeserializeObject<List<CouncilMember>>(json);

                    int ix = -1;

                    foreach (var m in councilJson)
                    {
                        ix = councilJson.FindIndex(x => Util.CompareStrings(x.SheetName, member));
                        if (ix == -1)
                        {
                            ix = councilJson.FindIndex(x => Util.CompareIncompleteStrings(x.SheetName, member) || Util.CompareStringsLevenshteinDistance(x.SheetName, member));
                        }
                    }

                    if (ix == -1)
                    {
                        var embed = new DiscordEmbedBuilder
                        {
                            Color = new DiscordColor("#FF0000"),
                            Title = "__**Error:**__",
                            Description = $"*{member} could not be found on council.*",
                            Footer = new DiscordEmbedBuilder.EmbedFooter
                            {
                                Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                            }
                        };
                        await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
                    }

                    else
                    {
                        bool threadFound = false;
                        foreach (var thread in councilJson[ix].AssignedThreadIds)
                        {
                            if (thread == ulong.Parse(threadId))
                            {
                                threadFound = true;
                                break;
                            }
                        }
                        bool threadExists = false;
                        DiscordChannel wipFeedbackChannel;
                        ctx.Guild.Channels.TryGetValue(369281592407097345, out wipFeedbackChannel);
                        List<DiscordChannel> threads = new List<DiscordChannel>((await wipFeedbackChannel.ListPublicArchivedThreadsAsync()).Threads);
                        foreach (var thread in wipFeedbackChannel.Threads)
                        {
                            threads.Add(thread);
                        }
                        foreach (var assignedThread in councilJson[ix].AssignedThreadIds)
                        {
                            foreach (var thread in threads)
                            {
                                if (thread.Id == assignedThread)
                                {
                                    threadExists = true;
                                    break;
                                }
                            }
                        }
                        if (!threadExists)
                        {
                            var embed = new DiscordEmbedBuilder
                            {
                                Color = new DiscordColor("#FF0000"),
                                Title = "__**Error:**__",
                                Description = $"*No thread exists with id {threadId}.*",
                                Footer = new DiscordEmbedBuilder.EmbedFooter
                                {
                                    Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                                }
                            };
                            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
                        }
                        else if (!threadFound)
                        {
                            var embed = new DiscordEmbedBuilder
                            {
                                Color = new DiscordColor("#FF0000"),
                                Title = "__**Error:**__",
                                Description = $"*Thread has not been assigned to council member.*",
                                Footer = new DiscordEmbedBuilder.EmbedFooter
                                {
                                    Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                                }
                            };
                            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
                        }
                        else
                        {
                            recentlyAssigned.Remove(councilJson[ix]);

                            councilJson[ix].AssignedThreadIds.Remove(ulong.Parse(threadId));

                            string assigned = JsonConvert.SerializeObject(recentlyAssigned);
                            File.WriteAllText("assigned.json", assigned);
                            string council = JsonConvert.SerializeObject(councilJson);
                            File.WriteAllText("council.json", council);

                            var embed = new DiscordEmbedBuilder
                            {
                                Color = new DiscordColor("#FF0000"),
                                Title = "__**Success:**__",
                                Description = $"*<#{threadId}> has been unassigned from {councilJson[ix].SheetName}.*",
                                Footer = new DiscordEmbedBuilder.EmbedFooter
                                {
                                    Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                                }
                            };
                            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
                        }
                    }
                }
                catch (Exception ex)
                {
                    var embed = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = "__**Error:**__",
                        Description = $"*{ex.Message}*",
                        Footer = new DiscordEmbedBuilder.EmbedFooter
                        {
                            Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                        }
                    };
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));

                    Console.WriteLine(ex.ToString());
                }
            }
        }
    }
}