using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using HtmlAgilityPack;
using IronPython.Hosting;
using Newtonsoft.Json;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System.Text;
using System.Linq.Expressions;
using System.Globalization;
using IronPython.Compiler.Ast;
using System.Threading;

namespace CTTB.Commands
{
    public class FunctionalCommands : BaseCommandModule
    {
        static string[] Scopes = { SheetsService.Scope.Spreadsheets };

        [Command("update")]
        [RequireRoles(RoleCheckMode.Any, "Pack & Bot Dev", "Admin")]
        public async Task Update(CommandContext ctx)
        {
            string rtttUrl = "http://tt.chadsoft.co.uk/original-track-leaderboards.json";
            string ctttUrl = "http://tt.chadsoft.co.uk/ctgp-leaderboards.json";
            string rttt200Url = "http://tt.chadsoft.co.uk/original-track-leaderboards-200cc.json";
            string cttt200Url = "http://tt.chadsoft.co.uk/ctgp-leaderboards-200cc.json";
            string ctwwUrl = "https://wiimmfi.de/stats/track/mv/ctgp?p=std,c1,0";
            string wwUrl = "https://wiimmfi.de/stats/track/mv/ww?p=std,c1,0";

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

            await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
        }

        [Command("staff")]
        //[RequireRoles(RoleCheckMode.Any, "Pack & Bot Dev", "Admin")]
        public async Task GetStaffGhosts(CommandContext ctx, [RemainingText] string track = "")
        {
            var json = string.Empty;
            var description = string.Empty;
            var embed = new DiscordEmbedBuilder { };

            using (var fs = File.OpenRead("config.json"))
            using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                json = await sr.ReadToEndAsync().ConfigureAwait(false);

            var configJson = JsonConvert.DeserializeObject<ConfigJson>(json);

            var service = new SheetsService(new BaseClientService.Initializer
            {
                ApplicationName = "Custom Track Testing Bot",
                ApiKey = configJson.ApiKey,
            });

            var request = service.Spreadsheets.Values.Get("1xwhKoyypCWq5tCRTI69ijJoDiaoAVsvYAxz-q4UBNqM", "'Staff Ghosts'!A1:C251");
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
            catch
            {
                embed = new DiscordEmbedBuilder
                {
                    Color = new DiscordColor("#FF0000"),
                    Title = "__**Error:**__",
                    Description = $"*Unknown error.*" +
                           "\n**c!staff [name of track]**",
                    Timestamp = DateTime.UtcNow
                };
                await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
            }
        }

        [Command("getissues")]
        //[RequireRoles(RoleCheckMode.Any, "Pack & Bot Dev", "Admin")]
        public async Task GetTrackIssues(CommandContext ctx, [RemainingText] string track = "")
        {
            var json = string.Empty;
            var description = string.Empty;
            var embed = new DiscordEmbedBuilder { };

            using (var fs = File.OpenRead("config.json"))
            using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                json = await sr.ReadToEndAsync().ConfigureAwait(false);

            var configJson = JsonConvert.DeserializeObject<ConfigJson>(json);

            var service = new SheetsService(new BaseClientService.Initializer
            {
                ApplicationName = "Custom Track Testing Bot",
                ApiKey = configJson.ApiKey,
            });

            var request = service.Spreadsheets.Values.Get("1xwhKoyypCWq5tCRTI69ijJoDiaoAVsvYAxz-q4UBNqM", "'CTGP Track Issues'!A1:G218");
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
            catch
            {
                embed = new DiscordEmbedBuilder
                {
                    Color = new DiscordColor("#FF0000"),
                    Title = "__**Error:**__",
                    Description = $"*Unknown error.*" +
                           "\n**c!getissues [name of track]**",
                    Timestamp = DateTime.UtcNow
                };
                await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
            }
        }

        [Command("reportissue")]
        [RequireRoles(RoleCheckMode.Any, "Pack & Bot Dev", "Admin")]
        public async Task ReportIssue(CommandContext ctx, string issueType = "", string track = "", [RemainingText] string issue = "")
        {
            var embed = new DiscordEmbedBuilder { };
            string json = string.Empty;
            string maj = string.Empty;
            string min = string.Empty;

            using (var fs = File.OpenRead("config.json"))
            using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                json = await sr.ReadToEndAsync().ConfigureAwait(false);

            var configJson = JsonConvert.DeserializeObject<ConfigJson>(json);

            var service = new SheetsService(new BaseClientService.Initializer
            {
                ApplicationName = "Custom Track Testing Bot",
                ApiKey = configJson.ApiKey,
            });

            var request = service.Spreadsheets.Values.Get("1xwhKoyypCWq5tCRTI69ijJoDiaoAVsvYAxz-q4UBNqM", "'CTGP Track Issues'!A1:G218");
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

                if (k != 0 && issue != "")
                {
                    UserCredential credential;

                    using (var stream =
                    new FileStream("client_secret.json", FileMode.Open, FileAccess.Read))
                    {
                        string credPath = System.Environment.GetFolderPath(
                            System.Environment.SpecialFolder.Personal);


                        credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                            GoogleClientSecrets.Load(stream).Secrets,
                            Scopes,
                            "user",
                            CancellationToken.None,
                            new FileDataStore(credPath, true)).Result;
                    }

                    service = new SheetsService(new BaseClientService.Initializer()
                    {
                        HttpClientInitializer = credential,
                        ApplicationName = "Custom Track Testing Bot",
                    });

                    var updateRequest = service.Spreadsheets.Values.Update(response, "1xwhKoyypCWq5tCRTI69ijJoDiaoAVsvYAxz-q4UBNqM", "'CTGP Track Issues'!A1:G218");
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
            catch
            {
                embed = new DiscordEmbedBuilder
                {
                    Color = new DiscordColor("#FF0000"),
                    Title = "__**Error:**__",
                    Description = $"*Unknown error.*" +
                               "\n**c!reportissue [issue category] [name of track (in quotes)] [issue]**",
                    Timestamp = DateTime.UtcNow
                };
                await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
            }
        }

        [Command("clearissues")]
        [RequireRoles(RoleCheckMode.Any, "Pack & Bot Dev", "Admin")]
        public async Task ClearTrackIssues(CommandContext ctx, [RemainingText] string track = "")
        {
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
                using (var fs = File.OpenRead("config.json"))
                using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                    json = await sr.ReadToEndAsync().ConfigureAwait(false);

                var configJson = JsonConvert.DeserializeObject<ConfigJson>(json);

                var service = new SheetsService(new BaseClientService.Initializer
                {
                    ApplicationName = "Custom Track Testing Bot",
                    ApiKey = configJson.ApiKey,
                });

                var request = service.Spreadsheets.Values.Get("1xwhKoyypCWq5tCRTI69ijJoDiaoAVsvYAxz-q4UBNqM", "'CTGP Track Issues'!A1:G218");
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

                    UserCredential credential;

                    using (var stream =
                    new FileStream("client_secret.json", FileMode.Open, FileAccess.Read))
                    {
                        string credPath = System.Environment.GetFolderPath(
                            System.Environment.SpecialFolder.Personal);


                        credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                            GoogleClientSecrets.Load(stream).Secrets,
                            Scopes,
                            "user",
                            CancellationToken.None,
                            new FileDataStore(credPath, true)).Result;
                    }

                    service = new SheetsService(new BaseClientService.Initializer()
                    {
                        HttpClientInitializer = credential,
                        ApplicationName = "Custom Track Testing Bot",
                    });

                    var updateRequest = service.Spreadsheets.Values.Update(response, "1xwhKoyypCWq5tCRTI69ijJoDiaoAVsvYAxz-q4UBNqM", "'CTGP Track Issues'!A1:G218");
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
                catch
                {
                    embed = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = "__**Error:**__",
                        Description = $"*Unknown error.*" +
                                  "\n**c!clearissues [track]**",
                        Timestamp = DateTime.UtcNow
                    };
                    await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
                }
            }
        }

        [Command("replaceissues")]
        [RequireRoles(RoleCheckMode.Any, "Pack & Bot Dev", "Admin")]
        public async Task ReplaceTrackIssues(CommandContext ctx, string track = "", string newTrack = "", string author = "", string version = "", string slot = "", string laps = "")
        {
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
                using (var fs = File.OpenRead("config.json"))
                using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                    json = await sr.ReadToEndAsync().ConfigureAwait(false);

                var configJson = JsonConvert.DeserializeObject<ConfigJson>(json);

                var service = new SheetsService(new BaseClientService.Initializer
                {
                    ApplicationName = "Custom Track Testing Bot",
                    ApiKey = configJson.ApiKey,
                });

                var request = service.Spreadsheets.Values.Get("1xwhKoyypCWq5tCRTI69ijJoDiaoAVsvYAxz-q4UBNqM", "'CTGP Track Issues'!A1:G218");
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

                    var orderedResponse = response.Values.OrderBy(x => x[0].ToString()).ToList();
                    for (int i = 1; i < response.Values.Count; i++)
                    {
                        response.Values[i] = orderedResponse[i];
                    }

                    UserCredential credential;

                    using (var stream =
                    new FileStream("client_secret.json", FileMode.Open, FileAccess.Read))
                    {
                        string credPath = System.Environment.GetFolderPath(
                            System.Environment.SpecialFolder.Personal);


                        credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                            GoogleClientSecrets.Load(stream).Secrets,
                            Scopes,
                            "user",
                            CancellationToken.None,
                            new FileDataStore(credPath, true)).Result;
                    }

                    service = new SheetsService(new BaseClientService.Initializer()
                    {
                        HttpClientInitializer = credential,
                        ApplicationName = "Custom Track Testing Bot",
                    });

                    var updateRequest = service.Spreadsheets.Values.Update(response, "1xwhKoyypCWq5tCRTI69ijJoDiaoAVsvYAxz-q4UBNqM", "'CTGP Track Issues'!A1:G218");
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
                catch
                {
                    embed = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = "__**Error:**__",
                        Description = $"*Unknown error.*" +
                                  "\n**c!replaceissues [track] [new track] [author] [version] [slot] [speed/laps]**",
                        Timestamp = DateTime.UtcNow
                    };
                    await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
                }
            }
        }

        [Command("bkt")]
        //[RequireRoles(RoleCheckMode.Any, "Pack & Bot Dev", "Admin")]
        public async Task GetBestTimes(CommandContext ctx, string trackType = "rts", [RemainingText] string track = "")
        {
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
                        "\n**c!besttime [rts/cts/rts200/cts200] [name of track]**",
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
                               "\n**c!besttime [rts/cts/rts200/cts200] [name of track]**",
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
            catch
            {
                if (trackType != "rts" && trackType != "cts" && trackType != "rts200" && trackType != "cts200")
                {
                    embed = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = "__**Error:**__",
                        Description = "*Track type is not valid.*" +
                        "\n**c!besttime [rts/cts/rts200/cts200] [name of track]**",
                        Timestamp = DateTime.UtcNow
                    };
                }
                else
                {
                    embed = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = $"__**Error:**__",
                        Description = "*Unknown Error.*" +
                        "\n**c!besttime [rts/cts/rts200/cts200] [name of track]**",
                        Timestamp = DateTime.UtcNow
                    };
                }
                await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
            }
        }

        [Command("wwpop")]
        //[RequireRoles(RoleCheckMode.Any, "Pack & Bot Dev", "Admin")]
        public async Task WWPopularityRequest(CommandContext ctx, string trackType = "rts", int display = 0)
        {
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
            catch
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
                        Description = "*Unknown Error.*" +
                        "\n**c!wwpop [rts/cts] [range(1-32/218)]**",
                        Timestamp = DateTime.UtcNow
                    };
                }
                await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
            }
        }

        [Command("ttpop")]
        //[RequireRoles(RoleCheckMode.Any, "Pack & Bot Dev", "Admin")]
        public async Task TTPopularityRequest(CommandContext ctx, string trackType = "rts", int display = 0)
        {
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
            catch
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
                        Description = "*Unknown Error.*" +
                        "\n**c!wwpop [rts/cts] [range(1-32/218)]**",
                        Timestamp = DateTime.UtcNow
                    };
                }
                await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
            }
        }

        [Command("wwpopsearch")]
        //[RequireRoles(RoleCheckMode.Any, "Pack & Bot Dev", "Admin")]
        public async Task WWPopularitySearch(CommandContext ctx, string trackType = "rts", [RemainingText] string track = "")
        {
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
            catch
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
                        Description = "*Unknown Error.*" +
                        "\n**c!wwpopsearch [rts/cts] [name of track]**",
                        Timestamp = DateTime.UtcNow
                    };
                }
                await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
            }
        }

        [Command("ttpopsearch")]
        //[RequireRoles(RoleCheckMode.Any, "Pack & Bot Dev", "Admin")]
        public async Task TTPopularitySearch(CommandContext ctx, string trackType = "rts", [RemainingText] string track = "")
        {
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
            catch
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
                        Description = "*Unknown Error.*" +
                        "\n**c!ttpopsearch [rts/cts] [name of track]**",
                        Timestamp = DateTime.UtcNow
                    };
                }
                await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
            }
        }
    }
}
