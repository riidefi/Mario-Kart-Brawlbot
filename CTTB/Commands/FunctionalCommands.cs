using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Newtonsoft.Json;
using OpenQA.Selenium;
using System;
using System.Globalization;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Sheets.v4;
using Google.Apis.Services;
using System.Text;
using System.Security.Cryptography.X509Certificates;
using System.Timers;
using DSharpPlus;
using DSharpPlus.EventArgs;
using System.Linq.Expressions;
using IronPython.Runtime;
using Google.Apis.Sheets.v4.Data;
using IronPython.Runtime.Operations;

namespace CTTB.Commands
{
    public class FunctionalCommands : BaseCommandModule
    {
        /*public void UpdateTimer()
        {
            var timer = new Timer(604800000);
            timer.AutoReset = true;
            timer.Elapsed += async (s, e) => await Update(null);
            timer.Start();
        }*/

        [Command("update")]
        [RequireRoles(RoleCheckMode.Any, "Pack & Bot Dev", "Admin")]
        public async Task Update(CommandContext ctx)
        {
            if (ctx != null)
                await ctx.TriggerTypingAsync();

            string rtttUrl = "http://tt.chadsoft.co.uk/original-track-leaderboards.json";
            string ctttUrl = "http://tt.chadsoft.co.uk/ctgp-leaderboards.json";
            string rttt200Url = "http://tt.chadsoft.co.uk/original-track-leaderboards-200cc.json";
            string cttt200Url = "http://tt.chadsoft.co.uk/ctgp-leaderboards-200cc.json";
            string ctwwUrl = "https://wiimmfi.de/stats/track/mv/ctgp?m=json&p=std,c1,0";
            string wwUrl = "https://wiimmfi.de/stats/track/mv/ww?m=json&p=std,c1,0";

            // Leaderboards

            var rtRawJson = JsonConvert.DeserializeObject<LeaderboardInfo>(new WebClient().DownloadString(rtttUrl));

            var ctRawJson = JsonConvert.DeserializeObject<LeaderboardInfo>(new WebClient().DownloadString(ctttUrl));

            var rtRaw200Json = JsonConvert.DeserializeObject<LeaderboardInfo>(new WebClient().DownloadString(rttt200Url));

            var ctRaw200Json = JsonConvert.DeserializeObject<LeaderboardInfo>(new WebClient().DownloadString(cttt200Url));

            var rtJson = JsonConvert.SerializeObject(rtRawJson.Leaderboard);
            File.WriteAllText("rts.json", rtJson);

            var ctJson = JsonConvert.SerializeObject(ctRawJson.Leaderboard);
            File.WriteAllText("cts.json", ctJson);

            var rt200Json = JsonConvert.SerializeObject(rtRaw200Json.Leaderboard);
            File.WriteAllText("rts200.json", rt200Json);

            var ct200Json = JsonConvert.SerializeObject(ctRaw200Json.Leaderboard);
            File.WriteAllText("cts200.json", ct200Json);

            var embed = new DiscordEmbedBuilder
            {
                Color = new DiscordColor("#FF0000"),
                Title = $"__**Notice:**__",
                Description = "Database has been updated.",
                Timestamp = DateTime.UtcNow
            };

            if (ctx != null)
                await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
        }

        [Command("nextupdate")]
        public async Task GetNextUpdate(CommandContext ctx)
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

                string description = "**New:**";

                int k = 3;
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
                description += $"\n**Major:**";
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
                description += $"\n**Minor:**";
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

                embed = new DiscordEmbedBuilder
                {
                    Color = new DiscordColor("#FF0000"),
                    Title = $"__**{response.Values[1][1]}:**__",
                    Description = description,
                    Timestamp = DateTime.UtcNow
                };
                await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                embed = new DiscordEmbedBuilder
                {
                    Color = new DiscordColor("#FF0000"),
                    Title = $"__**Error:**__",
                    Description = $"*An exception has occured.*" +
                           "\n**c!nextupdate**",
                    Timestamp = DateTime.UtcNow
                };
                await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);

                Console.WriteLine(ex.ToString());
            }
        }

        [Command("getsummary")]
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
                               "\n**c!getsummary [name of track]**",
                        Timestamp = DateTime.UtcNow
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

                    string trackDisplay = string.Empty;

                    for (int i = 0; i < response.Values.Count; i++)
                    {
                        if (j < 1)
                        {
                            if (response.Values[i][2].ToString().ToLowerInvariant().Contains(track.ToLowerInvariant()))
                            {
                                var tally = response.Values[i][1].ToString().split("\n");
                                if (tally[0].ToString() == "✘")
                                {
                                    tally[0] = DiscordEmoji.FromName(ctx.Client, ":No:");
                                }
                                else if (tally[0].ToString() == "✔")
                                {
                                    tally[0] = DiscordEmoji.FromName(ctx.Client, ":Yes:");
                                }
                                description = $"**{response.Values[i][2]} {response.Values[i][4]} - {response.Values[i][3]}**\n{tally[1]} {tally[0]}\n\n{response.Values[i][6]}";
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
                                   "\n**c!getsummary [name of track]**",
                            Timestamp = DateTime.UtcNow
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
                            Timestamp = DateTime.UtcNow
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
                    Description = $"*An exception has occured.*" +
                           "\n**c!getsummary [track]**",
                    Timestamp = DateTime.UtcNow
                };
                await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);

                Console.WriteLine(ex.ToString());
            }
        }

        [Command("addhw")]
        [RequireRoles(RoleCheckMode.Any, "Pack & Bot Dev", "Admin")]
        public async Task AddHomework(CommandContext ctx, string track = "", string author = "", string version = "", string download = "", string slot = "", string lapSpeed = "1/3", [RemainingText] string notes = "")
        {
            var embed = new DiscordEmbedBuilder { };

            try
            {
                if (ctx.Channel.Id == 217126063803727872 || ctx.Channel.Id == 750123394237726847 || ctx.Channel.Id == 935200150710808626 || ctx.Channel.Id == 946835035372257320)
                {
                    string description = string.Empty;

                    if (track == "")
                    {
                        embed = new DiscordEmbedBuilder
                        {
                            Color = new DiscordColor("#FF0000"),
                            Title = $"__**Error:**__",
                            Description = $"*Track was not inputted.*" +
                                   "\n**c!addhw [name of track] [author] [version] [slot (e.g. Luigi Circuit\nbeginner_course)] [speed/lap modifiers] [notes]**",
                            Timestamp = DateTime.UtcNow
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
                                   "\n**c!addhw [name of track] [author] [version] [slot (e.g. Luigi Circuit\nbeginner_course)] [speed/lap modifiers] [notes]**",
                            Timestamp = DateTime.UtcNow
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
                                   "\n**c!addhw [name of track] [author] [version] [slot (e.g. Luigi Circuit\nbeginner_course)] [speed/lap modifiers] [notes]**",
                            Timestamp = DateTime.UtcNow
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
                                   "\n**c!addhw [name of track] [author] [version] [slot (e.g. Luigi Circuit\nbeginner_course)] [speed/lap modifiers] [notes]**",
                            Timestamp = DateTime.UtcNow
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

                        IList<object> obj = new List<Object>();
                        obj.Add(track);
                        obj.Add($"{dueMonth} {due.Day}, {due.Year}");
                        obj.Add(author);
                        obj.Add(version);
                        obj.Add(dl);
                        obj.Add(slot);
                        obj.Add(lapSpeed);
                        obj.Add(notes);
                        obj.Add($"=COUNTIF($M{countResponse.Values.Count + 1}:$AO{countResponse.Values.Count + 1}, \"yes*\")");
                        obj.Add($"=COUNTIF($M{countResponse.Values.Count + 1}:$AO{countResponse.Values.Count + 1}, \"fixes*\")");
                        obj.Add($"=COUNTIF($M{countResponse.Values.Count + 1}:$AO{countResponse.Values.Count + 1}, \"neutral*\")");
                        obj.Add($"=COUNTIF($M{countResponse.Values.Count + 1}:$AO{countResponse.Values.Count + 1}, \"no*\")");
                        IList<IList<Object>> values = new List<IList<Object>>();
                        values.Add(obj);

                        var request = service.Spreadsheets.Values.Append(new Google.Apis.Sheets.v4.Data.ValueRange() { Values = values }, "1I9yFsomTcvFT4hp6eN2azsfv6MsIy1897tBFX_gmtss", "'Track Evaluating'!A1:A1");
                        request.InsertDataOption = SpreadsheetsResource.ValuesResource.AppendRequest.InsertDataOptionEnum.INSERTROWS;
                        request.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
                        var response = await request.ExecuteAsync();

                        embed = new DiscordEmbedBuilder
                        {
                            Color = new DiscordColor("#FF0000"),
                            Title = $"__**Success:**__",
                            Description = $"*{track} has been added as homework.*",
                            Timestamp = DateTime.UtcNow
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
                    Description = $"*An exception has occured.*" +
                           "\n**c!addhw [name of track] [author] [version] [download link] [slot (e.g. Luigi Circuit (beginner_course)] [[speed/lap modifiers] [notes]**",
                    Timestamp = DateTime.UtcNow
                };
                await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);

                Console.WriteLine(ex.ToString());

            }
        }

        [Command("delhw")]
        [RequireRoles(RoleCheckMode.Any, "Pack & Bot Dev", "Admin")]
        public async Task DeleteHomework(CommandContext ctx, [RemainingText] string track = "")
        {
            var embed = new DiscordEmbedBuilder { };
            try
            {
                if (ctx.Channel.Id == 217126063803727872 || ctx.Channel.Id == 750123394237726847 || ctx.Channel.Id == 935200150710808626 || ctx.Channel.Id == 946835035372257320)
                {
                    if (track == "")
                    {
                        embed = new DiscordEmbedBuilder
                        {
                            Color = new DiscordColor("#FF0000"),
                            Title = $"__**Error:**__",
                            Description = $"*Track was not inputted.*" +
                                   "\n**c!submithw [yes/fixes/neutral/no] [name of track] [feedback]**",
                            Timestamp = DateTime.UtcNow
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
                            if (response.Values[i][0].ToString().ToLowerInvariant() == track.ToLowerInvariant())
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
                                       "\n**c!delhw [name of track]**",
                                Timestamp = DateTime.UtcNow
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
                                Timestamp = DateTime.UtcNow
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
                    Description = $"*An exception has occured.*" +
                           "\n**c!delhw [name of track]**",
                    Timestamp = DateTime.UtcNow
                };
                await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);

                Console.WriteLine(ex.ToString());
            }
        }

        [Command("submithw")]
        [RequireRoles(RoleCheckMode.Any, "Track Council")]
        public async Task SubmitHomework(CommandContext ctx, string vote = "", string track = "", [RemainingText] string feedback = "")
        {
            var embed = new DiscordEmbedBuilder { };
            try
            {
                if (ctx.Channel.Id == 217126063803727872 || ctx.Channel.Id == 750123394237726847 || ctx.Channel.Id == 935200150710808626 || ctx.Channel.Id == 946835035372257320)
                {
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
                                   "\n**c!submithw [yes/fixes/neutral/no] [name of track] [feedback]**",
                            Timestamp = DateTime.UtcNow
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
                                   "\n**c!submithw [yes/fixes/neutral/no] [name of track] [feedback]**",
                            Timestamp = DateTime.UtcNow
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
                                      "\n**c!submithw [yes/fixes/neutral/no] [name of track] [feedback]**",
                            Timestamp = DateTime.UtcNow
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
                            if (response.Values[0][i].ToString() == member)
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
                           "\n**c!submithw [yes/fixes/neutral/no] [name of track] [feedback]**",
                                Timestamp = DateTime.UtcNow
                            };
                            await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
                        }

                        else
                        {

                            int j = 0;

                            foreach (var t in response.Values)
                            {
                                if (j > 0)
                                {
                                    break;
                                }
                                else if (t[0].ToString().ToLowerInvariant() == track.ToLowerInvariant())
                                {
                                    t[ix] = vote + "\n" + feedback;
                                    j++;
                                }
                            }

                            var updateRequest = service.Spreadsheets.Values.Update(response, "1I9yFsomTcvFT4hp6eN2azsfv6MsIy1897tBFX_gmtss", $"'Track Evaluating'!A1:AO10");
                            updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;
                            var update = await updateRequest.ExecuteAsync();

                            if (j == 0)
                            {
                                embed = new DiscordEmbedBuilder
                                {
                                    Color = new DiscordColor("#FF0000"),
                                    Title = "__**Error:**__",
                                    Description = $"*{track} could not be found.*" +
                               "\n**c!submithw [yes/fixes/neutral/no] [name of track] [feedback]**",
                                    Timestamp = DateTime.UtcNow
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
                                    Timestamp = DateTime.UtcNow
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
                    Description = $"*An exception has occured.*" +
                           "\n**c!submithw [yes/fixes/neutral/no] [name of track] [feedback]**",
                    Timestamp = DateTime.UtcNow
                };
                await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);

                Console.WriteLine(ex.ToString());
            }
        }

        [Command("gethw")]
        [RequireRoles(RoleCheckMode.Any, "Track Council")]
        public async Task GetHomework(CommandContext ctx, string track = "", string mention = "")
        {
            var embed = new DiscordEmbedBuilder { };
            try
            {
                if (ctx.Channel.Id == 217126063803727872 || ctx.Channel.Id == 750123394237726847 || ctx.Channel.Id == 935200150710808626 || ctx.Channel.Id == 946835035372257320)
                {
                    string description = string.Empty;
                    string json = string.Empty;
                    string member = string.Empty;

                    int j = 0;
                    foreach (var role in ctx.Member.Roles)
                    {
                        if (role.Name == "Admin" || role.Name == "Pack & Bot Dev")
                        {
                            j++;
                            break;
                        }
                    }
                    if (j == 0)
                    {
                        mention = $"<@{ctx.Member.Id}>";
                    }

                    if (track == "")
                    {
                        embed = new DiscordEmbedBuilder
                        {
                            Color = new DiscordColor("#FF0000"),
                            Title = $"__**Error:**__",
                            Description = $"*Track was not inputted.*" +
                                   "\n**c!gethw [name of track] [mention/name]**",
                            Timestamp = DateTime.UtcNow
                        };
                        await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
                    }
                    else
                    {
                        using (var fs = File.OpenRead("council.json"))
                        using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                            json = await sr.ReadToEndAsync().ConfigureAwait(false);
                        List<CouncilMember> councilJson = JsonConvert.DeserializeObject<List<CouncilMember>>(json);

                        int l = 0;

                        if (mention == "")
                        {
                            mention = $"<@{ctx.Member.Id}>";
                            l++;
                        }
                        else if (!mention.Contains("<") || !mention.Contains(">") || !mention.Contains("@"))
                        {
                            foreach (var m in councilJson)
                            {
                                if (m.SheetName.ToLowerInvariant() == mention.ToLowerInvariant())
                                {
                                    mention = $"<@{m.DiscordId}>";
                                    l++;
                                }
                            }
                        }
                        foreach (var m in councilJson)
                        {
                            if ($"<@!{m.DiscordId}>" == mention)
                            {
                                l++;
                            }
                        }

                        ulong parsedMention = 0;

                        if (l > 0)
                        {
                            parsedMention = ulong.Parse(mention.Replace("<", "").Replace(">", "").Replace("@", "").Replace("!", "").Replace("&", ""));
                            foreach (var m in councilJson)
                            {
                                if (m.DiscordId == parsedMention)
                                {
                                    member = m.SheetName;
                                }
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
                            if (response.Values[0][i].ToString() == member)
                            {
                                ix = i;
                            }
                        }

                        j = 0;
                        int k = 0;
                        l = 0;
                        string trackDisplay = string.Empty;

                        foreach (var m in response.Values[0])
                        {
                            if (m.ToString() == member)
                            {
                                k++;
                            }
                        }

                        if (ix > 11)
                        {
                            foreach (var t in response.Values)
                            {
                                if (j < 1)
                                {
                                    j++;
                                }
                                else if (t[0].ToString().ToLowerInvariant() == track.ToLowerInvariant())
                                {
                                    if (t[ix].ToString() == "")
                                    {
                                        description = $"*{mention} has not done their homework yet.*";
                                    }
                                    else
                                    {
                                        if (t[ix].ToString().ToCharArray().Length > 3500)
                                        {
                                            description = $"**Homework of {mention}:**\n{t[ix].ToString().Remove(3499)}...\n\n*For full feedback go to the [Track Council Sheet](https://docs.google.com/spreadsheets/d/1I9yFsomTcvFT4hp6eN2azsfv6MsIy1897tBFX_gmtss/edit#gid=906385082).*";
                                        }
                                        else
                                        {
                                            description = $"**Homework of {mention}:**\n{t[ix]}";
                                        }
                                    }
                                    trackDisplay = t[0].ToString();
                                    l++;
                                }
                            }
                        }

                        if (ix < 0)
                        {
                            embed = new DiscordEmbedBuilder
                            {
                                Color = new DiscordColor("#FF0000"),
                                Title = $"__**Error:**__",
                                Description = $"*{mention} could not be found on council.*" +
                                   "\n**c!gethw [name of track] [mention/name]**",
                                Timestamp = DateTime.UtcNow
                            };
                            await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
                        }

                        else if (l == 0)
                        {
                            embed = new DiscordEmbedBuilder
                            {
                                Color = new DiscordColor("#FF0000"),
                                Title = $"__**Error:**__",
                                Description = $"*{track} could not be found.*" +
                                   "\n**c!gethw [name of track] [mention/name]**",
                                Timestamp = DateTime.UtcNow
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
                                Timestamp = DateTime.UtcNow
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
                    Description = $"*An exception has occured.*" +
                           "\n**c!gethw [name of track] [mention/name]**",
                    Timestamp = DateTime.UtcNow
                };
                await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);

                Console.WriteLine(ex.ToString());
            }
        }

        [Command("hw")]
        [RequireRoles(RoleCheckMode.Any, "Track Council")]
        public async Task GetHomework(CommandContext ctx)
        {
            var embed = new DiscordEmbedBuilder { };
            try
            {
                if (ctx.Channel.Id == 217126063803727872 || ctx.Channel.Id == 750123394237726847 || ctx.Channel.Id == 935200150710808626 || ctx.Channel.Id == 946835035372257320)
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
                        while (t.Count < 41)
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
                        description += $"{t[0]} | {tRaw[1]} | [Download]({t[4].ToString().Split('"')[1]}) | {tally}\n";
                    }
                    embed = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = $"__**Council Homework:**__",
                        Description = description,
                        Timestamp = DateTime.UtcNow
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
                    Description = $"*An exception has occured.*" +
                           "\n**c!hw [name of track]**",
                    Timestamp = DateTime.UtcNow
                };
                await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);

                Console.WriteLine(ex.ToString());
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
                        if (trackList[i].Name.ToLowerInvariant().Contains(track.ToLowerInvariant()))
                        {
                            foreach (var t in response.Values)
                            {
                                if (t[0].ToString().ToLowerInvariant().Contains(track.ToLowerInvariant()))
                                {
                                    description = $"**Easy:**\n*{t[1]}*\n**Expert:**\n*{t[2]}*";
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
                        Description = $"*{track} could not be found.*" +
                        "\n**c!staff [name of track]**",
                        Timestamp = DateTime.UtcNow
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
                        Timestamp = DateTime.UtcNow
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
                    Description = $"*An exception has occured.*" +
                           "\n**c!staff [name of track]**",
                    Timestamp = DateTime.UtcNow
                };
                await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);

                Console.WriteLine(ex.ToString());
            }
        }

        [Command("getinfo")]
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
                        if (Regex.Replace(trackList[i].Name.ToLowerInvariant(), "_", " ").Contains(track.ToLowerInvariant()))
                        {
                            foreach (var t in response.Values)
                            {
                                if (t[0].ToString().ToLowerInvariant().Contains(track.ToLowerInvariant()))
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

                if (j < 1)
                {
                    embed = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = "__**Error:**__",
                        Description = $"*{track} could not be found.\nThe track does not exist, or is not in CTGP.*" +
                        "\n**c!getinfo [name of track]**",
                        Timestamp = DateTime.UtcNow
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
                        Timestamp = DateTime.UtcNow
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
                    Description = $"*An exception has occured.*" +
                           "\n**c!getinfo [name of track]**",
                    Timestamp = DateTime.UtcNow
                };
                await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);

                Console.WriteLine(ex.ToString());
            }
        }

        [Command("getissues")]
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

                for (int i = 0; i < trackList.Count; i++)
                {
                    if (j < 1)
                    {
                        if (Regex.Replace(trackList[i].Name.ToLowerInvariant(), "_", " ").Contains(track.ToLowerInvariant()))
                        {
                            foreach (var t in response.Values)
                            {
                                if (t[0].ToString().ToLowerInvariant().Contains(track.ToLowerInvariant()))
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
                        "\n**c!getissues [name of track]**",
                        Timestamp = DateTime.UtcNow
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
                        Timestamp = DateTime.UtcNow
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
                    Description = $"*An exception has occured.*" +
                           "\n**c!getissues [name of track]**",
                    Timestamp = DateTime.UtcNow
                };
                await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);

                Console.WriteLine(ex.ToString());
            }
        }

        [Command("reportissue")]
        [RequireRoles(RoleCheckMode.Any, "Pack & Bot Dev", "Admin")]
        public async Task ReportIssue(CommandContext ctx, string issueType = "", string track = "", [RemainingText] string issue = "")
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

            if (issueType.ToLowerInvariant() == "major")
            {
                k = 5;
            }

            else if (issueType.ToLowerInvariant() == "minor")
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
                            if (Regex.Replace(trackList[i].Name.ToLowerInvariant(), "_", " ") == track.ToLowerInvariant())
                            {
                                foreach (var t in response.Values)
                                {
                                    if (t[0].ToString().ToLowerInvariant() == track.ToLowerInvariant())
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
                else if (issue == "")
                {
                    embed = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = "__**Error:**__",
                        Description = $"*No issue was inputted.*" +
                               "\n**c!reportissue [issue category] [name of track (in quotes)] [issue]**",
                        Timestamp = DateTime.UtcNow
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
                               "\n**c!reportissue [issue category] [name of track (in quotes)] [issue]**",
                        Timestamp = DateTime.UtcNow
                    };
                    await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
                }

                if (j < 1)
                {
                    embed = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = "__**Error:**__",
                        Description = $"*{track} could not be found.*" +
                                  "\n**c!reportissue [issue category] [name of track (in quotes)] [issue]**",
                        Timestamp = DateTime.UtcNow
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
                        Timestamp = DateTime.UtcNow
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
                    Description = $"*An exception has occured.*" +
                               "\n**c!reportissue [issue category] [name of track (in quotes)] [issue]**",
                    Timestamp = DateTime.UtcNow
                };
                await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);

                Console.WriteLine(ex.ToString());
            }
        }

        [Command("clearissues")]
        [RequireRoles(RoleCheckMode.Any, "Pack & Bot Dev", "Admin")]
        public async Task ClearTrackIssues(CommandContext ctx, [RemainingText] string track = "")
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
                                  "\n**c!clearissues [track]**",
                    Timestamp = DateTime.UtcNow
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
                            if (Regex.Replace(trackList[i].Name.ToLowerInvariant(), "_", " ") == track.ToLowerInvariant())
                            {
                                foreach (var t in response.Values)
                                {
                                    if (t[0].ToString().ToLowerInvariant() == track.ToLowerInvariant())
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
                        Timestamp = DateTime.UtcNow
                    };
                    await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    embed = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = "__**Error:**__",
                        Description = $"*An exception has occured.*" +
                                  "\n**c!clearissues [track]**",
                        Timestamp = DateTime.UtcNow
                    };
                    await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);

                    Console.WriteLine(ex.ToString());
                }
            }
        }

        [Command("replaceissues")]
        [RequireRoles(RoleCheckMode.Any, "Pack & Bot Dev", "Admin")]
        public async Task ReplaceTrackIssues(CommandContext ctx, string track = "", string newTrack = "", string author = "", string version = "", string slot = "", string laps = "")
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
                                  "\n**c!replaceissues [track] [new track] [author] [version] [slot] [laps]**",
                    Timestamp = DateTime.UtcNow
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

                var request = service.Spreadsheets.Values.Get("1xwhKoyypCWq5tCRTI69ijJoDiaoAVsvYAxz-q4UBNqM", "'CTGP Track Issues'!A2:G219");
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
                            if (Regex.Replace(trackList[i].Name.ToLowerInvariant(), "_", " ") == track.ToLowerInvariant())
                            {
                                foreach (var t in response.Values)
                                {
                                    if (t[0].ToString().ToLowerInvariant() == track.ToLowerInvariant())
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
                            Timestamp = DateTime.UtcNow
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

                        var updateRequest = service.Spreadsheets.Values.Update(response, "1xwhKoyypCWq5tCRTI69ijJoDiaoAVsvYAxz-q4UBNqM", "'CTGP Track Issues'!A2:G219");
                        updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.RAW;
                        var update = await updateRequest.ExecuteAsync();

                        embed = new DiscordEmbedBuilder
                        {
                            Color = new DiscordColor("#FF0000"),
                            Title = $"__**{newTrack} has now replaced {track}:**__",
                            Description = description,
                            Timestamp = DateTime.UtcNow
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
                        Description = $"*An exception has occured.*" +
                                  "\n**c!replaceissues [track] [new track] [author] [version] [slot] [speed/laps]**",
                        Timestamp = DateTime.UtcNow
                    };
                    await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);

                    Console.WriteLine(ex.ToString());
                }
            }
        }

        [Command("bkt")]
        public async Task GetBestTimes(CommandContext ctx, string trackType = "rts", [RemainingText] string track = "")
        {
            await ctx.TriggerTypingAsync();

            if (trackType == "rt")
                trackType = "rts";
            if (trackType == "ct")
                trackType = "cts";

            string json = "";
            string description = "";

            var embed = new DiscordEmbedBuilder { };

            try
            {
                json = File.ReadAllText($"{trackType}.json");
                List<Track> trackList = JsonConvert.DeserializeObject<List<Track>>(json);

                int j = 0;
                List<Track> trackDisplay = new List<Track>();

                for (int i = 0; i < trackList.Count; i++)
                {
                    if (trackList[i].Name.ToLowerInvariant().Contains(track.ToLowerInvariant()))
                    {
                        trackDisplay.Add(trackList[i]);
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
                        "\n**c!bkt [rts/cts/rts200/cts200] [name of track]**",
                        Timestamp = DateTime.UtcNow
                    };
                    await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
                }
                else if (j == 1)
                {
                    embed = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = $"__**Best Time on {track}:**__",
                        Description = $"{trackDisplay[0].Name} - *{trackDisplay[0].BestTime}*",
                        Timestamp = DateTime.UtcNow
                    };
                    await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
                }
                else
                {
                    if (trackDisplay[0].Category == 16)
                    {
                        for (int i = 0; i < trackDisplay.Count; i++)
                        {
                            if (trackDisplay[i].Category == 16)
                            {
                                description += $"{trackDisplay[i].Name} (Shortcut) - *{trackDisplay[i].BestTime}*\n";
                            }
                            if (trackDisplay[i].Category == 1 || trackDisplay[i].Category == 5)
                            {
                                description += $"{trackDisplay[i].Name} (Glitch) - *{trackDisplay[i].BestTime}*\n";
                            }
                            if (trackDisplay[i].Category == 2 || trackDisplay[i].Category == 6)
                            {
                                description += $"{trackDisplay[i].Name} (No Shortcut) - *{trackDisplay[i].BestTime}*\n";
                            }
                        }
                    }
                    else
                    {
                        for (int i = 0; i < trackDisplay.Count; i++)
                        {
                            if (trackDisplay[i].Category == 0 || trackDisplay[i].Category == 4)
                            {
                                description += $"{trackDisplay[i].Name} (No Shortcut) - *{trackDisplay[i].BestTime}*\n";
                            }
                            if (trackDisplay[i].Category == 1 || trackDisplay[i].Category == 5)
                            {
                                description += $"{trackDisplay[i].Name} (Glitch) - *{trackDisplay[i].BestTime}*\n";
                            }
                            if (trackDisplay[i].Category == 2 || trackDisplay[i].Category == 6)
                            {
                                description += $"{trackDisplay[i].Name} (Shortcut) - *{trackDisplay[i].BestTime}*\n";
                            }
                        }
                    }
                    if (track.Length < 3)
                    {

                        embed = new DiscordEmbedBuilder
                        {
                            Color = new DiscordColor("#FF0000"),
                            Title = $"__**Error:**__",
                            Description = "*Please input a track name to filter with (min 3 chars).*" +
                               "\n**c!bkt [rts/cts/rts200/cts200] [name of track]**",
                            Timestamp = DateTime.UtcNow
                        };
                    }
                    else
                    {
                        embed = new DiscordEmbedBuilder
                        {
                            Color = new DiscordColor("#FF0000"),
                            Title = $"__**Best Times on {track}:**__",
                            Description = description,
                            Timestamp = DateTime.UtcNow
                        };
                    }
                    await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);

                }
            }
            catch (Exception ex)
            {
                if (trackType != "rts" && trackType != "cts" && trackType != "rts200" && trackType != "cts200")
                {
                    embed = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = "__**Error:**__",
                        Description = "*Track type is not valid.*" +
                        "\n**c!bkt [rts/cts/rts200/cts200] [name of track]**",
                        Timestamp = DateTime.UtcNow
                    };
                }
                else
                {
                    embed = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = $"__**Error:**__",
                        Description = "*An exception has occured.*" +
                        "\n**c!bkt [rts/cts/rts200/cts200] [name of track]**",
                        Timestamp = DateTime.UtcNow
                    };
                }
                await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);

                Console.WriteLine(ex.ToString());
            }
        }

        [Command("wwpop")]
        public async Task WWPopularityRequest(CommandContext ctx, string trackType = "rts", int display = 0)
        {
            await ctx.TriggerTypingAsync();

            if (trackType == "rt")
                trackType = "rts";
            if (trackType == "ct")
                trackType = "cts";

            var embed = new DiscordEmbedBuilder { };

            string json = "";

            if (trackType == "rts")
            {
                if (display < 1)
                {
                    display = 1;
                }
                else if (display > 12)
                {
                    display = 12;
                }
            }
            else if (trackType == "cts")
            {
                if (display < 1)
                {
                    display = 1;
                }
                else if (display > 198)
                {
                    display = 198;
                }
            }

            try
            {
                json = File.ReadAllText($"{trackType}.json");
                List<Track> trackList = JsonConvert.DeserializeObject<List<Track>>(json);
                for (int i = 0; i < trackList.Count; i++)
                {
                    if (trackList[i].Category % 16 != 0)
                    {
                        trackList.RemoveAt(i);
                        i--;
                    }
                }
                trackList = trackList.OrderBy(a => a.WiimmfiScore).ToList();
                trackList.Reverse();

                string description = "";

                for (int i = display - 1; i < display + 20; i++)
                {
                    description = description + $"**{i + 1})** {trackList[i].Name} *({trackList[i].WiimmfiScore})*\n";
                }

                embed = new DiscordEmbedBuilder
                {
                    Color = new DiscordColor("#FF0000"),
                    Title = $"__**Displaying {display} - {display + 20}:**__",
                    Description = description,
                    Timestamp = DateTime.UtcNow
                };

                await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                if (trackType != "rts" && trackType != "cts")
                {
                    embed = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = "__**Error:**__",
                        Description = "*Track type is not valid.*" +
                        "\n**c!wwpop [rts/cts] [range(1-32/218)]**",
                        Timestamp = DateTime.UtcNow
                    };
                }
                else
                {
                    embed = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = $"__**Error:**__",
                        Description = "*An exception has occured.*" +
                        "\n**c!wwpop [rts/cts] [range(1-32/218)]**",
                        Timestamp = DateTime.UtcNow
                    };
                }
                await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);

                Console.WriteLine(ex.ToString());
            }
        }

        [Command("ttpop")]
        public async Task TTPopularityRequest(CommandContext ctx, string trackType = "rts", int display = 0)
        {
            await ctx.TriggerTypingAsync();

            if (trackType == "rt")
                trackType = "rts";
            if (trackType == "ct")
                trackType = "cts";

            var embed = new DiscordEmbedBuilder { };

            string json = "";

            if (trackType == "rts")
            {
                if (display < 1)
                {
                    display = 1;
                }
                else if (display > 12)
                {
                    display = 12;
                }
            }
            else if (trackType == "cts")
            {
                if (display < 1)
                {
                    display = 1;
                }
                else if (display > 198)
                {
                    display = 198;
                }
            }

            try
            {
                json = File.ReadAllText($"{trackType}.json");
                List<Track> trackList = JsonConvert.DeserializeObject<List<Track>>(json);
                for (int i = 0; i < trackList.Count; i++)
                {
                    if (trackList[i].Category % 16 != 0)
                    {
                        trackList.RemoveAt(i);
                        i--;
                    }
                }
                trackList = trackList.OrderBy(a => a.TimeTrialScore).ToList();
                trackList.Reverse();

                string description = "";

                for (int i = display - 1; i < display + 20; i++)
                {
                    description = description + $"**{i + 1})** {trackList[i].Name} *({trackList[i].TimeTrialScore})*\n";
                }

                embed = new DiscordEmbedBuilder
                {
                    Color = new DiscordColor("#FF0000"),
                    Title = $"__**Displaying {display} - {display + 20}:**__",
                    Description = description,
                    Timestamp = DateTime.UtcNow
                };

                await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                if (trackType != "rts" && trackType != "cts")
                {
                    embed = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = "__**Error:**__",
                        Description = "*Track type is not valid.*" +
                        "\n**c!wwpop [rts/cts] [range(1-32/218)]**",
                        Timestamp = DateTime.UtcNow
                    };
                }
                else
                {
                    embed = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = $"__**Error:**__",
                        Description = "*An exception has occured.*" +
                        "\n**c!wwpop [rts/cts] [range(1-32/218)]**",
                        Timestamp = DateTime.UtcNow
                    };
                }
                await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);

                Console.WriteLine(ex.ToString());
            }
        }

        [Command("wwpopsearch")]
        public async Task WWPopularitySearch(CommandContext ctx, string trackType = "rts", [RemainingText] string track = "")
        {
            await ctx.TriggerTypingAsync();

            if (trackType == "rt")
                trackType = "rts";
            if (trackType == "ct")
                trackType = "cts";

            var embed = new DiscordEmbedBuilder { };

            string json = "";

            try
            {
                json = File.ReadAllText($"{trackType}.json");
                List<Track> trackList = JsonConvert.DeserializeObject<List<Track>>(json);
                for (int i = 0; i < trackList.Count; i++)
                {
                    if (trackList[i].Category % 16 != 0)
                    {
                        trackList.RemoveAt(i);
                        i--;
                    }
                }
                trackList = trackList.OrderBy(a => a.WiimmfiScore).ToList();
                trackList.Reverse();

                string description = "";

                for (int i = 0; i < trackList.Count; i++)
                {
                    if (trackList[i].Name.ToLowerInvariant().Contains(track.ToLowerInvariant()))
                    {
                        description = description + $"**{i + 1})** {trackList[i].Name} *({trackList[i].WiimmfiScore})*\n";
                    }
                }

                if (description == "")
                {
                    embed = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = "__**Error:**__",
                        Description = $"*{track} could not be found.*" +
                        "\n**c!wwpopsearch [rts/cts] [name of track]**",
                        Timestamp = DateTime.UtcNow
                    };
                }

                else if (track.Length < 3)
                {

                    embed = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = $"__**Error:**__",
                        Description = "*Please input a track name to filter with (min 3 chars).*" +
                           "\n**c!wwpopsearch [rts/cts] [name of track]**",
                        Timestamp = DateTime.UtcNow
                    };
                }

                else
                {
                    embed = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = $"__**Displaying tracks containing *{track}*:**__",
                        Description = description,
                        Timestamp = DateTime.UtcNow
                    };
                }

                await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                if (trackType != "rts" && trackType != "cts")
                {
                    embed = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = "__**Error:**__",
                        Description = "*Track type is not valid.*" +
                        "\n**c!wwpopsearch [rts/cts] [name of track]**",
                        Timestamp = DateTime.UtcNow
                    };
                }
                else if (track == "")
                {
                    embed = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = "__**Error:**__",
                        Description = "*Track argument is missing.*" +
                        "\n**c!wwpopsearch [rts/cts] [name of track]**",
                        Timestamp = DateTime.UtcNow
                    };
                }
                else
                {
                    embed = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = $"__**Error:**__",
                        Description = "*An exception has occured.*" +
                        "\n**c!wwpopsearch [rts/cts] [name of track]**",
                        Timestamp = DateTime.UtcNow
                    };
                }
                await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);

                Console.WriteLine(ex.ToString());
            }
        }

        [Command("ttpopsearch")]
        public async Task TTPopularitySearch(CommandContext ctx, string trackType = "rts", [RemainingText] string track = "")
        {
            await ctx.TriggerTypingAsync();

            if (trackType == "rt")
                trackType = "rts";
            if (trackType == "ct")
                trackType = "cts";

            var embed = new DiscordEmbedBuilder { };

            string json = "";

            try
            {
                json = File.ReadAllText($"{trackType}.json");
                List<Track> trackList = JsonConvert.DeserializeObject<List<Track>>(json);
                for (int i = 0; i < trackList.Count; i++)
                {
                    if (trackList[i].Category % 16 != 0)
                    {
                        trackList.RemoveAt(i);
                        i--;
                    }
                }
                trackList = trackList.OrderBy(a => a.TimeTrialScore).ToList();
                trackList.Reverse();

                string description = "";

                for (int i = 0; i < trackList.Count; i++)
                {
                    if (trackList[i].Name.ToLowerInvariant().Contains(track.ToLowerInvariant()))
                    {
                        description = description + $"**{i + 1})** {trackList[i].Name} *({trackList[i].TimeTrialScore})*\n";
                    }
                }

                if (description == "")
                {
                    embed = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = "__**Error:**__",
                        Description = $"*{track} could not be found.*" +
                        "\n**c!ttpopsearch [rts/cts] [name of track]**",
                        Timestamp = DateTime.UtcNow
                    };
                }

                else if (track.Length < 3)
                {

                    embed = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = $"__**Error:**__",
                        Description = "*Please input a track name to filter with (min 3 chars).*" +
                           "\n**c!ttpopsearch [rts/cts] [name of track]**",
                        Timestamp = DateTime.UtcNow
                    };
                }

                else
                {
                    embed = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = $"__**Displaying tracks containing *{track}*:**__",
                        Description = description,
                        Timestamp = DateTime.UtcNow
                    };
                }

                await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                if (trackType != "rts" && trackType != "cts")
                {
                    embed = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = "__**Error:**__",
                        Description = "*Track type is not valid.*" +
                        "\n**c!ttpopsearch [rts/cts] [name of track]**",
                        Timestamp = DateTime.UtcNow
                    };
                }
                else if (track == "")
                {
                    embed = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = "__**Error:**__",
                        Description = "*Track argument is missing.*" +
                        "\n**c!ttpopsearch [rts/cts] [name of track]**",
                        Timestamp = DateTime.UtcNow
                    };
                }
                else
                {
                    embed = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = $"__**Error:**__",
                        Description = "*An exception has occured.*" +
                        "\n**c!ttpopsearch [rts/cts] [name of track]**",
                        Timestamp = DateTime.UtcNow
                    };
                }
                await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);

                Console.WriteLine(ex.ToString());
            }
        }
    }
}
