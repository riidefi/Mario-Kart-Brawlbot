using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using IronPython.Compiler.Ast;
using IronPython.Runtime.Operations;
using Newtonsoft.Json;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace MKBB.Commands
{
    public class Info : ApplicationCommandModule
    {
        [SlashCommand("nextupdate", "Displays the next update(s) coming to CTGP.")]
        public async Task GetNextUpdate(InteractionContext ctx)
        {
            try
            {
                await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder() { IsEphemeral = true });
                string serviceAccountEmail = "brawlbox@custom-track-testing-bot.iam.gserviceaccount.com";

                var certificate = new X509Certificate2(@"key.p12", "notasecret", X509KeyStorageFlags.Exportable);

                ServiceAccountCredential credential = new ServiceAccountCredential(
                   new ServiceAccountCredential.Initializer(serviceAccountEmail).FromCertificate(certificate));

                var service = new SheetsService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "Mario Kart Brawlbot",
                });

                var request = service.Spreadsheets.Values.Get("1xwhKoyypCWq5tCRTI69ijJoDiaoAVsvYAxz-q4UBNqM", "'Update Queue'");
                request.ValueRenderOption = SpreadsheetsResource.ValuesResource.GetRequest.ValueRenderOptionEnum.FORMULA;
                var response = await request.ExecuteAsync();
                response.Values.Add(new List<object>());
                foreach (var t in response.Values)
                {
                    while (t.Count < 11)
                    {
                        t.Add("");
                    }
                }

                int k = 1;

                string title = $"__**{response.Values[k][1]}:**__";
                k += 2;

                List<DiscordEmbedBuilder> embeds = new List<DiscordEmbedBuilder>();

            NewEmbed:
                string description = $"**{response.Values[k][1]}:**";
            NewSection:
                string header = response.Values[k][1].ToString();

                while (response.Values[k][1].ToString() == header || response.Values[k][1].ToString() == "")
                {
                    if (response.Values[k][2].ToString() == "")
                    {
                        break;
                    }
                    string dl = response.Values[k][5].ToString().Contains("=HYPERLINK") ? $"[{response.Values[k][5].ToString().Split('"')[3]}]({response.Values[k][5].ToString().Split('"')[1]})" : "-";
                    description += response.Values[k][2].ToString() == "TBD" ? "\n*TBD*" : $"\n{response.Values[k][2]} {response.Values[k][4]} | {response.Values[k][3]} | {dl}";
                    k++;
                }

                if (response.Values[k][2].ToString() == "")
                {
                    var embed = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = title,
                        Description = description,
                        Url = "https://docs.google.com/spreadsheets/d/1xwhKoyypCWq5tCRTI69ijJoDiaoAVsvYAxz-q4UBNqM/edit#gid=1751905284",
                        Footer = new DiscordEmbedBuilder.EmbedFooter
                        {
                            Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                        }
                    };
                    embeds.Add(embed);
                    k++;
                    if (k >= response.Values.Count)
                    {
                        goto EndOfUpdates;
                    }
                    title = $"__**{response.Values[k][1]}:**__";
                    k += 2;
                    goto NewEmbed;
                }
                description += $"\n**{response.Values[k][1]}:**";
                goto NewSection;

            EndOfUpdates:
                DiscordWebhookBuilder builder = new DiscordWebhookBuilder().AddEmbed(embeds[0]);

                if (embeds.Count > 1)
                {
                    builder.AddComponents(Util.GeneratePageArrows());
                }

                var message = await ctx.EditResponseAsync(builder);

                if (embeds.Count > 1)
                {
                    PendingPaginator pending = new PendingPaginator() { CurrentPage = 0, MessageId = message.Id, Context = ctx, Pages = embeds };

                    Util.PendingInteractions.Add(pending);
                }
            }
            catch (Exception ex)
            {
                await Util.ThrowError(ctx, ex);
            }
        }

        [SlashCommand("summary", "Gets the summary review from submission of the track specified.")]
        public async Task GetSummary(InteractionContext ctx,
            [Option("track-name", "The track name of the summary requested.")] string track)
        {
            try
            {
                await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder() { IsEphemeral = true });
                string description = string.Empty;

                string serviceAccountEmail = "brawlbox@custom-track-testing-bot.iam.gserviceaccount.com";

                var certificate = new X509Certificate2(@"key.p12", "notasecret", X509KeyStorageFlags.Exportable);

                ServiceAccountCredential credential = new ServiceAccountCredential(
                   new ServiceAccountCredential.Initializer(serviceAccountEmail).FromCertificate(certificate));

                var service = new SheetsService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "Mario Kart Brawlbot",
                });

                var request = service.Spreadsheets.Values.Get("1I9yFsomTcvFT4hp6eN2azsfv6MsIy1897tBFX_gmtss", "'Track Evaluation Log'");
                var response = await request.ExecuteAsync();
                foreach (var t in response.Values)
                {
                    while (t.Count < 7)
                    {
                        t.Add("");
                    }
                }

                string trackDisplay = string.Empty;

                int index = Util.ListNameCheck(response.Values, track, ix2: 2);

                if (index > 0)
                {

                    int titleIx = index;

                    while (response.Values[titleIx][0].ToString() != "delimiter")
                    {
                        titleIx--;
                    }

                    string dateString = response.Values[titleIx][1].ToString();

                    var tally = response.Values[index][1].ToString().split("\n");
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
                    if (response.Values[index][6].ToString().ToCharArray().Length > 3500)
                    {
                        description = $"__**{dateString}**__\n**{response.Values[index][2]} {response.Values[index][4]} - {response.Values[index][3]}**\n{tally[1]} {tally[0]}\n\n{response.Values[index][6].ToString().Remove(3499)}...\n\n*For full summary go to the [Track Evaluation Log](https://docs.google.com/spreadsheets/d/1I9yFsomTcvFT4hp6eN2azsfv6MsIy1897tBFX_gmtss/edit#gid=798417105).*";
                    }
                    else
                    {
                        description = $"__**{dateString}**__\n**{response.Values[index][2]} {response.Values[index][4]} - {response.Values[index][3]}**\n{tally[1]} {tally[0]}\n\n{response.Values[index][6]}";
                    }
                    trackDisplay = response.Values[index][2].ToString();
                }
                if (index < 0)
                {
                    var embed = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = "__**Error:**__",
                        Description = $"*{track} could not be found.*",
                        Url = "https://wiki.tockdom.com/wiki/CTGP_Revolution/Track_Wishlist",
                        Footer = new DiscordEmbedBuilder.EmbedFooter
                        {
                            Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                        }
                    };
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
                }
                else
                {
                    var embed = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = $"__**Summary for {trackDisplay} (First result):**__",
                        Description = description,
                        Url = "https://wiki.tockdom.com/wiki/CTGP_Revolution/Track_Wishlist",
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

        [SlashCommand("staff", "Gets the staff ghosts for the track specified.")]
        public async Task GetStaffGhosts(InteractionContext ctx,
            [Option("track-name", "The track that the staff ghosts are set on.")] string track)
        {
            try
            {
                await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder() { IsEphemeral = true });

                var description = string.Empty;

                string serviceAccountEmail = "brawlbox@custom-track-testing-bot.iam.gserviceaccount.com";

                var certificate = new X509Certificate2(@"key.p12", "notasecret", X509KeyStorageFlags.Exportable);

                ServiceAccountCredential credential = new ServiceAccountCredential(
                   new ServiceAccountCredential.Initializer(serviceAccountEmail).FromCertificate(certificate));

                var service = new SheetsService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "Mario Kart Brawlbot",
                });

                var request = service.Spreadsheets.Values.Get("1xwhKoyypCWq5tCRTI69ijJoDiaoAVsvYAxz-q4UBNqM", "'Staff Ghosts'");
                var response = await request.ExecuteAsync();

                string json = File.ReadAllText("rts.json");
                List<Track> trackList = JsonConvert.DeserializeObject<List<Track>>(json);

                json = File.ReadAllText("cts.json");
                foreach (var t in JsonConvert.DeserializeObject<List<Track>>(json))
                {
                    trackList.Add(t);
                }

                Track trackDisplay = new Track();
                int found1 = Util.ListNameCheck(trackList, track);
                int found2 = Util.ListNameCheck(response.Values, track, ix2: 0);

                description = $"**Easy:**\n*[{response.Values[found2][1]}](https://chadsoft.co.uk/time-trials/rkgd/{response.Values[found2 + 251][1].ToString().Substring(0, 2)}/{response.Values[found2 + 251][1].ToString().Substring(2, 2)}/{response.Values[found2 + 251][1].ToString().Substring(4)}.html)*\n**Expert:**\n*[{response.Values[found2][2]}](https://chadsoft.co.uk/time-trials/rkgd/{response.Values[found2 + 251][2].ToString().Substring(0, 2)}/{response.Values[found2 + 251][2].ToString().Substring(2, 2)}/{response.Values[found2 + 251][2].ToString().Substring(4)}.html)*";
                trackDisplay = trackList[found1];

                if (found1 < 0 || found2 < 0)
                {
                    var embed = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = "__**Error:**__",
                        Description = $"*{track} could not be found.*",
                        Url = "https://docs.google.com/spreadsheets/d/1xwhKoyypCWq5tCRTI69ijJoDiaoAVsvYAxz-q4UBNqM/edit#gid=1188255728",
                        Footer = new DiscordEmbedBuilder.EmbedFooter
                        {
                            Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                        }
                    };
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
                }
                else
                {
                    var embed = new DiscordEmbedBuilder
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
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
                }
            }
            catch (Exception ex)
            {
                await Util.ThrowError(ctx, ex);
            }
        }

        [SlashCommand("info", "Gets the information for the track specified.")]
        public async Task GetTrackInfo(InteractionContext ctx,
            [Option("track-name", "The track the information is being requested for.")] string track)
        {
            try
            {
                await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder() { IsEphemeral = true });

                string description = string.Empty;

                string serviceAccountEmail = "brawlbox@custom-track-testing-bot.iam.gserviceaccount.com";

                var certificate = new X509Certificate2(@"key.p12", "notasecret", X509KeyStorageFlags.Exportable);

                ServiceAccountCredential credential = new ServiceAccountCredential(
                   new ServiceAccountCredential.Initializer(serviceAccountEmail).FromCertificate(certificate));

                var service = new SheetsService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "Mario Kart Brawlbot",
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

                string json = File.ReadAllText("cts.json");
                List<Track> trackList = JsonConvert.DeserializeObject<List<Track>>(json);

                Track trackDisplay = new Track();
                int found1 = Util.ListNameCheck(trackList, track);
                int found2 = Util.ListNameCheck(response.Values, track, ix2: 0);

                description = $"**Author:**\n*{response.Values[found2][1]}*\n**Version:**\n*{response.Values[found2][2]}*\n**Track/Music Slots:**\n*{response.Values[found2][3]}*\n**Speed/Lap Count:**\n*{response.Values[found2][4]}*";
                trackDisplay = trackList[found1];

                if (found1 < 0 || found2 < 0)
                {
                    var embed = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = "__**Error:**__",
                        Description = $"*{track} could not be found.\nThe track does not exist, or is not in CTGP.*",
                        Url = "https://docs.google.com/spreadsheets/d/1xwhKoyypCWq5tCRTI69ijJoDiaoAVsvYAxz-q4UBNqM/edit#gid=1971102004",
                        Footer = new DiscordEmbedBuilder.EmbedFooter
                        {
                            Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                        }
                    };
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
                }
                else
                {
                    var embed = new DiscordEmbedBuilder
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
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
                }
            }
            catch (Exception ex)
            {
                await Util.ThrowError(ctx, ex);
            }
        }

        [SlashCommand("bkt", "Gets the bkts (glitch and shortcut categories included) of the track specified.")]
        public async Task GetBestTimes(InteractionContext ctx,
            [Option("track-name", "The track that the best-known times are set on.")] string track = "")
        {
            try
            {
                await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder() { IsEphemeral = true });
                string description = "";
                string json = File.ReadAllText($"rts.json");
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

                List<Track> trackDisplay = new List<Track>();
                List<Track> trackDisplay200 = new List<Track>();

                int found = Util.ListNameCheck(trackList, track);
                if (found > -1)
                {
                    trackDisplay.Add(trackList[found]);
                    foreach (var t in trackList.FindAll(x => x.SHA1 == trackDisplay[0].SHA1 && x.Category != trackDisplay[0].Category))
                    {
                        trackDisplay.Add(t);
                    }
                }

                found = Util.ListNameCheck(trackList200, track);
                if (found > -1)
                {
                    trackDisplay200.Add(trackList200[found]);
                    foreach (var t in trackList200.FindAll(x => x.SHA1 == trackDisplay200[0].SHA1 && x.Category != trackDisplay200[0].Category))
                    {
                        trackDisplay200.Add(t);
                    }
                }

                if (found < 0)
                {
                    var embed = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = "__**Error:**__",
                        Description = $"*{track} could not be found.*",
                        Url = "https://chadsoft.co.uk/time-trials/",
                        Footer = new DiscordEmbedBuilder.EmbedFooter
                        {
                            Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                        }
                    };
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
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

                    var embed = new DiscordEmbedBuilder
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
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));

                }
            }
            catch (Exception ex)
            {
                await Util.ThrowError(ctx, ex);
            }
        }

        [SlashCommand("pop", "Gets the amount of times a track has been played on Wiimmfi for the last 3 months")]
        public async Task WWPopularityRequest(InteractionContext ctx,
            [Option("search", "Can use rts/cts to get a leaderboard, or input a track name to get the popularity for it.")] string arg,
            [Choice("Online", "online")]
            [Choice("Time Trials", "tts")]
            [Option("metric-category", "Can specify either online or time trial popularity.")] string metric)
        {
            try
            {
                await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder() { IsEphemeral = true });

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
                trackListCts = trackListCts.OrderByDescending(a => metric == "online" ? a.WiimmfiScore : a.TimeTrialScore).ToList();

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
                trackListRts = trackListRts.OrderByDescending(a => metric == "online" ? a.WiimmfiScore : a.TimeTrialScore).ToList();

                List<DiscordEmbedBuilder> embeds = new List<DiscordEmbedBuilder>();

                if (arg.ToLowerInvariant().Contains("rts"))
                {
                    for (int i = 0; i < 21; i++)
                    {
                        description1 = description1 + $"**{i + 1})** {trackListRts[i].Name} *({(metric == "online" ? trackListRts[i].WiimmfiScore:trackListRts[i].TimeTrialScore)})*\n";
                    }
                    for (int i = 21; i < 32; i++)
                    {
                        description2 = description2 + $"**{i + 1})** {trackListRts[i].Name} *({(metric == "online" ? trackListRts[i].WiimmfiScore : trackListRts[i].TimeTrialScore)})*\n";
                    }
                    embeds = new List<DiscordEmbedBuilder>{
                        new DiscordEmbedBuilder
                        {
                            Color = new DiscordColor("#FF0000"),
                            Title = $"__**Displaying 1-21:**__",
                            Description = description1,
                            Url = metric == "online" ? "https://wiimmfi.de/stats/track/mv/ww?p=std,c1,0" : "https://chadsoft.co.uk/time-trials/",
                            Footer = new DiscordEmbedBuilder.EmbedFooter
                            {
                                Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                            }
                        },
                        new DiscordEmbedBuilder
                        {
                            Color = new DiscordColor("#FF0000"),
                            Title = $"__**Displaying 22-32:**__",
                            Description = description2,
                            Url = metric == "online" ? "https://wiimmfi.de/stats/track/mv/ww?p=std,c1,0" : "https://chadsoft.co.uk/time-trials/",
                            Footer = new DiscordEmbedBuilder.EmbedFooter
                            {
                                Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                            }
                        }
                    };
                }

                else if (arg.ToLowerInvariant().Contains("cts"))
                {
                    for (int i = 0; i < 21; i++)
                    {
                        description1 = description1 + $"**{i + 1})** {trackListCts[i].Name} *({(metric == "online" ? trackListCts[i].WiimmfiScore : trackListCts[i].TimeTrialScore)})*\n";
                    }
                    for (int i = 21; i < 42; i++)
                    {
                        description2 = description2 + $"**{i + 1})** {trackListCts[i].Name} *({(metric == "online" ? trackListCts[i].WiimmfiScore : trackListCts[i].TimeTrialScore)})*\n";
                    }
                    for (int i = 42; i < 63; i++)
                    {
                        description3 = description3 + $"**{i + 1})** {trackListCts[i].Name} *({(metric == "online" ? trackListCts[i].WiimmfiScore : trackListCts[i].TimeTrialScore)})*\n";
                    }
                    for (int i = 63; i < 84; i++)
                    {
                        description4 = description4 + $"**{i + 1})** {trackListCts[i].Name} *({(metric == "online" ? trackListCts[i].WiimmfiScore : trackListCts[i].TimeTrialScore)})*\n";
                    }
                    for (int i = 84; i < 105; i++)
                    {
                        description5 = description5 + $"**{i + 1})** {trackListCts[i].Name} *({(metric == "online" ? trackListCts[i].WiimmfiScore : trackListCts[i].TimeTrialScore)})*\n";
                    }
                    for (int i = 105; i < 126; i++)
                    {
                        description6 = description6 + $"**{i + 1})** {trackListCts[i].Name} *({(metric == "online" ? trackListCts[i].WiimmfiScore : trackListCts[i].TimeTrialScore)})*\n";
                    }
                    for (int i = 126; i < 147; i++)
                    {
                        description7 = description7 + $"**{i + 1})** {trackListCts[i].Name} *({(metric == "online" ? trackListCts[i].WiimmfiScore : trackListCts[i].TimeTrialScore)})*\n";
                    }
                    for (int i = 147; i < 168; i++)
                    {
                        description8 = description8 + $"**{i + 1})** {trackListCts[i].Name} *({(metric == "online" ? trackListCts[i].WiimmfiScore : trackListCts[i].TimeTrialScore)})*\n";
                    }
                    for (int i = 168; i < 189; i++)
                    {
                        description9 = description9 + $"**{i + 1})** {trackListCts[i].Name} *({(metric == "online" ? trackListCts[i].WiimmfiScore : trackListCts[i].TimeTrialScore)})*\n";
                    }
                    for (int i = 189; i < 210; i++)
                    {
                        description10 = description10 + $"**{i + 1})** {trackListCts[i].Name} *({(metric == "online" ? trackListCts[i].WiimmfiScore : trackListCts[i].TimeTrialScore)})*\n";
                    }
                    for (int i = 210; i < 218; i++)
                    {
                        description11 = description11 + $"**{i + 1})** {trackListCts[i].Name} *({(metric == "online" ? trackListCts[i].WiimmfiScore : trackListCts[i].TimeTrialScore)})*\n";
                    }
                    embeds = new List<DiscordEmbedBuilder>{
                        new DiscordEmbedBuilder
                        {
                            Color = new DiscordColor("#FF0000"),
                            Title = $"__**Displaying 1-21:**__",
                            Description = description1,
                            Url = metric == "online" ? "https://wiimmfi.de/stats/track/mv/ww?p=std,c1,0" : "https://chadsoft.co.uk/time-trials/",
                            Footer = new DiscordEmbedBuilder.EmbedFooter
                            {
                                Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                            }
                        },
                            new DiscordEmbedBuilder
                        {
                            Color = new DiscordColor("#FF0000"),
                            Title = $"__**Displaying 22-42:**__",
                            Description = description2,
                            Url = metric == "online" ? "https://wiimmfi.de/stats/track/mv/ww?p=std,c1,0" : "https://chadsoft.co.uk/time-trials/",
                            Footer = new DiscordEmbedBuilder.EmbedFooter
                            {
                                Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                            }
                        },
                        new DiscordEmbedBuilder
                        {
                            Color = new DiscordColor("#FF0000"),
                            Title = $"__**Displaying 43-63:**__",
                            Description = description3,
                            Url = metric == "online" ? "https://wiimmfi.de/stats/track/mv/ww?p=std,c1,0" : "https://chadsoft.co.uk/time-trials/",
                            Footer = new DiscordEmbedBuilder.EmbedFooter
                            {
                                Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                            }
                        },
                        new DiscordEmbedBuilder
                        {
                            Color = new DiscordColor("#FF0000"),
                            Title = $"__**Displaying 64-84:**__",
                            Description = description4,
                            Url = metric == "online" ? "https://wiimmfi.de/stats/track/mv/ww?p=std,c1,0" : "https://chadsoft.co.uk/time-trials/",
                            Footer = new DiscordEmbedBuilder.EmbedFooter
                            {
                                Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                            }
                        },
                        new DiscordEmbedBuilder
                        {
                            Color = new DiscordColor("#FF0000"),
                            Title = $"__**Displaying 85-105:**__",
                            Description = description5,
                            Url = metric == "online" ? "https://wiimmfi.de/stats/track/mv/ww?p=std,c1,0" : "https://chadsoft.co.uk/time-trials/",
                            Footer = new DiscordEmbedBuilder.EmbedFooter
                            {
                                Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                            }
                        },
                        new DiscordEmbedBuilder
                        {
                            Color = new DiscordColor("#FF0000"),
                            Title = $"__**Displaying 106-126:**__",
                            Description = description6,
                            Url = metric == "online" ? "https://wiimmfi.de/stats/track/mv/ww?p=std,c1,0" : "https://chadsoft.co.uk/time-trials/",
                            Footer = new DiscordEmbedBuilder.EmbedFooter
                            {
                                Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                            }
                        },
                        new DiscordEmbedBuilder
                        {
                            Color = new DiscordColor("#FF0000"),
                            Title = $"__**Displaying 127-147:**__",
                            Description = description7,
                            Url = metric == "online" ? "https://wiimmfi.de/stats/track/mv/ww?p=std,c1,0" : "https://chadsoft.co.uk/time-trials/",
                            Footer = new DiscordEmbedBuilder.EmbedFooter
                            {
                                Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                            }
                        },
                        new DiscordEmbedBuilder
                        {
                            Color = new DiscordColor("#FF0000"),
                            Title = $"__**Displaying 148-168:**__",
                            Description = description8,
                            Url = metric == "online" ? "https://wiimmfi.de/stats/track/mv/ww?p=std,c1,0" : "https://chadsoft.co.uk/time-trials/",
                            Footer = new DiscordEmbedBuilder.EmbedFooter
                            {
                                Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                            }
                        },
                        new DiscordEmbedBuilder
                        {
                            Color = new DiscordColor("#FF0000"),
                            Title = $"__**Displaying 169-189:**__",
                            Description = description9,
                            Url = metric == "online" ? "https://wiimmfi.de/stats/track/mv/ww?p=std,c1,0" : "https://chadsoft.co.uk/time-trials/",
                            Footer = new DiscordEmbedBuilder.EmbedFooter
                            {
                                Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                            }
                        },
                        new DiscordEmbedBuilder
                        {
                            Color = new DiscordColor("#FF0000"),
                            Title = $"__**Displaying 190-210:**__",
                            Description = description10,
                            Url = metric == "online" ? "https://wiimmfi.de/stats/track/mv/ww?p=std,c1,0" : "https://chadsoft.co.uk/time-trials/",
                            Footer = new DiscordEmbedBuilder.EmbedFooter
                            {
                                Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                            }
                        },
                        new DiscordEmbedBuilder
                        {
                            Color = new DiscordColor("#FF0000"),
                            Title = $"__**Displaying 211-218:**__",
                            Description = description11,
                            Url = metric == "online" ? "https://wiimmfi.de/stats/track/mv/ww?p=std,c1,0" : "https://chadsoft.co.uk/time-trials/",
                            Footer = new DiscordEmbedBuilder.EmbedFooter
                            {
                                Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                            }
                        }
                    };
                }

                else
                {
                    int c = 0;
                    int d = 0;
                    description1 = $"__**Nintendo Tracks**__:\n";
                    for (int i = 0; i < trackListRts.Count; i++)
                    {
                        if (Util.CompareStrings(trackListRts[i].Name, arg) || Util.CompareIncompleteStrings(trackListRts[i].Name, arg) || Util.CompareStringAbbreviation(arg, trackListRts[i].Name) || Util.CompareStringsLevenshteinDistance(arg, trackListRts[i].Name))
                        {
                            description1 = description1 + $"**{i + 1})** {trackListRts[i].Name} *({(metric == "online" ? trackListRts[i].WiimmfiScore : trackListRts[i].TimeTrialScore)})*\n";
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
                        if (Util.CompareStrings(trackListCts[i].Name, arg) || Util.CompareIncompleteStrings(trackListCts[i].Name, arg) || Util.CompareStringAbbreviation(arg, trackListCts[i].Name) || Util.CompareStringsLevenshteinDistance(arg, trackListCts[i].Name))
                        {
                            description1 = description1 + $"**{i + 1})** {trackListCts[i].Name} *({(metric == "online" ? trackListCts[i].WiimmfiScore : trackListCts[i].TimeTrialScore)})*\n";
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
                if (description1 == "")
                {
                    var embed = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = "__**Error:**__",
                        Description = $"*{arg} could not be found.*",
                        Url = metric == "online" ? "https://wiimmfi.de/stats/track/mv/ww?p=std,c1,0" : "https://chadsoft.co.uk/time-trials/",
                        Footer = new DiscordEmbedBuilder.EmbedFooter
                        {
                            Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                        }
                    };
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
                }

                else if (arg.ToLowerInvariant().Contains("rts") || arg.ToLowerInvariant().Contains("cts"))
                {
                    var message = await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embeds[0]).AddComponents(Util.GeneratePageArrows()));

                    PendingPaginator pending = new PendingPaginator() { CurrentPage = 0, MessageId = message.Id, Context = ctx, Pages = embeds };

                    Util.PendingInteractions.Add(pending);
                }
                else
                {
                    var embed = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = $"__**Displaying tracks containing *{arg}*:**__",
                        Description = description1,
                        Url = metric == "online" ? "https://wiimmfi.de/stats/track/mv/ww?p=std,c1,0" : "https://chadsoft.co.uk/time-trials/",
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

        [SlashCommand("rating", "Gets the ranking from the most recent track rating forms.")]
        public async Task GetTrackRating(InteractionContext ctx,
            [Option("track-name", "The track that the best-known times are set on.")] string track = "")
        {
            try
            {
                await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder() { IsEphemeral = true });
                string description = "";
                string serviceAccountEmail = "brawlbox@custom-track-testing-bot.iam.gserviceaccount.com";

                var certificate = new X509Certificate2(@"key.p12", "notasecret", X509KeyStorageFlags.Exportable);

                ServiceAccountCredential credential = new ServiceAccountCredential(
                   new ServiceAccountCredential.Initializer(serviceAccountEmail).FromCertificate(certificate));

                var service = new SheetsService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "Mario Kart Brawlbot",
                });

                var request = service.Spreadsheets.Values.Get("1xwhKoyypCWq5tCRTI69ijJoDiaoAVsvYAxz-q4UBNqM", $"'Early {DateTime.Now.Year} Track Rating Data'!A226:Y443");
                var response = await request.ExecuteAsync();

                int e = 0;
                List<string> earlyTrackDisplay = new List<string>();

                for (int i = 0; i < response.Values.Count; i++)
                {
                    if (Util.CompareStrings(response.Values[i][0].ToString(), track) || Util.CompareIncompleteStrings(response.Values[i][0].ToString(), track) || Util.CompareStringAbbreviation(track, response.Values[i][0].ToString()) || Util.CompareStringsLevenshteinDistance(track, response.Values[i][0].ToString()))
                    {
                        earlyTrackDisplay.Add(response.Values[i][0].ToString());
                        e++;
                    }
                }

                int m = 0;
                List<string> midTrackDisplay = new List<string>();

                if (DateTime.Now > DateTime.ParseExact($"02/07/{DateTime.Now.Year}", "dd/MM/yyyy", CultureInfo.InvariantCulture))
                {
                    request = service.Spreadsheets.Values.Get("1xwhKoyypCWq5tCRTI69ijJoDiaoAVsvYAxz-q4UBNqM", $"'Mid {DateTime.Now.Year} Track Rating Data'!A226:Y443");
                    response = await request.ExecuteAsync();

                    for (int i = 0; i < response.Values.Count; i++)
                    {
                        if (Util.CompareStrings(response.Values[i][0].ToString(), track) || Util.CompareIncompleteStrings(response.Values[i][0].ToString(), track) || Util.CompareStringAbbreviation(track, response.Values[i][0].ToString()) || Util.CompareStringsLevenshteinDistance(track, response.Values[i][0].ToString()))
                        {
                            midTrackDisplay.Add(response.Values[i][0].ToString());
                            m++;
                        }
                    }
                }
                if (e < 1 && m < 1)
                {
                    var embed = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = "__**Error:**__",
                        Description = $"*{track} was not found in the latest track rating polls.*",
                        Url = "https://docs.google.com/spreadsheets/d/1xwhKoyypCWq5tCRTI69ijJoDiaoAVsvYAxz-q4UBNqM/edit#gid=595190106",
                        Footer = new DiscordEmbedBuilder.EmbedFooter
                        {
                            Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                        }
                    };
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
                }
                else if (track == "")
                {
                    response.Values = response.Values.OrderBy(t => int.Parse(t[6].ToString())).ToList();
                    string description1 = "";
                    string description2 = "";
                    string description3 = "";
                    string description4 = "";
                    string description5 = "";
                    string description6 = "";
                    string description7 = "";
                    string description8 = "";
                    string description9 = "";
                    string description10 = "";
                    string description11 = "";
                    for (int i = 0; i < 21; i++)
                    {
                        description1 = description1 + $"**{i + 1})** {response.Values[i][0]} *({Util.RankNumber(response.Values[i][12].ToString())} / {Util.RankNumber(response.Values[i][18].ToString())} / {Util.RankNumber(response.Values[i][24].ToString())})*\n";
                    }
                    for (int i = 21; i < 42; i++)
                    {
                        description2 = description2 + $"**{i + 1})** {response.Values[i][0]} *({Util.RankNumber(response.Values[i][12].ToString())} / {Util.RankNumber(response.Values[i][18].ToString())} / {Util.RankNumber(response.Values[i][24].ToString())})*\n";
                    }
                    for (int i = 42; i < 63; i++)
                    {
                        description3 = description3 + $"**{i + 1})** {response.Values[i][0]} *({Util.RankNumber(response.Values[i][12].ToString())} / {Util.RankNumber(response.Values[i][18].ToString())} / {Util.RankNumber(response.Values[i][24].ToString())})*\n";
                    }
                    for (int i = 63; i < 84; i++)
                    {
                        description4 = description4 + $"**{i + 1})** {response.Values[i][0]} *({Util.RankNumber(response.Values[i][12].ToString())} / {Util.RankNumber(response.Values[i][18].ToString())} / {Util.RankNumber(response.Values[i][24].ToString())})*\n";
                    }
                    for (int i = 84; i < 105; i++)
                    {
                        description5 = description5 + $"**{i + 1})** {response.Values[i][0]} *({Util.RankNumber(response.Values[i][12].ToString())} / {Util.RankNumber(response.Values[i][18].ToString())} / {Util.RankNumber(response.Values[i][24].ToString())})*\n";
                    }
                    for (int i = 105; i < 126; i++)
                    {
                        description6 = description6 + $"**{i + 1})** {response.Values[i][0]} *({Util.RankNumber(response.Values[i][12].ToString())} / {Util.RankNumber(response.Values[i][18].ToString())} / {Util.RankNumber(response.Values[i][24].ToString())})*\n";
                    }
                    for (int i = 126; i < 147; i++)
                    {
                        description7 = description7 + $"**{i + 1})** {response.Values[i][0]} *({Util.RankNumber(response.Values[i][12].ToString())} / {Util.RankNumber(response.Values[i][18].ToString())} / {Util.RankNumber(response.Values[i][24].ToString())})*\n";
                    }
                    for (int i = 147; i < 168; i++)
                    {
                        description8 = description8 + $"**{i + 1})** {response.Values[i][0]} *({Util.RankNumber(response.Values[i][12].ToString())} / {Util.RankNumber(response.Values[i][18].ToString())} / {Util.RankNumber(response.Values[i][24].ToString())})*\n";
                    }
                    for (int i = 168; i < 189; i++)
                    {
                        description9 = description9 + $"**{i + 1})** {response.Values[i][0]} *({Util.RankNumber(response.Values[i][12].ToString())} / {Util.RankNumber(response.Values[i][18].ToString())} / {Util.RankNumber(response.Values[i][24].ToString())})*\n";
                    }
                    for (int i = 189; i < 210; i++)
                    {
                        description10 = description10 + $"**{i + 1})** {response.Values[i][0]} *({Util.RankNumber(response.Values[i][12].ToString())} / {Util.RankNumber(response.Values[i][18].ToString())} / {Util.RankNumber(response.Values[i][24].ToString())})*\n";
                    }
                    for (int i = 210; i < 218; i++)
                    {
                        description11 = description11 + $"**{i + 1})** {response.Values[i][0]} *({Util.RankNumber(response.Values[i][12].ToString())} / {Util.RankNumber(response.Values[i][18].ToString())} / {Util.RankNumber(response.Values[i][24].ToString())})*\n";
                    }
                    List<DiscordEmbedBuilder> embeds = new List<DiscordEmbedBuilder>{
                        new DiscordEmbedBuilder
                        {
                            Color = new DiscordColor("#FF0000"),
                            Title = $"__**Displaying 1-21 (Comp/Non-comp/Creators):**__",
                            Description = description1,
                            Url = "https://docs.google.com/spreadsheets/d/1xwhKoyypCWq5tCRTI69ijJoDiaoAVsvYAxz-q4UBNqM",
                            Footer = new DiscordEmbedBuilder.EmbedFooter
                            {
                                Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                            }
                        },
                            new DiscordEmbedBuilder
                        {
                            Color = new DiscordColor("#FF0000"),
                            Title = $"__**Displaying 22-42 (Comp/Non-comp/Creators):**__",
                            Description = description2,
                            Url = "https://docs.google.com/spreadsheets/d/1xwhKoyypCWq5tCRTI69ijJoDiaoAVsvYAxz-q4UBNqM",
                            Footer = new DiscordEmbedBuilder.EmbedFooter
                            {
                                Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                            }
                        },
                        new DiscordEmbedBuilder
                        {
                            Color = new DiscordColor("#FF0000"),
                            Title = $"__**Displaying 43-63 (Comp/Non-comp/Creators):**__",
                            Description = description3,
                            Url = "https://docs.google.com/spreadsheets/d/1xwhKoyypCWq5tCRTI69ijJoDiaoAVsvYAxz-q4UBNqM",
                            Footer = new DiscordEmbedBuilder.EmbedFooter
                            {
                                Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                            }
                        },
                        new DiscordEmbedBuilder
                        {
                            Color = new DiscordColor("#FF0000"),
                            Title = $"__**Displaying 64-84 (Comp/Non-comp/Creators):**__",
                            Description = description4,
                            Url = "https://docs.google.com/spreadsheets/d/1xwhKoyypCWq5tCRTI69ijJoDiaoAVsvYAxz-q4UBNqM",
                            Footer = new DiscordEmbedBuilder.EmbedFooter
                            {
                                Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                            }
                        },
                        new DiscordEmbedBuilder
                        {
                            Color = new DiscordColor("#FF0000"),
                            Title = $"__**Displaying 85-105 (Comp/Non-comp/Creators):**__",
                            Description = description5,
                            Url = "https://docs.google.com/spreadsheets/d/1xwhKoyypCWq5tCRTI69ijJoDiaoAVsvYAxz-q4UBNqM",
                            Footer = new DiscordEmbedBuilder.EmbedFooter
                            {
                                Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                            }
                        },
                        new DiscordEmbedBuilder
                        {
                            Color = new DiscordColor("#FF0000"),
                            Title = $"__**Displaying 106-126 (Comp/Non-comp/Creators):**__",
                            Description = description6,
                            Url = "https://docs.google.com/spreadsheets/d/1xwhKoyypCWq5tCRTI69ijJoDiaoAVsvYAxz-q4UBNqM",
                            Footer = new DiscordEmbedBuilder.EmbedFooter
                            {
                                Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                            }
                        },
                        new DiscordEmbedBuilder
                        {
                            Color = new DiscordColor("#FF0000"),
                            Title = $"__**Displaying 127-147 (Comp/Non-comp/Creators):**__",
                            Description = description7,
                            Url = "https://docs.google.com/spreadsheets/d/1xwhKoyypCWq5tCRTI69ijJoDiaoAVsvYAxz-q4UBNqM",
                            Footer = new DiscordEmbedBuilder.EmbedFooter
                            {
                                Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                            }
                        },
                        new DiscordEmbedBuilder
                        {
                            Color = new DiscordColor("#FF0000"),
                            Title = $"__**Displaying 148-168 (Comp/Non-comp/Creators):**__",
                            Description = description8,
                            Url = "https://docs.google.com/spreadsheets/d/1xwhKoyypCWq5tCRTI69ijJoDiaoAVsvYAxz-q4UBNqM",
                            Footer = new DiscordEmbedBuilder.EmbedFooter
                            {
                                Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                            }
                        },
                        new DiscordEmbedBuilder
                        {
                            Color = new DiscordColor("#FF0000"),
                            Title = $"__**Displaying 169-189 (Comp/Non-comp/Creators):**__",
                            Description = description9,
                            Url = "https://docs.google.com/spreadsheets/d/1xwhKoyypCWq5tCRTI69ijJoDiaoAVsvYAxz-q4UBNqM",
                            Footer = new DiscordEmbedBuilder.EmbedFooter
                            {
                                Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                            }
                        },
                        new DiscordEmbedBuilder
                        {
                            Color = new DiscordColor("#FF0000"),
                            Title = $"__**Displaying 190-210 (Comp/Non-comp/Creators):**__",
                            Description = description10,
                            Url = "https://docs.google.com/spreadsheets/d/1xwhKoyypCWq5tCRTI69ijJoDiaoAVsvYAxz-q4UBNqM",
                            Footer = new DiscordEmbedBuilder.EmbedFooter
                            {
                                Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                            }
                        },
                        new DiscordEmbedBuilder
                        {
                            Color = new DiscordColor("#FF0000"),
                            Title = $"__**Displaying 211-218 (Comp/Non-comp/Creators):**__",
                            Description = description11,
                            Url = "https://docs.google.com/spreadsheets/d/1xwhKoyypCWq5tCRTI69ijJoDiaoAVsvYAxz-q4UBNqM",
                            Footer = new DiscordEmbedBuilder.EmbedFooter
                            {
                                Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                            }
                        }
                    };
                    var message = await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embeds[0]).AddComponents(Util.GeneratePageArrows()));

                    PendingPaginator pending = new PendingPaginator() { CurrentPage = 0, MessageId = message.Id, Context = ctx, Pages = embeds };

                    Util.PendingInteractions.Add(pending);
                }
                else
                {
                    request = service.Spreadsheets.Values.Get("1xwhKoyypCWq5tCRTI69ijJoDiaoAVsvYAxz-q4UBNqM", $"'Early {DateTime.Now.Year} Track Rating Graphs'");
                    response = await request.ExecuteAsync();
                    foreach (var t in response.Values)
                    {
                        while (t.Count < 15)
                        {
                            t.Add("");
                        }
                    }

                    int ix = -1;

                    for (int i = 0; i < response.Values.Count; i++)
                    {
                        if (response.Values[i][12].ToString().Contains("Average Track"))
                        {
                            ix = i + 2;
                            break;
                        }
                    }

                    string earlyAverage = $"{Math.Round((double.Parse(response.Values[ix][12].ToString().Replace("%", string.Empty)) / (double.Parse(response.Values[ix][12].ToString().Replace("%", string.Empty)) + double.Parse(response.Values[ix][13].ToString().Replace("%", string.Empty)) + double.Parse(response.Values[ix][14].ToString().Replace("%", string.Empty)))) * 100)}/{Math.Round((double.Parse(response.Values[ix][13].ToString().Replace("%", string.Empty)) / (double.Parse(response.Values[ix][12].ToString().Replace("%", string.Empty)) + double.Parse(response.Values[ix][13].ToString().Replace("%", string.Empty)) + double.Parse(response.Values[ix][14].ToString().Replace("%", string.Empty)))) * 100)}/{Math.Round((double.Parse(response.Values[ix][14].ToString().Replace("%", string.Empty)) / (double.Parse(response.Values[ix][12].ToString().Replace("%", string.Empty)) + double.Parse(response.Values[ix][13].ToString().Replace("%", string.Empty)) + double.Parse(response.Values[ix][14].ToString().Replace("%", string.Empty)))) * 100)}";

                    request = service.Spreadsheets.Values.Get("1xwhKoyypCWq5tCRTI69ijJoDiaoAVsvYAxz-q4UBNqM", $"'Early {DateTime.Now.Year} Track Rating Data'!A226:Y443");
                    response = await request.ExecuteAsync();

                    if (earlyTrackDisplay.Count > 0)
                    {
                        description += $"__**Early {DateTime.Now.Year} Track Rating Data (Average: {earlyAverage}%):**__\n";
                        for (int i = 0; i < earlyTrackDisplay.Count; i++)
                        {
                            foreach (var t in response.Values)
                            {
                                if (earlyTrackDisplay[i] == t[0].ToString())
                                {
                                    description += $"__{t[0]}__:\nAll - {Math.Round((double.Parse(t[2].ToString()) / (double.Parse(t[2].ToString()) + double.Parse(t[3].ToString()) + double.Parse(t[4].ToString()))) * 100)}/{Math.Round((double.Parse(t[3].ToString()) / (double.Parse(t[2].ToString()) + double.Parse(t[3].ToString()) + double.Parse(t[4].ToString()))) * 100)}/{Math.Round((double.Parse(t[4].ToString()) / (double.Parse(t[2].ToString()) + double.Parse(t[3].ToString()) + double.Parse(t[4].ToString()))) * 100)}% - {Util.RankNumber(t[6].ToString())}\n";
                                    description += $"Comp - {Math.Round((double.Parse(t[8].ToString()) / (double.Parse(t[8].ToString()) + double.Parse(t[9].ToString()) + double.Parse(t[10].ToString()))) * 100)}/{Math.Round((double.Parse(t[9].ToString()) / (double.Parse(t[8].ToString()) + double.Parse(t[9].ToString()) + double.Parse(t[10].ToString()))) * 100)}/{Math.Round((double.Parse(t[10].ToString()) / (double.Parse(t[8].ToString()) + double.Parse(t[9].ToString()) + double.Parse(t[10].ToString()))) * 100)}% - {Util.RankNumber(t[12].ToString())}\n";
                                    description += $"Non-Comp - {Math.Round((double.Parse(t[14].ToString()) / (double.Parse(t[14].ToString()) + double.Parse(t[15].ToString()) + double.Parse(t[16].ToString()))) * 100)}/{Math.Round((double.Parse(t[15].ToString()) / (double.Parse(t[14].ToString()) + double.Parse(t[15].ToString()) + double.Parse(t[16].ToString()))) * 100)}/{Math.Round((double.Parse(t[16].ToString()) / (double.Parse(t[14].ToString()) + double.Parse(t[15].ToString()) + double.Parse(t[16].ToString()))) * 100)}% - {Util.RankNumber(t[18].ToString())}\n";
                                    description += $"Creators - {Math.Round((double.Parse(t[20].ToString()) / (double.Parse(t[20].ToString()) + double.Parse(t[21].ToString()) + double.Parse(t[22].ToString()))) * 100)}/{Math.Round((double.Parse(t[21].ToString()) / (double.Parse(t[20].ToString()) + double.Parse(t[21].ToString()) + double.Parse(t[22].ToString()))) * 100)}/{Math.Round((double.Parse(t[22].ToString()) / (double.Parse(t[20].ToString()) + double.Parse(t[21].ToString()) + double.Parse(t[22].ToString()))) * 100)}% - {Util.RankNumber(t[24].ToString())}\n";
                                }
                            }
                        }
                    }

                    if (DateTime.Now > DateTime.ParseExact($"02/07/{DateTime.Now.Year}", "dd/MM/yyyy", CultureInfo.InvariantCulture))
                    {
                        request = service.Spreadsheets.Values.Get("1xwhKoyypCWq5tCRTI69ijJoDiaoAVsvYAxz-q4UBNqM", $"'Mid {DateTime.Now.Year} Track Rating Graphs'");
                        response = await request.ExecuteAsync();
                        foreach (var t in response.Values)
                        {
                            while (t.Count < 15)
                            {
                                t.Add("");
                            }
                        }

                        ix = -1;

                        for (int i = 0; i < response.Values.Count; i++)
                        {
                            if (response.Values[i][12].ToString().Contains("Average Track"))
                            {
                                ix = i + 2;
                                break;
                            }
                        }

                        string midAverage = $"{Math.Round((double.Parse(response.Values[ix][12].ToString().Replace("%", string.Empty)) / (double.Parse(response.Values[ix][12].ToString().Replace("%", string.Empty)) + double.Parse(response.Values[ix][13].ToString().Replace("%", string.Empty)) + double.Parse(response.Values[ix][14].ToString().Replace("%", string.Empty)))) * 100)}/{Math.Round((double.Parse(response.Values[ix][13].ToString().Replace("%", string.Empty)) / (double.Parse(response.Values[ix][12].ToString().Replace("%", string.Empty)) + double.Parse(response.Values[ix][13].ToString().Replace("%", string.Empty)) + double.Parse(response.Values[ix][14].ToString().Replace("%", string.Empty)))) * 100)}/{Math.Round((double.Parse(response.Values[ix][14].ToString().Replace("%", string.Empty)) / (double.Parse(response.Values[ix][12].ToString().Replace("%", string.Empty)) + double.Parse(response.Values[ix][13].ToString().Replace("%", string.Empty)) + double.Parse(response.Values[ix][14].ToString().Replace("%", string.Empty)))) * 100)}";

                        request = service.Spreadsheets.Values.Get("1xwhKoyypCWq5tCRTI69ijJoDiaoAVsvYAxz-q4UBNqM", $"'Mid {DateTime.Now.Year} Track Rating Data'!A226:Y443");
                        response = await request.ExecuteAsync();

                        if (midTrackDisplay.Count > 0)
                        {
                            description += $"__**Mid {DateTime.Now.Year} Track Rating Data (Average: {midAverage}%):**__\n";
                            for (int i = 0; i < midTrackDisplay.Count; i++)
                            {
                                foreach (var t in response.Values)
                                {
                                    if (midTrackDisplay[i] == t[0].ToString())
                                    {
                                        description += $"__{t[0]}__:\nAll - {Math.Round((double.Parse(t[2].ToString()) / (double.Parse(t[2].ToString()) + double.Parse(t[3].ToString()) + double.Parse(t[4].ToString()))) * 100)}/{Math.Round((double.Parse(t[3].ToString()) / (double.Parse(t[2].ToString()) + double.Parse(t[3].ToString()) + double.Parse(t[4].ToString()))) * 100)}/{Math.Round((double.Parse(t[4].ToString()) / (double.Parse(t[2].ToString()) + double.Parse(t[3].ToString()) + double.Parse(t[4].ToString()))) * 100)}% - {Util.RankNumber(t[6].ToString())}\n";
                                        description += $"Comp - {Math.Round((double.Parse(t[8].ToString()) / (double.Parse(t[8].ToString()) + double.Parse(t[9].ToString()) + double.Parse(t[10].ToString()))) * 100)}/{Math.Round((double.Parse(t[9].ToString()) / (double.Parse(t[8].ToString()) + double.Parse(t[9].ToString()) + double.Parse(t[10].ToString()))) * 100)}/{Math.Round((double.Parse(t[10].ToString()) / (double.Parse(t[8].ToString()) + double.Parse(t[9].ToString()) + double.Parse(t[10].ToString()))) * 100)}% - {Util.RankNumber(t[12].ToString())}\n";
                                        description += $"Non-Comp - {Math.Round((double.Parse(t[14].ToString()) / (double.Parse(t[14].ToString()) + double.Parse(t[15].ToString()) + double.Parse(t[16].ToString()))) * 100)}/{Math.Round((double.Parse(t[15].ToString()) / (double.Parse(t[14].ToString()) + double.Parse(t[15].ToString()) + double.Parse(t[16].ToString()))) * 100)}/{Math.Round((double.Parse(t[16].ToString()) / (double.Parse(t[14].ToString()) + double.Parse(t[15].ToString()) + double.Parse(t[16].ToString()))) * 100)}% - {Util.RankNumber(t[18].ToString())}\n";
                                        description += $"Creators - {Math.Round((double.Parse(t[20].ToString()) / (double.Parse(t[20].ToString()) + double.Parse(t[21].ToString()) + double.Parse(t[22].ToString()))) * 100)}/{Math.Round((double.Parse(t[21].ToString()) / (double.Parse(t[20].ToString()) + double.Parse(t[21].ToString()) + double.Parse(t[22].ToString()))) * 100)}/{Math.Round((double.Parse(t[22].ToString()) / (double.Parse(t[20].ToString()) + double.Parse(t[21].ToString()) + double.Parse(t[22].ToString()))) * 100)}% - {Util.RankNumber(t[24].ToString())}\n";
                                    }
                                }
                            }
                        }
                    }

                    if (description.ToCharArray().Length == 0)
                    {
                        var embed = new DiscordEmbedBuilder
                        {
                            Color = new DiscordColor("#FF0000"),
                            Title = "__**Error:**__",
                            Description = $"*{track} was not found in the latest track rating polls.*",
                            Url = "https://docs.google.com/spreadsheets/d/1xwhKoyypCWq5tCRTI69ijJoDiaoAVsvYAxz-q4UBNqM/edit#gid=595190106",
                            Footer = new DiscordEmbedBuilder.EmbedFooter
                            {
                                Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                            }
                        };
                        await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
                    }
                    else
                    {
                        var embed = new DiscordEmbedBuilder
                        {
                            Color = new DiscordColor("#FF0000"),
                            Title = $"__**{DateTime.Now.Year} Track Ratings for {track} (Remove/Indifferent/Keep - Rank):**__",
                            Description = description,
                            Url = "https://docs.google.com/spreadsheets/d/1xwhKoyypCWq5tCRTI69ijJoDiaoAVsvYAxz-q4UBNqM/edit#gid=595190106",
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
    }
}