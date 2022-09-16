﻿using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace MKBB.Commands
{

    public class Homework : ApplicationCommandModule
    {
        [SlashCommand("addhw", "Adds homework to the council sheet.")]
        [SlashRequireUserPermissions(Permissions.Administrator)]
        public async Task AddHomework(InteractionContext ctx,
            [Option("track-name", "The name of the track being added.")] string track,
            [Option("authors", "The author or authors of the track being added.")] string author,
            [Option("version", "The version number of the track being added.")] string version,
            [Option("download", "The download link of the track being added.")] string download,
            [Option("slot", "The track slot of the track being added e.g. 'Luigi Circuit - beginner_course'")] string slot,
            [Option("lap-and-speed-modifiers", "The lap and speed modifiers of the track being added, the most common being '1 / 3'.")] string lapSpeed,
            [Option("notes", "Any additional notes for council members.")] string notes = "")
        {
            try
            {
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

                await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder() { IsEphemeral = true });

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

                IList<object> obj = new List<object>
                            {
                                track,
                                $"{dueMonth} {due.Day}, {due.Year}",
                                author,
                                "'" + version,
                                dl,
                                slot,
                                lapSpeed,
                                notes,
                                $"=COUNTIF($M{countResponse.Values.Count + 1}:{countResponse.Values.Count + 1}, \"yes*\")",
                                $"=COUNTIF($M{countResponse.Values.Count + 1}:{countResponse.Values.Count + 1}, \"fixes*\")",
                                $"=COUNTIF($M{countResponse.Values.Count + 1}:{countResponse.Values.Count + 1}, \"neutral*\")",
                                $"=COUNTIF($M{countResponse.Values.Count + 1}:{countResponse.Values.Count + 1}, \"no*\")"
                            };
                IList<IList<object>> values = new List<IList<object>> { obj };

                var appendRequest = service.Spreadsheets.Values.Append(new ValueRange() { Values = values }, "1I9yFsomTcvFT4hp6eN2azsfv6MsIy1897tBFX_gmtss", "'Track Evaluating'");
                appendRequest.InsertDataOption = SpreadsheetsResource.ValuesResource.AppendRequest.InsertDataOptionEnum.INSERTROWS;
                appendRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
                appendRequest.ResponseValueRenderOption = SpreadsheetsResource.ValuesResource.AppendRequest.ResponseValueRenderOptionEnum.FORMULA;
                var appendResponse = await appendRequest.ExecuteAsync();

                notes = notes != "" ? $"*{notes}*" : notes;

                var embed = new DiscordEmbedBuilder
                {
                    Color = new DiscordColor("#FF0000"),
                    Title = $"__**Success:**__",
                    Description = $"*{track} has been added as homework.*",
                    Url = Util.GetCouncilUrl(),
                    Footer = new DiscordEmbedBuilder.EmbedFooter
                    {
                        Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                    }
                };
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));

                DiscordChannel channel = ctx.Channel;

                foreach (var c in ctx.Guild.Channels)
                {
                    if (c.Value.Id == 635313521487511554)
                    {
                        channel = c.Value;
                    }
                }

                await channel.SendMessageAsync($"<@&608386209655554058> {track} has been added as homework. It is due for {dueMonth} {due.Day}, {due.Year}.\n{notes}");
            }
            catch (Exception ex)
            {
                await Util.ThrowError(ctx, ex);

            }
        }

        [SlashCommand("delhw", "Deletes homework from the council sheet.")]
        [SlashRequireUserPermissions(Permissions.Administrator)]
        public async Task DeleteHomework(InteractionContext ctx,
            [Option("track-name", "The name of the track being removed.")] string track)
        {
            try
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

                var request = service.Spreadsheets.Values.Get("1I9yFsomTcvFT4hp6eN2azsfv6MsIy1897tBFX_gmtss", "'Track Evaluating'");
                var response = await request.ExecuteAsync();

                int ix = -1;

                string trackDisplay = string.Empty;

                for (int i = 0; i < response.Values.Count; i++)
                {
                    if (Util.CompareStrings(response.Values[i][0].ToString(), track))
                    {
                        ix = i;
                        trackDisplay = response.Values[i][0].ToString();
                    }
                }
                if (ix < 0)
                {
                    var embed = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = "__**Error:**__",
                        Description = $"*{track} could not be found.*",
                        Url = Util.GetCouncilUrl(),
                        Footer = new DiscordEmbedBuilder.EmbedFooter
                        {
                            Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                        }
                    };
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
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


                    var embed = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = "__**Success:**__",
                        Description = $"*{trackDisplay} has been deleted from homework.*",
                        Url = Util.GetCouncilUrl(),
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

        [SlashCommand("submithw", "For council to submit their votes and feedback")]
        public async Task SubmitHomework(InteractionContext ctx,
            [Choice("Yes", "Yes")]
            [Choice("No", "No")]
            [Choice("Neutral", "Neutral")]
            [Choice("Needs-Fixes", "Fixes")]
            [Option("vote", "The vote for the tally of the track.")] string vote,
            [Option("track-name", "The name of the track you're adding feedback to.")] string track,
            [Option("feedback", "Reasoning to back up your vote and feedback for the author.")] string feedback)
        {
            try
            {
                await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder() { IsEphemeral = true });

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

                string serviceAccountEmail = "brawlbox@custom-track-testing-bot.iam.gserviceaccount.com";

                var certificate = new X509Certificate2(@"key.p12", "notasecret", X509KeyStorageFlags.Exportable);

                ServiceAccountCredential credential = new ServiceAccountCredential(
                   new ServiceAccountCredential.Initializer(serviceAccountEmail).FromCertificate(certificate));

                var service = new SheetsService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "Mario Kart Brawlbot",
                });

                var request = service.Spreadsheets.Values.Get("1I9yFsomTcvFT4hp6eN2azsfv6MsIy1897tBFX_gmtss", "'Track Evaluating'");
                request.ValueRenderOption = SpreadsheetsResource.ValuesResource.GetRequest.ValueRenderOptionEnum.FORMULA;
                var response = await request.ExecuteAsync();

                int ix = -1;

                for (int i = 0; i < response.Values[0].Count; i++)
                {
                    if (Util.CompareStrings(response.Values[0][i].ToString(), member))
                    {
                        ix = i;
                    }
                }

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
                    else if (Util.CompareStringAbbreviation(track, t[0].ToString()) || Util.CompareStringsLevenshteinDistance(track, t[0].ToString()))
                    {
                        t[ix] = vote + "\n" + feedback;
                        j++;
                    }
                }

                SpreadsheetsResource.ValuesResource.UpdateRequest updateRequest;

                if (response.Values[0].Count < 27)
                {
                    updateRequest = service.Spreadsheets.Values.Update(response, "1I9yFsomTcvFT4hp6eN2azsfv6MsIy1897tBFX_gmtss", $"'Track Evaluating'!A1:{Util.strAlpha[response.Values[0].Count - 1]}{response.Values.Count}");
                }
                else
                {
                    updateRequest = service.Spreadsheets.Values.Update(response, "1I9yFsomTcvFT4hp6eN2azsfv6MsIy1897tBFX_gmtss", $"'Track Evaluating'!A1:A{Util.strAlpha[response.Values[0].Count % 26 - 1]}{response.Values.Count}");
                }
                updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;
                var update = await updateRequest.ExecuteAsync();

                if (j == 0)
                {
                    var embed = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = "__**Error:**__",
                        Description = $"*{track} could not be found.*",
                        Url = Util.GetCouncilUrl(),
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
                        Title = "__**Success:**__",
                        Description = $"*Homework for {track} has been submitted successfully.*",
                        Url = Util.GetCouncilUrl(),
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

        [SlashCommand("gethw", "For council and admins to get homework feedback.")]
        public async Task GetSpecificHomework(InteractionContext ctx,
            [Option("track-name", "The name of the track you're requesting feedback of.")] string track,
            [Option("member", "The name of the council member you are requesting the feedback of.")] string member)
        {
            try
            {
                await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder() { IsEphemeral = true });

                string json;
                using (var fs = File.OpenRead("council.json"))
                using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                    json = await sr.ReadToEndAsync().ConfigureAwait(false);
                List<CouncilMember> councilJson = JsonConvert.DeserializeObject<List<CouncilMember>>(json);

                int ix = -1;

                if (member == "")
                {
                    ix = councilJson.FindIndex(x => x.DiscordId == ctx.Member.Id);
                    if (ix != -1)
                    {
                        member = councilJson[ix].SheetName;
                    }
                }
                else
                {
                    ix = councilJson.FindIndex(x => Util.CompareStrings(x.SheetName, member));
                    if (ix == -1)
                    {
                        ix = councilJson.FindIndex(x => Util.CompareIncompleteStrings(x.SheetName, member) || Util.CompareStringsLevenshteinDistance(x.SheetName, member));
                    }
                    if (ix != -1)
                    {
                        member = councilJson[ix].SheetName;
                    }
                }

                string description = string.Empty;

                if (ix < 0)
                {
                    var embed = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = $"__**Error:**__",
                        Description = $"*{member} could not be found on council.*",
                        Url = Util.GetCouncilUrl(),
                        Footer = new DiscordEmbedBuilder.EmbedFooter
                        {
                            Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                        }
                    };
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
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
                        ApplicationName = "Mario Kart Brawlbot",
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

                    for (int i = 12; i < response.Values[0].Count; i++)
                    {
                        if (Util.CompareStrings(member, response.Values[0][i].ToString()))
                        {
                            sheetIx = i;
                        }
                    }

                    int j = 0;
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
                            else if (Util.CompareIncompleteStrings(t[0].ToString(), track) || Util.CompareStrings(t[0].ToString(), track))
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

                    if (sheetIx < 0)
                    {
                        var embed = new DiscordEmbedBuilder
                        {
                            Color = new DiscordColor("#FF0000"),
                            Title = $"__**Error:**__",
                            Description = $"*{member} could not be found on council.*",
                            Url = Util.GetCouncilUrl(),
                            Footer = new DiscordEmbedBuilder.EmbedFooter
                            {
                                Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                            }
                        };
                        await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
                    }

                    else if (l == 0)
                    {
                        var embed = new DiscordEmbedBuilder
                        {
                            Color = new DiscordColor("#FF0000"),
                            Title = $"__**Error:**__",
                            Description = $"*{track} could not be found.*",
                            Url = Util.GetCouncilUrl(),
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
                            Title = $"__**{trackDisplay}**__",
                            Description = description,
                            Url = Util.GetCouncilUrl(),
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

        [SlashCommand("hw", "Get's a list of all the current homework, and information related to it.")]
        public async Task GetHomework(InteractionContext ctx)
        {
            try
            {
                string description = string.Empty;
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
                var embed = new DiscordEmbedBuilder
                {
                    Color = new DiscordColor("#FF0000"),
                    Title = $"__**Council Homework:**__",
                    Description = description,
                    Url = Util.GetCouncilUrl(),
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

        [SlashCommand("addmissedhw", "To increment a council member's missed homework count.")]
        [SlashRequireUserPermissions(Permissions.Administrator)]
        public async Task IncrementMissedHw(InteractionContext ctx,
            [Option("member", "The name of the council member you are incrementing the missed homework count of.")] string member)
        {
            try
            {
                await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder() { IsEphemeral = true });
                string json;
                using (var fs = File.OpenRead("council.json"))
                using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                    json = await sr.ReadToEndAsync().ConfigureAwait(false);
                List<CouncilMember> councilJson = JsonConvert.DeserializeObject<List<CouncilMember>>(json);

                int ix = councilJson.FindIndex(x => Util.CompareStrings(x.SheetName, member));
                councilJson[ix].TimesMissedHw++;
                var embed = new DiscordEmbedBuilder
                {
                    Color = new DiscordColor("#FF0000"),
                    Title = "__**Notice:**__",
                    Description = $"*Missed homework count for {councilJson[ix].SheetName} has been incremented.*",
                    Footer = new DiscordEmbedBuilder.EmbedFooter
                    {
                        Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                    }
                };
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
                string council = JsonConvert.SerializeObject(councilJson);
                File.WriteAllText("council.json", council);
            }
            catch (Exception ex)
            {
                await Util.ThrowError(ctx, ex);
            }
        }

        [SlashCommand("removemissedhw", "To decrement a council member's missed homework count.")]
        [SlashRequireUserPermissions(Permissions.Administrator)]
        public async Task DecrementMissedHw(InteractionContext ctx,
            [Option("member", "The name of the council member you are decrementing the missed homework count of.")] string member)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder() { IsEphemeral = true });
            try
            {
                string json;
                using (var fs = File.OpenRead("council.json"))
                using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                    json = await sr.ReadToEndAsync().ConfigureAwait(false);
                List<CouncilMember> councilJson = JsonConvert.DeserializeObject<List<CouncilMember>>(json);

                int ix = councilJson.FindIndex(x => Util.CompareIncompleteStrings(x.SheetName, member) || Util.CompareStringsLevenshteinDistance(x.SheetName, member));

                councilJson[ix].TimesMissedHw--;
                var embed = new DiscordEmbedBuilder
                {
                    Color = new DiscordColor("#FF0000"),
                    Title = "__**Notice:**__",
                    Description = $"*Missed homework count for {councilJson[ix].SheetName} has been decremented.*",
                    Footer = new DiscordEmbedBuilder.EmbedFooter
                    {
                        Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                    }
                };
                string council = JsonConvert.SerializeObject(councilJson);
                File.WriteAllText("council.json", council);
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
            }
            catch (Exception ex)
            {
                await Util.ThrowError(ctx, ex);
            }
        }

        [SlashCommand("resetmissedhw", "To reset a council member's, or all of council's missed homework count.")]
        [SlashRequireUserPermissions(Permissions.Administrator)]
        public async Task ResetMissedHw(InteractionContext ctx,
            [Option("member", "The name of the council member you are resetting the missed homework count of.")] string member)
        {
            try
            {
                await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder() { IsEphemeral = true });
                string json;
                using (var fs = File.OpenRead("council.json"))
                using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                    json = await sr.ReadToEndAsync().ConfigureAwait(false);
                List<CouncilMember> councilJson = JsonConvert.DeserializeObject<List<CouncilMember>>(json);

                int ix = councilJson.FindIndex(x => Util.CompareIncompleteStrings(x.SheetName, member) || Util.CompareStringsLevenshteinDistance(x.SheetName, member));

                councilJson[ix].TimesMissedHw = 0;
                var embed = new DiscordEmbedBuilder
                {
                    Color = new DiscordColor("#FF0000"),
                    Title = "__**Notice:**__",
                    Description = $"*Missed homework count for {councilJson[ix].SheetName} has been reset.*",
                    Footer = new DiscordEmbedBuilder.EmbedFooter
                    {
                        Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                    }
                };
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
                string council = JsonConvert.SerializeObject(councilJson);
                File.WriteAllText("council.json", council);
            }
            catch (Exception ex)
            {
                await Util.ThrowError(ctx, ex);
            }
        }

        [SlashCommand("missedhw", "Either gets a specific member's missed homework count, or a list of all.")]
        public async Task DisplayMissedHw(InteractionContext ctx,
            [Option("member", "The name of the council member you are requesting the missed homework count of.")] string member = "")
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

                int ix = -1;

                foreach (var role in ctx.Member.Roles)
                {
                    if (role.Name == "Admin")
                    {
                        if (Util.CompareStrings(member, "all"))
                        {
                            foreach (var m in councilJson)
                            {
                                description += $"*{m.SheetName}: {m.TimesMissedHw}*\n";
                            }
                        }
                        else
                        {
                            ix = councilJson.FindIndex(x => Util.CompareStrings(x.SheetName, member));
                            if (ix != -1)
                            {
                                member = councilJson[ix].SheetName;
                            }
                            if (ix >= 0)
                            {
                                description += $"*{councilJson[ix].SheetName}: {councilJson[ix].TimesMissedHw}*";
                            }
                        }
                    }
                }

                if (description == string.Empty)
                {
                    ix = councilJson.FindIndex(x => x.DiscordId == ctx.Member.Id);
                    if (ix >= 0)
                    {
                        description += $"*{councilJson[ix].SheetName}: {councilJson[ix].TimesMissedHw}*";
                    }
                }

                var embed = new DiscordEmbedBuilder
                {
                    Color = new DiscordColor("#FF0000"),
                    Title = $"__**Council Members Missed Homework Count:**__",
                    Description = description,
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
}