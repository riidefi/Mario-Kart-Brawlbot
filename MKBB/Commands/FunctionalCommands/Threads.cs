using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static IronPython.Modules.PythonDateTime;

namespace MKBB.Commands
{
    public class Threads : ApplicationCommandModule
    {
        [SlashCommand("removeassignedthread", "Remove all council members from a specified thread.")]
        [SlashRequireUserPermissions(Permissions.ManageGuild)]
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
                await Util.ThrowError(ctx, ex);
            }
        }

        [SlashCommand("assignedthreads", "To view the threads you are assigned to.")]
        // Council
        public async Task CheckAssignedThreads(InteractionContext ctx,
            [Option("member", "The council member you want to view the threads of.")] DiscordUser member)
        {
            try
            {
                await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder() { IsEphemeral = true });
                string json;
                using (var fs = File.OpenRead("council.json"))
                using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                    json = await sr.ReadToEndAsync().ConfigureAwait(false);
                List<CouncilMember> councilJson = JsonConvert.DeserializeObject<List<CouncilMember>>(json);

                string description = string.Empty;

                int ix = councilJson.FindIndex(x => x.DiscordId == member.Id);
                var embed = new DiscordEmbedBuilder();
                if (ix < 0)
                {
                    embed = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = $"__**Error:**__",
                        Description = $"*{member.Mention} could not be found on council.*",
                        Url = Util.GetCouncilUrl(),
                        Footer = new DiscordEmbedBuilder.EmbedFooter
                        {
                            Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                        }
                    };
                }
                else
                {
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

                    embed = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = $"__**Assigned Threads of {councilJson[ix].SheetName}:**__",
                        Description = description,
                        Footer = new DiscordEmbedBuilder.EmbedFooter
                        {
                            Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                        }
                    };
                }
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
            }
            catch (Exception ex)
            {
                await Util.ThrowError(ctx, ex);
            }
        }

        [SlashCommand("randomassign", "Randomly assign council members to a thread.")]
        [SlashRequireUserPermissions(Permissions.ManageGuild)]
        public async Task AssignCouncilMembersToThread(InteractionContext ctx)
        {
            if (ctx.Channel.IsThread && ctx.Channel.ParentId == 1046936322574655578)
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

                    DiscordThreadChannel thread = (DiscordThreadChannel)ctx.Channel;

                    for (int i = 0; i < ctx.Channel.Name.Split(',').Length; i++)
                    {
                        int ix = councilJson.FindIndex(x => x.DiscordId == thread.CreatorId);
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

                    var toBePinned = await ctx.Channel.SendMessageAsync(
                    $"<@{assignedMembers[0].DiscordId}>, " +
                    $"<@{assignedMembers[1].DiscordId}>, " +
                    $"<@{assignedMembers[2].DiscordId}>, " +
                    $"<@{assignedMembers[3].DiscordId}>, and " +
                    $"<@{assignedMembers[4].DiscordId}> have been assigned to this thread.");

                    await toBePinned.PinAsync();

                    string assigned = JsonConvert.SerializeObject(recentlyAssigned);
                    File.WriteAllText("assigned.json", assigned);
                    string council = JsonConvert.SerializeObject(defaultCouncil);
                    File.WriteAllText("council.json", council);

                    var embed = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = "__**Success:**__",
                        Description = $"*Council members have been assigned.*",
                        Footer = new DiscordEmbedBuilder.EmbedFooter
                        {
                            Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                        }
                    };
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
                }

                catch (Exception ex)
                {
                    await Util.ThrowError(ctx, ex);
                }
            }
        }

        [SlashCommand("assign", "Assign a thread to a council member.")]
        [SlashRequireUserPermissions(Permissions.ManageGuild)]
        public async Task AssignCouncilMemberToThread(InteractionContext ctx,
            [Option("member", "The council member you want to remove the thread from.")] DiscordUser member,
            [Option("thread-id", "The id of the thread you wish to remove the specified member from.")] string threadId)
        {
            if (ctx.Channel.IsThread && ctx.Channel.ParentId == 1046936322574655578 || ctx.Channel.ParentId == 369281592407097345)
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

                    int ix = councilJson.FindIndex(x => x.DiscordId == member.Id);
                    var embed = new DiscordEmbedBuilder();
                    if (ix < 0)
                    {
                        embed = new DiscordEmbedBuilder
                        {
                            Color = new DiscordColor("#FF0000"),
                            Title = $"__**Error:**__",
                            Description = $"*{member.Mention} could not be found on council.*",
                            Url = Util.GetCouncilUrl(),
                            Footer = new DiscordEmbedBuilder.EmbedFooter
                            {
                                Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                            }
                        };
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
                            embed = new DiscordEmbedBuilder
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
                            embed = new DiscordEmbedBuilder
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

                            embed = new DiscordEmbedBuilder
                            {
                                Color = new DiscordColor("#FF0000"),
                                Title = "__**Success:**__",
                                Description = $"*<#{threadId}> has been assigned to {councilJson[ix].SheetName}.*",
                                Footer = new DiscordEmbedBuilder.EmbedFooter
                                {
                                    Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                                }
                            };
                        }
                    }
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
                }
                catch (Exception ex)
                {
                    await Util.ThrowError(ctx, ex);
                }
            }
        }

        [SlashCommand("unassign", "Unassign a thread to a council member.")]
        [SlashRequireUserPermissions(Permissions.ManageGuild)]
        public async Task UnassignCouncilMemberToThread(InteractionContext ctx,
            [Option("member", "The council member you want to remove the thread from.")] DiscordUser member,
            [Option("thread-id", "The id of the thread you wish to remove the specified member from.")] string threadId)
        {
            if (ctx.Channel.IsThread && ctx.Channel.ParentId == 1046936322574655578 || ctx.Channel.ParentId == 369281592407097345)
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

                    int ix = councilJson.FindIndex(x => x.DiscordId == member.Id);
                    var embed = new DiscordEmbedBuilder();
                    if (ix < 0)
                    {
                        embed = new DiscordEmbedBuilder
                        {
                            Color = new DiscordColor("#FF0000"),
                            Title = $"__**Error:**__",
                            Description = $"*{member.Mention} could not be found on council.*",
                            Url = Util.GetCouncilUrl(),
                            Footer = new DiscordEmbedBuilder.EmbedFooter
                            {
                                Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                            }
                        };
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
                            embed = new DiscordEmbedBuilder
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
                            embed = new DiscordEmbedBuilder
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

                            embed = new DiscordEmbedBuilder
                            {
                                Color = new DiscordColor("#FF0000"),
                                Title = "__**Success:**__",
                                Description = $"*<#{threadId}> has been unassigned from {councilJson[ix].SheetName}.*",
                                Footer = new DiscordEmbedBuilder.EmbedFooter
                                {
                                    Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                                }
                            };
                        }
                    }
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
                }
                catch (Exception ex)
                {
                    await Util.ThrowError(ctx, ex);
                }
            }
        }

        //[SlashCommand("pinmessage", "Pin a message in the thread.")]
        public async Task PinMessage(InteractionContext ctx,
            [Option("message", "The message to pin in the thread.")] string message,
            [Option("file", "The file to pin in the thread.")] DiscordAttachment attachment = null)
        {
            if (ctx.Channel.IsThread && ctx.Channel.ParentId == 1046936322574655578 || ctx.Channel.ParentId == 369281592407097345)
            {
                try
                {
                    await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder() { IsEphemeral = true });

                    DiscordThreadChannel thread = (DiscordThreadChannel)ctx.Channel;

                    if (ctx.Member.Id != thread.CreatorId)
                    {
                        await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(new DiscordEmbedBuilder
                        {
                            Color = new DiscordColor("#FF0000"),
                            Title = "__**Error:**__",
                            Description = $"*You are not the thread's creator - pin request denied.*",
                            Footer = new DiscordEmbedBuilder.EmbedFooter
                            {
                                Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                            }
                        }));
                    }
                    else
                    {
                        DiscordMessageBuilder builder = new DiscordMessageBuilder();
                        if (attachment == null)
                        {
                            builder.WithContent(message);
                        }
                        else
                        {
                            WebClient webClient = new WebClient();
                            await webClient.DownloadFileTaskAsync(new Uri(attachment.Url, UriKind.Absolute), attachment.FileName);
                            builder.WithContent(message).AddFile(attachment.FileName, new FileStream(attachment.FileName, FileMode.Open, FileAccess.Read));
                        }

                        var toBePinned = await ctx.Channel.SendMessageAsync(builder);

                        if (attachment != null)
                        {
                            File.Delete(attachment.FileName);
                        }

                        foreach (var msg in await ctx.Channel.GetPinnedMessagesAsync())
                        {
                            await msg.UnpinAsync();
                        }
                        await toBePinned.PinAsync();

                        var embed = new DiscordEmbedBuilder
                        {
                            Color = new DiscordColor("#FF0000"),
                            Title = "__**Success:**__",
                            Description = $"*Message has been pinned.*",
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
                    await Util.ThrowError(ctx, ex);
                }
            }
        }
    }
}