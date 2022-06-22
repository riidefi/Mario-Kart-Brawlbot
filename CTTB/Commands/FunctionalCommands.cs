using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using HtmlAgilityPack;
using IronPython.Runtime.Operations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.DevTools.V85.Fetch;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Xml;

namespace CTTB.Commands
{
    public class FunctionalCommands : BaseCommandModule
    {
        Scrape Scraper = new Scrape();

        System.Timers.Timer dailyTimer;

        int lastHwDateChecked;

        public bool CompareStrings(string arg1, string arg2)
        {
            if (arg1.Replace(".", string.Empty).Replace("_", " ").Replace("`", string.Empty).Replace("'", string.Empty).ToLowerInvariant() == arg2.Replace(".", string.Empty).Replace("_", " ").Replace("'", string.Empty).ToLowerInvariant())
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool CompareIncompleteStrings(string arg1, string arg2)
        {
            if (arg1.Replace(".", string.Empty).Replace("_", " ").Replace("`", string.Empty).Replace("'", string.Empty).ToLowerInvariant().Contains(arg2.Replace(".", string.Empty).Replace("_", " ").Replace("'", string.Empty).ToLowerInvariant()))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool CompareStringsLevenshteinDistance(string arg1, string arg2)
        {
            arg1 = arg1.ToLowerInvariant();
            arg2 = arg2.ToLowerInvariant();

            var bounds = new { Height = arg1.Length + 1, Width = arg2.Length + 1 };

            int[,] matrix = new int[bounds.Height, bounds.Width];

            for (int height = 0; height < bounds.Height; height++) { matrix[height, 0] = height; };
            for (int width = 0; width < bounds.Width; width++) { matrix[0, width] = width; };

            for (int height = 1; height < bounds.Height; height++)
            {
                for (int width = 1; width < bounds.Width; width++)
                {
                    int cost = (arg1[height - 1] == arg2[width - 1]) ? 0 : 1;
                    int insertion = matrix[height, width - 1] + 1;
                    int deletion = matrix[height - 1, width] + 1;
                    int substitution = matrix[height - 1, width - 1] + cost;

                    int distance = Math.Min(insertion, Math.Min(deletion, substitution));

                    if (height > 1 && width > 1 && arg1[height - 1] == arg2[width - 2] && arg1[height - 2] == arg2[width - 1])
                    {
                        distance = Math.Min(distance, matrix[height - 2, width - 2] + cost);
                    }

                    matrix[height, width] = distance;
                }
            }

            if (matrix[bounds.Height - 1, bounds.Width - 1] > 2)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public bool CompareStringAbbreviation(string abbr, string full)
        {
            string[] fullArray = full.Split(' ');

            string abbreviation = string.Empty;

            if (full.Contains(" ") && (fullArray[0] == "SNES" ||
                fullArray[0] == "N64" ||
                fullArray[0] == "GBA" ||
                fullArray[0] == "GCN" ||
                fullArray[0] == "DS" ||
                fullArray[0] == "3DS"))
            {
                abbreviation = $"{fullArray[0]} ";
                fullArray[0] = "";
            }

            for (int i = 0; i < fullArray.Length; i++)
            {
                if (fullArray[i].Length != 0)
                {
                    abbreviation += fullArray[i].Substring(0, 1);
                }
            }
            if (abbreviation.ToLowerInvariant() == abbr.ToLowerInvariant())
            {
                return true;
            }

            return false;
        }

        [Command("update")]
        [RequireRoles(RoleCheckMode.Any, "Admin")]
        public async Task UpdateTimer(CommandContext ctx, [RemainingText] string arg = "")
        {
            if (ctx.Guild.Id == 180306609233330176)
            {
                await ctx.TriggerTypingAsync();
                await Update(ctx, arg);

                var embed = new DiscordEmbedBuilder
                {
                    Color = new DiscordColor("#FF0000"),
                    Title = $"__**Notice:**__",
                    Description = "Database has been updated.",
                    Footer = new DiscordEmbedBuilder.EmbedFooter
                    {
                        Text = $"Server Time: {DateTime.Now}"
                    }
                };

                await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
            }
        }

        public async Task Update(CommandContext ctx, string arg)
        {
            try
            {
                string rtttUrl = "http://tt.chadsoft.co.uk/original-track-leaderboards.json";
                string ctttUrl = "http://tt.chadsoft.co.uk/ctgp-leaderboards.json";
                string rttt200Url = "http://tt.chadsoft.co.uk/original-track-leaderboards-200cc.json";
                string cttt200Url = "http://tt.chadsoft.co.uk/ctgp-leaderboards-200cc.json";
                string ctwwUrl1 = "https://wiimmfi.de/stats/track/mv/ctgp?m=json&p=std,c1,0";
                string ctwwUrl2 = "https://wiimmfi.de/stats/track/mv/ctgp?m=json&p=std,c1,0,100";
                string ctwwUrl3 = "https://wiimmfi.de/stats/track/mv/ctgp?m=json&p=std,c1,0,200";
                string wwUrl = "https://wiimmfi.de/stats/track/mv/ww?m=json&p=std,c1,0";

                // Leaderboards

                var webClient = new WebClient();

                var rtRawJson = JsonConvert.DeserializeObject<LeaderboardInfo>(await webClient.DownloadStringTaskAsync(rtttUrl));
                var ctRawJson = JsonConvert.DeserializeObject<LeaderboardInfo>(await webClient.DownloadStringTaskAsync(ctttUrl));
                var rtRaw200Json = JsonConvert.DeserializeObject<LeaderboardInfo>(await webClient.DownloadStringTaskAsync(rttt200Url));
                var ctRaw200Json = JsonConvert.DeserializeObject<LeaderboardInfo>(await webClient.DownloadStringTaskAsync(cttt200Url));

                foreach (var track in rtRawJson.Leaderboard)
                {
                    track.LeaderboardLink = track.Link.Href.LeaderboardLink;
                    track.Link = null;
                }
                foreach (var track in ctRawJson.Leaderboard)
                {
                    track.LeaderboardLink = track.Link.Href.LeaderboardLink;
                    track.Link = null;
                }
                foreach (var track in rtRaw200Json.Leaderboard)
                {
                    track.LeaderboardLink = track.Link.Href.LeaderboardLink;
                    track.Link = null;
                }
                foreach (var track in ctRaw200Json.Leaderboard)
                {
                    track.LeaderboardLink = track.Link.Href.LeaderboardLink;
                    track.Link = null;
                }

                var rtJson = JsonConvert.SerializeObject(rtRawJson.Leaderboard);
                var ctJson = JsonConvert.SerializeObject(ctRawJson.Leaderboard);
                var rt200Json = JsonConvert.SerializeObject(rtRaw200Json.Leaderboard);
                var ct200Json = JsonConvert.SerializeObject(ctRaw200Json.Leaderboard);

                string ctwwDl1 = await webClient.DownloadStringTaskAsync(ctwwUrl1);
                string ctwwDl2 = await webClient.DownloadStringTaskAsync(ctwwUrl2);
                string ctwwDl3 = await webClient.DownloadStringTaskAsync(ctwwUrl3);
                string wwDl = await webClient.DownloadStringTaskAsync(wwUrl);

                List<Track> trackListRt = JsonConvert.DeserializeObject<List<Track>>(rtJson);
                List<Track> trackListRt200 = JsonConvert.DeserializeObject<List<Track>>(rt200Json);
                List<Track> trackList = JsonConvert.DeserializeObject<List<Track>>(ctJson);
                List<Track> trackList200 = JsonConvert.DeserializeObject<List<Track>>(ct200Json);
                List<Track> trackListNc = JsonConvert.DeserializeObject<List<Track>>(ctJson);
                List<Track> trackList200Nc = JsonConvert.DeserializeObject<List<Track>>(ct200Json);
                for (int i = 0; i < trackListNc.Count; i++)
                {
                    if (trackListNc[i].Category % 16 != 0)
                    {
                        trackListNc.RemoveAt(i);
                        i--;
                    }
                }
                for (int i = 0; i < trackList200Nc.Count; i++)
                {
                    if (trackList200Nc[i].Category % 16 != 0 || trackList200Nc[i].Category != 4)
                    {
                        trackList200Nc.RemoveAt(i);
                        i--;
                    }
                }

                string oldRtJson = File.ReadAllText("rts.json");
                List<Track> oldRtTrackList = JsonConvert.DeserializeObject<List<Track>>(oldRtJson);

                for (int i = 0; i < oldRtTrackList.Count; i++)
                {
                    trackListRt[i].WiimmfiName = oldRtTrackList[i].WiimmfiName;
                    trackListRt[i].WiimmfiScore = oldRtTrackList[i].WiimmfiScore;
                    trackListRt[i].BKTLink = oldRtTrackList[i].BKTLink;
                    trackListRt[i].BKTHolder = oldRtTrackList[i].BKTHolder;
                    trackListRt[i].CategoryName = oldRtTrackList[i].CategoryName;
                }

                oldRtJson = File.ReadAllText("rts200.json");
                oldRtTrackList = JsonConvert.DeserializeObject<List<Track>>(oldRtJson);

                for (int i = 0; i < oldRtTrackList.Count; i++)
                {
                    trackListRt200[i].WiimmfiName = oldRtTrackList[i].WiimmfiName;
                    trackListRt200[i].WiimmfiScore = oldRtTrackList[i].WiimmfiScore;
                    trackListRt200[i].BKTLink = oldRtTrackList[i].BKTLink;
                    trackListRt200[i].BKTHolder = oldRtTrackList[i].BKTHolder;
                    trackListRt200[i].CategoryName = oldRtTrackList[i].CategoryName;
                }

                string oldJson = File.ReadAllText("cts.json");
                List<Track> oldTrackList = JsonConvert.DeserializeObject<List<Track>>(oldJson);

                for (int i = 0; i < oldTrackList.Count; i++)
                {
                    trackList[i].WiimmfiName = oldTrackList[i].WiimmfiName;
                    trackList[i].WiimmfiScore = oldTrackList[i].WiimmfiScore;
                    trackList[i].BKTLink = oldTrackList[i].BKTLink;
                    trackList[i].BKTHolder = oldTrackList[i].BKTHolder;
                    trackList[i].CategoryName = oldTrackList[i].CategoryName;
                }

                oldJson = File.ReadAllText("cts200.json");
                oldTrackList = JsonConvert.DeserializeObject<List<Track>>(oldJson);

                for (int i = 0; i < oldTrackList.Count; i++)
                {
                    trackList200[i].WiimmfiName = oldTrackList[i].WiimmfiName;
                    trackList200[i].WiimmfiScore = oldTrackList[i].WiimmfiScore;
                    trackList200[i].BKTLink = oldTrackList[i].BKTLink;
                    trackList200[i].BKTHolder = oldTrackList[i].BKTHolder;
                    trackList200[i].CategoryName = oldTrackList[i].CategoryName;
                }

                try
                {
                    if (CompareIncompleteStrings(arg, "wiimmfi") || CompareIncompleteStrings(arg, "all"))
                    {
                        await Scraper.WiimmfiScrape(rtJson,
                            rt200Json,
                            ctwwDl1,
                            ctwwDl2,
                            ctwwDl3,
                            wwDl,
                            trackListRt,
                            trackListRt200,
                            trackList,
                            trackList200,
                            trackListNc,
                            trackList200Nc);
                    }
                }
                catch
                {
                    Thread.Sleep(300000);
                    await Scraper.WiimmfiScrape(rtJson,
                             rt200Json,
                             ctwwDl1,
                             ctwwDl2,
                             ctwwDl3,
                             wwDl,
                             trackListRt,
                             trackListRt200,
                             trackList,
                             trackList200,
                             trackListNc,
                             trackList200Nc);
                }

                try
                {
                    if (CompareIncompleteStrings(arg, "bkts") || CompareIncompleteStrings(arg, "all"))
                    {
                        await Scraper.GetBKTLeaderboards(trackListRt, trackListRt200, trackList, trackList200);
                    }
                }
                catch
                {
                    Thread.Sleep(300000);
                    await Scraper.GetBKTLeaderboards(trackListRt, trackListRt200, trackList, trackList200);
                }

                try
                {
                    await Scraper.GetSlotIds(trackListRt, trackListRt200, trackList, trackList200);
                }
                catch
                {
                    Thread.Sleep(300000);
                    await Scraper.GetSlotIds(trackListRt, trackListRt200, trackList, trackList200);
                }

                JsonSerializerSettings settings = new JsonSerializerSettings()
                {
                    DefaultValueHandling = DefaultValueHandling.Ignore
                };

                ctJson = JsonConvert.SerializeObject(trackList, settings);
                ct200Json = JsonConvert.SerializeObject(trackList200, settings);

                rtJson = JsonConvert.SerializeObject(trackListRt, settings);
                rt200Json = JsonConvert.SerializeObject(trackListRt200, settings);

                File.WriteAllText("rts.json", rtJson);
                File.WriteAllText("cts.json", ctJson);
                File.WriteAllText("rts200.json", rt200Json);
                File.WriteAllText("cts200.json", ct200Json);

                var today = DateTime.Now;
                File.WriteAllText("lastUpdated.txt", today.ToString());

                var processInfo = new ProcessStartInfo();
                processInfo.FileName = @"sudo";
                processInfo.Arguments = $"cp rts.json /var/www/brawlbox/";
                processInfo.CreateNoWindow = true;
                processInfo.WindowStyle = ProcessWindowStyle.Hidden;
                processInfo.UseShellExecute = false;
                processInfo.RedirectStandardOutput = true;

                var process = new Process();
                process.StartInfo = processInfo;
                process.Start();
                process.WaitForExit();

                processInfo = new ProcessStartInfo();
                processInfo.FileName = @"sudo";
                processInfo.Arguments = $"cp rts200.json /var/www/brawlbox/";
                processInfo.CreateNoWindow = true;
                processInfo.WindowStyle = ProcessWindowStyle.Hidden;
                processInfo.UseShellExecute = false;
                processInfo.RedirectStandardOutput = true;

                process = new Process();
                process.StartInfo = processInfo;
                process.Start();
                process.WaitForExit();

                processInfo = new ProcessStartInfo();
                processInfo.FileName = @"sudo";
                processInfo.Arguments = $"cp cts.json /var/www/brawlbox/";
                processInfo.CreateNoWindow = true;
                processInfo.WindowStyle = ProcessWindowStyle.Hidden;
                processInfo.UseShellExecute = false;
                processInfo.RedirectStandardOutput = true;

                process = new Process();
                process.StartInfo = processInfo;
                process.Start();
                process.WaitForExit();

                processInfo = new ProcessStartInfo();
                processInfo.FileName = @"sudo";
                processInfo.Arguments = $"cp cts200.json /var/www/brawlbox/";
                processInfo.CreateNoWindow = true;
                processInfo.WindowStyle = ProcessWindowStyle.Hidden;
                processInfo.UseShellExecute = false;
                processInfo.RedirectStandardOutput = true;

                process = new Process();
                process.StartInfo = processInfo;
                process.Start();
                process.WaitForExit();
            }

            catch (Exception ex)
            {
                var embed = new DiscordEmbedBuilder
                {
                    Color = new DiscordColor("#FF0000"),
                    Title = $"__**Error:**__",
                    Description = $"*{ex.Message}*" +
                              "\n**c!update**",
                    Footer = new DiscordEmbedBuilder.EmbedFooter
                    {
                        Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                    }
                };
                await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);

                Console.WriteLine(ex.ToString());
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
                    string json;
                    if (member == "reset" && ctx.Member.Roles.Select(x => x.Name == "Admin").Count() > 0)
                    {
                        using (var fs = File.OpenRead("council.json"))
                        using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                            json = await sr.ReadToEndAsync().ConfigureAwait(false);
                        List<CouncilMember> councilJson = JsonConvert.DeserializeObject<List<CouncilMember>>(json);

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
                        string description = string.Empty;
                        using (var fs = File.OpenRead("council.json"))
                        using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                            json = await sr.ReadToEndAsync().ConfigureAwait(false);
                        List<CouncilMember> councilJson = JsonConvert.DeserializeObject<List<CouncilMember>>(json);

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
                                ix = councilJson.FindIndex(x => CompareIncompleteStrings(x.SheetName, member) || CompareStringsLevenshteinDistance(x.SheetName, member));
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

                        int[] randomNums = new int[councilJson.Count];

                        for (int i = 0; i < councilJson.Count; i++)
                        {
                            randomNums[i] = i;
                        }

                        randomNums = randomNums.OrderBy(x => rng.Next()).ToArray();

                        if (councilJson.Count < 3)
                        {
                            for (int i = 0; i < councilJson.Count; i++)
                            {
                                assignedMembers.Add(councilJson[i]);
                                if (councilJson[i].AssignedThreadIds.Count == 3)
                                {
                                    councilJson[i].AssignedThreadIds.RemoveAt(0);
                                }
                                councilJson[i].AssignedThreadIds.Add(ctx.Channel.Id);
                            }
                            using (var fs = File.OpenRead("council.json"))
                            using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                                json = await sr.ReadToEndAsync().ConfigureAwait(false);
                            councilJson = JsonConvert.DeserializeObject<List<CouncilMember>>(json);
                            recentlyAssigned = new List<CouncilMember>();
                        }

                        for (int i = 0; i < 3; i++)
                        {
                            if (assignedMembers.Count == 3)
                            {
                                break;
                            }
                            assignedMembers.Add(councilJson[randomNums[i]]);
                            if (councilJson[randomNums[i]].AssignedThreadIds.Count == 3)
                            {
                                councilJson[randomNums[i]].AssignedThreadIds.RemoveAt(0);
                            }
                            councilJson[randomNums[i]].AssignedThreadIds.Add(ctx.Channel.Id);
                        }

                        if (assignedMembers.Count(x => x.CompPlayer == true) == 3)
                        {
                            for (int i = 2; i < councilJson.Count; i++)
                            {
                                if (!councilJson[i].CompPlayer)
                                {
                                    assignedMembers.Add(councilJson[i]);
                                    if (councilJson[i].AssignedThreadIds.Count == 3)
                                    {
                                        councilJson[i].AssignedThreadIds.RemoveAt(0);
                                    }
                                    councilJson[i].AssignedThreadIds.Add(ctx.Channel.Id);
                                    break;
                                }
                            }
                        }
                        else if (assignedMembers.Count(x => x.CompPlayer == false) == 3)
                        {
                            for (int i = 2; i < councilJson.Count; i++)
                            {
                                if (councilJson[randomNums[i]].CompPlayer)
                                {
                                    assignedMembers.Add(councilJson[randomNums[i]]);
                                    if (councilJson[randomNums[i]].AssignedThreadIds.Count == 3)
                                    {
                                        councilJson[randomNums[i]].AssignedThreadIds.RemoveAt(0);
                                    }
                                    councilJson[randomNums[i]].AssignedThreadIds.Add(ctx.Channel.Id);
                                    break;
                                }
                            }
                        }
                        else
                        {
                            assignedMembers.Add(councilJson[randomNums[2]]);
                        }

                        while (assignedMembers.Count < 5)
                        {
                            assignedMembers.Add(councilJson[randomNums[2]]);
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
                        string council = JsonConvert.SerializeObject(councilJson);
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

        [Command("checkmissedhw")]
        [RequireRoles(RoleCheckMode.Any, "Admin")]
        public async Task CheckHw(CommandContext ctx, [RemainingText] string placeholder)
        {
            await ctx.TriggerTypingAsync();
            var embed = new DiscordEmbedBuilder() { };
            try
            {
                if (ctx.Channel.Id == 217126063803727872 || ctx.Channel.Id == 750123394237726847 || ctx.Channel.Id == 935200150710808626 || ctx.Channel.Id == 946835035372257320 || ctx.Channel.Id == 751534710068477953)
                {
                    List<string> trackDisplay = new List<string>();
                    string json;

                    string serviceAccountEmail = "brawlbox@custom-track-testing-bot.iam.gserviceaccount.com";
                    var certificate = new X509Certificate2(@"key.p12", "notasecret", X509KeyStorageFlags.Exportable);
                    ServiceAccountCredential credential = new ServiceAccountCredential(
                       new ServiceAccountCredential.Initializer(serviceAccountEmail).FromCertificate(certificate));
                    var service = new SheetsService(new BaseClientService.Initializer()
                    {
                        HttpClientInitializer = credential,
                        ApplicationName = "Custom Track Testing Bot",
                    });

                    var temp = service.Spreadsheets.Values.Get("1I9yFsomTcvFT4hp6eN2azsfv6MsIy1897tBFX_gmtss", "'Track Evaluation Log'");
                    var tempResponse = await temp.ExecuteAsync();
                    var today = int.Parse(tempResponse.Values[tempResponse.Values.Count - 1][tempResponse.Values[tempResponse.Values.Count - 1].Count - 1].ToString());

                    var request = service.Spreadsheets.Values.Get("1I9yFsomTcvFT4hp6eN2azsfv6MsIy1897tBFX_gmtss", "'Track Evaluating'");
                    request.ValueRenderOption = SpreadsheetsResource.ValuesResource.GetRequest.ValueRenderOptionEnum.FORMULA;
                    var response = await request.ExecuteAsync();
                    foreach (var t in response.Values)
                    {
                        while (t.Count < response.Values[0].Count)
                        {
                            t.Add("");
                        }
                    }

                    using (var fs = File.OpenRead("council.json"))
                    using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                        json = await sr.ReadToEndAsync().ConfigureAwait(false);
                    List<CouncilMember> councilJson = JsonConvert.DeserializeObject<List<CouncilMember>>(json);

                    for (int i = 1; i < response.Values.Count; i++)
                    {
                        if (today == int.Parse(response.Values[i][1].ToString()))
                        {
                            trackDisplay.Add(response.Values[i][0].ToString());
                        }
                        if (lastHwDateChecked != int.Parse(response.Values[i][1].ToString()) && today > int.Parse(response.Values[i][1].ToString()))
                        {
                            lastHwDateChecked = int.Parse(response.Values[i][1].ToString());
                            for (int j = 12; j < response.Values[0].Count; j++)
                            {
                                bool check = false;
                                for (int k = 1; k < response.Values.Count; k++)
                                {
                                    if ((response.Values[k][j].ToString() == "" ||
                                        response.Values[k][j].ToString().ToLowerInvariant() == "yes" ||
                                        response.Values[k][j].ToString().ToLowerInvariant() == "no" ||
                                        response.Values[k][j].ToString().ToLowerInvariant() == "neutral" ||
                                        response.Values[k][j].ToString().ToLowerInvariant() == "fixes" ||
                                        !response.Values[k][j].ToString().ToLowerInvariant().Contains("yes") ||
                                        !response.Values[k][j].ToString().ToLowerInvariant().Contains("no") ||
                                        !response.Values[k][j].ToString().ToLowerInvariant().Contains("neutral") ||
                                        !response.Values[k][j].ToString().ToLowerInvariant().Contains("fixes")) &&
                                        response.Values[k][j].ToString() != "This member is the author thus cannot vote")
                                    {
                                        check = true;
                                    }
                                }
                                int ix = councilJson.FindIndex(x => x.SheetName == response.Values[0][j].ToString());
                                if (check)
                                {
                                    councilJson[ix].TimesMissedHw++;
                                    councilJson[ix].HwInARow = 0;
                                    if (councilJson[ix].TimesMissedHw > 0 && councilJson[ix].TimesMissedHw % 3 == 0)
                                    {
                                        string message = $"Hello {councilJson[ix].SheetName}. Just to let you know, you appear to have not completed council homework in a while, have been inconsistent with your homework, or are not completing it sufficiently enough. Just to remind you, if you miss homework too many times, admins might have to remove you from council. If you have an issue which stops you from doing homework, please let an admin know.";

                                        var members = ctx.Guild.GetAllMembersAsync();
                                        foreach (var member in members.Result)
                                        {
                                            if (member.Id == councilJson[ix].DiscordId)
                                            {
                                                try
                                                {
                                                    Console.WriteLine($"DM'd Member: {councilJson[ix].SheetName}");
                                                    await member.SendMessageAsync(message).ConfigureAwait(false);
                                                }
                                                catch (Exception ex)
                                                {
                                                    Console.WriteLine(ex.Message);
                                                    Console.WriteLine("DMs are likely closed.");
                                                }
                                            }
                                        }
                                    }
                                }
                                bool completed = true;
                                for (int k = 1; k < response.Values.Count; k++)
                                {
                                    if ((response.Values[k][j].ToString() == "" ||
                                        response.Values[k][j].ToString().ToLowerInvariant() == "yes" ||
                                        response.Values[k][j].ToString().ToLowerInvariant() == "no" ||
                                        response.Values[k][j].ToString().ToLowerInvariant() == "neutral" ||
                                        response.Values[k][j].ToString().ToLowerInvariant() == "fixes" ||
                                        !response.Values[k][j].ToString().ToLowerInvariant().Contains("yes") ||
                                        !response.Values[k][j].ToString().ToLowerInvariant().Contains("no") ||
                                        !response.Values[k][j].ToString().ToLowerInvariant().Contains("neutral") ||
                                        !response.Values[k][j].ToString().ToLowerInvariant().Contains("fixes")) &&
                                        response.Values[k][j].ToString() != "This member is the author thus cannot vote")
                                    {
                                        completed = false;
                                    }
                                }
                                if (completed)
                                {
                                    councilJson[ix].HwInARow++;
                                    if (councilJson[ix].HwInARow > 4)
                                    {
                                        councilJson[ix].TimesMissedHw = 0;
                                    }
                                }
                            }
                        }
                    }
                    embed = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = "__**Notice:**__",
                        Description = $"*Any missed homework has been recorded.*",
                        Footer = new DiscordEmbedBuilder.EmbedFooter
                        {
                            Text = $"Server Time: {DateTime.Now}"
                        }
                    };
                    string council = JsonConvert.SerializeObject(councilJson);
                    File.WriteAllText("council.json", council);
                    await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);

                    DiscordChannel channel = ctx.Channel;

                    foreach (var c in ctx.Guild.Channels)
                    {
                        if (c.Value.Id == 635313521487511554)
                        {
                            channel = c.Value;
                        }
                    }

                    string listOfTracks = "";
                    if (trackDisplay.Count > 0)
                    {
                        listOfTracks = trackDisplay[0];
                        for (int i = 1; i < trackDisplay.Count - 1; i++)
                        {
                            listOfTracks += $", {trackDisplay[i]}";
                        }
                        if (trackDisplay.Count == 1)
                        {
                            listOfTracks += " is";
                        }
                        else
                        {
                            listOfTracks += " are";
                        }

                        await channel.SendMessageAsync($"<@&608386209655554058> {listOfTracks} due for today.");
                    }
                }
            }
            catch (Exception ex)
            {
                embed = new DiscordEmbedBuilder
                {
                    Color = new DiscordColor("#FF0000"),
                    Title = $"__**Error:**__",
                    Description = $"*{ex.Message}*" +
                       "\n**c!checkmissedhw**",
                    Url = "https://docs.google.com/spreadsheets/d/1xwhKoyypCWq5tCRTI69ijJoDiaoAVsvYAxz-q4UBNqM/edit#gid=595190106",
                    Footer = new DiscordEmbedBuilder.EmbedFooter
                    {
                        Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                    }
                };
                await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);

                Console.WriteLine(ex.ToString());
            }
        }

        [Command("addmissedhw")]
        [RequireRoles(RoleCheckMode.Any, "Admin")]
        public async Task IncrementMissedHw(CommandContext ctx, [RemainingText] string member = "")
        {
            await ctx.TriggerTypingAsync();
            var embed = new DiscordEmbedBuilder() { };
            try
            {
                if (ctx.Channel.Id == 217126063803727872 || ctx.Channel.Id == 750123394237726847 || ctx.Channel.Id == 935200150710808626 || ctx.Channel.Id == 946835035372257320 || ctx.Channel.Id == 751534710068477953)
                {
                    string json;
                    using (var fs = File.OpenRead("council.json"))
                    using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                        json = await sr.ReadToEndAsync().ConfigureAwait(false);
                    List<CouncilMember> councilJson = JsonConvert.DeserializeObject<List<CouncilMember>>(json);

                    int ix = councilJson.FindIndex(x => CompareIncompleteStrings(x.SheetName, member) || CompareStringsLevenshteinDistance(x.SheetName, member));

                    if (ix != -1)
                    {
                        councilJson[ix].TimesMissedHw++;
                        embed = new DiscordEmbedBuilder
                        {
                            Color = new DiscordColor("#FF0000"),
                            Title = "__**Notice:**__",
                            Description = $"*Missed homework count for {councilJson[ix].SheetName} has been incremented.*",
                            Footer = new DiscordEmbedBuilder.EmbedFooter
                            {
                                Text = $"Server Time: {DateTime.Now}"
                            }
                        };
                        string council = JsonConvert.SerializeObject(councilJson);
                        File.WriteAllText("council.json", council);
                    }
                    else if (member == "")
                    {
                        embed = new DiscordEmbedBuilder
                        {
                            Color = new DiscordColor("#FF0000"),
                            Title = "__**Error:**__",
                            Description = $"*{member} was not inputted.*",
                            Footer = new DiscordEmbedBuilder.EmbedFooter
                            {
                                Text = $"Server Time: {DateTime.Now}"
                            }
                        };
                    }
                    else
                    {
                        embed = new DiscordEmbedBuilder
                        {
                            Color = new DiscordColor("#FF0000"),
                            Title = "__**Error:**__",
                            Description = $"*{member} could not be found.*",
                            Footer = new DiscordEmbedBuilder.EmbedFooter
                            {
                                Text = $"Server Time: {DateTime.Now}"
                            }
                        };
                    }
                    await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                embed = new DiscordEmbedBuilder
                {
                    Color = new DiscordColor("#FF0000"),
                    Title = $"__**Error:**__",
                    Description = $"*{ex.Message}*" +
                       "\n**c!resetmissedhw member**",
                    Url = "https://docs.google.com/spreadsheets/d/1xwhKoyypCWq5tCRTI69ijJoDiaoAVsvYAxz-q4UBNqM/edit#gid=595190106",
                    Footer = new DiscordEmbedBuilder.EmbedFooter
                    {
                        Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                    }
                };
                await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);

                Console.WriteLine(ex.ToString());
            }
        }

        [Command("removemissedhw")]
        [RequireRoles(RoleCheckMode.Any, "Admin")]
        public async Task DecrementMissedHw(CommandContext ctx, [RemainingText] string member = "")
        {
            await ctx.TriggerTypingAsync();
            var embed = new DiscordEmbedBuilder() { };
            try
            {
                if (ctx.Channel.Id == 217126063803727872 || ctx.Channel.Id == 750123394237726847 || ctx.Channel.Id == 935200150710808626 || ctx.Channel.Id == 946835035372257320 || ctx.Channel.Id == 751534710068477953)
                {
                    string json;
                    using (var fs = File.OpenRead("council.json"))
                    using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                        json = await sr.ReadToEndAsync().ConfigureAwait(false);
                    List<CouncilMember> councilJson = JsonConvert.DeserializeObject<List<CouncilMember>>(json);

                    int ix = councilJson.FindIndex(x => CompareIncompleteStrings(x.SheetName, member) || CompareStringsLevenshteinDistance(x.SheetName, member));

                    if (ix != -1)
                    {
                        councilJson[ix].TimesMissedHw--;
                        embed = new DiscordEmbedBuilder
                        {
                            Color = new DiscordColor("#FF0000"),
                            Title = "__**Notice:**__",
                            Description = $"*Missed homework count for {councilJson[ix].SheetName} has been decremented.*",
                            Footer = new DiscordEmbedBuilder.EmbedFooter
                            {
                                Text = $"Server Time: {DateTime.Now}"
                            }
                        };
                        string council = JsonConvert.SerializeObject(councilJson);
                        File.WriteAllText("council.json", council);
                    }
                    else if (member == "")
                    {
                        embed = new DiscordEmbedBuilder
                        {
                            Color = new DiscordColor("#FF0000"),
                            Title = "__**Error:**__",
                            Description = $"*{member} was not inputted.*",
                            Footer = new DiscordEmbedBuilder.EmbedFooter
                            {
                                Text = $"Server Time: {DateTime.Now}"
                            }
                        };
                    }
                    else
                    {
                        embed = new DiscordEmbedBuilder
                        {
                            Color = new DiscordColor("#FF0000"),
                            Title = "__**Error:**__",
                            Description = $"*{member} could not be found.*",
                            Footer = new DiscordEmbedBuilder.EmbedFooter
                            {
                                Text = $"Server Time: {DateTime.Now}"
                            }
                        };
                    }
                    await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                embed = new DiscordEmbedBuilder
                {
                    Color = new DiscordColor("#FF0000"),
                    Title = $"__**Error:**__",
                    Description = $"*{ex.Message}*" +
                       "\n**c!resetmissedhw member**",
                    Url = "https://docs.google.com/spreadsheets/d/1xwhKoyypCWq5tCRTI69ijJoDiaoAVsvYAxz-q4UBNqM/edit#gid=595190106",
                    Footer = new DiscordEmbedBuilder.EmbedFooter
                    {
                        Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                    }
                };
                await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);

                Console.WriteLine(ex.ToString());
            }
        }

        [Command("resetmissedhw")]
        [RequireRoles(RoleCheckMode.Any, "Admin")]
        public async Task ResetMissedHw(CommandContext ctx, [RemainingText] string member = "")
        {
            await ctx.TriggerTypingAsync();
            var embed = new DiscordEmbedBuilder() { };
            try
            {
                if (ctx.Channel.Id == 217126063803727872 || ctx.Channel.Id == 750123394237726847 || ctx.Channel.Id == 935200150710808626 || ctx.Channel.Id == 946835035372257320 || ctx.Channel.Id == 751534710068477953)
                {
                    string json;
                    using (var fs = File.OpenRead("council.json"))
                    using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                        json = await sr.ReadToEndAsync().ConfigureAwait(false);
                    List<CouncilMember> councilJson = JsonConvert.DeserializeObject<List<CouncilMember>>(json);

                    int ix = councilJson.FindIndex(x => CompareIncompleteStrings(x.SheetName, member) || CompareStringsLevenshteinDistance(x.SheetName, member));

                    if (ix != -1)
                    {
                        councilJson[ix].TimesMissedHw = 0;
                        embed = new DiscordEmbedBuilder
                        {
                            Color = new DiscordColor("#FF0000"),
                            Title = "__**Notice:**__",
                            Description = $"*Missed homework count for {councilJson[ix].SheetName} has been reset.*",
                            Footer = new DiscordEmbedBuilder.EmbedFooter
                            {
                                Text = $"Server Time: {DateTime.Now}"
                            }
                        };
                        string council = JsonConvert.SerializeObject(councilJson);
                        File.WriteAllText("council.json", council);
                    }
                    else if (member == "")
                    {
                        foreach (var m in councilJson)
                        {
                            m.TimesMissedHw = 0;
                        }
                        embed = new DiscordEmbedBuilder
                        {
                            Color = new DiscordColor("#FF0000"),
                            Title = "__**Notice:**__",
                            Description = "*Missed homework count for all members has been reset.*",
                            Footer = new DiscordEmbedBuilder.EmbedFooter
                            {
                                Text = $"Server Time: {DateTime.Now}"
                            }
                        };
                        string council = JsonConvert.SerializeObject(councilJson);
                        File.WriteAllText("council.json", council);
                    }
                    else
                    {
                        embed = new DiscordEmbedBuilder
                        {
                            Color = new DiscordColor("#FF0000"),
                            Title = "__**Error:**__",
                            Description = $"*{member} could not be found.*",
                            Footer = new DiscordEmbedBuilder.EmbedFooter
                            {
                                Text = $"Server Time: {DateTime.Now}"
                            }
                        };
                    }
                    await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                embed = new DiscordEmbedBuilder
                {
                    Color = new DiscordColor("#FF0000"),
                    Title = $"__**Error:**__",
                    Description = $"*{ex.Message}*" +
                       "\n**c!resetmissedhw member**",
                    Url = "https://docs.google.com/spreadsheets/d/1xwhKoyypCWq5tCRTI69ijJoDiaoAVsvYAxz-q4UBNqM/edit#gid=595190106",
                    Footer = new DiscordEmbedBuilder.EmbedFooter
                    {
                        Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                    }
                };
                await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);

                Console.WriteLine(ex.ToString());
            }
        }

        [Command("missedhw")]
        [RequireRoles(RoleCheckMode.Any, "Admin")]
        public async Task DisplayMissedHw(CommandContext ctx, [RemainingText] string placeholder)
        {
            await ctx.TriggerTypingAsync();
            try
            {
                if (ctx.Channel.Id == 217126063803727872 || ctx.Channel.Id == 750123394237726847 || ctx.Channel.Id == 935200150710808626 || ctx.Channel.Id == 946835035372257320 || ctx.Channel.Id == 751534710068477953)
                {
                    string json;
                    using (var fs = File.OpenRead("council.json"))
                    using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                        json = await sr.ReadToEndAsync().ConfigureAwait(false);
                    List<CouncilMember> councilJson = JsonConvert.DeserializeObject<List<CouncilMember>>(json);

                    string description = string.Empty;
                    foreach (var member in councilJson)
                    {
                        description += $"*{member.SheetName}: {member.TimesMissedHw}*\n";
                    }

                    var embed = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = $"__**Council Members Missed Homework Count:**__",
                        Description = description,
                        Footer = new DiscordEmbedBuilder.EmbedFooter
                        {
                            Text = $"Server Time: {DateTime.Now}"
                        }
                    };
                    await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                var embed = new DiscordEmbedBuilder
                {
                    Color = new DiscordColor("#FF0000"),
                    Title = $"__**Error:**__",
                    Description = $"*{ex.Message}*" +
                       "\n**c!missedhw**",
                    Url = "https://docs.google.com/spreadsheets/d/1xwhKoyypCWq5tCRTI69ijJoDiaoAVsvYAxz-q4UBNqM/edit#gid=595190106",
                    Footer = new DiscordEmbedBuilder.EmbedFooter
                    {
                        Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                    }
                };
                await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);

                Console.WriteLine(ex.ToString());
            }
        }

        [Command("starttimer")]
        [RequireRoles(RoleCheckMode.Any, "Admin")]
        public async Task StartTimers(CommandContext ctx, [RemainingText] string arg = "")
        {
            if (ctx.Guild.Id == 180306609233330176)
            {
                await ctx.TriggerTypingAsync();
                var embed = new DiscordEmbedBuilder() { };

                if (dailyTimer != null)
                {
                    dailyTimer.Stop();
                }
                dailyTimer = new System.Timers.Timer(86400000);
                dailyTimer.AutoReset = true;
                dailyTimer.Elapsed += async (s, e) => await Update(ctx, "all");
                dailyTimer.Elapsed += async (s, e) => await CheckHw(ctx, "");
                dailyTimer.Start();
                embed = new DiscordEmbedBuilder
                {
                    Color = new DiscordColor("#FF0000"),
                    Title = $"__**Notice:**__",
                    Description = "Timer has been started.",
                    Footer = new DiscordEmbedBuilder.EmbedFooter
                    {
                        Text = $"Server Time: {DateTime.Now}"
                    }
                };
                await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
            }
        }

        [Command("createtest")]
        [RequireRoles(RoleCheckMode.Any, "Admin")]
        public async Task CreateTestPack(CommandContext ctx, [RemainingText] string placeholder)
        {
            if (ctx.Guild.Id == 180306609233330176)
            {
                try
                {
                    await ctx.TriggerTypingAsync();

                    string serviceAccountEmail = "brawlbox@custom-track-testing-bot.iam.gserviceaccount.com";

                    var certificate = new X509Certificate2(@"key.p12", "notasecret", X509KeyStorageFlags.Exportable);

                    ServiceAccountCredential credential = new ServiceAccountCredential(
                       new ServiceAccountCredential.Initializer(serviceAccountEmail).FromCertificate(certificate));

                    var service = new SheetsService(new BaseClientService.Initializer()
                    {
                        HttpClientInitializer = credential,
                        ApplicationName = "Custom Track Testing Bot",
                    });

                    var request = service.Spreadsheets.Values.Get("19CF8UP1ubGfkM31uHha6bxYFENnQ-k7L7AEtozENnvk", "'Track Pack Downloads'");
                    var responseRaw = await request.ExecuteAsync();
                    request.ValueRenderOption = SpreadsheetsResource.ValuesResource.GetRequest.ValueRenderOptionEnum.FORMULA;
                    var response = await request.ExecuteAsync();

                    foreach (var v in response.Values)
                    {
                        while (v.Count < 10)
                        {
                            v.Add("");
                        }
                    }

                    string[] courseStrings = new[] { "Luigi Circuit",
            "Moo Moo Meadows",
            "Mushroom Gorge",
            "Toad's Factory",
            "Mario Circuit",
            "Coconut Mall",
            "DK's Snowboard Cross",
            "Wario's Gold Mine",
            "Daisy Circuit",
            "Koopa Cape",
            "Maple Treeway",
            "Grumble Volcano",
            "Dry Dry Ruins",
            "Moonview Highway",
            "Bowser Castle",
            "Rainbow Road",
            "GCN Peach Beach",
            "DS Yoshi Falls",
            "SNES Ghost Valley 2",
            "N64 Mario Raceway",
            "N64 Sherbet Land",
            "GBA Shy Guy Beach",
            "DS Delfino Square",
            "GCN Waluigi Stadium",
            "DS Desert Hills",
            "GBA Bowser Castle 3",
            "N64 DK's Jungle Parkway",
            "GCN Mario Circuit",
            "SNES Mario Circuit 3",
            "DS Peach Gardens",
            "GCN DK Mountain",
            "N64 Bowser's Castle",
            "Block Plaza",
            "Delfino Pier",
            "Funky Stadium",
            "Chain Chomp Wheel",
            "Thwomp Desert",
            "SNES Battle Course 4",
            "GBA Battle Course 3",
            "N64 Skyscraper",
            "GCN Cookie Land",
            "DS Twilight House" };
                    string[] courseMusicIds = new[] { "117",
            "119",
            "121",
            "123",
            "125",
            "127",
            "129",
            "131",
            "135",
            "133",
            "143",
            "139",
            "137",
            "141",
            "145",
            "147",
            "165",
            "173",
            "151",
            "159",
            "157",
            "149",
            "175",
            "169",
            "177",
            "155",
            "161",
            "167",
            "153",
            "179",
            "171",
            "163",
            "33",
            "32",
            "35",
            "34",
            "36",
            "39",
            "40",
            "41",
            "37",
            "38"};
                    string[] courseSlotIds = new[] { "T11",
            "T12",
            "T13",
            "T14",
            "T21",
            "T22",
            "T23",
            "T24",
            "T31",
            "T32",
            "T33",
            "T34",
            "T41",
            "T42",
            "T43",
            "T44",
            "T51",
            "T52",
            "T53",
            "T54",
            "T61",
            "T62",
            "T63",
            "T64",
            "T71",
            "T72",
            "T73",
            "T74",
            "T81",
            "T82",
            "T83",
            "T84" };

                    int[] trackSlots = new int[12];
                    int[] musicSlots = new int[12];

                    Directory.CreateDirectory(@"workdir/");
                    Directory.CreateDirectory(@"workdir/input");
                    Directory.CreateDirectory(@"workdir/output");
                    Directory.CreateDirectory(@"output/");
                    Directory.CreateDirectory(@"output/CTTP");
                    Directory.CreateDirectory(@"output/CTTP/rel");
                    Directory.CreateDirectory(@"output/CTTP/Scene");
                    Directory.CreateDirectory(@"output/CTTP/Scene/YourMom");

                    for (int i = 3; i < 15; i++)
                    {
                        WebClient webClient = new WebClient();
                        await webClient.DownloadFileTaskAsync(response.Values[i][5].ToString().Split('"')[1], $@"workdir/input/{response.Values[i][2]}.szs");
                        for (int j = 0; j < courseStrings.Length; j++)
                        {
                            if (response.Values[i][6].ToString().Split('/')[0].Remove(response.Values[i][6].ToString().Split('/')[0].Length - 1) == courseStrings[j])
                            {
                                trackSlots[i - 3] = j + 1;
                            }
                            if (response.Values[i][6].ToString().Split('/')[1].Substring(1) == courseStrings[j])
                            {
                                musicSlots[i - 3] = j + 1;
                            }
                            else if (musicSlots[i - 3] == 0)
                            {
                                musicSlots[i - 3] = trackSlots[i - 3];
                            }
                        }
                    }

                    List<CupInfo> cups = new List<CupInfo>(3);
                    for (int i = 0; i < 3; i++)
                    {
                        cups.Add(new CupInfo(response.Values[3 + i * 4][2].ToString(),
                            response.Values[4 + i * 4][2].ToString(),
                            response.Values[5 + i * 4][2].ToString(),
                            response.Values[6 + i * 4][2].ToString(),
                            response.Values[3 + i * 4][2].ToString(),
                            response.Values[4 + i * 4][2].ToString(),
                            response.Values[5 + i * 4][2].ToString(),
                            response.Values[6 + i * 4][2].ToString(),
                            trackSlots[0 + i * 4],
                            trackSlots[1 + i * 4],
                            trackSlots[2 + i * 4],
                            trackSlots[3 + i * 4],
                            musicSlots[0 + i * 4],
                            musicSlots[1 + i * 4],
                            musicSlots[2 + i * 4],
                            musicSlots[3 + i * 4]));
                    }

                    string fileName = @"workdir/config.txt";

                    if (File.Exists(fileName))
                    {
                        File.Delete(fileName);
                    }
                    using (StreamWriter streamwriter = File.CreateText(fileName))
                    {
                        streamwriter.WriteLine("#CT-CODE\n\n");
                        streamwriter.WriteLine("[RACING-TRACK-LIST]\n");
                        streamwriter.WriteLine("%LE-FLAGS  = 1\n");
                        streamwriter.WriteLine("%WIIMM-CUP = 0\n");
                        streamwriter.WriteLine("N N$SWAP | N$F_WII");

                        for (int i = 0; i < cups.Count; i++)
                        {
                            streamwriter.WriteLine($"\nC \"{i + 1}\" # {i + 12}");
                            streamwriter.WriteLine($"T {courseMusicIds[Convert.ToInt32(cups[i].Track1Music) - 1]}; {courseSlotIds[Convert.ToInt32(cups[i].Track1Slot) - 1]}; 0x00; \"{cups[i].Track1Name}\"; \"{cups[i].Track1Name.Replace("???", "secret")}\"; \"\"");
                            streamwriter.WriteLine($"T {courseMusicIds[Convert.ToInt32(cups[i].Track2Music) - 1]}; {courseSlotIds[Convert.ToInt32(cups[i].Track2Slot) - 1]}; 0x00; \"{cups[i].Track2Name}\"; \"{cups[i].Track2Name.Replace("???", "secret")}\"; \"\"");
                            streamwriter.WriteLine($"T {courseMusicIds[Convert.ToInt32(cups[i].Track3Music) - 1]}; {courseSlotIds[Convert.ToInt32(cups[i].Track3Slot) - 1]}; 0x00; \"{cups[i].Track3Name}\"; \"{cups[i].Track3Name.Replace("???", "secret")}\"; \"\"");
                            streamwriter.WriteLine($"T {courseMusicIds[Convert.ToInt32(cups[i].Track4Music) - 1]}; {courseSlotIds[Convert.ToInt32(cups[i].Track4Slot) - 1]}; 0x00; \"{cups[i].Track4Name}\"; \"{cups[i].Track4Name.Replace("???", "secret")}\"; \"\"");

                        }
                        streamwriter.Close();
                    }

                    File.WriteAllBytes(@"workdir/lecode-JAP.bin", Properties.Resources.lecode_JAP);
                    File.WriteAllBytes(@"workdir/lecode-USA.bin", Properties.Resources.lecode_USA);
                    File.WriteAllBytes(@"workdir/lecode-PAL.bin", Properties.Resources.lecode_PAL);

                    File.WriteAllBytes(@"workdir/cygattr-1.dll", Properties.Resources.cygattr_1);
                    File.WriteAllBytes(@"workdir/cygcrypto-1.1.dll", Properties.Resources.cygcrypto_1_1);
                    File.WriteAllBytes(@"workdir/cygiconv-2.dll", Properties.Resources.cygiconv_2);
                    File.WriteAllBytes(@"workdir/cygintl-8.dll", Properties.Resources.cygintl_8);
                    File.WriteAllBytes(@"workdir/cygncursesw-10.dll", Properties.Resources.cygncursesw_10);
                    File.WriteAllBytes(@"workdir/cygpcre-1.dll", Properties.Resources.cygpcre_1);
                    File.WriteAllBytes(@"workdir/cygpng16-16.dll", Properties.Resources.cygpng16_16);
                    File.WriteAllBytes(@"workdir/cygreadline7.dll", Properties.Resources.cygreadline7);
                    File.WriteAllBytes(@"workdir/cygwin1.dll", Properties.Resources.cygwin1);
                    File.WriteAllBytes(@"workdir/cygz.dll", Properties.Resources.cygz);

                    File.WriteAllBytes(@"workdir/MenuSingle_E_reg.szs", Properties.Resources.MenuSingle_E_reg);
                    File.WriteAllBytes(@"workdir/MenuSingle_E_mom.szs", Properties.Resources.MenuSingle_E_mom);

                    var processInfo = new ProcessStartInfo();
                    processInfo.FileName = @"wszst";
                    processInfo.Arguments = @"extract MenuSingle_E_reg.szs";
                    processInfo.WorkingDirectory = @"workdir/";
                    processInfo.CreateNoWindow = true;
                    processInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    processInfo.UseShellExecute = false;

                    var process = new Process();
                    process.StartInfo = processInfo;
                    process.Start();

                    process.WaitForExit();

                    processInfo = new ProcessStartInfo();
                    processInfo.FileName = @"wbmgt";
                    processInfo.Arguments = @"decode Common.bmg";
                    processInfo.WorkingDirectory = @"workdir/MenuSingle_E_reg.d/message/";
                    processInfo.CreateNoWindow = true;
                    processInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    processInfo.UseShellExecute = false;

                    process = new Process();
                    process.StartInfo = processInfo;
                    process.Start();

                    process.WaitForExit();

                    StreamWriter sw = new StreamWriter(@"workdir/MenuSingle_E_reg.d/message/Common.txt", true);
                    sw.WriteLine("\n\n#--- [7000:7fff] LE-CODE: track names");
                    sw.WriteLine(@"  7000	= \c{blue1}Wii \c{off}Mario Circuit");
                    sw.WriteLine(@"  7001	= \c{blue1}Wii \c{off}Moo Moo Meadows");
                    sw.WriteLine(@"  7002	= \c{blue1}Wii \c{off}Mushroom Gorge");
                    sw.WriteLine(@"  7003	= \c{blue1}Wii \c{off}Grumble Volcano");
                    sw.WriteLine(@"  7004	= \c{blue1}Wii \c{off}Toad's Factory");
                    sw.WriteLine(@"  7005	= \c{blue1}Wii \c{off}Coconut Mall");
                    sw.WriteLine(@"  7006	= \c{blue1}Wii \c{off}DK's Snowboard Cross");
                    sw.WriteLine(@"  7007	= \c{blue1}Wii \c{off}Wario's Gold Mine");
                    sw.WriteLine(@"  7008	= \c{blue1}Wii \c{off}Luigi Circuit");
                    sw.WriteLine(@"  7009	= \c{blue1}Wii \c{off}Daisy Circuit");
                    sw.WriteLine(@"  700a	= \c{blue1}Wii \c{off}Moonview Highway");
                    sw.WriteLine(@"  700b	= \c{blue1}Wii \c{off}Maple Treeway");
                    sw.WriteLine(@"  700c	= \c{blue1}Wii \c{off}Bowser's Castle");
                    sw.WriteLine(@"  700d	= \c{blue1}Wii \c{off}Rainbow Road");
                    sw.WriteLine(@"  700e	= \c{blue1}Wii \c{off}Dry Dry Ruins");
                    sw.WriteLine(@"  700f	= \c{blue1}Wii \c{off}Koopa Cape");
                    sw.WriteLine(@"  7010	= \c{blue1}GCN \c{off}Peach Beach");
                    sw.WriteLine(@"  7011	= \c{blue1}GCN \c{off}Mario Circuit");
                    sw.WriteLine(@"  7012	= \c{blue1}GCN \c{off}Waluigi Stadium");
                    sw.WriteLine(@"  7013	= \c{blue1}GCN \c{off}DK Mountain");
                    sw.WriteLine(@"  7014	= \c{blue1}DS \c{off}Yoshi Falls");
                    sw.WriteLine(@"  7015	= \c{blue1}DS \c{off}Desert Hills");
                    sw.WriteLine(@"  7016	= \c{blue1}DS \c{off}Peach Gardens");
                    sw.WriteLine(@"  7017	= \c{blue1}DS \c{off}Delfino Square");
                    sw.WriteLine(@"  7018	= \c{blue1}SNES \c{off}Mario Circuit 3");
                    sw.WriteLine(@"  7019	= \c{blue1}SNES \c{off}Ghost Valley 2");
                    sw.WriteLine(@"  701a	= \c{blue1}N64 \c{off}Mario Raceway");
                    sw.WriteLine(@"  701b	= \c{blue1}N64 \c{off}Sherbet Land");
                    sw.WriteLine(@"  701c	= \c{blue1}N64 \c{off}Bowser's Castle");
                    sw.WriteLine(@"  701d	= \c{blue1}N64 \c{off}DK's Jungle Parkway");
                    sw.WriteLine(@"  701e	= \c{blue1}GBA \c{off}Bowser Castle 3");
                    sw.WriteLine(@"  701f	= \c{blue1}GBA \c{off}Shy Guy Beach");
                    sw.WriteLine(@"  7020	= \c{yor7}-----\c{off}");
                    sw.WriteLine(@"  7021	= \c{yor7}-----\c{off}");
                    sw.WriteLine(@"  7022	= \c{yor7}-----\c{off}");
                    sw.WriteLine(@"  7023	= \c{yor7}-----\c{off}");
                    sw.WriteLine(@"  7024	= \c{yor7}-----\c{off}");
                    sw.WriteLine(@"  7025	= \c{yor7}-----\c{off}");
                    sw.WriteLine(@"  7026	= \c{yor7}-----\c{off}");
                    sw.WriteLine(@"  7027	= \c{yor7}-----\c{off}");
                    sw.WriteLine(@"  7028	= \c{yor7}-----\c{off}");
                    sw.WriteLine(@"  7029	= \c{yor7}-----\c{off}");
                    sw.WriteLine(@"  702a	/");
                    sw.WriteLine(@"  702b	/");
                    sw.WriteLine(@"  702c	/");
                    sw.WriteLine(@"  702d	/");
                    sw.WriteLine(@"  702e	/");
                    sw.WriteLine(@"  702f	/");
                    sw.WriteLine(@"  7030	/");
                    sw.WriteLine(@"  7031	/");
                    sw.WriteLine(@"  7032	/");
                    sw.WriteLine(@"  7033	/");
                    sw.WriteLine(@"  7034	/");
                    sw.WriteLine(@"  7035	/");
                    sw.WriteLine(@"  7036	= Ring Mission");
                    sw.WriteLine(@"  7037	= Winningrun Demo");
                    sw.WriteLine(@"  7038	= Loser Demo");
                    sw.WriteLine(@"  7039	= Draw Demo");
                    sw.WriteLine(@"  703a	= Ending Demo");
                    sw.WriteLine(@"  703b	/");
                    sw.WriteLine(@"  703c	/");
                    sw.WriteLine(@"  703d	/");
                    sw.WriteLine(@"  703e	/");
                    sw.WriteLine(@"  703f	/");
                    sw.WriteLine(@"  7040	/");
                    sw.WriteLine(@"  7041	/");
                    sw.WriteLine(@"  7042	/");
                    sw.WriteLine(@"  7043	=  Wiimms SZS Toolset v2.25a r8443");
                    for (int i = 0; i < cups.Count; i++)
                    {
                        sw.WriteLine($"  {28740 + i * 4:X}	= {cups[i].Track1BMG}");
                        sw.WriteLine($"  {28741 + i * 4:X}	= {cups[i].Track2BMG}");
                        sw.WriteLine($"  {28742 + i * 4:X}	= {cups[i].Track3BMG}");
                        sw.WriteLine($"  {28743 + i * 4:X}	= {cups[i].Track4BMG}");
                    }
                    sw.WriteLine(@"");
                    sw.WriteLine(@" 18697	/");
                    sw.WriteLine(@" 18698	/");
                    sw.WriteLine(@" 18699	/");
                    sw.WriteLine(@" 1869a	/");
                    sw.WriteLine(@" 1869b	/");
                    sw.WriteLine(@" 1869c	/");
                    sw.WriteLine(@" 1869d	/");
                    sw.WriteLine(@" 1869e	/");
                    sw.Close();

                    processInfo = new ProcessStartInfo();
                    processInfo.FileName = @"wbmgt";
                    processInfo.Arguments = @"encode Common.txt -o";
                    processInfo.WorkingDirectory = @"workdir/MenuSingle_E_reg.d/message/";
                    processInfo.CreateNoWindow = true;
                    processInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    processInfo.UseShellExecute = false;

                    process = new Process();
                    process.StartInfo = processInfo;
                    process.Start();

                    process.WaitForExit();

                    processInfo = new ProcessStartInfo();
                    processInfo.FileName = @"wszst";
                    processInfo.Arguments = @"create MenuSingle_E_reg.d -o";
                    processInfo.WorkingDirectory = @"workdir/";
                    processInfo.CreateNoWindow = true;
                    processInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    processInfo.UseShellExecute = false;

                    process = new Process();
                    process.StartInfo = processInfo;
                    process.Start();

                    process.WaitForExit();

                    processInfo = new ProcessStartInfo();
                    processInfo.FileName = @"wszst";
                    processInfo.Arguments = @"extract MenuSingle_E_mom.szs";
                    processInfo.WorkingDirectory = @"workdir/";
                    processInfo.CreateNoWindow = true;
                    processInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    processInfo.UseShellExecute = false;

                    process = new Process();
                    process.StartInfo = processInfo;
                    process.Start();

                    process.WaitForExit();

                    processInfo = new ProcessStartInfo();
                    processInfo.FileName = @"wbmgt";
                    processInfo.Arguments = @"decode Common.bmg";
                    processInfo.WorkingDirectory = @"workdir/MenuSingle_E_mom.d/message/";
                    processInfo.CreateNoWindow = true;
                    processInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    processInfo.UseShellExecute = false;

                    process = new Process();
                    process.StartInfo = processInfo;
                    process.Start();

                    process.WaitForExit();

                    sw = new StreamWriter(@"workdir/MenuSingle_E_mom.d/message/Common.txt", true);
                    sw.WriteLine("\n\n#--- [7000:7fff] LE-CODE: track names");
                    sw.WriteLine(@"  7000	= \c{blue1}Wii \c{off}Mario Circuit");
                    sw.WriteLine(@"  7001	= \c{blue1}Wii \c{off}Moo Moo Meadows");
                    sw.WriteLine(@"  7002	= \c{blue1}Wii \c{off}Mushroom Gorge");
                    sw.WriteLine(@"  7003	= \c{blue1}Wii \c{off}Grumble Volcano");
                    sw.WriteLine(@"  7004	= \c{blue1}Wii \c{off}Toad's Factory");
                    sw.WriteLine(@"  7005	= \c{blue1}Wii \c{off}Coconut Mall");
                    sw.WriteLine(@"  7006	= \c{blue1}Wii \c{off}DK's Snowboard Cross");
                    sw.WriteLine(@"  7007	= \c{blue1}Wii \c{off}Wario's Gold Mine");
                    sw.WriteLine(@"  7008	= \c{blue1}Wii \c{off}Luigi Circuit");
                    sw.WriteLine(@"  7009	= \c{blue1}Wii \c{off}Daisy Circuit");
                    sw.WriteLine(@"  700a	= \c{blue1}Wii \c{off}Moonview Highway");
                    sw.WriteLine(@"  700b	= \c{blue1}Wii \c{off}Maple Treeway");
                    sw.WriteLine(@"  700c	= \c{blue1}Wii \c{off}Bowser's Castle");
                    sw.WriteLine(@"  700d	= \c{blue1}Wii \c{off}Rainbow Road");
                    sw.WriteLine(@"  700e	= \c{blue1}Wii \c{off}Dry Dry Ruins");
                    sw.WriteLine(@"  700f	= \c{blue1}Wii \c{off}Koopa Cape");
                    sw.WriteLine(@"  7010	= \c{blue1}GCN \c{off}Peach Beach");
                    sw.WriteLine(@"  7011	= \c{blue1}GCN \c{off}Mario Circuit");
                    sw.WriteLine(@"  7012	= \c{blue1}GCN \c{off}Waluigi Stadium");
                    sw.WriteLine(@"  7013	= \c{blue1}GCN \c{off}DK Mountain");
                    sw.WriteLine(@"  7014	= \c{blue1}DS \c{off}Yoshi Falls");
                    sw.WriteLine(@"  7015	= \c{blue1}DS \c{off}Desert Hills");
                    sw.WriteLine(@"  7016	= \c{blue1}DS \c{off}Peach Gardens");
                    sw.WriteLine(@"  7017	= \c{blue1}DS \c{off}Delfino Square");
                    sw.WriteLine(@"  7018	= \c{blue1}SNES \c{off}Mario Circuit 3");
                    sw.WriteLine(@"  7019	= \c{blue1}SNES \c{off}Ghost Valley 2");
                    sw.WriteLine(@"  701a	= \c{blue1}N64 \c{off}Mario Raceway");
                    sw.WriteLine(@"  701b	= \c{blue1}N64 \c{off}Sherbet Land");
                    sw.WriteLine(@"  701c	= \c{blue1}N64 \c{off}Bowser's Castle");
                    sw.WriteLine(@"  701d	= \c{blue1}N64 \c{off}DK's Jungle Parkway");
                    sw.WriteLine(@"  701e	= \c{blue1}GBA \c{off}Bowser Castle 3");
                    sw.WriteLine(@"  701f	= \c{blue1}GBA \c{off}Shy Guy Beach");
                    sw.WriteLine(@"  7020	= \c{yor7}-----\c{off}");
                    sw.WriteLine(@"  7021	= \c{yor7}-----\c{off}");
                    sw.WriteLine(@"  7022	= \c{yor7}-----\c{off}");
                    sw.WriteLine(@"  7023	= \c{yor7}-----\c{off}");
                    sw.WriteLine(@"  7024	= \c{yor7}-----\c{off}");
                    sw.WriteLine(@"  7025	= \c{yor7}-----\c{off}");
                    sw.WriteLine(@"  7026	= \c{yor7}-----\c{off}");
                    sw.WriteLine(@"  7027	= \c{yor7}-----\c{off}");
                    sw.WriteLine(@"  7028	= \c{yor7}-----\c{off}");
                    sw.WriteLine(@"  7029	= \c{yor7}-----\c{off}");
                    sw.WriteLine(@"  702a	/");
                    sw.WriteLine(@"  702b	/");
                    sw.WriteLine(@"  702c	/");
                    sw.WriteLine(@"  702d	/");
                    sw.WriteLine(@"  702e	/");
                    sw.WriteLine(@"  702f	/");
                    sw.WriteLine(@"  7030	/");
                    sw.WriteLine(@"  7031	/");
                    sw.WriteLine(@"  7032	/");
                    sw.WriteLine(@"  7033	/");
                    sw.WriteLine(@"  7034	/");
                    sw.WriteLine(@"  7035	/");
                    sw.WriteLine(@"  7036	= Ring Mission");
                    sw.WriteLine(@"  7037	= Winningrun Demo");
                    sw.WriteLine(@"  7038	= Loser Demo");
                    sw.WriteLine(@"  7039	= Draw Demo");
                    sw.WriteLine(@"  703a	= Ending Demo");
                    sw.WriteLine(@"  703b	/");
                    sw.WriteLine(@"  703c	/");
                    sw.WriteLine(@"  703d	/");
                    sw.WriteLine(@"  703e	/");
                    sw.WriteLine(@"  703f	/");
                    sw.WriteLine(@"  7040	/");
                    sw.WriteLine(@"  7041	/");
                    sw.WriteLine(@"  7042	/");
                    sw.WriteLine(@"  7043	=  Wiimms SZS Toolset v2.25a r8443");
                    for (int i = 0; i < cups.Count; i++)
                    {
                        sw.WriteLine($"  {28740 + i * 4:X}	= {cups[i].Track1BMG}");
                        sw.WriteLine($"  {28741 + i * 4:X}	= {cups[i].Track2BMG}");
                        sw.WriteLine($"  {28742 + i * 4:X}	= {cups[i].Track3BMG}");
                        sw.WriteLine($"  {28743 + i * 4:X}	= {(cups[i].Track4BMG.ToLowerInvariant() == "secret" ? "???" : cups[i].Track4BMG)}");
                    }
                    sw.WriteLine(@"");
                    sw.WriteLine(@" 18697	/");
                    sw.WriteLine(@" 18698	/");
                    sw.WriteLine(@" 18699	/");
                    sw.WriteLine(@" 1869a	/");
                    sw.WriteLine(@" 1869b	/");
                    sw.WriteLine(@" 1869c	/");
                    sw.WriteLine(@" 1869d	/");
                    sw.WriteLine(@" 1869e	/");
                    sw.Close();

                    processInfo = new ProcessStartInfo();
                    processInfo.FileName = @"wbmgt";
                    processInfo.Arguments = @"encode Common.txt -o";
                    processInfo.WorkingDirectory = @"workdir/MenuSingle_E_mom.d/message/";
                    processInfo.CreateNoWindow = true;
                    processInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    processInfo.UseShellExecute = false;

                    process = new Process();
                    process.StartInfo = processInfo;
                    process.Start();

                    process.WaitForExit();

                    processInfo = new ProcessStartInfo();
                    processInfo.FileName = @"wszst";
                    processInfo.Arguments = @"create MenuSingle_E_mom.d -o";
                    processInfo.WorkingDirectory = @"workdir/";
                    processInfo.CreateNoWindow = true;
                    processInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    processInfo.UseShellExecute = false;

                    process = new Process();
                    process.StartInfo = processInfo;
                    process.Start();

                    process.WaitForExit();

                    processInfo = new ProcessStartInfo();
                    processInfo.FileName = @"wlect";
                    processInfo.Arguments = @"patch lecode-PAL.bin --le-define config.txt --custom-tt -o";
                    processInfo.WorkingDirectory = @"workdir/";
                    processInfo.CreateNoWindow = true;
                    processInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    processInfo.UseShellExecute = false;

                    process = new Process();
                    process.StartInfo = processInfo;
                    process.Start();

                    process.WaitForExit();

                    processInfo = new ProcessStartInfo();
                    processInfo.FileName = @"wlect";
                    processInfo.Arguments = @"patch lecode-USA.bin --le-define config.txt --custom-tt -o";
                    processInfo.WorkingDirectory = @"workdir/";
                    processInfo.CreateNoWindow = true;
                    processInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    processInfo.UseShellExecute = false;

                    process = new Process();
                    process.StartInfo = processInfo;
                    process.Start();

                    process.WaitForExit();

                    processInfo = new ProcessStartInfo();
                    processInfo.FileName = @"wlect";
                    processInfo.Arguments = @"patch lecode-JAP.bin --le-define config.txt --custom-tt -o";
                    processInfo.WorkingDirectory = @"workdir/";
                    processInfo.CreateNoWindow = true;
                    processInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    processInfo.UseShellExecute = false;

                    process = new Process();
                    process.StartInfo = processInfo;
                    process.Start();

                    process.WaitForExit();

                    processInfo = new ProcessStartInfo();
                    processInfo.FileName = @"wlect";
                    processInfo.Arguments = @"patch lecode-PAL.bin -od lecode-PAL.bin --le-define config.txt --track-dir output --copy-tracks input";
                    processInfo.WorkingDirectory = @"workdir/";
                    processInfo.CreateNoWindow = true;
                    processInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    processInfo.UseShellExecute = false;
                    processInfo.RedirectStandardOutput = true;

                    process = new Process();
                    process.StartInfo = processInfo;
                    process.Start();
                    process.WaitForExit();

                    Directory.Move(@"workdir/output", @"output/CTTP/Course");
                    File.Move(@"workdir/MenuSingle_E_reg.szs", @"output/CTTP/Scene/MenuSingle_E.szs");
                    File.Move(@"workdir/MenuSingle_E_mom.szs", @"output/CTTP/Scene/YourMom/MenuSingle_E.szs");
                    File.Move(@"workdir/lecode-PAL.bin", @"output/CTTP/rel/lecode-PAL.bin");
                    File.Move(@"workdir/lecode-USA.bin", @"output/CTTP/rel/lecode-USA.bin");
                    File.Move(@"workdir/lecode-JAP.bin", @"output/CTTP/rel/lecode-JAP.bin");
                    Directory.Delete(@"workdir/", true);

                    string dateString = responseRaw.Values[1][1].ToString();
                    var date = DateTime.Parse(dateString);

                    ZipFile.CreateFromDirectory(@"output/", $"{(date.Year + "-" + date.Month + "-" + date.Day).Replace(",", string.Empty).Replace(' ', '_')}_Track_Test.zip");

                    /*
                    using (var fs = new FileStream($"{(date.Year + "-" + date.Month + "-" + date.Day).Replace(",", string.Empty).Replace(' ', '_')}_Track_Test.zip", FileMode.Open, FileAccess.Read))
                    {
                        await ctx.TriggerTypingAsync();
                        var msg = await new DiscordMessageBuilder()
                            .WithFiles(new Dictionary<string, Stream>() { { $"{(date.Year + "-" + date.Month + "-" + date.Day).Replace(",", string.Empty).Replace(' ', '_')}_Track_Test.zip", fs } })
                            .SendAsync(ctx.Channel);
                    }
                    */

                    processInfo = new ProcessStartInfo();
                    processInfo.FileName = @"sudo";
                    processInfo.Arguments = $"cp '{(date.Year + "-" + date.Month + "-" + date.Day).Replace(",", string.Empty).Replace(' ', '_')}_Track_Test.zip' /var/www/brawlbox/";
                    processInfo.CreateNoWindow = true;
                    processInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    processInfo.UseShellExecute = false;
                    processInfo.RedirectStandardOutput = true;

                    process = new Process();
                    process.StartInfo = processInfo;
                    process.Start();
                    process.WaitForExit();

                    await ctx.Channel.SendMessageAsync($"https://brawlbox.xyz/{(date.Year + "-" + date.Month + "-" + date.Day).Replace(",", string.Empty).Replace(' ', '_')}_Track_Test.zip");

                    File.Delete($"{(date.Year + "-" + date.Month + "-" + date.Day).Replace(",", string.Empty).Replace(' ', '_')}_Track_Test.zip");
                    Directory.Delete(@"output/", true);
                }
                catch (Exception ex)
                {
                    var embed = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = "__**Error:**__",
                        Description = $"*{ex.Message}*" +
                            "\n**c!createtest**",
                        Footer = new DiscordEmbedBuilder.EmbedFooter
                        {
                            Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                        }
                    };
                    await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);

                    Console.WriteLine(ex.ToString());

                    if (Directory.Exists(@"workdir/"))
                    {
                        Directory.Delete(@"workdir/", true);
                    }

                    if (Directory.Exists(@"output/"))
                    {
                        Directory.Delete(@"output/", true);
                    }
                }
            }
        }

        [Command("dmrole")]
        [RequireRoles(RoleCheckMode.Any, "Admin")]
        public async Task DMRole(CommandContext ctx, string role = "", [RemainingText] string message = "")
        {
            if (ctx.Guild.Id == 180306609233330176)
            {
                if (ctx.Channel.Id == 751534710068477953)
                {
                    try
                    {
                        DiscordRole discordRole = null;
                        var embed = new DiscordEmbedBuilder() { };
                        foreach (var r in ctx.Guild.Roles.Values)
                        {
                            if (r.Id.ToString() == role.Replace("<@&", string.Empty).Replace(">", string.Empty))
                            {
                                discordRole = r;
                            }
                        }
                        if (discordRole == null)
                        {
                            embed = new DiscordEmbedBuilder
                            {
                                Color = new DiscordColor("#FF0000"),
                                Title = "__**Error:**__",
                                Description = $"*{role} could not be found in the server.*" +
                                "\n**c!dmrole role message**",
                                Footer = new DiscordEmbedBuilder.EmbedFooter
                                {
                                    Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                                }
                            };
                            await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
                        }
                        else if (message == "")
                        {
                            embed = new DiscordEmbedBuilder
                            {
                                Color = new DiscordColor("#FF0000"),
                                Title = "__**Error:**__",
                                Description = $"*Message was empty.*" +
                                "\n**c!dmrole role message**",
                                Footer = new DiscordEmbedBuilder.EmbedFooter
                                {
                                    Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                                }
                            };
                            await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
                        }
                        else
                        {
                            var members = ctx.Guild.GetAllMembersAsync();
                            foreach (var member in members.Result)
                            {
                                foreach (var r in member.Roles)
                                {
                                    if (r == discordRole)
                                    {
                                        try
                                        {
                                            await member.SendMessageAsync(message).ConfigureAwait(false);
                                        }
                                        catch (Exception ex)
                                        {
                                            Console.WriteLine(ex.Message);
                                            Console.WriteLine("DMs are likely closed.");
                                        }
                                    }
                                }
                            }

                            embed = new DiscordEmbedBuilder
                            {
                                Color = new DiscordColor("#FF0000"),
                                Title = "__**Success:**__",
                                Description = $"*Message was sent to {role} successfully.*",
                                Footer = new DiscordEmbedBuilder.EmbedFooter
                                {
                                    Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                                }
                            };
                            await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
                        }
                    }
                    catch (Exception ex)
                    {
                        var embed = new DiscordEmbedBuilder
                        {
                            Color = new DiscordColor("#FF0000"),
                            Title = "__**Error:**__",
                            Description = $"*{ex.Message}*" +
                                "\n**c!dmrole role message**",
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

        [Command("dlbkt")]
        public async Task GetBKTs(CommandContext ctx, [RemainingText] string arg = "")
        {
            var embed = new DiscordEmbedBuilder() { };
            var time = DateTime.Now.ToString();

            try
            {
                await ctx.Channel.TriggerTypingAsync();

                Directory.CreateDirectory("rkgs");
                Directory.CreateDirectory("rkgs/150");
                Directory.CreateDirectory("rkgs/200");

                string json = string.Empty;
                List<Track> trackList = new List<Track>();
                List<Track> trackList200 = new List<Track>();

                if (CompareStrings(arg, "rts"))
                {
                    json = File.ReadAllText($"rts.json");
                    foreach (var t in JsonConvert.DeserializeObject<List<Track>>(json))
                    {
                        trackList.Add(t);
                    }
                    json = File.ReadAllText($"rts200.json");
                    foreach (var t in JsonConvert.DeserializeObject<List<Track>>(json))
                    {
                        trackList200.Add(t);
                    }
                }

                else if (CompareStrings(arg, "cts"))
                {
                    json = File.ReadAllText($"cts.json");
                    foreach (var t in JsonConvert.DeserializeObject<List<Track>>(json))
                    {
                        trackList.Add(t);
                    }
                    json = File.ReadAllText($"cts200.json");
                    foreach (var t in JsonConvert.DeserializeObject<List<Track>>(json))
                    {
                        trackList200.Add(t);
                    }
                }

                else
                {
                    json = File.ReadAllText($"rts.json");
                    foreach (var t in JsonConvert.DeserializeObject<List<Track>>(json))
                    {
                        trackList.Add(t);
                    }
                    json = File.ReadAllText($"rts200.json");
                    foreach (var t in JsonConvert.DeserializeObject<List<Track>>(json))
                    {
                        trackList200.Add(t);
                    }
                    json = File.ReadAllText($"cts.json");
                    foreach (var t in JsonConvert.DeserializeObject<List<Track>>(json))
                    {
                        trackList.Add(t);
                    }
                    json = File.ReadAllText($"cts200.json");
                    foreach (var t in JsonConvert.DeserializeObject<List<Track>>(json))
                    {
                        trackList200.Add(t);
                    }
                }

                if (arg == "")
                {
                    embed = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = $"__**Error:**__",
                        Description = "*Argument was not inputted.*" +
                           "\n**c!dlbkt track/all**",
                        Url = "https://chadsoft.co.uk/time-trials/",
                        Footer = new DiscordEmbedBuilder.EmbedFooter
                        {
                            Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                        }
                    };
                    await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
                }
                else if (CompareStrings(arg, "all") ||
                    CompareStrings(arg, "rts") ||
                    CompareStrings(arg, "cts"))
                {
                    Parallel.ForEach(trackList, track =>
                    {
                        Task.WaitAll(Scraper.Dl150ccBKT(track));
                    });

                    Parallel.ForEach(trackList200, track =>
                    {
                        Task.WaitAll(Scraper.Dl200ccBKT(track));
                    });

                    ZipFile.CreateFromDirectory(@"rkgs",
                        $"All BKT RKGs - {String.Join("", time.Split('\\', '/', ':', '*', '?', '"', '<', '>', '|'))}.zip");
                    using (var fs = new FileStream($"All BKT RKGs - {String.Join("", time.Split('\\', '/', ':', '*', '?', '"', '<', '>', '|'))}.zip", FileMode.Open, FileAccess.Read))
                    {
                        var msg = await new DiscordMessageBuilder()
                            .WithFiles(new Dictionary<string, Stream>() { { $"{arg.ToUpperInvariant().Split('S')[0]} BKT RKGs - {String.Join("", time.Split('\\', '/', ':', '*', '?', '"', '<', '>', '|'))}.zip", fs } })
                            .SendAsync(ctx.Channel);
                    }
                    Directory.Delete("rkgs", true);
                    File.Delete($"All BKT RKGs - {String.Join("", time.Split('\\', '/', ':', '*', '?', '"', '<', '>', '|'))}.zip");
                }
                else
                {
                    List<Track> trackDisplay = new List<Track>();
                    List<Track> trackDisplay200 = new List<Track>();

                    foreach (var track in trackList)
                    {
                        if (CompareIncompleteStrings(track.Name, arg))
                        {
                            trackDisplay.Add(track);
                        }
                    }

                    foreach (var track in trackList200)
                    {
                        if (CompareIncompleteStrings(track.Name, arg))
                        {
                            trackDisplay200.Add(track);
                        }
                    }

                    if (trackDisplay.Count < 1)
                    {
                        embed = new DiscordEmbedBuilder
                        {
                            Color = new DiscordColor("#FF0000"),
                            Title = $"__**Error:**__",
                            Description = $"*{arg} could not be found.*" +
                               "\n**c!dlbkt track/all**",
                            Url = "https://chadsoft.co.uk/time-trials/",
                            Footer = new DiscordEmbedBuilder.EmbedFooter
                            {
                                Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                            }
                        };
                        await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
                    }
                    else
                    {
                        Parallel.ForEach(trackDisplay, track =>
                        {
                            Task.WaitAll(Scraper.Dl150ccBKT(track));
                        });

                        Parallel.ForEach(trackDisplay200, track =>
                        {
                            Task.WaitAll(Scraper.Dl200ccBKT(track));
                        });
                        ZipFile.CreateFromDirectory(@"rkgs", $"BKT RKGs containing {arg} - {String.Join("", time.Split('\\', '/', ':', '*', '?', '"', '<', '>', '|'))}.zip");
                        using (var fs = new FileStream($"BKT RKGs containing {arg} - {String.Join("", time.Split('\\', '/', ':', '*', '?', '"', '<', '>', '|'))}.zip", FileMode.Open, FileAccess.Read))
                        {
                            var msg = await new DiscordMessageBuilder()
                                .WithFiles(new Dictionary<string, Stream>() { { $"BKT RKGs containing {arg} - {String.Join("", time.Split('\\', '/', ':', '*', '?', '"', '<', '>', '|'))}.zip", fs } })
                                .SendAsync(ctx.Channel);
                        }
                    }
                    Directory.Delete("rkgs", true);
                    File.Delete($"BKT RKGs containing {arg} - {String.Join("", time.Split('\\', '/', ':', '*', '?', '"', '<', '>', '|'))}.zip");
                }
            }

            catch (Exception ex)
            {
                embed = new DiscordEmbedBuilder
                {
                    Color = new DiscordColor("#FF0000"),
                    Title = $"__**Error:**__",
                    Description = $"*{ex.Message}*" +
                       "\n**c!dlbkt track/all**",
                    Url = "https://chadsoft.co.uk/time-trials/",
                    Footer = new DiscordEmbedBuilder.EmbedFooter
                    {
                        Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                    }
                };
                await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);

                Console.WriteLine(ex.ToString());
            }

        }

        [Command("rating")]
        public async Task GetTrackRating(CommandContext ctx, [RemainingText] string track = "")
        {
            await ctx.TriggerTypingAsync();
            string description = "";

            var embed = new DiscordEmbedBuilder { };

            try
            {
                string serviceAccountEmail = "brawlbox@custom-track-testing-bot.iam.gserviceaccount.com";

                var certificate = new X509Certificate2(@"key.p12", "notasecret", X509KeyStorageFlags.Exportable);

                ServiceAccountCredential credential = new ServiceAccountCredential(
                   new ServiceAccountCredential.Initializer(serviceAccountEmail).FromCertificate(certificate));

                var service = new SheetsService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "Custom Track Testing Bot",
                });

                var request = service.Spreadsheets.Values.Get("1xwhKoyypCWq5tCRTI69ijJoDiaoAVsvYAxz-q4UBNqM", "'Early 2022 Track Rating Data'!A226:Q443");
                var response = await request.ExecuteAsync();

                int j = 0;
                List<string> trackDisplay = new List<string>();

                for (int i = 0; i < response.Values.Count; i++)
                {
                    if (CompareIncompleteStrings(response.Values[i][0].ToString(), track))
                    {
                        trackDisplay.Add(response.Values[i][0].ToString());
                        j++;
                    }
                }


                if (track == "average")
                {
                    request = service.Spreadsheets.Values.Get("1xwhKoyypCWq5tCRTI69ijJoDiaoAVsvYAxz-q4UBNqM", "'Early 2022 Track Rating Graphs'");
                    response = await request.ExecuteAsync();

                    description += $"__**Average**__:\n{Math.Round((double.Parse(response.Values[4][12].ToString()) / (double.Parse(response.Values[4][12].ToString()) + double.Parse(response.Values[4][13].ToString()) + double.Parse(response.Values[4][14].ToString()))) * 100)}/{Math.Round((double.Parse(response.Values[4][13].ToString()) / (double.Parse(response.Values[4][12].ToString()) + double.Parse(response.Values[4][13].ToString()) + double.Parse(response.Values[4][14].ToString()))) * 100)}/{Math.Round((double.Parse(response.Values[4][14].ToString()) / (double.Parse(response.Values[4][12].ToString()) + double.Parse(response.Values[4][13].ToString()) + double.Parse(response.Values[4][14].ToString()))) * 100)}%\n";

                    embed = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = $"__**Early 2022 Track Rating Average (Remove/Indifferent/Keep):**__",
                        Description = description,
                        Url = "https://docs.google.com/spreadsheets/d/1xwhKoyypCWq5tCRTI69ijJoDiaoAVsvYAxz-q4UBNqM/edit#gid=595190106",
                        Footer = new DiscordEmbedBuilder.EmbedFooter
                        {
                            Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                        }
                    };
                    await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
                }
                else if (j < 1)
                {
                    embed = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = "__**Error:**__",
                        Description = $"*{track} was not found in the latest track rating poll.*" +
                        "\n**c!rating track**",
                        Url = "https://docs.google.com/spreadsheets/d/1xwhKoyypCWq5tCRTI69ijJoDiaoAVsvYAxz-q4UBNqM/edit#gid=595190106",
                        Footer = new DiscordEmbedBuilder.EmbedFooter
                        {
                            Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                        }
                    };
                    await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
                }
                else
                {
                    for (int i = 0; i < trackDisplay.Count; i++)
                    {
                        foreach (var t in response.Values)
                        {
                            if (trackDisplay[i] == t[0].ToString())
                            {
                                description += $"__**{t[0]}**__:\nAll - {Math.Round((double.Parse(t[2].ToString()) / (double.Parse(t[2].ToString()) + double.Parse(t[3].ToString()) + double.Parse(t[4].ToString()))) * 100)}/{Math.Round((double.Parse(t[3].ToString()) / (double.Parse(t[2].ToString()) + double.Parse(t[3].ToString()) + double.Parse(t[4].ToString()))) * 100)}/{Math.Round((double.Parse(t[4].ToString()) / (double.Parse(t[2].ToString()) + double.Parse(t[3].ToString()) + double.Parse(t[4].ToString()))) * 100)}%\n";
                                description += $"Comp - {Math.Round((double.Parse(t[6].ToString()) / (double.Parse(t[6].ToString()) + double.Parse(t[7].ToString()) + double.Parse(t[8].ToString()))) * 100)}/{Math.Round((double.Parse(t[7].ToString()) / (double.Parse(t[6].ToString()) + double.Parse(t[7].ToString()) + double.Parse(t[8].ToString()))) * 100)}/{Math.Round((double.Parse(t[8].ToString()) / (double.Parse(t[6].ToString()) + double.Parse(t[7].ToString()) + double.Parse(t[8].ToString()))) * 100)}%\n";
                                description += $"Non-Comp - {Math.Round((double.Parse(t[10].ToString()) / (double.Parse(t[10].ToString()) + double.Parse(t[11].ToString()) + double.Parse(t[12].ToString()))) * 100)}/{Math.Round((double.Parse(t[11].ToString()) / (double.Parse(t[10].ToString()) + double.Parse(t[11].ToString()) + double.Parse(t[12].ToString()))) * 100)}/{Math.Round((double.Parse(t[12].ToString()) / (double.Parse(t[10].ToString()) + double.Parse(t[11].ToString()) + double.Parse(t[12].ToString()))) * 100)}%\n";
                                description += $"Creators - {Math.Round((double.Parse(t[14].ToString()) / (double.Parse(t[14].ToString()) + double.Parse(t[15].ToString()) + double.Parse(t[16].ToString()))) * 100)}/{Math.Round((double.Parse(t[15].ToString()) / (double.Parse(t[14].ToString()) + double.Parse(t[15].ToString()) + double.Parse(t[16].ToString()))) * 100)}/{Math.Round((double.Parse(t[16].ToString()) / (double.Parse(t[14].ToString()) + double.Parse(t[15].ToString()) + double.Parse(t[16].ToString()))) * 100)}%\n";
                            }
                        }
                    }

                    if (description.ToCharArray().Length > 800)
                    {
                        embed = new DiscordEmbedBuilder
                        {
                            Color = new DiscordColor("#FF0000"),
                            Title = $"__**Error:**__",
                            Description = "*Embed too large. Please refine your search.*" +
                                   "\n**c!rating track**",
                            Url = "https://docs.google.com/spreadsheets/d/1xwhKoyypCWq5tCRTI69ijJoDiaoAVsvYAxz-q4UBNqM/edit#gid=595190106",
                            Footer = new DiscordEmbedBuilder.EmbedFooter
                            {
                                Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                            }
                        };
                    }
                    else
                    {
                        embed = new DiscordEmbedBuilder
                        {
                            Color = new DiscordColor("#FF0000"),
                            Title = $"__**Early 2022 Track Ratings for {track} (Remove/Indifferent/Keep):**__",
                            Description = description,
                            Url = "https://docs.google.com/spreadsheets/d/1xwhKoyypCWq5tCRTI69ijJoDiaoAVsvYAxz-q4UBNqM/edit#gid=595190106",
                            Footer = new DiscordEmbedBuilder.EmbedFooter
                            {
                                Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                            }
                        };
                    }
                    await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
                }
            }

            catch (Exception ex)
            {
                embed = new DiscordEmbedBuilder
                {
                    Color = new DiscordColor("#FF0000"),
                    Title = $"__**Error:**__",
                    Description = $"*{ex.Message}*" +
                       "\n**c!rating track**",
                    Url = "https://docs.google.com/spreadsheets/d/1xwhKoyypCWq5tCRTI69ijJoDiaoAVsvYAxz-q4UBNqM/edit#gid=595190106",
                    Footer = new DiscordEmbedBuilder.EmbedFooter
                    {
                        Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                    }
                };
                await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);

                Console.WriteLine(ex.ToString());
            }
        }

        [Command("nextupdate")]
        public async Task GetNextUpdate(CommandContext ctx, [RemainingText] string placeholder)
        {
            var embed = new DiscordEmbedBuilder { };

            try
            {
                string serviceAccountEmail = "brawlbox@custom-track-testing-bot.iam.gserviceaccount.com";

                var certificate = new X509Certificate2(@"key.p12", "notasecret", X509KeyStorageFlags.Exportable);

                ServiceAccountCredential credential = new ServiceAccountCredential(
                   new ServiceAccountCredential.Initializer(serviceAccountEmail).FromCertificate(certificate));

                var service = new SheetsService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "Custom Track Testing Bot",
                });

                await ctx.TriggerTypingAsync();

                var request = service.Spreadsheets.Values.Get("1xwhKoyypCWq5tCRTI69ijJoDiaoAVsvYAxz-q4UBNqM", "'Update Queue'");
                request.ValueRenderOption = SpreadsheetsResource.ValuesResource.GetRequest.ValueRenderOptionEnum.FORMULA;
                var response = await request.ExecuteAsync();
                foreach (var t in response.Values)
                {
                    while (t.Count < 11)
                    {
                        t.Add("");
                    }
                }

                int k = 3;

                string description = $"**{response.Values[k][1]}:**";

                while (response.Values[k][2].ToString() != "delimiter")
                {
                    if (response.Values[k][2].ToString() == "")
                    {
                        description += "\n*TBD*";
                    }
                    else
                    {
                        description += $"\n{response.Values[k][2]} {response.Values[k][4]} | {response.Values[k][3]} | [{response.Values[k][5].ToString().Split('"')[3]}]({response.Values[k][5].ToString().Split('"')[1]})";
                    }
                    k++;
                }
                k++;
                if (response.Values[k][1].ToString() != "end")
                {
                    description += $"\n**{response.Values[k][1]}:**";
                    while (response.Values[k][2].ToString() != "delimiter")
                    {
                        if (response.Values[k][2].ToString() == "")
                        {
                            description += "\n*TBD*";
                        }
                        else
                        {
                            description += $"\n{response.Values[k][2]} {response.Values[k][4]} | {response.Values[k][3]} | [{response.Values[k][5].ToString().Split('"')[3]}]({response.Values[k][5].ToString().Split('"')[1]})";
                        }
                        k++;
                    }
                    k++;
                }
                if (response.Values[k][1].ToString() != "end")
                {
                    description += $"\n**{response.Values[k][1]}:**";
                    while (response.Values[k][2].ToString() != "delimiter")
                    {
                        if (response.Values[k][2].ToString() == "")
                        {
                            description += "\n*TBD*";
                        }
                        else
                        {
                            description += $"\n{response.Values[k][2]} {response.Values[k][4]} | {response.Values[k][3]} | [{response.Values[k][5].ToString().Split('"')[3]}]({response.Values[k][5].ToString().Split('"')[1]})";
                        }
                        k++;
                    }
                }

                embed = new DiscordEmbedBuilder
                {
                    Color = new DiscordColor("#FF0000"),
                    Title = $"__**{response.Values[1][1]}:**__",
                    Description = description,
                    Url = "https://docs.google.com/spreadsheets/d/1xwhKoyypCWq5tCRTI69ijJoDiaoAVsvYAxz-q4UBNqM/edit#gid=1751905284",
                    Footer = new DiscordEmbedBuilder.EmbedFooter
                    {
                        Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                    }
                };
                await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                embed = new DiscordEmbedBuilder
                {
                    Color = new DiscordColor("#FF0000"),
                    Title = $"__**Error:**__",
                    Description = $"*{ex.Message}*" +
                           "\n**c!nextupdate**",
                    Url = "https://docs.google.com/spreadsheets/d/1xwhKoyypCWq5tCRTI69ijJoDiaoAVsvYAxz-q4UBNqM/edit#gid=1751905284",
                    Footer = new DiscordEmbedBuilder.EmbedFooter
                    {
                        Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                    }
                };
                await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);

                Console.WriteLine(ex.ToString());
            }
        }

        [Command("summary")]
        public async Task GetSummary(CommandContext ctx, [RemainingText] string track = "")
        {
            var embed = new DiscordEmbedBuilder { };

            try
            {
                string description = string.Empty;

                if (track == "")
                {
                    embed = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = $"__**Error:**__",
                        Description = $"*Track was not inputted.*" +
                               "\n**c!summary track**",
                        Url = "https://docs.google.com/spreadsheets/d/1I9yFsomTcvFT4hp6eN2azsfv6MsIy1897tBFX_gmtss/edit#gid=798417105",
                        Footer = new DiscordEmbedBuilder.EmbedFooter
                        {
                            Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                        }
                    };
                    await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
                }
                else
                {
                    string serviceAccountEmail = "brawlbox@custom-track-testing-bot.iam.gserviceaccount.com";

                    var certificate = new X509Certificate2(@"key.p12", "notasecret", X509KeyStorageFlags.Exportable);

                    ServiceAccountCredential credential = new ServiceAccountCredential(
                       new ServiceAccountCredential.Initializer(serviceAccountEmail).FromCertificate(certificate));

                    var service = new SheetsService(new BaseClientService.Initializer()
                    {
                        HttpClientInitializer = credential,
                        ApplicationName = "Custom Track Testing Bot",
                    });

                    await ctx.TriggerTypingAsync();

                    var request = service.Spreadsheets.Values.Get("1I9yFsomTcvFT4hp6eN2azsfv6MsIy1897tBFX_gmtss", "'Track Evaluation Log'");
                    var response = await request.ExecuteAsync();
                    foreach (var t in response.Values)
                    {
                        while (t.Count < 7)
                        {
                            t.Add("");
                        }
                    }

                    int j = 0;
                    int k = 0;

                    string trackDisplay = string.Empty;

                    for (int i = 0; i < response.Values.Count; i++)
                    {
                        if (j < 1)
                        {
                            if ((CompareIncompleteStrings(response.Values[i][2].ToString(), track) ||
                                CompareStringAbbreviation(track, response.Values[i][2].ToString()) ||
                                CompareStringsLevenshteinDistance(response.Values[i][2].ToString(), track)) &&
                                !CompareIncompleteStrings(response.Values[i][0].ToString(), "ignore"))
                            {
                                k = i;

                                while (response.Values[k][0].ToString() != "delimiter")
                                {
                                    k--;
                                }

                                string dateString = response.Values[k][1].ToString();

                                var tally = response.Values[i][1].ToString().split("\n");
                                if (tally[0].ToString() == "✘")
                                {
                                    tally[0] = DiscordEmoji.FromName(ctx.Client, ":No:");
                                }
                                else if (tally[0].ToString() == "✔")
                                {
                                    tally[0] = DiscordEmoji.FromName(ctx.Client, ":Yes:");
                                }
                                if (tally.Count == 1)
                                {
                                    tally.Add("");
                                }
                                if (response.Values[i][6].ToString().ToCharArray().Length > 3500)
                                {
                                    description = $"__**{dateString}**__\n**{response.Values[i][2]} {response.Values[i][4]} - {response.Values[i][3]}**\n{tally[1]} {tally[0]}\n\n{response.Values[i][6].ToString().Remove(3499)}...\n\n*For full summary go to the [Track Evaluation Log](https://docs.google.com/spreadsheets/d/1I9yFsomTcvFT4hp6eN2azsfv6MsIy1897tBFX_gmtss/edit#gid=798417105).*";
                                }
                                else
                                {
                                    description = $"__**{dateString}**__\n**{response.Values[i][2]} {response.Values[i][4]} - {response.Values[i][3]}**\n{tally[1]} {tally[0]}\n\n{response.Values[i][6]}";
                                }
                                j++;
                                trackDisplay = response.Values[i][2].ToString();
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                    if (j < 1)
                    {
                        embed = new DiscordEmbedBuilder
                        {
                            Color = new DiscordColor("#FF0000"),
                            Title = "__**Error:**__",
                            Description = $"*{track} could not be found.*" +
                                   "\n**c!summary track**",
                            Url = "https://docs.google.com/spreadsheets/d/1I9yFsomTcvFT4hp6eN2azsfv6MsIy1897tBFX_gmtss/edit#gid=798417105",
                            Footer = new DiscordEmbedBuilder.EmbedFooter
                            {
                                Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                            }
                        };
                        await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
                    }
                    else
                    {
                        embed = new DiscordEmbedBuilder
                        {
                            Color = new DiscordColor("#FF0000"),
                            Title = $"__**Summary for {trackDisplay} (First result):**__",
                            Description = description,
                            Url = "https://docs.google.com/spreadsheets/d/1I9yFsomTcvFT4hp6eN2azsfv6MsIy1897tBFX_gmtss/edit#gid=798417105",
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
                embed = new DiscordEmbedBuilder
                {
                    Color = new DiscordColor("#FF0000"),
                    Title = $"__**Error:**__",
                    Description = $"*{ex.Message}*" +
                           "\n**c!summary track**",
                    Url = "https://docs.google.com/spreadsheets/d/1I9yFsomTcvFT4hp6eN2azsfv6MsIy1897tBFX_gmtss/edit#gid=798417105",
                    Footer = new DiscordEmbedBuilder.EmbedFooter
                    {
                        Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                    }
                };
                await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);

                Console.WriteLine(ex.ToString());
            }
        }

        [Command("addhw")]
        [RequireRoles(RoleCheckMode.Any, "Admin")]
        public async Task AddHomework(CommandContext ctx, string track = "", string author = "", string version = "", string download = "", string slot = "", string lapSpeed = "1/3", [RemainingText] string notes = "")
        {
            if (ctx.Guild.Id == 180306609233330176)
            {
                var embed = new DiscordEmbedBuilder { };

                try
                {
                    if (ctx.Channel.Id == 217126063803727872 || ctx.Channel.Id == 750123394237726847 || ctx.Channel.Id == 935200150710808626 || ctx.Channel.Id == 946835035372257320 || ctx.Channel.Id == 751534710068477953)
                    {
                        string description = string.Empty;

                        if (track == "")
                        {
                            embed = new DiscordEmbedBuilder
                            {
                                Color = new DiscordColor("#FF0000"),
                                Title = $"__**Error:**__",
                                Description = $"*Track was not inputted.*" +
                                       "\n**c!addhw \"track\" \"author\" \"version\" \"download link\" \"slot (e.g. Luigi Circuit - beginner_course)\" \"speed/lap modifiers\" notes**",
                                Url = "https://docs.google.com/spreadsheets/d/1I9yFsomTcvFT4hp6eN2azsfv6MsIy1897tBFX_gmtss/edit#gid=906385082",
                                Footer = new DiscordEmbedBuilder.EmbedFooter
                                {
                                    Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                                }
                            };
                            await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
                        }
                        else if (author == "")
                        {
                            embed = new DiscordEmbedBuilder
                            {
                                Color = new DiscordColor("#FF0000"),
                                Title = $"__**Error:**__",
                                Description = $"*Author was not inputted.*" +
                                       "\n**c!addhw \"track\" \"author\" \"version\" \"download link\" \"slot (e.g. Luigi Circuit - beginner_course)\" \"speed/lap modifiers\" notes**",
                                Url = "https://docs.google.com/spreadsheets/d/1I9yFsomTcvFT4hp6eN2azsfv6MsIy1897tBFX_gmtss/edit#gid=906385082",
                                Footer = new DiscordEmbedBuilder.EmbedFooter
                                {
                                    Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                                }
                            };
                            await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
                        }
                        else if (version == "")
                        {
                            embed = new DiscordEmbedBuilder
                            {
                                Color = new DiscordColor("#FF0000"),
                                Title = $"__**Error:**__",
                                Description = $"*Version was not inputted.*" +
                                       "\n**c!addhw \"track\" \"author\" \"version\" \"download link\" \"slot (e.g. Luigi Circuit - beginner_course)\" \"speed/lap modifiers\" notes**",
                                Url = "https://docs.google.com/spreadsheets/d/1I9yFsomTcvFT4hp6eN2azsfv6MsIy1897tBFX_gmtss/edit#gid=906385082",
                                Footer = new DiscordEmbedBuilder.EmbedFooter
                                {
                                    Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                                }
                            };
                            await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
                        }
                        else if (slot == "")
                        {
                            embed = new DiscordEmbedBuilder
                            {
                                Color = new DiscordColor("#FF0000"),
                                Title = $"__**Error:**__",
                                Description = $"*Slot was not inputted.*" +
                                       "\n**c!addhw \"track\" \"author\" \"version\" \"download link\" \"slot (e.g. Luigi Circuit - beginner_course)\" \"speed/lap modifiers\" notes**",
                                Url = "https://docs.google.com/spreadsheets/d/1I9yFsomTcvFT4hp6eN2azsfv6MsIy1897tBFX_gmtss/edit#gid=906385082",
                                Footer = new DiscordEmbedBuilder.EmbedFooter
                                {
                                    Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                                }
                            };
                            await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
                        }
                        else
                        {
                            string serviceAccountEmail = "brawlbox@custom-track-testing-bot.iam.gserviceaccount.com";

                            var certificate = new X509Certificate2(@"key.p12", "notasecret", X509KeyStorageFlags.Exportable);

                            ServiceAccountCredential credential = new ServiceAccountCredential(
                               new ServiceAccountCredential.Initializer(serviceAccountEmail).FromCertificate(certificate));

                            var service = new SheetsService(new BaseClientService.Initializer()
                            {
                                HttpClientInitializer = credential,
                                ApplicationName = "Custom Track Testing Bot",
                            });

                            await ctx.TriggerTypingAsync();

                            var countRequest = service.Spreadsheets.Values.Get("1I9yFsomTcvFT4hp6eN2azsfv6MsIy1897tBFX_gmtss", "'Track Evaluating'");
                            var countResponse = await countRequest.ExecuteAsync();

                            var due = DateTime.Today;

                            due = due.AddDays(10);
                            while (due.Day % 10 != 0)
                            {
                                due = due.AddDays(1);
                            }
                            string dueMonth = string.Empty;
                            switch (due.Month)
                            {
                                case 1:
                                    dueMonth = "January";
                                    break;
                                case 2:
                                    dueMonth = "Feburary";
                                    break;
                                case 3:
                                    dueMonth = "March";
                                    break;
                                case 4:
                                    dueMonth = "April";
                                    break;
                                case 5:
                                    dueMonth = "May";
                                    break;
                                case 6:
                                    dueMonth = "June";
                                    break;
                                case 7:
                                    dueMonth = "July";
                                    break;
                                case 8:
                                    dueMonth = "August";
                                    break;
                                case 9:
                                    dueMonth = "September";
                                    break;
                                case 10:
                                    dueMonth = "October";
                                    break;
                                case 11:
                                    dueMonth = "November";
                                    break;
                                case 12:
                                    dueMonth = "December";
                                    break;
                            }

                            string dl = string.Empty;
                            if (download.ToLowerInvariant().Contains("discord"))
                            {
                                dl = $"=HYPERLINK(\"{download}\", \"Discord\")";
                            }
                            else if (download.ToLowerInvariant().Contains("google"))
                            {
                                dl = $"=HYPERLINK(\"{download}\", \"Google Drive\")";
                            }
                            else if (download.ToLowerInvariant().Contains("mega"))
                            {
                                dl = $"=HYPERLINK(\"{download}\", \"Mega\")";
                            }
                            else if (download.ToLowerInvariant().Contains("mediafire"))
                            {
                                dl = $"=HYPERLINK(\"{download}\", \"MediaFire\")";
                            }
                            else if (download.ToLowerInvariant().Contains("icedrive"))
                            {
                                dl = $"=HYPERLINK(\"{download}\", \"Icedrive\")";
                            }
                            else if (download.ToLowerInvariant().Contains("sync"))
                            {
                                dl = $"=HYPERLINK(\"{download}\", \"Sync\")";
                            }
                            else if (download.ToLowerInvariant().Contains("pcloud"))
                            {
                                dl = $"=HYPERLINK(\"{download}\", \"pCloud\")";
                            }
                            else
                            {
                                dl = $"=HYPERLINK(\"{download}\", \"Unregistered\")";
                            }

                            IList<object> obj = new List<object>();
                            obj.Add(track);
                            obj.Add($"{dueMonth} {due.Day}, {due.Year}");
                            obj.Add(author);
                            obj.Add($"'{version}");
                            obj.Add(dl);
                            obj.Add(slot);
                            obj.Add(lapSpeed);
                            obj.Add(notes);
                            obj.Add($"=COUNTIF($M{countResponse.Values.Count + 1}:{countResponse.Values.Count + 1}, \"yes*\")");
                            obj.Add($"=COUNTIF($M{countResponse.Values.Count + 1}:{countResponse.Values.Count + 1}, \"fixes*\")");
                            obj.Add($"=COUNTIF($M{countResponse.Values.Count + 1}:{countResponse.Values.Count + 1}, \"neutral*\")");
                            obj.Add($"=COUNTIF($M{countResponse.Values.Count + 1}:{countResponse.Values.Count + 1}, \"no*\")");
                            IList<IList<object>> values = new List<IList<object>>();
                            values.Add(obj);

                            string strAlpha = "";

                            for (int i = 65; i <= 90; i++)
                            {
                                strAlpha += ((char)i).ToString() + "";
                            }

                            var appendRequest = service.Spreadsheets.Values.Append(new ValueRange() { Values = values }, "1I9yFsomTcvFT4hp6eN2azsfv6MsIy1897tBFX_gmtss", "'Track Evaluating'");
                            appendRequest.InsertDataOption = SpreadsheetsResource.ValuesResource.AppendRequest.InsertDataOptionEnum.INSERTROWS;
                            appendRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
                            appendRequest.ResponseValueRenderOption = SpreadsheetsResource.ValuesResource.AppendRequest.ResponseValueRenderOptionEnum.FORMULA;
                            var appendResponse = await appendRequest.ExecuteAsync();

                            embed = new DiscordEmbedBuilder
                            {
                                Color = new DiscordColor("#FF0000"),
                                Title = $"__**Success:**__",
                                Description = $"*{track} has been added as homework.*",
                                Url = "https://docs.google.com/spreadsheets/d/1I9yFsomTcvFT4hp6eN2azsfv6MsIy1897tBFX_gmtss/edit#gid=906385082",
                                Footer = new DiscordEmbedBuilder.EmbedFooter
                                {
                                    Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                                }
                            };
                            await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);

                            DiscordChannel channel = ctx.Channel;

                            foreach (var c in ctx.Guild.Channels)
                            {
                                if (c.Value.Id == 635313521487511554)
                                {
                                    channel = c.Value;
                                }
                            }

                            await channel.SendMessageAsync($"<@&608386209655554058> {track} has been added as homework. It is due for {dueMonth} {due.Day}, {due.Year}.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    embed = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = $"__**Error:**__",
                        Description = $"*{ex.Message}*" +
                               "\n**c!addhw \"track\" \"author\" \"version\" \"download link\" \"slot (e.g. Luigi Circuit - beginner_course)\" \"speed/lap modifiers\" notes**",
                        Url = "https://docs.google.com/spreadsheets/d/1I9yFsomTcvFT4hp6eN2azsfv6MsIy1897tBFX_gmtss/edit#gid=906385082",
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

        [Command("delhw")]
        [RequireRoles(RoleCheckMode.Any, "Admin")]
        public async Task DeleteHomework(CommandContext ctx, [RemainingText] string track = "")
        {
            if (ctx.Guild.Id == 180306609233330176)
            {
                var embed = new DiscordEmbedBuilder { };
                try
                {
                    if (ctx.Channel.Id == 217126063803727872 || ctx.Channel.Id == 750123394237726847 || ctx.Channel.Id == 935200150710808626 || ctx.Channel.Id == 946835035372257320 || ctx.Channel.Id == 751534710068477953)
                    {
                        if (track == "")
                        {
                            embed = new DiscordEmbedBuilder
                            {
                                Color = new DiscordColor("#FF0000"),
                                Title = $"__**Error:**__",
                                Description = $"*Track was not inputted.*" +
                                       "\n**c!delhw track**",
                                Url = "https://docs.google.com/spreadsheets/d/1I9yFsomTcvFT4hp6eN2azsfv6MsIy1897tBFX_gmtss/edit#gid=906385082",
                                Footer = new DiscordEmbedBuilder.EmbedFooter
                                {
                                    Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                                }
                            };
                            await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
                        }
                        else
                        {
                            string description = string.Empty;
                            string json = string.Empty;
                            string member = string.Empty;

                            using (var fs = File.OpenRead("council.json"))
                            using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                                json = await sr.ReadToEndAsync().ConfigureAwait(false);
                            List<CouncilMember> councilJson = JsonConvert.DeserializeObject<List<CouncilMember>>(json);

                            foreach (var m in councilJson)
                            {
                                if (m.DiscordId == ctx.Member.Id)
                                {
                                    member = m.SheetName;
                                }
                            }

                            await ctx.TriggerTypingAsync();

                            string serviceAccountEmail = "brawlbox@custom-track-testing-bot.iam.gserviceaccount.com";

                            var certificate = new X509Certificate2(@"key.p12", "notasecret", X509KeyStorageFlags.Exportable);

                            ServiceAccountCredential credential = new ServiceAccountCredential(
                               new ServiceAccountCredential.Initializer(serviceAccountEmail).FromCertificate(certificate));

                            var service = new SheetsService(new BaseClientService.Initializer()
                            {
                                HttpClientInitializer = credential,
                                ApplicationName = "Custom Track Testing Bot",
                            });

                            var request = service.Spreadsheets.Values.Get("1I9yFsomTcvFT4hp6eN2azsfv6MsIy1897tBFX_gmtss", "'Track Evaluating'");
                            var response = await request.ExecuteAsync();

                            int ix = -1;

                            string trackDisplay = string.Empty;

                            for (int i = 0; i < response.Values.Count; i++)
                            {
                                if (CompareStrings(response.Values[i][0].ToString(), track))
                                {
                                    ix = i;
                                    trackDisplay = response.Values[i][0].ToString();
                                }
                            }
                            if (ix < 0)
                            {
                                embed = new DiscordEmbedBuilder
                                {
                                    Color = new DiscordColor("#FF0000"),
                                    Title = "__**Error:**__",
                                    Description = $"*{track} could not be found.*" +
                                           "\n**c!delhw track**",
                                    Url = "https://docs.google.com/spreadsheets/d/1I9yFsomTcvFT4hp6eN2azsfv6MsIy1897tBFX_gmtss/edit#gid=906385082",
                                    Footer = new DiscordEmbedBuilder.EmbedFooter
                                    {
                                        Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                                    }
                                };
                                await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
                            }
                            else
                            {
                                var req = new Request
                                {
                                    DeleteDimension = new DeleteDimensionRequest
                                    {
                                        Range = new DimensionRange
                                        {
                                            SheetId = 906385082,
                                            Dimension = "ROWS",
                                            StartIndex = ix,
                                            EndIndex = ix + 1
                                        }
                                    }
                                };

                                var deleteRequest = new BatchUpdateSpreadsheetRequest { Requests = new List<Request> { req } };
                                var deleteResponse = service.Spreadsheets.BatchUpdate(deleteRequest, "1I9yFsomTcvFT4hp6eN2azsfv6MsIy1897tBFX_gmtss").Execute();


                                embed = new DiscordEmbedBuilder
                                {
                                    Color = new DiscordColor("#FF0000"),
                                    Title = "__**Success:**__",
                                    Description = $"*{trackDisplay} has been deleted from homework.*",
                                    Url = "https://docs.google.com/spreadsheets/d/1I9yFsomTcvFT4hp6eN2azsfv6MsIy1897tBFX_gmtss/edit#gid=906385082",
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
                    embed = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = "__**Error:**__",
                        Description = $"*{ex.Message}*" +
                               "\n**c!delhw track**",
                        Url = "https://docs.google.com/spreadsheets/d/1I9yFsomTcvFT4hp6eN2azsfv6MsIy1897tBFX_gmtss/edit#gid=906385082",
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

        [Command("submithw")]
        [RequireRoles(RoleCheckMode.Any, "Track Council", "Admin")]
        public async Task SubmitHomework(CommandContext ctx, string vote = "", string track = "", [RemainingText] string feedback = "")
        {
            if (ctx.Guild.Id == 180306609233330176)
            {
                var embed = new DiscordEmbedBuilder { };
                try
                {
                    if (ctx.Channel.Id == 217126063803727872 || ctx.Channel.Id == 750123394237726847 || ctx.Channel.Id == 935200150710808626 || ctx.Channel.Id == 946835035372257320 || ctx.Channel.Id == 751534710068477953)
                    {
                        string strAlpha = "";

                        for (int i = 65; i <= 90; i++)
                        {
                            strAlpha += ((char)i).ToString() + "";
                        }

                        string description = string.Empty;
                        string json = string.Empty;
                        string member = string.Empty;

                        if (vote == "")
                        {
                            embed = new DiscordEmbedBuilder
                            {
                                Color = new DiscordColor("#FF0000"),
                                Title = $"__**Error:**__",
                                Description = $"*Track was not inputted.*" +
                                       "\n**c!submithw yes/fixes/neutral/no \"track\" feedback**",
                                Url = "https://docs.google.com/spreadsheets/d/1I9yFsomTcvFT4hp6eN2azsfv6MsIy1897tBFX_gmtss/edit#gid=906385082",
                                Footer = new DiscordEmbedBuilder.EmbedFooter
                                {
                                    Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                                }
                            };
                            await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
                        }
                        else if (track == "")
                        {
                            embed = new DiscordEmbedBuilder
                            {
                                Color = new DiscordColor("#FF0000"),
                                Title = $"__**Error:**__",
                                Description = $"*Author was not inputted.*" +
                                       "\n**c!submithw yes/fixes/neutral/no \"track\" feedback**",
                                Url = "https://docs.google.com/spreadsheets/d/1I9yFsomTcvFT4hp6eN2azsfv6MsIy1897tBFX_gmtss/edit#gid=906385082",
                                Footer = new DiscordEmbedBuilder.EmbedFooter
                                {
                                    Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                                }
                            };
                            await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
                        }
                        else if (vote.ToLowerInvariant() != "no" && vote.ToLowerInvariant() != "yes" && vote.ToLowerInvariant() != "neutral" && vote.ToLowerInvariant() != "fixes")
                        {
                            embed = new DiscordEmbedBuilder
                            {
                                Color = new DiscordColor("#FF0000"),
                                Title = $"__**Error:**__",
                                Description = $"*{vote} is not a valid vote.*" +
                                          "\n**c!submithw yes/fixes/neutral/no \"track\" feedback**",
                                Url = "https://docs.google.com/spreadsheets/d/1I9yFsomTcvFT4hp6eN2azsfv6MsIy1897tBFX_gmtss/edit#gid=906385082",
                                Footer = new DiscordEmbedBuilder.EmbedFooter
                                {
                                    Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                                }
                            };
                            await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
                        }
                        else
                        {
                            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;

                            vote = textInfo.ToTitleCase(vote);

                            using (var fs = File.OpenRead("council.json"))
                            using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                                json = await sr.ReadToEndAsync().ConfigureAwait(false);
                            List<CouncilMember> councilJson = JsonConvert.DeserializeObject<List<CouncilMember>>(json);

                            foreach (var m in councilJson)
                            {
                                if (m.DiscordId == ctx.Member.Id)
                                {
                                    member = m.SheetName;
                                }
                            }

                            await ctx.TriggerTypingAsync();

                            string serviceAccountEmail = "brawlbox@custom-track-testing-bot.iam.gserviceaccount.com";

                            var certificate = new X509Certificate2(@"key.p12", "notasecret", X509KeyStorageFlags.Exportable);

                            ServiceAccountCredential credential = new ServiceAccountCredential(
                               new ServiceAccountCredential.Initializer(serviceAccountEmail).FromCertificate(certificate));

                            var service = new SheetsService(new BaseClientService.Initializer()
                            {
                                HttpClientInitializer = credential,
                                ApplicationName = "Custom Track Testing Bot",
                            });

                            var request = service.Spreadsheets.Values.Get("1I9yFsomTcvFT4hp6eN2azsfv6MsIy1897tBFX_gmtss", "'Track Evaluating'");
                            request.ValueRenderOption = SpreadsheetsResource.ValuesResource.GetRequest.ValueRenderOptionEnum.FORMULA;
                            var response = await request.ExecuteAsync();

                            int ix = -1;

                            for (int i = 0; i < response.Values[0].Count; i++)
                            {
                                if (CompareStrings(response.Values[0][i].ToString(), member))
                                {
                                    ix = i;
                                }
                            }

                            if (ix < 0)
                            {
                                embed = new DiscordEmbedBuilder
                                {
                                    Color = new DiscordColor("#FF0000"),
                                    Title = "__**Error:**__",
                                    Description = $"*<@{ctx.Member.Id}> is not able to submit feedback.*" +
                                       "\n**c!submithw yes/fixes/neutral/no \"track\" feedback**",
                                    Url = "https://docs.google.com/spreadsheets/d/1I9yFsomTcvFT4hp6eN2azsfv6MsIy1897tBFX_gmtss/edit#gid=906385082",
                                    Footer = new DiscordEmbedBuilder.EmbedFooter
                                    {
                                        Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                                    }
                                };
                                await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
                            }

                            else
                            {

                                int j = 0;

                                foreach (var t in response.Values)
                                {
                                    while (t.Count < response.Values[0].Count)
                                    {
                                        t.Add("");
                                    }
                                    if (j > 0)
                                    {
                                        break;
                                    }
                                    else if (CompareStringAbbreviation(track, t[0].ToString()) || CompareStringsLevenshteinDistance(track, t[0].ToString()))
                                    {
                                        t[ix] = vote + "\n" + feedback;
                                        j++;
                                    }
                                }

                                SpreadsheetsResource.ValuesResource.UpdateRequest updateRequest;

                                if (response.Values[0].Count < 27)
                                {
                                    updateRequest = service.Spreadsheets.Values.Update(response, "1I9yFsomTcvFT4hp6eN2azsfv6MsIy1897tBFX_gmtss", $"'Track Evaluating'!A1:{strAlpha[response.Values[0].Count - 1]}{response.Values.Count}");
                                }
                                else
                                {
                                    updateRequest = service.Spreadsheets.Values.Update(response, "1I9yFsomTcvFT4hp6eN2azsfv6MsIy1897tBFX_gmtss", $"'Track Evaluating'!A1:A{strAlpha[response.Values[0].Count % 26 - 1]}{response.Values.Count}");
                                }
                                updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;
                                var update = await updateRequest.ExecuteAsync();

                                if (j == 0)
                                {
                                    embed = new DiscordEmbedBuilder
                                    {
                                        Color = new DiscordColor("#FF0000"),
                                        Title = "__**Error:**__",
                                        Description = $"*{track} could not be found.*" +
                                           "\n**c!submithw yes/fixes/neutral/no \"track\" feedback**",
                                        Url = "https://docs.google.com/spreadsheets/d/1I9yFsomTcvFT4hp6eN2azsfv6MsIy1897tBFX_gmtss/edit#gid=906385082",
                                        Footer = new DiscordEmbedBuilder.EmbedFooter
                                        {
                                            Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                                        }
                                    };
                                    await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
                                }
                                else
                                {
                                    embed = new DiscordEmbedBuilder
                                    {
                                        Color = new DiscordColor("#FF0000"),
                                        Title = "__**Success:**__",
                                        Description = $"*Homework for {track} has been submitted successfully.*",
                                        Url = "https://docs.google.com/spreadsheets/d/1I9yFsomTcvFT4hp6eN2azsfv6MsIy1897tBFX_gmtss/edit#gid=906385082",
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
                }
                catch (Exception ex)
                {
                    embed = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = "__**Error:**__",
                        Description = $"*{ex.Message}*" +
                               "\n**c!submithw yes/fixes/neutral/no \"track\" feedback**",
                        Url = "https://docs.google.com/spreadsheets/d/1I9yFsomTcvFT4hp6eN2azsfv6MsIy1897tBFX_gmtss/edit#gid=906385082",
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

        [Command("gethw")]
        [RequireRoles(RoleCheckMode.Any, "Track Council", "Admin")]
        public async Task GetSpecificHomework(CommandContext ctx, string track = "", string member = "")
        {
            if (ctx.Guild.Id == 180306609233330176)
            {
                var embed = new DiscordEmbedBuilder { };
                try
                {
                    if (ctx.Channel.Id == 217126063803727872 || ctx.Channel.Id == 750123394237726847 || ctx.Channel.Id == 935200150710808626 || ctx.Channel.Id == 946835035372257320 || ctx.Channel.Id == 751534710068477953)
                    {
                        string json;
                        using (var fs = File.OpenRead("council.json"))
                        using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                            json = await sr.ReadToEndAsync().ConfigureAwait(false);
                        List<CouncilMember> councilJson = JsonConvert.DeserializeObject<List<CouncilMember>>(json);

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
                                ix = councilJson.FindIndex(x => CompareIncompleteStrings(x.SheetName, member) || CompareStringsLevenshteinDistance(x.SheetName, member));
                            }
                        }

                        string description = string.Empty;

                        if (track == "")
                        {
                            embed = new DiscordEmbedBuilder
                            {
                                Color = new DiscordColor("#FF0000"),
                                Title = $"__**Error:**__",
                                Description = $"*Track was not inputted.*" +
                                       "\n**c!gethw track/all mention/name**",
                                Url = "https://docs.google.com/spreadsheets/d/1I9yFsomTcvFT4hp6eN2azsfv6MsIy1897tBFX_gmtss/edit#gid=906385082",
                                Footer = new DiscordEmbedBuilder.EmbedFooter
                                {
                                    Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                                }
                            };
                            await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
                        }
                        else
                        {
                            await ctx.TriggerTypingAsync();

                            string serviceAccountEmail = "brawlbox@custom-track-testing-bot.iam.gserviceaccount.com";

                            var certificate = new X509Certificate2(@"key.p12", "notasecret", X509KeyStorageFlags.Exportable);

                            ServiceAccountCredential credential = new ServiceAccountCredential(
                               new ServiceAccountCredential.Initializer(serviceAccountEmail).FromCertificate(certificate));

                            var service = new SheetsService(new BaseClientService.Initializer()
                            {
                                HttpClientInitializer = credential,
                                ApplicationName = "Custom Track Testing Bot",
                            });

                            var request = service.Spreadsheets.Values.Get("1I9yFsomTcvFT4hp6eN2azsfv6MsIy1897tBFX_gmtss", "'Track Evaluating'");
                            request.ValueRenderOption = SpreadsheetsResource.ValuesResource.GetRequest.ValueRenderOptionEnum.FORMULA;
                            var response = await request.ExecuteAsync();

                            for (int i = 0; i < response.Values.Count; i++)
                            {
                                while (response.Values[i].Count < response.Values[0].Count)
                                {
                                    response.Values[i].Add("");
                                }
                            }

                            int sheetIx = -1;

                            if (CompareStrings(member, "all"))
                            {
                                for (int i = 1; i < response.Values.Count; i++)
                                {
                                    if (CompareStringAbbreviation(track, response.Values[i][0].ToString()) || CompareStringsLevenshteinDistance(track, response.Values[i][0].ToString()))
                                    {
                                        sheetIx = i;
                                    }
                                }
                            }
                            else
                            {
                                for (int i = 12; i < response.Values[0].Count; i++)
                                {
                                    if (CompareStringAbbreviation(member, response.Values[0][i].ToString()) || CompareStringsLevenshteinDistance(member, response.Values[0][i].ToString()))
                                    {
                                        sheetIx = i;
                                    }
                                }
                            }

                            j = 0;
                            int k = 0;
                            int l = 0;
                            string trackDisplay = string.Empty;

                            foreach (var m in response.Values[0])
                            {
                                if (m.ToString() == member)
                                {
                                    k++;
                                }
                            }

                            List<DiscordEmbedBuilder> embeds = new List<DiscordEmbedBuilder>();

                            if (sheetIx > 0)
                            {
                                if (track.ToLowerInvariant() != "all")
                                {
                                    if (CompareStrings(member, "all"))
                                    {
                                        for (int i = 12; i < response.Values[sheetIx].Count; i++)
                                        {
                                            if (response.Values[sheetIx][i].ToString() == "")
                                            {
                                                description = $"*{response.Values[0][i]} has not done their homework yet.*";
                                            }
                                            else
                                            {
                                                if (response.Values[sheetIx][i].ToString().ToCharArray().Length > 3500)
                                                {
                                                    description = $"**Homework of {response.Values[0][i]}:**\n{response.Values[sheetIx][i].ToString().Remove(3499)}...\n\n*For full feedback go to the [Track Council Sheet](https://docs.google.com/spreadsheets/d/1I9yFsomTcvFT4hp6eN2azsfv6MsIy1897tBFX_gmtss/edit#gid=906385082).*";
                                                }
                                                else
                                                {
                                                    description = $"**Homework of {response.Values[0][i]}:**\n{response.Values[sheetIx][i]}";
                                                }
                                            }
                                            trackDisplay = response.Values[sheetIx][i].ToString();

                                            embed = new DiscordEmbedBuilder
                                            {
                                                Color = new DiscordColor("#FF0000"),
                                                Title = $"__**{response.Values[sheetIx][0]}**__",
                                                Description = description,
                                                Url = "https://docs.google.com/spreadsheets/d/1I9yFsomTcvFT4hp6eN2azsfv6MsIy1897tBFX_gmtss/edit#gid=906385082",
                                                Footer = new DiscordEmbedBuilder.EmbedFooter
                                                {
                                                    Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                                                }
                                            };

                                            embeds.Add(embed);
                                        }
                                    }
                                    else
                                    {
                                        foreach (var t in response.Values)
                                        {
                                            while (t.Count < response.Values[0].Count)
                                            {
                                                t.Add("");
                                            }
                                            if (j < 1)
                                            {
                                                j++;
                                            }
                                            else if (CompareStrings(t[0].ToString(), track))
                                            {
                                                if (t[sheetIx].ToString() == "")
                                                {
                                                    description = $"*<@{councilJson[ix].DiscordId}> has not done their homework yet.*";
                                                }
                                                else
                                                {
                                                    if (t[sheetIx].ToString().ToCharArray().Length > 3500)
                                                    {
                                                        description = $"**Homework of <@{councilJson[ix].DiscordId}>:**\n{t[sheetIx].ToString().Remove(3499)}...\n\n*For full feedback go to the [Track Council Sheet](https://docs.google.com/spreadsheets/d/1I9yFsomTcvFT4hp6eN2azsfv6MsIy1897tBFX_gmtss/edit#gid=906385082).*";
                                                    }
                                                    else
                                                    {
                                                        description = $"**Homework of <@{councilJson[ix].DiscordId}>:**\n{t[sheetIx]}";
                                                    }
                                                }
                                                trackDisplay = t[0].ToString();
                                                l++;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    foreach (var t in response.Values)
                                    {
                                        if (j < 1)
                                        {
                                            j++;
                                        }
                                        else
                                        {
                                            if (t[sheetIx].ToString() == "")
                                            {
                                                description = $"*{councilJson[ix].SheetName} has not done their homework yet.*";
                                            }
                                            else
                                            {
                                                if (t.ToString().ToCharArray().Length > 3500)
                                                {
                                                    description = $"**Homework of <@{councilJson[ix].DiscordId}>:**\n{t[sheetIx].ToString().Remove(3499)}...\n\n*For full feedback go to the [Track Council Sheet](https://docs.google.com/spreadsheets/d/1I9yFsomTcvFT4hp6eN2azsfv6MsIy1897tBFX_gmtss/edit#gid=906385082).*";
                                                }
                                                else
                                                {
                                                    description = $"**Homework of <@{councilJson[ix].DiscordId}>:**\n{t[sheetIx]}";
                                                }
                                            }
                                            trackDisplay = t[0].ToString();

                                            embed = new DiscordEmbedBuilder
                                            {
                                                Color = new DiscordColor("#FF0000"),
                                                Title = $"__**{trackDisplay}**__",
                                                Description = description,
                                                Url = "https://docs.google.com/spreadsheets/d/1I9yFsomTcvFT4hp6eN2azsfv6MsIy1897tBFX_gmtss/edit#gid=906385082",
                                                Footer = new DiscordEmbedBuilder.EmbedFooter
                                                {
                                                    Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                                                }
                                            };
                                            embeds.Add(embed);
                                        }
                                    }
                                }
                            }

                            if (sheetIx < 0)
                            {
                                embed = new DiscordEmbedBuilder
                                {
                                    Color = new DiscordColor("#FF0000"),
                                    Title = $"__**Error:**__",
                                    Description = $"*{member} could not be found on council.*" +
                                       "\n**c!gethw track/all mention/name**",
                                    Url = "https://docs.google.com/spreadsheets/d/1I9yFsomTcvFT4hp6eN2azsfv6MsIy1897tBFX_gmtss/edit#gid=906385082",
                                    Footer = new DiscordEmbedBuilder.EmbedFooter
                                    {
                                        Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                                    }
                                };
                                await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
                            }

                            else if (CompareStrings(member, "all"))
                            {
                                List<Page> pages = new List<Page>();

                                foreach (var e in embeds)
                                {
                                    pages.Add(new Page("", e));
                                }

                                await ctx.Channel.SendPaginatedMessageAsync(ctx.User, pages);
                            }

                            else if (CompareStrings(track, "all"))
                            {
                                List<Page> pages = new List<Page>();

                                foreach (var e in embeds)
                                {
                                    pages.Add(new Page("", e));
                                }

                                await ctx.Channel.SendPaginatedMessageAsync(ctx.User, pages);
                            }

                            else if (l == 0)
                            {
                                embed = new DiscordEmbedBuilder
                                {
                                    Color = new DiscordColor("#FF0000"),
                                    Title = $"__**Error:**__",
                                    Description = $"*{track} could not be found.*" +
                                       "\n**c!gethw track/all mention/name**",
                                    Url = "https://docs.google.com/spreadsheets/d/1I9yFsomTcvFT4hp6eN2azsfv6MsIy1897tBFX_gmtss/edit#gid=906385082",
                                    Footer = new DiscordEmbedBuilder.EmbedFooter
                                    {
                                        Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                                    }
                                };
                                await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
                            }
                            else
                            {
                                embed = new DiscordEmbedBuilder
                                {
                                    Color = new DiscordColor("#FF0000"),
                                    Title = $"__**{trackDisplay}**__",
                                    Description = description,
                                    Url = "https://docs.google.com/spreadsheets/d/1I9yFsomTcvFT4hp6eN2azsfv6MsIy1897tBFX_gmtss/edit#gid=906385082",
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
                    embed = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = "__**Error:**__",
                        Description = $"*{ex.Message}*" +
                               "\n**c!gethw track/all mention/name**",
                        Url = "https://docs.google.com/spreadsheets/d/1I9yFsomTcvFT4hp6eN2azsfv6MsIy1897tBFX_gmtss/edit#gid=906385082",
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

        [Command("hw")]
        [RequireRoles(RoleCheckMode.Any, "Track Council", "Admin")]
        public async Task GetHomework(CommandContext ctx, [RemainingText] string placeholder)
        {
            if (ctx.Guild.Id == 180306609233330176)
            {
                var embed = new DiscordEmbedBuilder { };
                try
                {
                    if (ctx.Channel.Id == 217126063803727872 || ctx.Channel.Id == 750123394237726847 || ctx.Channel.Id == 935200150710808626 || ctx.Channel.Id == 946835035372257320 || ctx.Channel.Id == 751534710068477953)
                    {
                        string description = string.Empty;
                        await ctx.TriggerTypingAsync();

                        string serviceAccountEmail = "brawlbox@custom-track-testing-bot.iam.gserviceaccount.com";

                        var certificate = new X509Certificate2(@"key.p12", "notasecret", X509KeyStorageFlags.Exportable);

                        ServiceAccountCredential credential = new ServiceAccountCredential(
                           new ServiceAccountCredential.Initializer(serviceAccountEmail).FromCertificate(certificate));

                        var service = new SheetsService(new BaseClientService.Initializer()
                        {
                            HttpClientInitializer = credential,
                            ApplicationName = "Custom Track Testing Bot",
                        });

                        var temp = service.Spreadsheets.Values.Get("1I9yFsomTcvFT4hp6eN2azsfv6MsIy1897tBFX_gmtss", "'Track Evaluation Log'");
                        var tempResponse = await temp.ExecuteAsync();
                        var today = int.Parse(tempResponse.Values[tempResponse.Values.Count - 1][tempResponse.Values[tempResponse.Values.Count - 1].Count - 1].ToString());

                        var request = service.Spreadsheets.Values.Get("1I9yFsomTcvFT4hp6eN2azsfv6MsIy1897tBFX_gmtss", "'Track Evaluating'");
                        var responseRaw = await request.ExecuteAsync();

                        request.ValueRenderOption = SpreadsheetsResource.ValuesResource.GetRequest.ValueRenderOptionEnum.FORMULA;
                        var response = await request.ExecuteAsync();
                        foreach (var t in response.Values)
                        {
                            while (t.Count < response.Values[0].Count)
                            {
                                t.Add("");
                            }
                        }

                        int j = 0;

                        for (int i = 1; i < response.Values.Count; i++)
                        {
                            var t = response.Values[i];
                            var tRaw = responseRaw.Values[i];
                            string tally = "*Unreviewed*";
                            if (today >= int.Parse(t[1].ToString()))
                            {
                                var emote = string.Empty;
                                if ((double.Parse(tRaw[8].ToString()) + double.Parse(tRaw[9].ToString())) / (double.Parse(tRaw[8].ToString()) + double.Parse(tRaw[9].ToString()) + double.Parse(tRaw[11].ToString())) >= 2.0 / 3.0)
                                {
                                    emote = DiscordEmoji.FromName(ctx.Client, ":Yes:");
                                }
                                else
                                {
                                    emote = DiscordEmoji.FromName(ctx.Client, ":No:");
                                }
                                tally = $"{tRaw[8]}/{tRaw[9]}/{tRaw[10]}/{tRaw[11]} {emote}";
                            }
                            try
                            {
                                description += $"{t[0]} | {tRaw[1]} | [Download]({t[4].ToString().Split('"')[1]}) | {tally}\n";
                            }
                            catch
                            {
                                description += $"{t[0]} | {tRaw[1]} | *Check spreadsheet for download* | {tally}\n";
                            }
                        }
                        embed = new DiscordEmbedBuilder
                        {
                            Color = new DiscordColor("#FF0000"),
                            Title = $"__**Council Homework:**__",
                            Description = description,
                            Url = "https://docs.google.com/spreadsheets/d/1I9yFsomTcvFT4hp6eN2azsfv6MsIy1897tBFX_gmtss/edit#gid=906385082",
                            Footer = new DiscordEmbedBuilder.EmbedFooter
                            {
                                Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                            }
                        };
                        await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
                    }
                }
                catch (Exception ex)
                {
                    embed = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = "__**Error:**__",
                        Description = $"*{ex.Message}*" +
                               "\n**c!hw**",
                        Url = "https://docs.google.com/spreadsheets/d/1I9yFsomTcvFT4hp6eN2azsfv6MsIy1897tBFX_gmtss/edit#gid=906385082",
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

        [Command("staff")]
        public async Task GetStaffGhosts(CommandContext ctx, [RemainingText] string track = "Luigi Circuit")
        {
            await ctx.TriggerTypingAsync();

            var json = string.Empty;
            var description = string.Empty;
            var embed = new DiscordEmbedBuilder { };

            string serviceAccountEmail = "brawlbox@custom-track-testing-bot.iam.gserviceaccount.com";

            var certificate = new X509Certificate2(@"key.p12", "notasecret", X509KeyStorageFlags.Exportable);

            ServiceAccountCredential credential = new ServiceAccountCredential(
               new ServiceAccountCredential.Initializer(serviceAccountEmail).FromCertificate(certificate));

            var service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "Custom Track Testing Bot",
            });

            var request = service.Spreadsheets.Values.Get("1xwhKoyypCWq5tCRTI69ijJoDiaoAVsvYAxz-q4UBNqM", "'Staff Ghosts'");
            var response = await request.ExecuteAsync();

            try
            {
                json = File.ReadAllText("rts.json");
                List<Track> trackList = JsonConvert.DeserializeObject<List<Track>>(json);

                json = File.ReadAllText("cts.json");
                foreach (var t in JsonConvert.DeserializeObject<List<Track>>(json))
                {
                    trackList.Add(t);
                }

                int j = 0;
                Track trackDisplay = new Track();

                for (int i = 0; i < trackList.Count; i++)
                {
                    if (j < 1)
                    {
                        if (CompareIncompleteStrings(trackList[i].Name, track) || CompareStringAbbreviation(track, trackList[i].Name) || CompareStringsLevenshteinDistance(track, trackList[i].Name))
                        {
                            for (int h = 0; i < response.Values.Count; h++)
                            {
                                if (h != 0)
                                {
                                    if (CompareIncompleteStrings(response.Values[h][0].ToString(), track) || CompareStringAbbreviation(track, response.Values[h][0].ToString()) || CompareStringsLevenshteinDistance(track, response.Values[h][0].ToString()))
                                    {
                                        description = $"**Easy:**\n*[{response.Values[h][1]}](https://chadsoft.co.uk/time-trials/rkgd/{response.Values[h + 251][1].ToString().Substring(0, 2)}/{response.Values[h + 251][1].ToString().Substring(2, 2)}/{response.Values[h + 251][1].ToString().Substring(4)}.html)*\n**Expert:**\n*[{response.Values[h][2]}](https://chadsoft.co.uk/time-trials/rkgd/{response.Values[h + 251][2].ToString().Substring(0, 2)}/{response.Values[h + 251][2].ToString().Substring(2, 2)}/{response.Values[h + 251][2].ToString().Substring(4)}.html)*";
                                        trackDisplay = trackList[i];
                                        j++;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                if (j < 1)
                {
                    embed = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = "__**Error:**__",
                        Description = $"*{track} could not be found.*" +
                               "\n**c!staff track**",
                        Url = "https://docs.google.com/spreadsheets/d/1xwhKoyypCWq5tCRTI69ijJoDiaoAVsvYAxz-q4UBNqM/edit#gid=1188255728",
                        Footer = new DiscordEmbedBuilder.EmbedFooter
                        {
                            Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                        }
                    };
                    await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
                }
                else if (j == 1)
                {
                    embed = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = $"__**Staff ghosts for {trackDisplay.Name} *(First result)*:**__",
                        Description = description,
                        Url = "https://docs.google.com/spreadsheets/d/1xwhKoyypCWq5tCRTI69ijJoDiaoAVsvYAxz-q4UBNqM/edit#gid=1188255728",
                        Footer = new DiscordEmbedBuilder.EmbedFooter
                        {
                            Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                        }
                    };
                    await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                embed = new DiscordEmbedBuilder
                {
                    Color = new DiscordColor("#FF0000"),
                    Title = "__**Error:**__",
                    Description = $"*{ex.Message}*" +
                           "\n**c!staff track**",
                    Url = "https://docs.google.com/spreadsheets/d/1xwhKoyypCWq5tCRTI69ijJoDiaoAVsvYAxz-q4UBNqM/edit#gid=1188255728",
                    Footer = new DiscordEmbedBuilder.EmbedFooter
                    {
                        Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                    }
                };
                await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);

                Console.WriteLine(ex.ToString());
            }
        }

        [Command("info")]
        public async Task GetTrackInfo(CommandContext ctx, [RemainingText] string track = "")
        {
            await ctx.TriggerTypingAsync();

            var json = string.Empty;
            var description = string.Empty;
            var embed = new DiscordEmbedBuilder { };

            string serviceAccountEmail = "brawlbox@custom-track-testing-bot.iam.gserviceaccount.com";

            var certificate = new X509Certificate2(@"key.p12", "notasecret", X509KeyStorageFlags.Exportable);

            ServiceAccountCredential credential = new ServiceAccountCredential(
               new ServiceAccountCredential.Initializer(serviceAccountEmail).FromCertificate(certificate));

            var service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "Custom Track Testing Bot",
            });

            var request = service.Spreadsheets.Values.Get("1xwhKoyypCWq5tCRTI69ijJoDiaoAVsvYAxz-q4UBNqM", "'CTGP Track Issues'");
            var response = await request.ExecuteAsync();
            foreach (var t in response.Values)
            {
                while (t.Count < 7)
                {
                    t.Add("");
                }
            }

            try
            {
                json = File.ReadAllText("cts.json");
                List<Track> trackList = JsonConvert.DeserializeObject<List<Track>>(json);

                int j = 0;
                Track trackDisplay = new Track();

                for (int i = 0; i < trackList.Count; i++)
                {
                    if (j < 1)
                    {
                        if (CompareIncompleteStrings(trackList[i].Name, track) || CompareStringAbbreviation(track, trackList[i].Name) || CompareStringsLevenshteinDistance(track, trackList[i].Name))
                        {
                            foreach (var t in response.Values)
                            {
                                if (CompareIncompleteStrings(t[0].ToString(), track) || CompareStringAbbreviation(track, t[0].ToString()) || CompareStringsLevenshteinDistance(track, t[0].ToString()))
                                {
                                    description = $"**Author:**\n*{t[1]}*\n**Version:**\n*{t[2]}*\n**Track/Music Slots:**\n*{t[3]}*\n**Speed/Lap Count:**\n*{t[4]}*";
                                    trackDisplay = trackList[i];
                                    j++;
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        break;
                    }
                }

                if (track == "")
                {
                    embed = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = "__**Error:**__",
                        Description = $"*No track inputted.*" +
                           "\n**c!info track**",
                        Url = "https://docs.google.com/spreadsheets/d/1xwhKoyypCWq5tCRTI69ijJoDiaoAVsvYAxz-q4UBNqM/edit#gid=1971102004",
                        Footer = new DiscordEmbedBuilder.EmbedFooter
                        {
                            Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                        }
                    };
                    await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
                }

                else if (j < 1)
                {
                    embed = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = "__**Error:**__",
                        Description = $"*{track} could not be found.\nThe track does not exist, or is not in CTGP.*" +
                        "\n**c!info track**",
                        Url = "https://docs.google.com/spreadsheets/d/1xwhKoyypCWq5tCRTI69ijJoDiaoAVsvYAxz-q4UBNqM/edit#gid=1971102004",
                        Footer = new DiscordEmbedBuilder.EmbedFooter
                        {
                            Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                        }
                    };
                    await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
                }
                else if (j == 1)
                {
                    embed = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = $"__**{trackDisplay.Name} *(First result)*:**__",
                        Description = description,
                        Url = "https://docs.google.com/spreadsheets/d/1xwhKoyypCWq5tCRTI69ijJoDiaoAVsvYAxz-q4UBNqM/edit#gid=1971102004",
                        Footer = new DiscordEmbedBuilder.EmbedFooter
                        {
                            Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                        }
                    };
                    await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                embed = new DiscordEmbedBuilder
                {
                    Color = new DiscordColor("#FF0000"),
                    Title = "__**Error:**__",
                    Description = $"*{ex.Message}*" +
                           "\n**c!info track**",
                    Url = "https://docs.google.com/spreadsheets/d/1xwhKoyypCWq5tCRTI69ijJoDiaoAVsvYAxz-q4UBNqM/edit#gid=1971102004",
                    Footer = new DiscordEmbedBuilder.EmbedFooter
                    {
                        Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                    }
                };
                await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);

                Console.WriteLine(ex.ToString());
            }
        }

        [Command("issues")]
        public async Task GetTrackIssues(CommandContext ctx, [RemainingText] string track = "")
        {
            await ctx.TriggerTypingAsync();

            var json = string.Empty;
            var description = string.Empty;
            var embed = new DiscordEmbedBuilder { };

            string serviceAccountEmail = "brawlbox@custom-track-testing-bot.iam.gserviceaccount.com";

            var certificate = new X509Certificate2(@"key.p12", "notasecret", X509KeyStorageFlags.Exportable);

            ServiceAccountCredential credential = new ServiceAccountCredential(
               new ServiceAccountCredential.Initializer(serviceAccountEmail).FromCertificate(certificate));

            var service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "Custom Track Testing Bot",
            });

            var request = service.Spreadsheets.Values.Get("1xwhKoyypCWq5tCRTI69ijJoDiaoAVsvYAxz-q4UBNqM", "'CTGP Track Issues'");
            var response = await request.ExecuteAsync();
            foreach (var t in response.Values)
            {
                while (t.Count < 7)
                {
                    t.Add("");
                }
            }

            try
            {
                json = File.ReadAllText("cts.json");
                List<Track> trackList = JsonConvert.DeserializeObject<List<Track>>(json);

                int j = 0;
                Track trackDisplay = new Track();
                string maj = string.Empty;
                string min = string.Empty;

                if (track == "")
                {
                    Dictionary<string, int> issueCount = new Dictionary<string, int>();

                    foreach (var v in response.Values)
                    {
                        if (v[0].ToString() != "Track")
                        {
                            int count = v[5].ToString().Count(c => c == '\n') + v[6].ToString().Count(c => c == '\n');
                            if (v[5].ToString().ToCharArray().Length != 0 && v[5].ToString().ToCharArray()[0] == '-')
                            {
                                count++;
                            }
                            if (v[6].ToString().ToCharArray().Length != 0 && v[6].ToString().ToCharArray()[0] == '-')
                            {
                                count++;
                            }
                            issueCount.Add(v[0].ToString(), count);
                        }
                    }
                    issueCount = issueCount.OrderByDescending(a => a.Value).ToDictionary(a => a.Key, a => a.Value);

                    List<DiscordEmbedBuilder> embeds = new List<DiscordEmbedBuilder>();

                    foreach (var t in issueCount.Keys.ToList())
                    {
                        for (int i = 0; i < response.Values.Count; i++)
                        {
                            if (CompareIncompleteStrings(t, response.Values[i][0].ToString()))
                            {
                                if (response.Values[i][5].ToString() == "")
                                {
                                    maj = "-No reported bugs";
                                }
                                else
                                {
                                    maj = response.Values[i][5].ToString();
                                }
                                if (response.Values[i][6].ToString() == "")
                                {
                                    min = "-No reported bugs";
                                }
                                else
                                {
                                    min = response.Values[i][6].ToString();
                                }
                                description = $"**Major:**\n*{maj}*\n**Minor:**\n*{min}*";
                                j++;
                                embed = new DiscordEmbedBuilder
                                {
                                    Color = new DiscordColor("#FF0000"),
                                    Title = $"__**Known issues on {response.Values[i][0]}:**__",
                                    Description = description,
                                    Url = "https://docs.google.com/spreadsheets/d/1xwhKoyypCWq5tCRTI69ijJoDiaoAVsvYAxz-q4UBNqM/edit#gid=1971102004",
                                    Footer = new DiscordEmbedBuilder.EmbedFooter
                                    {
                                        Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                                    }
                                };
                                embeds.Add(embed);
                            }
                        }
                    }

                    List<Page> pages = new List<Page>();

                    foreach (var e in embeds)
                    {
                        pages.Add(new Page("", e));
                    }

                    await ctx.Channel.SendPaginatedMessageAsync(ctx.User, pages);
                }
                else
                {

                    for (int i = 0; i < trackList.Count; i++)
                    {
                        if (j < 1)
                        {
                            if (CompareIncompleteStrings(trackList[i].Name, track) || CompareStringAbbreviation(track, trackList[i].Name) || CompareStringsLevenshteinDistance(track, trackList[i].Name))
                            {
                                foreach (var t in response.Values)
                                {
                                    if (CompareIncompleteStrings(t[0].ToString(), track) || CompareStringAbbreviation(track, t[0].ToString()) || CompareStringsLevenshteinDistance(track, t[0].ToString()))
                                    {
                                        if (t[5].ToString() == "")
                                        {
                                            maj = "-No reported bugs";
                                        }
                                        else
                                        {
                                            maj = t[5].ToString();
                                        }
                                        if (t[6].ToString() == "")
                                        {
                                            min = "-No reported bugs";
                                        }
                                        else
                                        {
                                            min = t[6].ToString();
                                        }
                                        description = $"**Major:**\n*{maj}*\n**Minor:**\n*{min}*";
                                        trackDisplay = trackList[i];
                                        j++;
                                        break;
                                    }
                                }
                            }
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (j < 1)
                    {
                        embed = new DiscordEmbedBuilder
                        {
                            Color = new DiscordColor("#FF0000"),
                            Title = "__**Error:**__",
                            Description = $"*{track} could not be found.\nThe track does not exist, or is not in CTGP.*" +
                            "\n**c!issues track**",
                            Url = "https://docs.google.com/spreadsheets/d/1xwhKoyypCWq5tCRTI69ijJoDiaoAVsvYAxz-q4UBNqM/edit#gid=1971102004",
                            Footer = new DiscordEmbedBuilder.EmbedFooter
                            {
                                Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                            }
                        };
                        await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
                    }
                    else if (j == 1)
                    {
                        embed = new DiscordEmbedBuilder
                        {
                            Color = new DiscordColor("#FF0000"),
                            Title = $"__**Known issues on {trackDisplay.Name} *(First result)*:**__",
                            Description = description,
                            Url = "https://docs.google.com/spreadsheets/d/1xwhKoyypCWq5tCRTI69ijJoDiaoAVsvYAxz-q4UBNqM/edit#gid=1971102004",
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
                embed = new DiscordEmbedBuilder
                {
                    Color = new DiscordColor("#FF0000"),
                    Title = "__**Error:**__",
                    Description = $"*{ex.Message}*" +
                           "\n**c!issues track**",
                    Url = "https://docs.google.com/spreadsheets/d/1xwhKoyypCWq5tCRTI69ijJoDiaoAVsvYAxz-q4UBNqM/edit#gid=1971102004",
                    Footer = new DiscordEmbedBuilder.EmbedFooter
                    {
                        Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                    }
                };
                await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);

                Console.WriteLine(ex.ToString());
            }
        }

        [Command("reportissue")]
        [RequireRoles(RoleCheckMode.Any, "Admin")]
        public async Task ReportIssue(CommandContext ctx, string issueType = "", string track = "", [RemainingText] string issue = "")
        {
            if (ctx.Guild.Id == 180306609233330176)
            {
                await ctx.TriggerTypingAsync();

                var embed = new DiscordEmbedBuilder { };
                string json = string.Empty;
                string maj = string.Empty;
                string min = string.Empty;

                string serviceAccountEmail = "brawlbox@custom-track-testing-bot.iam.gserviceaccount.com";

                var certificate = new X509Certificate2(@"key.p12", "notasecret", X509KeyStorageFlags.Exportable);

                ServiceAccountCredential credential = new ServiceAccountCredential(
                   new ServiceAccountCredential.Initializer(serviceAccountEmail).FromCertificate(certificate));

                var service = new SheetsService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "Custom Track Testing Bot",
                });

                var request = service.Spreadsheets.Values.Get("1xwhKoyypCWq5tCRTI69ijJoDiaoAVsvYAxz-q4UBNqM", "'CTGP Track Issues'!A1:G219");
                var response = await request.ExecuteAsync();
                foreach (var t in response.Values)
                {
                    while (t.Count < 7)
                    {
                        t.Add("");
                    }
                }

                int k = 0;

                if (CompareStrings(issueType, "major"))
                {
                    k = 5;
                }

                else if (CompareStrings(issueType, "minor"))
                {
                    k = 6;
                }

                try
                {
                    json = File.ReadAllText("cts.json");
                    List<Track> trackList = JsonConvert.DeserializeObject<List<Track>>(json);

                    int j = 0;

                    if (k != 0 && issue != "")
                    {
                        for (int i = 0; i < trackList.Count; i++)
                        {
                            if (j < 1)
                            {
                                if (CompareStrings(trackList[i].Name, track))
                                {
                                    foreach (var t in response.Values)
                                    {
                                        if (CompareStrings(t[0].ToString(), track))
                                        {
                                            if (t[k].ToString() != "")
                                            {
                                                t[k] = $"{t[k]}\n{issue}";
                                            }
                                            else
                                            {
                                                t[k] = issue;
                                            }
                                            if (t[5].ToString() == "")
                                            {
                                                maj = "-No reported bugs";
                                            }
                                            else
                                            {
                                                maj = t[5].ToString();
                                            }
                                            if (t[6].ToString() == "")
                                            {
                                                min = "-No reported bugs";
                                            }
                                            else
                                            {
                                                min = t[6].ToString();
                                            }
                                            j++;
                                            break;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                    if (issue == "")
                    {
                        embed = new DiscordEmbedBuilder
                        {
                            Color = new DiscordColor("#FF0000"),
                            Title = "__**Error:**__",
                            Description = $"*No issue was inputted.*" +
                                   "\n**c!reportissue major/minor \"track\" -Issue**",
                            Url = "https://docs.google.com/spreadsheets/d/1xwhKoyypCWq5tCRTI69ijJoDiaoAVsvYAxz-q4UBNqM/edit#gid=1971102004",
                            Footer = new DiscordEmbedBuilder.EmbedFooter
                            {
                                Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                            }
                        };
                        await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
                    }

                    else if (j < 1)
                    {
                        embed = new DiscordEmbedBuilder
                        {
                            Color = new DiscordColor("#FF0000"),
                            Title = "__**Error:**__",
                            Description = $"*{track} could not be found.*" +
                                      "\n**c!reportissue major/minor \"track\" -Issue**",
                            Url = "https://docs.google.com/spreadsheets/d/1xwhKoyypCWq5tCRTI69ijJoDiaoAVsvYAxz-q4UBNqM/edit#gid=1971102004",
                            Footer = new DiscordEmbedBuilder.EmbedFooter
                            {
                                Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                            }
                        };
                        await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
                    }

                    else if (k != 0 && issue != "")
                    {
                        var updateRequest = service.Spreadsheets.Values.Update(response, "1xwhKoyypCWq5tCRTI69ijJoDiaoAVsvYAxz-q4UBNqM", "'CTGP Track Issues'!A1:G219");
                        updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.RAW;
                        var update = await updateRequest.ExecuteAsync();

                        embed = new DiscordEmbedBuilder
                        {
                            Color = new DiscordColor("#FF0000"),
                            Title = "__**Issues Updated:**__",
                            Description = $"**Major:**\n*{maj}*\n**Minor:**\n*{min}*",
                            Url = "https://docs.google.com/spreadsheets/d/1xwhKoyypCWq5tCRTI69ijJoDiaoAVsvYAxz-q4UBNqM/edit#gid=1971102004",
                            Footer = new DiscordEmbedBuilder.EmbedFooter
                            {
                                Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                            }
                        };
                        await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
                    }

                    else
                    {
                        embed = new DiscordEmbedBuilder
                        {
                            Color = new DiscordColor("#FF0000"),
                            Title = "__**Error:**__",
                            Description = $"*{issueType} is not a valid issue category.*" +
                                   "\n**c!reportissue major/minor \"track\" -Issue**",
                            Url = "https://docs.google.com/spreadsheets/d/1xwhKoyypCWq5tCRTI69ijJoDiaoAVsvYAxz-q4UBNqM/edit#gid=1971102004",
                            Footer = new DiscordEmbedBuilder.EmbedFooter
                            {
                                Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                            }
                        };
                        await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
                    }
                }
                catch (Exception ex)
                {
                    embed = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = "__**Error:**__",
                        Description = $"*{ex.Message}*" +
                                   "\n**c!reportissue major/minor \"track\" -Issue**",
                        Url = "https://docs.google.com/spreadsheets/d/1xwhKoyypCWq5tCRTI69ijJoDiaoAVsvYAxz-q4UBNqM/edit#gid=1971102004",
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

        [Command("subissues")]
        public async Task GetSubmittedTrackIssues(CommandContext ctx, [RemainingText] string track = "")
        {
            await ctx.TriggerTypingAsync();

            var embed = new DiscordEmbedBuilder { };

            string trackDisplay = string.Empty;

            string serviceAccountEmail = "brawlbox@custom-track-testing-bot.iam.gserviceaccount.com";

            var certificate = new X509Certificate2(@"key.p12", "notasecret", X509KeyStorageFlags.Exportable);

            ServiceAccountCredential credential = new ServiceAccountCredential(
               new ServiceAccountCredential.Initializer(serviceAccountEmail).FromCertificate(certificate));

            var service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "Custom Track Testing Bot",
            });

            var request = service.Spreadsheets.Values.Get("1xwhKoyypCWq5tCRTI69ijJoDiaoAVsvYAxz-q4UBNqM", "'Backlog'");
            var response = await request.ExecuteAsync();
            foreach (var t in response.Values)
            {
                while (t.Count < 11)
                {
                    t.Add("");
                }
            }

            try
            {

                int j = 0;
                string maj = string.Empty;
                string min = string.Empty;
                string sub = string.Empty;

                if (track == "")
                {
                    Dictionary<string, int> issueCount = new Dictionary<string, int>();

                    for (int i = 5; i < response.Values.Count; i++)
                    {
                        if (response.Values[i][2].ToString() != "Track Name" && response.Values[i][2].ToString() != "delimiter" && response.Values[i][2].ToString() != "")
                        {
                            int count = response.Values[i][10].ToString().Count(c => c == '\n');
                            if (response.Values[i][10].ToString().ToCharArray().Length != 0 && response.Values[i][10].ToString().ToCharArray()[0] == '-')
                            {
                                count++;
                            }
                            issueCount.Add(response.Values[i][2].ToString(), count);
                        }
                    }
                    issueCount = issueCount.OrderByDescending(a => a.Value).ToDictionary(a => a.Key, a => a.Value);

                    List<DiscordEmbedBuilder> embeds = new List<DiscordEmbedBuilder>();

                    foreach (var t in issueCount.Keys.ToList())
                    {
                        for (int i = 5; i < response.Values.Count; i++)
                        {
                            if (response.Values[i][2].ToString() != "Track Name" && response.Values[i][2].ToString() != "delimiter" && response.Values[i][2].ToString() != "")
                            {
                                if (CompareIncompleteStrings(t, response.Values[i][2].ToString()))
                                {
                                    if (response.Values[i][8].ToString() == "")
                                    {
                                        maj = "-No reported bugs";
                                    }
                                    else
                                    {
                                        maj = response.Values[i][8].ToString();
                                    }
                                    if (response.Values[i][9].ToString() == "")
                                    {
                                        min = "-No reported bugs";
                                    }
                                    else
                                    {
                                        min = response.Values[i][9].ToString();
                                    }
                                    if (response.Values[i][10].ToString() == "")
                                    {
                                        sub = "-No reported bugs";
                                    }
                                    else
                                    {
                                        sub = response.Values[i][10].ToString();
                                    }
                                    j++;
                                    embed = new DiscordEmbedBuilder
                                    {
                                        Color = new DiscordColor("#FF0000"),
                                        Title = $"__**Known issues on {response.Values[i][2]}:**__",
                                        Description = $"**Major Issues:**\n*{maj}*\n**Minor Issues:**\n*{min}*\n**Submitted Issues:**\n*{sub}*",
                                        Url = "https://docs.google.com/spreadsheets/d/1xwhKoyypCWq5tCRTI69ijJoDiaoAVsvYAxz-q4UBNqM/edit#gid=1971102004",
                                        Footer = new DiscordEmbedBuilder.EmbedFooter
                                        {
                                            Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                                        }
                                    };
                                    embeds.Add(embed);
                                }
                            }
                        }
                    }

                    List<Page> pages = new List<Page>();

                    foreach (var e in embeds)
                    {
                        pages.Add(new Page("", e));
                    }

                    await ctx.Channel.SendPaginatedMessageAsync(ctx.User, pages);
                }
                else
                {
                    if (j < 1)
                    {
                        foreach (var t in response.Values)
                        {
                            if (t[2].ToString() != "Track Name" && t[2].ToString() != "delimiter" && t[2].ToString() != "")
                            {
                                if (CompareIncompleteStrings(t[2].ToString(), track) || CompareStringAbbreviation(track, t[2].ToString()) || CompareStringsLevenshteinDistance(track, t[2].ToString()))
                                {
                                    if (t[8].ToString() == "")
                                    {
                                        maj = "-No reported bugs";
                                    }
                                    else
                                    {
                                        maj = t[8].ToString();
                                    }
                                    if (t[9].ToString() == "")
                                    {
                                        min = "-No reported bugs";
                                    }
                                    else
                                    {
                                        min = t[9].ToString();
                                    }
                                    if (t[10].ToString() == "")
                                    {
                                        sub = "-No reported bugs";
                                    }
                                    else
                                    {
                                        sub = t[10].ToString();
                                    }
                                    trackDisplay = t[2].ToString();
                                    j++;
                                    break;
                                }
                            }
                        }
                    }

                    if (j < 1)
                    {
                        embed = new DiscordEmbedBuilder
                        {
                            Color = new DiscordColor("#FF0000"),
                            Title = "__**Error:**__",
                            Description = $"*{track} could not be found*" +
                            "\n**c!subissues track**",
                            Url = "https://docs.google.com/spreadsheets/d/1xwhKoyypCWq5tCRTI69ijJoDiaoAVsvYAxz-q4UBNqM/edit#gid=1971102004",
                            Footer = new DiscordEmbedBuilder.EmbedFooter
                            {
                                Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                            }
                        };
                        await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
                    }
                    else if (j == 1)
                    {
                        embed = new DiscordEmbedBuilder
                        {
                            Color = new DiscordColor("#FF0000"),
                            Title = $"__**Known issues on {trackDisplay} *(First result)*:**__",
                            Description = $"**Major Issues:**\n*{maj}*\n**Minor Issues:**\n*{min}*\n**Submitted Issues:**\n*{sub}*",
                            Url = "https://docs.google.com/spreadsheets/d/1xwhKoyypCWq5tCRTI69ijJoDiaoAVsvYAxz-q4UBNqM/edit#gid=1971102004",
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
                embed = new DiscordEmbedBuilder
                {
                    Color = new DiscordColor("#FF0000"),
                    Title = "__**Error:**__",
                    Description = $"*{ex.Message}*" +
                           "\n**c!subissues track**",
                    Url = "https://docs.google.com/spreadsheets/d/1xwhKoyypCWq5tCRTI69ijJoDiaoAVsvYAxz-q4UBNqM/edit#gid=1971102004",
                    Footer = new DiscordEmbedBuilder.EmbedFooter
                    {
                        Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                    }
                };
                await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);

                Console.WriteLine(ex.ToString());
            }
        }

        [Command("subreportissue")]
        [RequireRoles(RoleCheckMode.Any, "Admin")]
        public async Task SubmissionReportIssue(CommandContext ctx, string track = "", [RemainingText] string issue = "")
        {
            if (ctx.Guild.Id == 180306609233330176)
            {
                await ctx.TriggerTypingAsync();

                var embed = new DiscordEmbedBuilder { };
                string text = string.Empty;

                string serviceAccountEmail = "brawlbox@custom-track-testing-bot.iam.gserviceaccount.com";

                var certificate = new X509Certificate2(@"key.p12", "notasecret", X509KeyStorageFlags.Exportable);

                ServiceAccountCredential credential = new ServiceAccountCredential(
                   new ServiceAccountCredential.Initializer(serviceAccountEmail).FromCertificate(certificate));

                var service = new SheetsService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "Custom Track Testing Bot",
                });

                var tmprequest = service.Spreadsheets.Values.Get("1xwhKoyypCWq5tCRTI69ijJoDiaoAVsvYAxz-q4UBNqM", "'Backlog'");
                tmprequest.ValueRenderOption = SpreadsheetsResource.ValuesResource.GetRequest.ValueRenderOptionEnum.FORMULA;
                var tmpresponse = await tmprequest.ExecuteAsync();
                foreach (var t in tmpresponse.Values)
                {
                    while (t.Count < 12)
                    {
                        t.Add("");
                    }
                }

                string strAlpha = "";

                for (int i = 65; i <= 90; i++)
                {
                    strAlpha += ((char)i).ToString() + "";
                }

                var request = service.Spreadsheets.Values.Get("1xwhKoyypCWq5tCRTI69ijJoDiaoAVsvYAxz-q4UBNqM", $"'Backlog'!A5:{strAlpha[tmpresponse.Values[0].Count - 1]}{tmpresponse.Values.Count}");
                request.ValueRenderOption = SpreadsheetsResource.ValuesResource.GetRequest.ValueRenderOptionEnum.FORMULA;
                var response = await request.ExecuteAsync();
                foreach (var t in response.Values)
                {
                    while (t.Count < 12)
                    {
                        t.Add("");
                    }
                }

                try
                {
                    int j = 0;

                    if (issue != "")
                    {
                        foreach (var t in response.Values)
                        {
                            if (t[2].ToString() != "Track Name" && t[2].ToString() != "delimiter" && t[2].ToString() != "")
                            {
                                if (CompareStrings(t[2].ToString(), track))
                                {
                                    if (t[10].ToString() != "")
                                    {
                                        t[10] = $"{t[10]}\n{issue}";
                                    }
                                    else
                                    {
                                        t[10] = issue;
                                    }
                                    text = t[10].ToString();
                                    j++;
                                    break;
                                }
                            }
                        }
                        if (issue == "")
                        {
                            embed = new DiscordEmbedBuilder
                            {
                                Color = new DiscordColor("#FF0000"),
                                Title = "__**Error:**__",
                                Description = $"*No issue was inputted.*" +
                                       "\n**c!subreportissue \"track\" -Issue**",
                                Url = "https://docs.google.com/spreadsheets/d/1xwhKoyypCWq5tCRTI69ijJoDiaoAVsvYAxz-q4UBNqM/edit#gid=1971102004",
                                Footer = new DiscordEmbedBuilder.EmbedFooter
                                {
                                    Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                                }
                            };
                            await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
                        }

                        else if (j < 1)
                        {
                            embed = new DiscordEmbedBuilder
                            {
                                Color = new DiscordColor("#FF0000"),
                                Title = "__**Error:**__",
                                Description = $"*{track} could not be found.*" +
                                          "\n**c!subreportissue \"track\" -Issue**",
                                Url = "https://docs.google.com/spreadsheets/d/1xwhKoyypCWq5tCRTI69ijJoDiaoAVsvYAxz-q4UBNqM/edit#gid=1971102004",
                                Footer = new DiscordEmbedBuilder.EmbedFooter
                                {
                                    Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                                }
                            };
                            await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
                        }

                        else
                        {

                            var updateRequest = service.Spreadsheets.Values.Update(response, "1xwhKoyypCWq5tCRTI69ijJoDiaoAVsvYAxz-q4UBNqM", $"Backlog!A5:{strAlpha[response.Values[0].Count - 1]}{response.Values.Count + 4}");
                            updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;
                            var update = await updateRequest.ExecuteAsync();

                            embed = new DiscordEmbedBuilder
                            {
                                Color = new DiscordColor("#FF0000"),
                                Title = "__**Issues Updated:**__",
                                Description = $"*{text}*",
                                Url = "https://docs.google.com/spreadsheets/d/1xwhKoyypCWq5tCRTI69ijJoDiaoAVsvYAxz-q4UBNqM/edit#gid=1971102004",
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
                    embed = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = "__**Error:**__",
                        Description = $"*{ex.Message}*" +
                                   "\n**c!subreportissue major/minor \"track\" -Issue**",
                        Url = "https://docs.google.com/spreadsheets/d/1xwhKoyypCWq5tCRTI69ijJoDiaoAVsvYAxz-q4UBNqM/edit#gid=1971102004",
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

        [Command("clearissues")]
        [RequireRoles(RoleCheckMode.Any, "Admin")]
        public async Task ClearTrackIssues(CommandContext ctx, [RemainingText] string track = "")
        {
            if (ctx.Guild.Id == 180306609233330176)
            {
                await ctx.TriggerTypingAsync();

                var embed = new DiscordEmbedBuilder { };
                string json = string.Empty;
                if (track == "")
                {
                    embed = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = "__**Error:**__",
                        Description = $"*No track was inputted*" +
                                      "\n**c!clearissues track**",
                        Url = "https://docs.google.com/spreadsheets/d/1xwhKoyypCWq5tCRTI69ijJoDiaoAVsvYAxz-q4UBNqM/edit#gid=1971102004",
                        Footer = new DiscordEmbedBuilder.EmbedFooter
                        {
                            Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                        }
                    };
                    await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
                }
                else
                {
                    string serviceAccountEmail = "brawlbox@custom-track-testing-bot.iam.gserviceaccount.com";

                    var certificate = new X509Certificate2(@"key.p12", "notasecret", X509KeyStorageFlags.Exportable);

                    ServiceAccountCredential credential = new ServiceAccountCredential(
                       new ServiceAccountCredential.Initializer(serviceAccountEmail).FromCertificate(certificate));

                    var service = new SheetsService(new BaseClientService.Initializer()
                    {
                        HttpClientInitializer = credential,
                        ApplicationName = "Custom Track Testing Bot",
                    });

                    var request = service.Spreadsheets.Values.Get("1xwhKoyypCWq5tCRTI69ijJoDiaoAVsvYAxz-q4UBNqM", "'CTGP Track Issues'!A1:G219");
                    var response = await request.ExecuteAsync();
                    foreach (var t in response.Values)
                    {
                        while (t.Count < 7)
                        {
                            t.Add("");
                        }
                    }
                    try
                    {
                        json = File.ReadAllText("cts.json");
                        List<Track> trackList = JsonConvert.DeserializeObject<List<Track>>(json);

                        int j = 0;

                        for (int i = 0; i < trackList.Count; i++)
                        {
                            if (j < 1)
                            {
                                if (CompareStrings(trackList[i].Name, track))
                                {
                                    foreach (var t in response.Values)
                                    {
                                        if (CompareStrings(t[0].ToString(), track))
                                        {
                                            t[5] = "";
                                            t[6] = "";
                                            j++;
                                            break;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                break;
                            }
                        }

                        var updateRequest = service.Spreadsheets.Values.Update(response, "1xwhKoyypCWq5tCRTI69ijJoDiaoAVsvYAxz-q4UBNqM", "'CTGP Track Issues'!A1:G219");
                        updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.RAW;
                        var update = await updateRequest.ExecuteAsync();

                        embed = new DiscordEmbedBuilder
                        {
                            Color = new DiscordColor("#FF0000"),
                            Title = "__**Success:**__",
                            Description = $"*{track} issues have been cleared*",
                            Url = "https://docs.google.com/spreadsheets/d/1xwhKoyypCWq5tCRTI69ijJoDiaoAVsvYAxz-q4UBNqM/edit#gid=1971102004",
                            Footer = new DiscordEmbedBuilder.EmbedFooter
                            {
                                Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                            }
                        };
                        await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        embed = new DiscordEmbedBuilder
                        {
                            Color = new DiscordColor("#FF0000"),
                            Title = "__**Error:**__",
                            Description = $"*{ex.Message}*" +
                                      "\n**c!clearissues track**",
                            Url = "https://docs.google.com/spreadsheets/d/1xwhKoyypCWq5tCRTI69ijJoDiaoAVsvYAxz-q4UBNqM/edit#gid=1971102004",
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

        [Command("replaceissues")]
        [RequireRoles(RoleCheckMode.Any, "Admin")]
        public async Task ReplaceTrackIssues(CommandContext ctx, string track = "", string newTrack = "", string author = "", string version = "", string slot = "", string laps = "")
        {
            if (ctx.Guild.Id == 180306609233330176)
            {
                await ctx.TriggerTypingAsync();

                var embed = new DiscordEmbedBuilder { };
                string json = string.Empty;
                string description = string.Empty;

                if (track == "" || newTrack == "" || author == "" || version == "" || slot == "" || laps == "")
                {
                    embed = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = "__**Error:**__",
                        Description = $"*One of your arguments is missing data.*" +
                                      "\n**c!replaceissues \"old track\" \"new track\" \"author\" \"version\" \"slot\" laps**",
                        Url = "https://docs.google.com/spreadsheets/d/1xwhKoyypCWq5tCRTI69ijJoDiaoAVsvYAxz-q4UBNqM/edit#gid=1971102004",
                        Footer = new DiscordEmbedBuilder.EmbedFooter
                        {
                            Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                        }
                    };
                    await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
                }
                else
                {
                    string serviceAccountEmail = "brawlbox@custom-track-testing-bot.iam.gserviceaccount.com";

                    var certificate = new X509Certificate2(@"key.p12", "notasecret", X509KeyStorageFlags.Exportable);

                    ServiceAccountCredential credential = new ServiceAccountCredential(
                       new ServiceAccountCredential.Initializer(serviceAccountEmail).FromCertificate(certificate));

                    var service = new SheetsService(new BaseClientService.Initializer()
                    {
                        HttpClientInitializer = credential,
                        ApplicationName = "Custom Track Testing Bot",
                    });

                    var request = service.Spreadsheets.Values.Get("1xwhKoyypCWq5tCRTI69ijJoDiaoAVsvYAxz-q4UBNqM", "'CTGP Track Issues'!A2:H219");
                    var response = await request.ExecuteAsync();
                    foreach (var t in response.Values)
                    {
                        while (t.Count < 7)
                        {
                            t.Add("");
                        }
                    }
                    try
                    {
                        json = File.ReadAllText("cts.json");
                        List<Track> trackList = JsonConvert.DeserializeObject<List<Track>>(json);

                        int j = 0;

                        for (int i = 0; i < trackList.Count; i++)
                        {
                            if (j < 1)
                            {
                                if (CompareStrings(trackList[i].Name, track))
                                {
                                    foreach (var t in response.Values)
                                    {
                                        if (CompareStrings(t[0].ToString(), track))
                                        {
                                            t[0] = newTrack;
                                            t[1] = author;
                                            t[2] = version;
                                            t[3] = slot;
                                            t[4] = laps;
                                            t[5] = "";
                                            t[6] = "";
                                            description = $"**{newTrack}:**\nAuthor: *{author}*\nVersion: *{version}*\nSlots: *{slot}*\nSpeed/Laps: *{laps}*";
                                            j++;
                                            break;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                break;
                            }
                        }

                        if (j < 1)
                        {
                            embed = new DiscordEmbedBuilder
                            {
                                Color = new DiscordColor("#FF0000"),
                                Title = "__**Error:**__",
                                Description = $"*{track} could not be found.*" +
                                          "\n**c!replaceissues [track] [new track] [author] [version] [slot] [speed/laps]**",
                                Url = "https://docs.google.com/spreadsheets/d/1xwhKoyypCWq5tCRTI69ijJoDiaoAVsvYAxz-q4UBNqM/edit#gid=1971102004",
                                Footer = new DiscordEmbedBuilder.EmbedFooter
                                {
                                    Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                                }
                            };
                            await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
                        }
                        else
                        {
                            var orderedResponse = response.Values.OrderBy(x => x[0].ToString()).ToList();
                            for (int i = 0; i < response.Values.Count; i++)
                            {
                                response.Values[i] = orderedResponse[i];
                            }

                            var updateRequest = service.Spreadsheets.Values.Update(response, "1xwhKoyypCWq5tCRTI69ijJoDiaoAVsvYAxz-q4UBNqM", "'CTGP Track Issues'!A2:H219");
                            updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.RAW;
                            var update = await updateRequest.ExecuteAsync();

                            embed = new DiscordEmbedBuilder
                            {
                                Color = new DiscordColor("#FF0000"),
                                Title = $"__**{newTrack} has now replaced {track}:**__",
                                Description = description,
                                Url = "https://docs.google.com/spreadsheets/d/1xwhKoyypCWq5tCRTI69ijJoDiaoAVsvYAxz-q4UBNqM/edit#gid=1971102004",
                                Footer = new DiscordEmbedBuilder.EmbedFooter
                                {
                                    Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                                }
                            };
                            await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
                        }
                    }
                    catch (Exception ex)
                    {
                        embed = new DiscordEmbedBuilder
                        {
                            Color = new DiscordColor("#FF0000"),
                            Title = "__**Error:**__",
                            Description = $"*{ex.Message}*" +
                                      "\n**c!replaceissues [track] [new track] [author] [version] [slot] [speed/laps]**",
                            Url = "https://docs.google.com/spreadsheets/d/1xwhKoyypCWq5tCRTI69ijJoDiaoAVsvYAxz-q4UBNqM/edit#gid=1971102004",
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

        [Command("bkt")]
        public async Task GetBestTimes(CommandContext ctx, [RemainingText] string track = "")
        {
            await ctx.TriggerTypingAsync();

            string json = "";
            string description = "";

            var embed = new DiscordEmbedBuilder { };

            try
            {
                json = File.ReadAllText($"rts.json");
                List<Track> trackList = JsonConvert.DeserializeObject<List<Track>>(json);
                json = File.ReadAllText($"cts.json");
                foreach (var t in JsonConvert.DeserializeObject<List<Track>>(json))
                {
                    trackList.Add(t);
                }

                json = File.ReadAllText($"rts200.json");
                List<Track> trackList200 = JsonConvert.DeserializeObject<List<Track>>(json);
                json = File.ReadAllText($"cts200.json");
                foreach (var t in JsonConvert.DeserializeObject<List<Track>>(json))
                {
                    trackList200.Add(t);
                }

                int j = 0;
                List<Track> trackDisplay = new List<Track>();
                List<Track> trackDisplay200 = new List<Track>();

                for (int i = 0; i < trackList.Count; i++)
                {
                    if (trackDisplay.Count > 0)
                    {
                        if (trackList[i].Name == trackDisplay[0].Name)
                        {
                            trackDisplay.Add(trackList[i]);
                            j++;
                        }
                    }
                    else if (CompareIncompleteStrings(trackList[i].Name, track) || CompareStringAbbreviation(track, trackList[i].Name) || CompareStringsLevenshteinDistance(track, trackList[i].Name))
                    {
                        trackDisplay.Add(trackList[i]);
                        j++;
                    }
                }

                for (int i = 0; i < trackList200.Count; i++)
                {
                    if (trackDisplay200.Count > 0)
                    {
                        if (trackList200[i].Name == trackDisplay200[0].Name)
                        {
                            trackDisplay200.Add(trackList200[i]);
                            j++;
                        }
                    }
                    else if (CompareIncompleteStrings(trackList200[i].Name, track) || CompareStringAbbreviation(track, trackList200[i].Name) || CompareStringsLevenshteinDistance(track, trackList200[i].Name))
                    {
                        trackDisplay200.Add(trackList200[i]);
                        j++;
                    }
                }

                if (j < 1)
                {
                    embed = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = "__**Error:**__",
                        Description = $"*{track} could not be found.*" +
                        "\n**c!bkt track**",
                        Url = "https://chadsoft.co.uk/time-trials/",
                        Footer = new DiscordEmbedBuilder.EmbedFooter
                        {
                            Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                        }
                    };
                    await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
                }
                else
                {
                    if (trackDisplay.Count > 0)
                    {
                        description += "__**150cc:**__";
                        foreach (var t in trackDisplay)
                        {
                            description += $"\n[{t.BestTime}](https://chadsoft.co.uk/time-trials{t.BKTLink.Split('.')[0]}.html) *({t.CategoryName})* - {t.BKTHolder} *({t.BKTUploadTime.Split('T')[0]})*";
                        }
                    }

                    if (trackDisplay200.Count > 0)
                    {
                        description += "\n__**200cc:**__";
                        foreach (var t in trackDisplay200)
                        {
                            description += $"\n[{t.BestTime}](https://chadsoft.co.uk/time-trials{t.BKTLink.Split('.')[0]}.html) *({t.CategoryName})* - {t.BKTHolder} *({t.BKTUploadTime.Split('T')[0]})*";
                        }
                    }

                    embed = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = $"__**Best Times on {trackDisplay[0].Name} *(First result)*:**__",
                        Description = description,
                        Url = $"https://chadsoft.co.uk/time-trials/",
                        Footer = new DiscordEmbedBuilder.EmbedFooter
                        {
                            Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                        }
                    };
                    await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);

                }
            }
            catch (Exception ex)
            {
                embed = new DiscordEmbedBuilder
                {
                    Color = new DiscordColor("#FF0000"),
                    Title = $"__**Error:**__",
                    Description = $"*{ex.Message}*" +
                       "\n**c!bkt track**",
                    Url = "https://chadsoft.co.uk/time-trials/",
                    Footer = new DiscordEmbedBuilder.EmbedFooter
                    {
                        Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                    }
                };
                await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);

                Console.WriteLine(ex.ToString());
            }
        }

        [Command("pop")]
        public async Task WWPopularityRequest(CommandContext ctx, [RemainingText] string arg = "")
        {
            await ctx.TriggerTypingAsync();

            var embed = new DiscordEmbedBuilder { };
            string json = string.Empty;
            string description1 = string.Empty;
            string description2 = string.Empty;
            string description3 = string.Empty;
            string description4 = string.Empty;
            string description5 = string.Empty;
            string description6 = string.Empty;
            string description7 = string.Empty;
            string description8 = string.Empty;
            string description9 = string.Empty;
            string description10 = string.Empty;
            string description11 = string.Empty;

            try
            {
                json = File.ReadAllText($"cts.json");
                List<Track> trackListCts = JsonConvert.DeserializeObject<List<Track>>(json);
                for (int i = 0; i < trackListCts.Count; i++)
                {
                    if (trackListCts[i].Category % 16 != 0)
                    {
                        trackListCts.RemoveAt(i);
                        i--;
                    }
                }
                trackListCts = trackListCts.OrderByDescending(a => a.WiimmfiScore).ToList();

                json = File.ReadAllText($"rts.json");
                List<Track> trackListRts = JsonConvert.DeserializeObject<List<Track>>(json);
                for (int i = 0; i < trackListRts.Count; i++)
                {
                    if (trackListRts[i].Category % 16 != 0)
                    {
                        trackListRts.RemoveAt(i);
                        i--;
                    }
                }
                trackListRts = trackListRts.OrderByDescending(a => a.WiimmfiScore).ToList();

                List<Page> pages = new List<Page>();

                if (arg.ToLowerInvariant().Contains("rts"))
                {
                    for (int i = 0; i < 21; i++)
                    {
                        description1 = description1 + $"**{i + 1})** {trackListRts[i].Name} *({trackListRts[i].WiimmfiScore})*\n";
                    }
                    for (int i = 21; i < 32; i++)
                    {
                        description2 = description2 + $"**{i + 1})** {trackListRts[i].Name} *({trackListRts[i].WiimmfiScore})*\n";
                    }
                    var embed1 = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = $"__**Displaying 1-21:**__",
                        Description = description1,
                        Url = "https://wiimmfi.de/stats/track/mv/ww?p=std,c1,0",
                        Footer = new DiscordEmbedBuilder.EmbedFooter
                        {
                            Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                        }
                    };
                    var embed2 = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = $"__**Displaying 22-32:**__",
                        Description = description2,
                        Url = "https://wiimmfi.de/stats/track/mv/ww?p=std,c1,0",
                        Footer = new DiscordEmbedBuilder.EmbedFooter
                        {
                            Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                        }
                    };
                    Page page1 = new Page("", embed1);
                    Page page2 = new Page("", embed2);
                    pages.Add(page1);
                    pages.Add(page2);
                }

                else if (arg.ToLowerInvariant().Contains("cts"))
                {
                    for (int i = 0; i < 21; i++)
                    {
                        description1 = description1 + $"**{i + 1})** {trackListCts[i].Name} *({trackListCts[i].WiimmfiScore})*\n";
                    }
                    for (int i = 21; i < 42; i++)
                    {
                        description2 = description2 + $"**{i + 1})** {trackListCts[i].Name} *({trackListCts[i].WiimmfiScore})*\n";
                    }
                    for (int i = 42; i < 63; i++)
                    {
                        description3 = description3 + $"**{i + 1})** {trackListCts[i].Name} *({trackListCts[i].WiimmfiScore})*\n";
                    }
                    for (int i = 63; i < 84; i++)
                    {
                        description4 = description4 + $"**{i + 1})** {trackListCts[i].Name} *({trackListCts[i].WiimmfiScore})*\n";
                    }
                    for (int i = 84; i < 105; i++)
                    {
                        description5 = description5 + $"**{i + 1})** {trackListCts[i].Name} *({trackListCts[i].WiimmfiScore})*\n";
                    }
                    for (int i = 105; i < 126; i++)
                    {
                        description6 = description6 + $"**{i + 1})** {trackListCts[i].Name} *({trackListCts[i].WiimmfiScore})*\n";
                    }
                    for (int i = 126; i < 147; i++)
                    {
                        description7 = description7 + $"**{i + 1})** {trackListCts[i].Name} *({trackListCts[i].WiimmfiScore})*\n";
                    }
                    for (int i = 147; i < 168; i++)
                    {
                        description8 = description8 + $"**{i + 1})** {trackListCts[i].Name} *({trackListCts[i].WiimmfiScore})*\n";
                    }
                    for (int i = 168; i < 189; i++)
                    {
                        description9 = description9 + $"**{i + 1})** {trackListCts[i].Name} *({trackListCts[i].WiimmfiScore})*\n";
                    }
                    for (int i = 189; i < 210; i++)
                    {
                        description10 = description10 + $"**{i + 1})** {trackListCts[i].Name} *({trackListCts[i].WiimmfiScore})*\n";
                    }
                    for (int i = 210; i < 218; i++)
                    {
                        description11 = description11 + $"**{i + 1})** {trackListCts[i].Name} *({trackListCts[i].WiimmfiScore})*\n";
                    }
                    var embed1 = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = $"__**Displaying 1-21:**__",
                        Description = description1,
                        Url = "https://wiimmfi.de/stats/track/mv/ctgp?p=std,c1,0",
                        Footer = new DiscordEmbedBuilder.EmbedFooter
                        {
                            Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                        }
                    };
                    var embed2 = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = $"__**Displaying 22-42:**__",
                        Description = description2,
                        Url = "https://wiimmfi.de/stats/track/mv/ctgp?p=std,c1,0",
                        Footer = new DiscordEmbedBuilder.EmbedFooter
                        {
                            Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                        }
                    };
                    var embed3 = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = $"__**Displaying 43-63:**__",
                        Description = description3,
                        Url = "https://wiimmfi.de/stats/track/mv/ctgp?p=std,c1,0",
                        Footer = new DiscordEmbedBuilder.EmbedFooter
                        {
                            Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                        }
                    };
                    var embed4 = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = $"__**Displaying 64-84:**__",
                        Description = description4,
                        Url = "https://wiimmfi.de/stats/track/mv/ctgp?p=std,c1,0",
                        Footer = new DiscordEmbedBuilder.EmbedFooter
                        {
                            Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                        }
                    };
                    var embed5 = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = $"__**Displaying 85-105:**__",
                        Description = description5,
                        Url = "https://wiimmfi.de/stats/track/mv/ctgp?p=std,c1,0",
                        Footer = new DiscordEmbedBuilder.EmbedFooter
                        {
                            Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                        }
                    };
                    var embed6 = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = $"__**Displaying 106-126:**__",
                        Description = description6,
                        Url = "https://wiimmfi.de/stats/track/mv/ctgp?p=std,c1,0",
                        Footer = new DiscordEmbedBuilder.EmbedFooter
                        {
                            Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                        }
                    };
                    var embed7 = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = $"__**Displaying 127-147:**__",
                        Description = description7,
                        Url = "https://wiimmfi.de/stats/track/mv/ctgp?p=std,c1,0",
                        Footer = new DiscordEmbedBuilder.EmbedFooter
                        {
                            Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                        }
                    };
                    var embed8 = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = $"__**Displaying 148-168:**__",
                        Description = description8,
                        Url = "https://wiimmfi.de/stats/track/mv/ctgp?p=std,c1,0",
                        Footer = new DiscordEmbedBuilder.EmbedFooter
                        {
                            Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                        }
                    };
                    var embed9 = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = $"__**Displaying 169-189:**__",
                        Description = description9,
                        Url = "https://wiimmfi.de/stats/track/mv/ctgp?p=std,c1,0",
                        Footer = new DiscordEmbedBuilder.EmbedFooter
                        {
                            Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                        }
                    };
                    var embed10 = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = $"__**Displaying 190-210:**__",
                        Description = description10,
                        Url = "https://wiimmfi.de/stats/track/mv/ctgp?p=std,c1,0",
                        Footer = new DiscordEmbedBuilder.EmbedFooter
                        {
                            Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                        }
                    };
                    var embed11 = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = $"__**Displaying 211-218:**__",
                        Description = description11,
                        Url = "https://wiimmfi.de/stats/track/mv/ctgp?p=std,c1,0",
                        Footer = new DiscordEmbedBuilder.EmbedFooter
                        {
                            Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                        }
                    };
                    Page page1 = new Page("", embed1);
                    Page page2 = new Page("", embed2);
                    Page page3 = new Page("", embed3);
                    Page page4 = new Page("", embed4);
                    Page page5 = new Page("", embed5);
                    Page page6 = new Page("", embed6);
                    Page page7 = new Page("", embed7);
                    Page page8 = new Page("", embed8);
                    Page page9 = new Page("", embed9);
                    Page page10 = new Page("", embed10);
                    Page page11 = new Page("", embed11);
                    pages.Add(page1);
                    pages.Add(page2);
                    pages.Add(page3);
                    pages.Add(page4);
                    pages.Add(page5);
                    pages.Add(page6);
                    pages.Add(page7);
                    pages.Add(page8);
                    pages.Add(page9);
                    pages.Add(page10);
                    pages.Add(page11);
                }

                else
                {
                    int c = 0;
                    int d = 0;
                    description1 = $"__**Nintendo Tracks**__:\n";
                    for (int i = 0; i < trackListRts.Count; i++)
                    {
                        if (CompareIncompleteStrings(trackListRts[i].Name, arg) || CompareStringAbbreviation(arg, trackListRts[i].Name) || CompareStringsLevenshteinDistance(arg, trackListRts[i].Name))
                        {
                            description1 = description1 + $"**{i + 1})** {trackListRts[i].Name} *({trackListRts[i].WiimmfiScore})*\n";
                        }
                    }
                    if (description1 == $"__**Nintendo Tracks**__:\n")
                    {
                        description1 = $"__**Custom Tracks**__:\n";
                        d = description1.ToCharArray().Length;
                    }
                    else
                    {
                        d = description1.ToCharArray().Length;
                        description1 += $"__**Custom Tracks**__:\n";
                    }
                    for (int i = 0; i < trackListCts.Count; i++)
                    {
                        if (CompareIncompleteStrings(trackListCts[i].Name, arg) || CompareStringAbbreviation(arg, trackListCts[i].Name) || CompareStringsLevenshteinDistance(arg, trackListCts[i].Name))
                        {
                            description1 = description1 + $"**{i + 1})** {trackListCts[i].Name} *({trackListCts[i].WiimmfiScore})*\n";
                            c++;
                        }
                    }
                    if (c == 0)
                    {
                        if (description1.Contains($"__**Nintendo Tracks**__:\n"))
                        {
                            description1 = description1.Remove(d - 1, 23);
                        }
                        else
                        {
                            description1 = description1.Remove(0, d);
                        }
                    }
                }

                if (arg == "")
                {
                    embed = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = $"__**Error:**__",
                        Description = "*Please provide a category or a track name.*" +
                           "\n**c!pop rts/cts/track**",
                        Url = "https://wiimmfi.de/stats/track/mv/ctgp?p=std,c1,0",
                        Footer = new DiscordEmbedBuilder.EmbedFooter
                        {
                            Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                        }
                    };
                    await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
                }

                else if (description1 == "")
                {
                    embed = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = "__**Error:**__",
                        Description = $"*{arg} could not be found.*" +
                           "\n**c!pop rts/cts/track**",
                        Url = "https://wiimmfi.de/stats/track/mv/ctgp?p=std,c1,0",
                        Footer = new DiscordEmbedBuilder.EmbedFooter
                        {
                            Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                        }
                    };
                    await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
                }

                else if (description1.ToCharArray().Length > 800)
                {
                    embed = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = $"__**Error:**__",
                        Description = "*Embed too large. Please refine your search.*" +
                           "\n**c!pop rts/cts/track**",
                        Url = "https://wiimmfi.de/stats/track/mv/ctgp?p=std,c1,0",
                        Footer = new DiscordEmbedBuilder.EmbedFooter
                        {
                            Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                        }
                    };
                    await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
                }

                else if (arg.ToLowerInvariant().Contains("rts") || arg.ToLowerInvariant().Contains("cts"))
                {
                    await ctx.Channel.SendPaginatedMessageAsync(ctx.User, pages);
                }

                else
                {
                    embed = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = $"__**Displaying tracks containing *{arg}*:**__",
                        Description = description1,
                        Url = "https://wiimmfi.de/stats/track/mv/ctgp?p=std,c1,0",
                        Footer = new DiscordEmbedBuilder.EmbedFooter
                        {
                            Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                        }
                    };
                    await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                embed = new DiscordEmbedBuilder
                {
                    Color = new DiscordColor("#FF0000"),
                    Title = $"__**Error:**__",
                    Description = $"*{ex.Message}*" +
                        "\n**c!pop rts/cts/track**",
                    Url = "https://wiimmfi.de/stats/track/mv/ctgp?p=std,c1,0",
                    Footer = new DiscordEmbedBuilder.EmbedFooter
                    {
                        Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                    }
                };
                await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);

                Console.WriteLine(ex.ToString());
            }
        }

        [Command("ttpop")]
        public async Task TTPopularityRequest(CommandContext ctx, [RemainingText] string arg = "")
        {
            await ctx.TriggerTypingAsync();

            var embed = new DiscordEmbedBuilder { };
            string json = string.Empty;
            string description1 = string.Empty;
            string description2 = string.Empty;
            string description3 = string.Empty;
            string description4 = string.Empty;
            string description5 = string.Empty;
            string description6 = string.Empty;
            string description7 = string.Empty;
            string description8 = string.Empty;
            string description9 = string.Empty;
            string description10 = string.Empty;
            string description11 = string.Empty;

            try
            {
                json = File.ReadAllText($"cts.json");
                List<Track> trackListCts = JsonConvert.DeserializeObject<List<Track>>(json);

                int g = 0;
                var name = string.Empty;

                foreach (var track in trackListCts)
                {
                    if (name == track.Name)
                    {
                        g = trackListCts.FindIndex(ix => ix.Name.Contains(track.Name));
                        trackListCts[g].TimeTrialScore += track.TimeTrialScore;
                    }
                    else
                    {
                        name = track.Name;
                    }
                }

                for (int i = 0; i < trackListCts.Count; i++)
                {
                    if (trackListCts[i].Category % 16 != 0)
                    {
                        trackListCts.RemoveAt(i);
                        i--;
                    }
                }
                trackListCts = trackListCts.OrderByDescending(a => a.TimeTrialScore).ToList();

                json = File.ReadAllText($"rts.json");
                List<Track> trackListRts = JsonConvert.DeserializeObject<List<Track>>(json);

                foreach (var track in trackListCts)
                {
                    if (name == track.Name)
                    {
                        g = trackListCts.FindIndex(ix => ix.Name.Contains(track.Name));
                        trackListCts[g].TimeTrialScore += track.TimeTrialScore;
                    }
                    else
                    {
                        name = track.Name;
                    }
                }

                for (int i = 0; i < trackListRts.Count; i++)
                {
                    if (trackListRts[i].Category % 16 != 0)
                    {
                        trackListRts.RemoveAt(i);
                        i--;
                    }
                }
                trackListRts = trackListRts.OrderByDescending(a => a.TimeTrialScore).ToList();

                List<Page> pages = new List<Page>();

                if (arg.ToLowerInvariant().Contains("rts"))
                {
                    for (int i = 0; i < 21; i++)
                    {
                        description1 = description1 + $"**{i + 1})** {trackListRts[i].Name} *({trackListRts[i].TimeTrialScore})*\n";
                    }
                    for (int i = 21; i < 32; i++)
                    {
                        description2 = description2 + $"**{i + 1})** {trackListRts[i].Name} *({trackListRts[i].TimeTrialScore})*\n";
                    }
                    var embed1 = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = $"__**Displaying 1-21:**__",
                        Description = description1,
                        Url = "https://chadsoft.co.uk/time-trials/",
                        Footer = new DiscordEmbedBuilder.EmbedFooter
                        {
                            Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                        }
                    };
                    var embed2 = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = $"__**Displaying 22-32:**__",
                        Description = description2,
                        Url = "https://chadsoft.co.uk/time-trials/",
                        Footer = new DiscordEmbedBuilder.EmbedFooter
                        {
                            Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                        }
                    };
                    Page page1 = new Page("", embed1);
                    Page page2 = new Page("", embed2);
                    pages.Add(page1);
                    pages.Add(page2);
                }

                else if (arg.ToLowerInvariant().Contains("cts"))
                {
                    for (int i = 0; i < 21; i++)
                    {
                        description1 = description1 + $"**{i + 1})** {trackListCts[i].Name} *({trackListCts[i].TimeTrialScore})*\n";
                    }
                    for (int i = 21; i < 42; i++)
                    {
                        description2 = description2 + $"**{i + 1})** {trackListCts[i].Name} *({trackListCts[i].TimeTrialScore})*\n";
                    }
                    for (int i = 42; i < 63; i++)
                    {
                        description3 = description3 + $"**{i + 1})** {trackListCts[i].Name} *({trackListCts[i].TimeTrialScore})*\n";
                    }
                    for (int i = 63; i < 84; i++)
                    {
                        description4 = description4 + $"**{i + 1})** {trackListCts[i].Name} *({trackListCts[i].TimeTrialScore})*\n";
                    }
                    for (int i = 84; i < 105; i++)
                    {
                        description5 = description5 + $"**{i + 1})** {trackListCts[i].Name} *({trackListCts[i].TimeTrialScore})*\n";
                    }
                    for (int i = 105; i < 126; i++)
                    {
                        description6 = description6 + $"**{i + 1})** {trackListCts[i].Name} *({trackListCts[i].TimeTrialScore})*\n";
                    }
                    for (int i = 126; i < 147; i++)
                    {
                        description7 = description7 + $"**{i + 1})** {trackListCts[i].Name} *({trackListCts[i].TimeTrialScore})*\n";
                    }
                    for (int i = 147; i < 168; i++)
                    {
                        description8 = description8 + $"**{i + 1})** {trackListCts[i].Name} *({trackListCts[i].TimeTrialScore})*\n";
                    }
                    for (int i = 168; i < 189; i++)
                    {
                        description9 = description9 + $"**{i + 1})** {trackListCts[i].Name} *({trackListCts[i].TimeTrialScore})*\n";
                    }
                    for (int i = 189; i < 210; i++)
                    {
                        description10 = description10 + $"**{i + 1})** {trackListCts[i].Name} *({trackListCts[i].TimeTrialScore})*\n";
                    }
                    for (int i = 210; i < 218; i++)
                    {
                        description11 = description11 + $"**{i + 1})** {trackListCts[i].Name} *({trackListCts[i].TimeTrialScore})*\n";
                    }
                    var embed1 = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = $"__**Displaying 1-21:**__",
                        Description = description1,
                        Url = "https://chadsoft.co.uk/time-trials/",
                        Footer = new DiscordEmbedBuilder.EmbedFooter
                        {
                            Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                        }
                    };
                    var embed2 = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = $"__**Displaying 22-42:**__",
                        Description = description2,
                        Url = "https://chadsoft.co.uk/time-trials/",
                        Footer = new DiscordEmbedBuilder.EmbedFooter
                        {
                            Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                        }
                    };
                    var embed3 = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = $"__**Displaying 43-63:**__",
                        Description = description3,
                        Url = "https://chadsoft.co.uk/time-trials/",
                        Footer = new DiscordEmbedBuilder.EmbedFooter
                        {
                            Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                        }
                    };
                    var embed4 = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = $"__**Displaying 64-84:**__",
                        Description = description4,
                        Url = "https://chadsoft.co.uk/time-trials/",
                        Footer = new DiscordEmbedBuilder.EmbedFooter
                        {
                            Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                        }
                    };
                    var embed5 = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = $"__**Displaying 85-105:**__",
                        Description = description5,
                        Url = "https://chadsoft.co.uk/time-trials/",
                        Footer = new DiscordEmbedBuilder.EmbedFooter
                        {
                            Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                        }
                    };
                    var embed6 = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = $"__**Displaying 106-126:**__",
                        Description = description6,
                        Url = "https://chadsoft.co.uk/time-trials/",
                        Footer = new DiscordEmbedBuilder.EmbedFooter
                        {
                            Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                        }
                    };
                    var embed7 = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = $"__**Displaying 127-147:**__",
                        Description = description7,
                        Url = "https://chadsoft.co.uk/time-trials/",
                        Footer = new DiscordEmbedBuilder.EmbedFooter
                        {
                            Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                        }
                    };
                    var embed8 = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = $"__**Displaying 148-168:**__",
                        Description = description8,
                        Url = "https://chadsoft.co.uk/time-trials/",
                        Footer = new DiscordEmbedBuilder.EmbedFooter
                        {
                            Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                        }
                    };
                    var embed9 = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = $"__**Displaying 169-189:**__",
                        Description = description9,
                        Url = "https://chadsoft.co.uk/time-trials/",
                        Footer = new DiscordEmbedBuilder.EmbedFooter
                        {
                            Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                        }
                    };
                    var embed10 = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = $"__**Displaying 190-210:**__",
                        Description = description10,
                        Url = "https://chadsoft.co.uk/time-trials/",
                        Footer = new DiscordEmbedBuilder.EmbedFooter
                        {
                            Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                        }
                    };
                    var embed11 = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = $"__**Displaying 211-218:**__",
                        Description = description11,
                        Url = "https://chadsoft.co.uk/time-trials/",
                        Footer = new DiscordEmbedBuilder.EmbedFooter
                        {
                            Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                        }
                    };
                    Page page1 = new Page("", embed1);
                    Page page2 = new Page("", embed2);
                    Page page3 = new Page("", embed3);
                    Page page4 = new Page("", embed4);
                    Page page5 = new Page("", embed5);
                    Page page6 = new Page("", embed6);
                    Page page7 = new Page("", embed7);
                    Page page8 = new Page("", embed8);
                    Page page9 = new Page("", embed9);
                    Page page10 = new Page("", embed10);
                    Page page11 = new Page("", embed11);
                    pages.Add(page1);
                    pages.Add(page2);
                    pages.Add(page3);
                    pages.Add(page4);
                    pages.Add(page5);
                    pages.Add(page6);
                    pages.Add(page7);
                    pages.Add(page8);
                    pages.Add(page9);
                    pages.Add(page10);
                    pages.Add(page11);
                }

                else
                {
                    int c = 0;
                    int d = 0;
                    description1 = $"__**Nintendo Tracks**__:\n";
                    for (int i = 0; i < trackListRts.Count; i++)
                    {
                        if (CompareIncompleteStrings(trackListRts[i].Name, arg) || CompareStringAbbreviation(arg, trackListRts[i].Name) || CompareStringsLevenshteinDistance(arg, trackListRts[i].Name))
                        {
                            description1 = description1 + $"**{i + 1})** {trackListRts[i].Name} *({trackListRts[i].TimeTrialScore})*\n";
                        }
                    }
                    if (description1 == $"__**Nintendo Tracks**__:\n")
                    {
                        description1 = $"__**Custom Tracks**__:\n";
                        d = description1.ToCharArray().Length;
                    }
                    else
                    {
                        d = description1.ToCharArray().Length;
                        description1 += $"__**Custom Tracks**__:\n";
                    }
                    for (int i = 0; i < trackListCts.Count; i++)
                    {
                        if (CompareIncompleteStrings(trackListCts[i].Name, arg) || CompareStringAbbreviation(arg, trackListCts[i].Name) || CompareStringsLevenshteinDistance(arg, trackListCts[i].Name))
                        {
                            description1 = description1 + $"**{i + 1})** {trackListCts[i].Name} *({trackListCts[i].TimeTrialScore})*\n";
                            c++;
                        }
                    }
                    if (c == 0)
                    {
                        if (description1.Contains($"__**Nintendo Tracks**__:\n"))
                        {
                            description1 = description1.Remove(d - 1, 23);
                        }
                        else
                        {
                            description1 = description1.Remove(0, d);
                        }
                    }
                }

                if (arg == "")
                {
                    embed = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = $"__**Error:**__",
                        Description = "*Please provide a category or a track name.*" +
                           "\n**c!pop rts/cts/track**",
                        Url = "https://chadsoft.co.uk/time-trials/",
                        Footer = new DiscordEmbedBuilder.EmbedFooter
                        {
                            Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                        }
                    };
                    await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
                }

                else if (description1 == "")
                {
                    embed = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = "__**Error:**__",
                        Description = $"*{arg} could not be found.*" +
                           "\n**c!pop rts/cts/track**",
                        Url = "https://chadsoft.co.uk/time-trials/",
                        Footer = new DiscordEmbedBuilder.EmbedFooter
                        {
                            Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                        }
                    };
                    await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
                }

                else if (description1.ToCharArray().Length > 800)
                {

                    embed = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = $"__**Error:**__",
                        Description = "*Embed too large. Please refine your search.*" +
                           "\n**c!pop rts/cts/track**",
                        Url = "https://chadsoft.co.uk/time-trials/",
                        Footer = new DiscordEmbedBuilder.EmbedFooter
                        {
                            Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                        }
                    };
                    await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
                }

                else if (arg.ToLowerInvariant().Contains("rts") || arg.ToLowerInvariant().Contains("cts"))
                {
                    await ctx.Channel.SendPaginatedMessageAsync(ctx.User, pages);
                }

                else
                {
                    embed = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = $"__**Displaying tracks containing *{arg}*:**__",
                        Description = description1,
                        Url = "https://chadsoft.co.uk/time-trials/",
                        Footer = new DiscordEmbedBuilder.EmbedFooter
                        {
                            Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                        }
                    };
                    await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                embed = new DiscordEmbedBuilder
                {
                    Color = new DiscordColor("#FF0000"),
                    Title = $"__**Error:**__",
                    Description = $"*{ex.Message}*" +
                        "\n**c!pop rts/cts/track**",
                    Url = "https://chadsoft.co.uk/time-trials/",
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
