using DSharpPlus;
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
using Newtonsoft.Json;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace MKBB.Commands
{
    public class Issues : ApplicationCommandModule
    {
        [SlashCommand("reportissue", "Used to report issues on tracks.")]
        [SlashRequireUserPermissions(Permissions.Administrator)]
        public async Task ReportIssue(InteractionContext ctx,
            [Choice("Major", 5)]
            [Choice("Minor", 6)]
            [Option("severity", "The severity of the issue being reported.")] double issueType,
            [Option("track-name", "The track the issue is being reported on.")] string track,
            [Option("issue", "Suffixed with -. The issue that is being reported (preferably with footage).")] string issue)
        {
            try
            {
                await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder() { IsEphemeral = ctx.Channel.Id == 908709951411716166 ? false : true });

                string json = string.Empty;
                string maj = string.Empty;
                string min = string.Empty;

                int issueIx = int.Parse(issueType.ToString());

                string serviceAccountEmail = "brawlbox@custom-track-testing-bot.iam.gserviceaccount.com";

                var certificate = new X509Certificate2(@"key.p12", "notasecret", X509KeyStorageFlags.Exportable);

                ServiceAccountCredential credential = new ServiceAccountCredential(
                   new ServiceAccountCredential.Initializer(serviceAccountEmail).FromCertificate(certificate));

                var service = new SheetsService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "Mario Kart Brawlbot",
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

                json = File.ReadAllText("cts.json");
                List<Track> trackList = JsonConvert.DeserializeObject<List<Track>>(json);

                int index1 = Util.ListNameCheck(trackList, track);
                int index2 = Util.ListNameCheck(response.Values, track, ix2: 0);

                if (response.Values[index2][issueIx].ToString() != "")
                {
                    response.Values[index2][issueIx] = $"{response.Values[index2][issueIx]}\n{issue}";
                }
                else
                {
                    response.Values[index2][issueIx] = issue;
                }
                if (response.Values[index2][5].ToString() == "")
                {
                    maj = "-No reported bugs";
                }
                else
                {
                    maj = response.Values[index2][5].ToString();
                }
                if (response.Values[index2][6].ToString() == "")
                {
                    min = "-No reported bugs";
                }
                else
                {
                    min = response.Values[index2][6].ToString();
                }

                if (index1 < 0 || index2 < 0)
                {
                    var embed = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = "__**Error:**__",
                        Description = $"*{track} could not be found.*",
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
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
                }
            }
            catch (Exception ex)
            {
                await Util.ThrowError(ctx, ex);
            }
        }

        [SlashCommand("clearissues", "Clears the issues for a given track.")]
        [SlashRequireUserPermissions(Permissions.Administrator)]
        public async Task ClearTrackIssues(InteractionContext ctx,
            [Option("track-name", "The track the issue is being reported on.")] string track)
        {
            try
            {
                await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder() { IsEphemeral = ctx.Channel.Id == 908709951411716166 ? false : true });

                string json = string.Empty;
                string serviceAccountEmail = "brawlbox@custom-track-testing-bot.iam.gserviceaccount.com";

                var certificate = new X509Certificate2(@"key.p12", "notasecret", X509KeyStorageFlags.Exportable);

                ServiceAccountCredential credential = new ServiceAccountCredential(
                   new ServiceAccountCredential.Initializer(serviceAccountEmail).FromCertificate(certificate));

                var service = new SheetsService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "Mario Kart Brawlbot",
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
                json = File.ReadAllText("cts.json");
                List<Track> trackList = JsonConvert.DeserializeObject<List<Track>>(json);

                int index1 = Util.ListNameCheck(trackList, track);
                int index2 = Util.ListNameCheck(response.Values, track, ix2: 0);

                if (index1 > -1 && index2 > -1)
                {
                    response.Values[index2][5] = "";
                    response.Values[index2][6] = "";
                }

                var updateRequest = service.Spreadsheets.Values.Update(response, "1xwhKoyypCWq5tCRTI69ijJoDiaoAVsvYAxz-q4UBNqM", "'CTGP Track Issues'!A1:G219");
                updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.RAW;
                var update = await updateRequest.ExecuteAsync();

                if (index1 < 0 || index2 < 0)
                {
                    var embed = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = "__**Error:**__",
                        Description = $"*{track} could not be found.*",
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
                        Title = "__**Success:**__",
                        Description = $"*{track} issues have been cleared.*",
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

        [SlashCommand("replaceissues", "Replaces the track in the issues tab with a new track.")]
        [SlashRequireUserPermissions(Permissions.Administrator)]
        public async Task ReplaceTrackIssues(InteractionContext ctx,
            [Option("track-name", "The name of the track being replaced.")] string track = "",
            [Option("new-track-name", "The name of the track being added.")] string newTrack = "",
            [Option("authors", "The author or authors of the track being added.")] string author = "",
            [Option("version", "The version number of the track being added.")] string version = "",
            [Option("slots", "The track/music slots of the track being added e.g. 'Luigi Circuit / Block Plaza'")] string slot = "",
            [Option("lap-and-speed-modifiers", "The lap and speed modifiers of the track being added, the most common being '1 / 3'.")] string laps = "")
        {
            try
            {
                await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder() { IsEphemeral = ctx.Channel.Id == 908709951411716166 ? false : true });

                string json = string.Empty;
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

                var request = service.Spreadsheets.Values.Get("1xwhKoyypCWq5tCRTI69ijJoDiaoAVsvYAxz-q4UBNqM", "'CTGP Track Issues'!A2:H219");
                var response = await request.ExecuteAsync();
                foreach (var t in response.Values)
                {
                    while (t.Count < 7)
                    {
                        t.Add("");
                    }
                }
                json = File.ReadAllText("cts.json");
                List<Track> trackList = JsonConvert.DeserializeObject<List<Track>>(json);

                int index1 = Util.ListNameCheck(trackList, track);
                int index2 = Util.ListNameCheck(response.Values, track, ix2: 0);

                if (index1 > -1 && index2 > -1)
                {
                    response.Values[index2][0] = newTrack;
                    response.Values[index2][1] = author;
                    response.Values[index2][2] = version;
                    response.Values[index2][3] = slot;
                    response.Values[index2][4] = laps;
                    response.Values[index2][5] = "";
                    response.Values[index2][6] = "";
                    description = $"**{newTrack}:**\nAuthor: *{author}*\nVersion: *{version}*\nSlots: *{slot}*\nSpeed/Laps: *{laps}*";
                }

                if (index1 < 0 || index2 < 0)
                {
                    var embed = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = "__**Error:**__",
                        Description = $"*{track} could not be found.*",
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