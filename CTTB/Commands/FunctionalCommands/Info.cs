using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
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

namespace CTTB.Commands
{
    public class Info : BaseCommandModule
    {
        public Util Utility = new Util();

        [Command("rating")]
        public async Task GetTrackRating(CommandContext ctx, [RemainingText] string track = "")
        {
            try
            {
                await ctx.TriggerTypingAsync();
                string description = "";
                string serviceAccountEmail = "brawlbox@custom-track-testing-bot.iam.gserviceaccount.com";

                var certificate = new X509Certificate2(@"key.p12", "notasecret", X509KeyStorageFlags.Exportable);

                ServiceAccountCredential credential = new ServiceAccountCredential(
                   new ServiceAccountCredential.Initializer(serviceAccountEmail).FromCertificate(certificate));

                var service = new SheetsService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "Custom Track Testing Bot",
                });

                var request = service.Spreadsheets.Values.Get("1xwhKoyypCWq5tCRTI69ijJoDiaoAVsvYAxz-q4UBNqM", $"'Early {DateTime.Now.Year} Track Rating Data'!A226:Q443");
                var response = await request.ExecuteAsync();

                int e = 0;
                List<string> earlyTrackDisplay = new List<string>();

                for (int i = 0; i < response.Values.Count; i++)
                {
                    if (Utility.CompareStrings(response.Values[i][0].ToString(), track) || Utility.CompareIncompleteStrings(response.Values[i][0].ToString(), track) || Utility.CompareStringAbbreviation(track, response.Values[i][0].ToString()) || Utility.CompareStringsLevenshteinDistance(track, response.Values[i][0].ToString()))
                    {
                        earlyTrackDisplay.Add(response.Values[i][0].ToString());
                        e++;
                    }
                }

                int m = 0;
                List<string> midTrackDisplay = new List<string>();

                if (DateTime.Now > DateTime.ParseExact($"02/07/{DateTime.Now.Year}", "dd/MM/yyyy", CultureInfo.InvariantCulture))
                {
                    request = service.Spreadsheets.Values.Get("1xwhKoyypCWq5tCRTI69ijJoDiaoAVsvYAxz-q4UBNqM", $"'Mid {DateTime.Now.Year} Track Rating Data'!A226:Q443");
                    response = await request.ExecuteAsync();

                    for (int i = 0; i < response.Values.Count; i++)
                    {
                        if (Utility.CompareStrings(response.Values[i][0].ToString(), track) || Utility.CompareIncompleteStrings(response.Values[i][0].ToString(), track) || Utility.CompareStringAbbreviation(track, response.Values[i][0].ToString()) || Utility.CompareStringsLevenshteinDistance(track, response.Values[i][0].ToString()))
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
                        Description = $"*{track} was not found in the latest track rating polls.*" +
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

                    request = service.Spreadsheets.Values.Get("1xwhKoyypCWq5tCRTI69ijJoDiaoAVsvYAxz-q4UBNqM", $"'Early {DateTime.Now.Year} Track Rating Data'!A226:Q443");
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
                                    description += $"__{t[0]}__:\nAll - {Math.Round((double.Parse(t[2].ToString()) / (double.Parse(t[2].ToString()) + double.Parse(t[3].ToString()) + double.Parse(t[4].ToString()))) * 100)}/{Math.Round((double.Parse(t[3].ToString()) / (double.Parse(t[2].ToString()) + double.Parse(t[3].ToString()) + double.Parse(t[4].ToString()))) * 100)}/{Math.Round((double.Parse(t[4].ToString()) / (double.Parse(t[2].ToString()) + double.Parse(t[3].ToString()) + double.Parse(t[4].ToString()))) * 100)}%\n";
                                    description += $"Comp - {Math.Round((double.Parse(t[6].ToString()) / (double.Parse(t[6].ToString()) + double.Parse(t[7].ToString()) + double.Parse(t[8].ToString()))) * 100)}/{Math.Round((double.Parse(t[7].ToString()) / (double.Parse(t[6].ToString()) + double.Parse(t[7].ToString()) + double.Parse(t[8].ToString()))) * 100)}/{Math.Round((double.Parse(t[8].ToString()) / (double.Parse(t[6].ToString()) + double.Parse(t[7].ToString()) + double.Parse(t[8].ToString()))) * 100)}%\n";
                                    description += $"Non-Comp - {Math.Round((double.Parse(t[10].ToString()) / (double.Parse(t[10].ToString()) + double.Parse(t[11].ToString()) + double.Parse(t[12].ToString()))) * 100)}/{Math.Round((double.Parse(t[11].ToString()) / (double.Parse(t[10].ToString()) + double.Parse(t[11].ToString()) + double.Parse(t[12].ToString()))) * 100)}/{Math.Round((double.Parse(t[12].ToString()) / (double.Parse(t[10].ToString()) + double.Parse(t[11].ToString()) + double.Parse(t[12].ToString()))) * 100)}%\n";
                                    description += $"Creators - {Math.Round((double.Parse(t[14].ToString()) / (double.Parse(t[14].ToString()) + double.Parse(t[15].ToString()) + double.Parse(t[16].ToString()))) * 100)}/{Math.Round((double.Parse(t[15].ToString()) / (double.Parse(t[14].ToString()) + double.Parse(t[15].ToString()) + double.Parse(t[16].ToString()))) * 100)}/{Math.Round((double.Parse(t[16].ToString()) / (double.Parse(t[14].ToString()) + double.Parse(t[15].ToString()) + double.Parse(t[16].ToString()))) * 100)}%\n";
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

                        request = service.Spreadsheets.Values.Get("1xwhKoyypCWq5tCRTI69ijJoDiaoAVsvYAxz-q4UBNqM", $"'Mid {DateTime.Now.Year} Track Rating Data'!A226:Q443");
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
                                        description += $"__{t[0]}__:\nAll - {Math.Round((double.Parse(t[2].ToString()) / (double.Parse(t[2].ToString()) + double.Parse(t[3].ToString()) + double.Parse(t[4].ToString()))) * 100)}/{Math.Round((double.Parse(t[3].ToString()) / (double.Parse(t[2].ToString()) + double.Parse(t[3].ToString()) + double.Parse(t[4].ToString()))) * 100)}/{Math.Round((double.Parse(t[4].ToString()) / (double.Parse(t[2].ToString()) + double.Parse(t[3].ToString()) + double.Parse(t[4].ToString()))) * 100)}%\n";
                                        description += $"Comp - {Math.Round((double.Parse(t[6].ToString()) / (double.Parse(t[6].ToString()) + double.Parse(t[7].ToString()) + double.Parse(t[8].ToString()))) * 100)}/{Math.Round((double.Parse(t[7].ToString()) / (double.Parse(t[6].ToString()) + double.Parse(t[7].ToString()) + double.Parse(t[8].ToString()))) * 100)}/{Math.Round((double.Parse(t[8].ToString()) / (double.Parse(t[6].ToString()) + double.Parse(t[7].ToString()) + double.Parse(t[8].ToString()))) * 100)}%\n";
                                        description += $"Non-Comp - {Math.Round((double.Parse(t[10].ToString()) / (double.Parse(t[10].ToString()) + double.Parse(t[11].ToString()) + double.Parse(t[12].ToString()))) * 100)}/{Math.Round((double.Parse(t[11].ToString()) / (double.Parse(t[10].ToString()) + double.Parse(t[11].ToString()) + double.Parse(t[12].ToString()))) * 100)}/{Math.Round((double.Parse(t[12].ToString()) / (double.Parse(t[10].ToString()) + double.Parse(t[11].ToString()) + double.Parse(t[12].ToString()))) * 100)}%\n";
                                        description += $"Creators - {Math.Round((double.Parse(t[14].ToString()) / (double.Parse(t[14].ToString()) + double.Parse(t[15].ToString()) + double.Parse(t[16].ToString()))) * 100)}/{Math.Round((double.Parse(t[15].ToString()) / (double.Parse(t[14].ToString()) + double.Parse(t[15].ToString()) + double.Parse(t[16].ToString()))) * 100)}/{Math.Round((double.Parse(t[16].ToString()) / (double.Parse(t[14].ToString()) + double.Parse(t[15].ToString()) + double.Parse(t[16].ToString()))) * 100)}%\n";
                                    }
                                }
                            }
                        }
                    }

                    if (description.ToCharArray().Length > 1600)
                    {
                        var embed = new DiscordEmbedBuilder
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
                        await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
                    }
                    else if (description.ToCharArray().Length == 0)
                    {
                        var embed = new DiscordEmbedBuilder
                        {
                            Color = new DiscordColor("#FF0000"),
                            Title = "__**Error:**__",
                            Description = $"*{track} was not found in the latest track rating polls.*" +
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
                        var embed = new DiscordEmbedBuilder
                        {
                            Color = new DiscordColor("#FF0000"),
                            Title = $"__**{DateTime.Now.Year} Track Ratings for {track} (Remove/Indifferent/Keep):**__",
                            Description = description,
                            Url = "https://docs.google.com/spreadsheets/d/1xwhKoyypCWq5tCRTI69ijJoDiaoAVsvYAxz-q4UBNqM/edit#gid=595190106",
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
                        string dl = response.Values[k][5].ToString().Contains("=HYPERLINK") ? $"[{response.Values[k][5].ToString().Split('"')[3]}]({response.Values[k][5].ToString().Split('"')[1]})" : "-";
                        description += $"\n{response.Values[k][2]} {response.Values[k][4]} | {response.Values[k][3]} | {dl}";
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
                            string dl = response.Values[k][5].ToString().Contains("=HYPERLINK") ? $"[{response.Values[k][5].ToString().Split('"')[3]}]({response.Values[k][5].ToString().Split('"')[1]})" : "-";
                            description += $"\n{response.Values[k][2]} {response.Values[k][4]} | {response.Values[k][3]} | {dl}";
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
                            string dl = response.Values[k][5].ToString().Contains("=HYPERLINK") ? $"[{response.Values[k][5].ToString().Split('"')[3]}]({response.Values[k][5].ToString().Split('"')[1]})" : "-";
                            description += $"\n{response.Values[k][2]} {response.Values[k][4]} | {response.Values[k][3]} | {dl}";
                        }
                        k++;
                    }
                }

                var embed = new DiscordEmbedBuilder
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
                var embed = new DiscordEmbedBuilder
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
            try
            {
                await ctx.TriggerTypingAsync();
                string description = string.Empty;

                if (track == "")
                {
                    var embed = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = $"__**Error:**__",
                        Description = $"*Track was not inputted.*" +
                               "\n**c!summary track**",
                        Url = "https://wiki.tockdom.com/wiki/CTGP_Revolution/Track_Wishlist",
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
                            if ((Utility.CompareStrings(response.Values[i][2].ToString(), track) ||
                                Utility.CompareIncompleteStrings(response.Values[i][2].ToString(), track) ||
                                Utility.CompareStringAbbreviation(track, response.Values[i][2].ToString()) ||
                                Utility.CompareStringsLevenshteinDistance(response.Values[i][2].ToString(), track)) &&
                                !Utility.CompareIncompleteStrings(response.Values[i][0].ToString(), "ignore"))
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
                        var embed = new DiscordEmbedBuilder
                        {
                            Color = new DiscordColor("#FF0000"),
                            Title = "__**Error:**__",
                            Description = $"*{track} could not be found.*" +
                                   "\n**c!summary track**",
                            Url = "https://wiki.tockdom.com/wiki/CTGP_Revolution/Track_Wishlist",
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
                            Title = $"__**Summary for {trackDisplay} (First result):**__",
                            Description = description,
                            Url = "https://wiki.tockdom.com/wiki/CTGP_Revolution/Track_Wishlist",
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
                    Title = $"__**Error:**__",
                    Description = $"*{ex.Message}*" +
                           "\n**c!summary track**",
                    Url = "https://wiki.tockdom.com/wiki/CTGP_Revolution/Track_Wishlist",
                    Footer = new DiscordEmbedBuilder.EmbedFooter
                    {
                        Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                    }
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
                        if (Utility.CompareStrings(trackList[i].Name, track) || Utility.CompareIncompleteStrings(trackList[i].Name, track) || Utility.CompareStringAbbreviation(track, trackList[i].Name) || Utility.CompareStringsLevenshteinDistance(track, trackList[i].Name))
                        {
                            for (int h = 0; i < response.Values.Count; h++)
                            {
                                if (h != 0)
                                {
                                    if (Utility.CompareStrings(response.Values[h][0].ToString(), track) || Utility.CompareIncompleteStrings(response.Values[h][0].ToString(), track) || Utility.CompareStringAbbreviation(track, response.Values[h][0].ToString()) || Utility.CompareStringsLevenshteinDistance(track, response.Values[h][0].ToString()))
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
                    var embed = new DiscordEmbedBuilder
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
                        if (Utility.CompareStrings(trackList[i].Name, track) || Utility.CompareIncompleteStrings(trackList[i].Name, track) || Utility.CompareStringAbbreviation(track, trackList[i].Name) || Utility.CompareStringsLevenshteinDistance(track, trackList[i].Name))
                        {
                            foreach (var t in response.Values)
                            {
                                if (Utility.CompareStrings(t[0].ToString(), track) || Utility.CompareIncompleteStrings(t[0].ToString(), track) || Utility.CompareStringAbbreviation(track, t[0].ToString()) || Utility.CompareStringsLevenshteinDistance(track, t[0].ToString()))
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
                    var embed = new DiscordEmbedBuilder
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
                    var embed = new DiscordEmbedBuilder
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

        [Command("bkt")]
        public async Task GetBestTimes(CommandContext ctx, [RemainingText] string track = "")
        {

            try
            {
                await ctx.TriggerTypingAsync();
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
                    else if (Utility.CompareStrings(trackList[i].Name, track) || Utility.CompareIncompleteStrings(trackList[i].Name, track) || Utility.CompareStringAbbreviation(track, trackList[i].Name) || Utility.CompareStringsLevenshteinDistance(track, trackList[i].Name))
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
                    else if (Utility.CompareStrings(trackList200[i].Name, track) || Utility.CompareIncompleteStrings(trackList200[i].Name, track) || Utility.CompareStringAbbreviation(track, trackList200[i].Name) || Utility.CompareStringsLevenshteinDistance(track, trackList200[i].Name))
                    {
                        trackDisplay200.Add(trackList200[i]);
                        j++;
                    }
                }

                if (j < 1)
                {
                    var embed = new DiscordEmbedBuilder
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
                        if (Utility.CompareStrings(trackListRts[i].Name, arg) || Utility.CompareIncompleteStrings(trackListRts[i].Name, arg) || Utility.CompareStringAbbreviation(arg, trackListRts[i].Name) || Utility.CompareStringsLevenshteinDistance(arg, trackListRts[i].Name))
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
                        if (Utility.CompareStrings(trackListCts[i].Name, arg) || Utility.CompareIncompleteStrings(trackListCts[i].Name, arg) || Utility.CompareStringAbbreviation(arg, trackListCts[i].Name) || Utility.CompareStringsLevenshteinDistance(arg, trackListCts[i].Name))
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
                    var embed = new DiscordEmbedBuilder
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
                    var embed = new DiscordEmbedBuilder
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
                    var embed = new DiscordEmbedBuilder
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
                    var embed = new DiscordEmbedBuilder
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
                var embed = new DiscordEmbedBuilder
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
                        if (Utility.CompareStrings(trackListRts[i].Name, arg) || Utility.CompareIncompleteStrings(trackListRts[i].Name, arg) || Utility.CompareStringAbbreviation(arg, trackListRts[i].Name) || Utility.CompareStringsLevenshteinDistance(arg, trackListRts[i].Name))
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
                        if (Utility.CompareStrings(trackListCts[i].Name, arg) || Utility.CompareIncompleteStrings(trackListCts[i].Name, arg) || Utility.CompareStringAbbreviation(arg, trackListCts[i].Name) || Utility.CompareStringsLevenshteinDistance(arg, trackListCts[i].Name))
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
                    var embed = new DiscordEmbedBuilder
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
                    var embed = new DiscordEmbedBuilder
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
                    var embed = new DiscordEmbedBuilder
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
                    var embed = new DiscordEmbedBuilder
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
                var embed = new DiscordEmbedBuilder
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