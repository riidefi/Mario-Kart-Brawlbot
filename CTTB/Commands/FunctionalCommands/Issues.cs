using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Newtonsoft.Json;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace CTTB.Commands
{
    public class Issues : BaseCommandModule
    {
        public Util Utility = new Util();

        [Command("issues")]
        public async Task GetTrackIssues(CommandContext ctx, [RemainingText] string track = "")
        {
            await ctx.TriggerTypingAsync();

            var json = string.Empty;
            var description = string.Empty;

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
                            if (Utility.CompareIncompleteStrings(t, response.Values[i][0].ToString()))
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
                                var embed = new DiscordEmbedBuilder
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
                            if (Utility.CompareIncompleteStrings(trackList[i].Name, track) || Utility.CompareStringAbbreviation(track, trackList[i].Name) || Utility.CompareStringsLevenshteinDistance(track, trackList[i].Name))
                            {
                                foreach (var t in response.Values)
                                {
                                    if (Utility.CompareIncompleteStrings(t[0].ToString(), track) || Utility.CompareStringAbbreviation(track, t[0].ToString()) || Utility.CompareStringsLevenshteinDistance(track, t[0].ToString()))
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
                        var embed = new DiscordEmbedBuilder
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
                        var embed = new DiscordEmbedBuilder
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
                var embed = new DiscordEmbedBuilder
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

                if (Utility.CompareStrings(issueType, "major"))
                {
                    k = 5;
                }

                else if (Utility.CompareStrings(issueType, "minor"))
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
                                if (Utility.CompareStrings(trackList[i].Name, track))
                                {
                                    foreach (var t in response.Values)
                                    {
                                        if (Utility.CompareStrings(t[0].ToString(), track))
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
                        var embed = new DiscordEmbedBuilder
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
                        var embed = new DiscordEmbedBuilder
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

                        var embed = new DiscordEmbedBuilder
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
                        var embed = new DiscordEmbedBuilder
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
                    var embed = new DiscordEmbedBuilder
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
                                if (Utility.CompareIncompleteStrings(t, response.Values[i][2].ToString()))
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
                                    var embed = new DiscordEmbedBuilder
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
                                if (Utility.CompareIncompleteStrings(t[2].ToString(), track) || Utility.CompareStringAbbreviation(track, t[2].ToString()) || Utility.CompareStringsLevenshteinDistance(track, t[2].ToString()))
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
                        var embed = new DiscordEmbedBuilder
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
                        var embed = new DiscordEmbedBuilder
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
                var embed = new DiscordEmbedBuilder
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

        [Command("reportsubissue")]
        [RequireRoles(RoleCheckMode.Any, "Admin")]
        public async Task SubmissionReportIssue(CommandContext ctx, string track = "", [RemainingText] string issue = "")
        {
            if (ctx.Guild.Id == 180306609233330176)
            {
                try
                {
                    await ctx.TriggerTypingAsync();
                    if (track == "")
                    {
                        var embed = new DiscordEmbedBuilder
                        {
                            Color = new DiscordColor("#FF0000"),
                            Title = "__**Error:**__",
                            Description = $"*Track was not imputted.*" +
                                              "\n**c!subreportissue \"track\" -Issue**",
                            Url = "https://docs.google.com/spreadsheets/d/1xwhKoyypCWq5tCRTI69ijJoDiaoAVsvYAxz-q4UBNqM/edit#gid=1971102004",
                            Footer = new DiscordEmbedBuilder.EmbedFooter
                            {
                                Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                            }
                        };
                        await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
                    }

                    else if (issue == "")
                    {
                        var embed = new DiscordEmbedBuilder
                        {
                            Color = new DiscordColor("#FF0000"),
                            Title = "__**Error:**__",
                            Description = $"*Issue was not inputted.*" +
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
                        int j = 0;

                        if (issue != "")
                        {
                            foreach (var t in response.Values)
                            {
                                if (t[2].ToString() != "Track Name" && t[2].ToString() != "delimiter" && t[2].ToString() != "")
                                {
                                    if (Utility.CompareStrings(t[2].ToString(), track))
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
                                var embed = new DiscordEmbedBuilder
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
                                var embed = new DiscordEmbedBuilder
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

                                var embed = new DiscordEmbedBuilder
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
                }
                catch (Exception ex)
                {
                    var embed = new DiscordEmbedBuilder
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

                string json = string.Empty;
                if (track == "")
                {
                    var embed = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = "__**Error:**__",
                        Description = $"*No track was inputted.*" +
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
                                if (Utility.CompareStrings(trackList[i].Name, track))
                                {
                                    foreach (var t in response.Values)
                                    {
                                        if (Utility.CompareStrings(t[0].ToString(), track))
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

                        var embed = new DiscordEmbedBuilder
                        {
                            Color = new DiscordColor("#FF0000"),
                            Title = "__**Success:**__",
                            Description = $"*{track} issues have been cleared.*",
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
                        var embed = new DiscordEmbedBuilder
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

        [Command("clearsubissues")]
        [RequireRoles(RoleCheckMode.Any, "Admin")]
        public async Task ClearSubmittedTrackIssues(CommandContext ctx, [RemainingText] string track = "")
        {
            if (ctx.Guild.Id == 180306609233330176)
            {
                await ctx.TriggerTypingAsync();

                string json = string.Empty;
                if (track == "")
                {
                    var embed = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = "__**Error:**__",
                        Description = $"*No track was inputted.*" +
                                      "\n**c!clearsubissues track**",
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

                        foreach (var t in response.Values)
                        {
                            if (Utility.CompareStrings(t[2].ToString(), track))
                            {
                                t[10] = "";
                                j++;
                                break;
                            }
                        }

                        if (j == 0)
                        {
                            var embed = new DiscordEmbedBuilder
                            {
                                Color = new DiscordColor("#FF0000"),
                                Title = "__**Error:**__",
                                Description = $"*{track} could not be found.*" +
                                         "\n**c!clearsubissues track**",
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

                            var embed = new DiscordEmbedBuilder
                            {
                                Color = new DiscordColor("#FF0000"),
                                Title = "__**Success:**__",
                                Description = $"*{track} issues have been cleared.*",
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
                        var embed = new DiscordEmbedBuilder
                        {
                            Color = new DiscordColor("#FF0000"),
                            Title = "__**Error:**__",
                            Description = $"*{ex.Message}*" +
                                      "\n**c!clearsubissues track**",
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

                string json = string.Empty;
                string description = string.Empty;

                if (track == "" || newTrack == "" || author == "" || version == "" || slot == "" || laps == "")
                {
                    var embed = new DiscordEmbedBuilder
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
                                if (Utility.CompareStrings(trackList[i].Name, track))
                                {
                                    foreach (var t in response.Values)
                                    {
                                        if (Utility.CompareStrings(t[0].ToString(), track))
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
                            var embed = new DiscordEmbedBuilder
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

                            var embed = new DiscordEmbedBuilder
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
                        var embed = new DiscordEmbedBuilder
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
    }
}