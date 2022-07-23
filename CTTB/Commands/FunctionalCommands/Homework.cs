using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace CTTB.Commands
{
    public class Homework : BaseCommandModule
    {
        public Util Utility = new Util();

        [Command("addhw")]
        [RequireRoles(RoleCheckMode.Any, "Admin")]
        public async Task AddHomework(CommandContext ctx, string track = "", string author = "", string version = "", string download = "", string slot = "", string lapSpeed = "1/3", [RemainingText] string notes = "")
        {
            if (ctx.Guild.Id == 180306609233330176)
            {
                try
                {
                    if (ctx.Channel.Id == 217126063803727872 || ctx.Channel.Id == 750123394237726847 || ctx.Channel.Id == 935200150710808626 || ctx.Channel.Id == 946835035372257320 || ctx.Channel.Id == 751534710068477953)
                    {
                        string description = string.Empty;

                        if (track == "")
                        {
                            var embed = new DiscordEmbedBuilder
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
                            var embed = new DiscordEmbedBuilder
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
                            var embed = new DiscordEmbedBuilder
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
                            var embed = new DiscordEmbedBuilder
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

                            var appendRequest = service.Spreadsheets.Values.Append(new ValueRange() { Values = values }, "1I9yFsomTcvFT4hp6eN2azsfv6MsIy1897tBFX_gmtss", "'Track Evaluating'");
                            appendRequest.InsertDataOption = SpreadsheetsResource.ValuesResource.AppendRequest.InsertDataOptionEnum.INSERTROWS;
                            appendRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
                            appendRequest.ResponseValueRenderOption = SpreadsheetsResource.ValuesResource.AppendRequest.ResponseValueRenderOptionEnum.FORMULA;
                            var appendResponse = await appendRequest.ExecuteAsync();

                            var embed = new DiscordEmbedBuilder
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
                    var embed = new DiscordEmbedBuilder
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
                try
                {
                    if (ctx.Channel.Id == 217126063803727872 || ctx.Channel.Id == 750123394237726847 || ctx.Channel.Id == 935200150710808626 || ctx.Channel.Id == 946835035372257320 || ctx.Channel.Id == 751534710068477953)
                    {
                        if (track == "")
                        {
                            var embed = new DiscordEmbedBuilder
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
                                if (Utility.CompareStrings(response.Values[i][0].ToString(), track))
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


                                var embed = new DiscordEmbedBuilder
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
                    var embed = new DiscordEmbedBuilder
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
                try
                {
                    if (ctx.Channel.Id == 217126063803727872 || ctx.Channel.Id == 750123394237726847 || ctx.Channel.Id == 935200150710808626 || ctx.Channel.Id == 946835035372257320 || ctx.Channel.Id == 751534710068477953)
                    {

                        string description = string.Empty;
                        string json = string.Empty;
                        string member = string.Empty;

                        if (vote == "")
                        {
                            var embed = new DiscordEmbedBuilder
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
                            var embed = new DiscordEmbedBuilder
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
                            var embed = new DiscordEmbedBuilder
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
                                if (Utility.CompareStrings(response.Values[0][i].ToString(), member))
                                {
                                    ix = i;
                                }
                            }

                            if (ix < 0)
                            {
                                var embed = new DiscordEmbedBuilder
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
                                    else if (Utility.CompareStringAbbreviation(track, t[0].ToString()) || Utility.CompareStringsLevenshteinDistance(track, t[0].ToString()))
                                    {
                                        t[ix] = vote + "\n" + feedback;
                                        j++;
                                    }
                                }

                                SpreadsheetsResource.ValuesResource.UpdateRequest updateRequest;

                                if (response.Values[0].Count < 27)
                                {
                                    updateRequest = service.Spreadsheets.Values.Update(response, "1I9yFsomTcvFT4hp6eN2azsfv6MsIy1897tBFX_gmtss", $"'Track Evaluating'!A1:{Utility.strAlpha[response.Values[0].Count - 1]}{response.Values.Count}");
                                }
                                else
                                {
                                    updateRequest = service.Spreadsheets.Values.Update(response, "1I9yFsomTcvFT4hp6eN2azsfv6MsIy1897tBFX_gmtss", $"'Track Evaluating'!A1:A{Utility.strAlpha[response.Values[0].Count % 26 - 1]}{response.Values.Count}");
                                }
                                updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;
                                var update = await updateRequest.ExecuteAsync();

                                if (j == 0)
                                {
                                    var embed = new DiscordEmbedBuilder
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
                                    var embed = new DiscordEmbedBuilder
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
                    var embed = new DiscordEmbedBuilder
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
                        else if (member != "all")
                        {
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
                                ix = councilJson.FindIndex(x => Utility.CompareStrings(x.SheetName, member));
                                if (ix == -1)
                                {
                                    ix = councilJson.FindIndex(x => Utility.CompareIncompleteStrings(x.SheetName, member) || Utility.CompareStringsLevenshteinDistance(x.SheetName, member));
                                }
                                if (ix != -1)
                                {
                                    member = councilJson[ix].SheetName;
                                }
                            }
                        }

                        string description = string.Empty;

                        if (track == "")
                        {
                            var embed = new DiscordEmbedBuilder
                            {
                                Color = new DiscordColor("#FF0000"),
                                Title = $"__**Error:**__",
                                Description = $"*Track was not inputted.*" +
                                       "\n**c!gethw track/all name**",
                                Url = "https://docs.google.com/spreadsheets/d/1I9yFsomTcvFT4hp6eN2azsfv6MsIy1897tBFX_gmtss/edit#gid=906385082",
                                Footer = new DiscordEmbedBuilder.EmbedFooter
                                {
                                    Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                                }
                            };
                            await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
                        }
                        else if (ix < 0)
                        {
                            if (member == "")
                            {
                                var embed = new DiscordEmbedBuilder
                                {
                                    Color = new DiscordColor("#FF0000"),
                                    Title = $"__**Error:**__",
                                    Description = $"*<@{ctx.Message.Author.Id}> could not be found on council.*" +
                                       "\n**c!gethw track/all name**",
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
                                var embed = new DiscordEmbedBuilder
                                {
                                    Color = new DiscordColor("#FF0000"),
                                    Title = $"__**Error:**__",
                                    Description = $"*{member} could not be found on council.*" +
                                       "\n**c!gethw track/all name**",
                                    Url = "https://docs.google.com/spreadsheets/d/1I9yFsomTcvFT4hp6eN2azsfv6MsIy1897tBFX_gmtss/edit#gid=906385082",
                                    Footer = new DiscordEmbedBuilder.EmbedFooter
                                    {
                                        Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                                    }
                                };
                                await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
                            }
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

                            if (Utility.CompareStrings(member, "all"))
                            {
                                for (int i = 1; i < response.Values.Count; i++)
                                {
                                    if (Utility.CompareStringAbbreviation(track, response.Values[i][0].ToString()) || Utility.CompareStringsLevenshteinDistance(track, response.Values[i][0].ToString()))
                                    {
                                        sheetIx = i;
                                    }
                                }
                            }
                            else
                            {
                                for (int i = 12; i < response.Values[0].Count; i++)
                                {
                                    if (Utility.CompareStringAbbreviation(member, response.Values[0][i].ToString()) || Utility.CompareStrings(member, response.Values[0][i].ToString()))
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
                                    if (Utility.CompareStrings(member, "all"))
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

                                            var embed = new DiscordEmbedBuilder
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
                                            else if (Utility.CompareStrings(t[0].ToString(), track))
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

                                            var embed = new DiscordEmbedBuilder
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
                                var embed = new DiscordEmbedBuilder
                                {
                                    Color = new DiscordColor("#FF0000"),
                                    Title = $"__**Error:**__",
                                    Description = $"*{member} could not be found on council.*" +
                                       "\n**c!gethw track/all name**",
                                    Url = "https://docs.google.com/spreadsheets/d/1I9yFsomTcvFT4hp6eN2azsfv6MsIy1897tBFX_gmtss/edit#gid=906385082",
                                    Footer = new DiscordEmbedBuilder.EmbedFooter
                                    {
                                        Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                                    }
                                };
                                await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
                            }

                            else if (Utility.CompareStrings(member, "all"))
                            {
                                List<Page> pages = new List<Page>();

                                foreach (var e in embeds)
                                {
                                    pages.Add(new Page("", e));
                                }

                                await ctx.Channel.SendPaginatedMessageAsync(ctx.User, pages);
                            }

                            else if (Utility.CompareStrings(track, "all"))
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
                                var embed = new DiscordEmbedBuilder
                                {
                                    Color = new DiscordColor("#FF0000"),
                                    Title = $"__**Error:**__",
                                    Description = $"*{track} could not be found.*" +
                                       "\n**c!gethw track/all name**",
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
                                var embed = new DiscordEmbedBuilder
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
                    var embed = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = "__**Error:**__",
                        Description = $"*{ex.Message}*" +
                               "\n**c!gethw track/all name**",
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
                    var embed = new DiscordEmbedBuilder
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

        [Command("addmissedhw")]
        [RequireRoles(RoleCheckMode.Any, "Admin")]
        public async Task IncrementMissedHw(CommandContext ctx, [RemainingText] string member = "")
        {
            try
            {
                if (ctx.Channel.Id == 217126063803727872 || ctx.Channel.Id == 750123394237726847 || ctx.Channel.Id == 935200150710808626 || ctx.Channel.Id == 946835035372257320 || ctx.Channel.Id == 751534710068477953)
                {
                    await ctx.TriggerTypingAsync();
                    string json;
                    using (var fs = File.OpenRead("council.json"))
                    using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                        json = await sr.ReadToEndAsync().ConfigureAwait(false);
                    List<CouncilMember> councilJson = JsonConvert.DeserializeObject<List<CouncilMember>>(json);

                    int ix = councilJson.FindIndex(x => Utility.CompareIncompleteStrings(x.SheetName, member) || Utility.CompareStringsLevenshteinDistance(x.SheetName, member));

                    if (member == "")
                    {
                        var embed = new DiscordEmbedBuilder
                        {
                            Color = new DiscordColor("#FF0000"),
                            Title = "__**Error:**__",
                            Description = $"*Member was not inputted.*" +
                                "\n**c!addmissedhw member**",
                            Footer = new DiscordEmbedBuilder.EmbedFooter
                            {
                                Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                            }
                        };
                        await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
                    }
                    else if (ix != -1)
                    {
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
                        await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
                        string council = JsonConvert.SerializeObject(councilJson);
                        File.WriteAllText("council.json", council);
                    }
                    else
                    {
                        var embed = new DiscordEmbedBuilder
                        {
                            Color = new DiscordColor("#FF0000"),
                            Title = "__**Error:**__",
                            Description = $"*{member} could not be found.*" +
                                "\n**c!addmissedhw member**",
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
                       "\n**c!addmissedhw member**",
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

                    int ix = councilJson.FindIndex(x => Utility.CompareIncompleteStrings(x.SheetName, member) || Utility.CompareStringsLevenshteinDistance(x.SheetName, member));

                    if (member == "")
                    {
                        embed = new DiscordEmbedBuilder
                        {
                            Color = new DiscordColor("#FF0000"),
                            Title = "__**Error:**__",
                            Description = $"*Member was not inputted.*" +
                                "\n**c!removemissedhw member**",
                            Footer = new DiscordEmbedBuilder.EmbedFooter
                            {
                                Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                            }
                        };
                    }
                    else if (ix != -1)
                    {
                        councilJson[ix].TimesMissedHw--;
                        embed = new DiscordEmbedBuilder
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
                    }
                    else
                    {
                        embed = new DiscordEmbedBuilder
                        {
                            Color = new DiscordColor("#FF0000"),
                            Title = "__**Error:**__",
                            Description = $"*{member} could not be found.*" +
                                "\n**c!removemissedhw member**",
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
            try
            {
                if (ctx.Channel.Id == 217126063803727872 || ctx.Channel.Id == 750123394237726847 || ctx.Channel.Id == 935200150710808626 || ctx.Channel.Id == 946835035372257320 || ctx.Channel.Id == 751534710068477953)
                {
                    await ctx.TriggerTypingAsync();
                    string json;
                    using (var fs = File.OpenRead("council.json"))
                    using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                        json = await sr.ReadToEndAsync().ConfigureAwait(false);
                    List<CouncilMember> councilJson = JsonConvert.DeserializeObject<List<CouncilMember>>(json);

                    int ix = councilJson.FindIndex(x => Utility.CompareIncompleteStrings(x.SheetName, member) || Utility.CompareStringsLevenshteinDistance(x.SheetName, member));

                    if (ix != -1)
                    {
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
                        await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
                        string council = JsonConvert.SerializeObject(councilJson);
                        File.WriteAllText("council.json", council);
                    }
                    else if (member == "")
                    {
                        foreach (var m in councilJson)
                        {
                            m.TimesMissedHw = 0;
                        }
                        var embed = new DiscordEmbedBuilder
                        {
                            Color = new DiscordColor("#FF0000"),
                            Title = "__**Notice:**__",
                            Description = "*Missed homework count for all members has been reset.*",
                            Footer = new DiscordEmbedBuilder.EmbedFooter
                            {
                                Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                            }
                        };
                        await ctx.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
                        string council = JsonConvert.SerializeObject(councilJson);
                        File.WriteAllText("council.json", council);
                    }
                    else
                    {
                        var embed = new DiscordEmbedBuilder
                        {
                            Color = new DiscordColor("#FF0000"),
                            Title = "__**Error:**__",
                            Description = $"*{member} could not be found.*",
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
            try
            {
                if (ctx.Channel.Id == 217126063803727872 || ctx.Channel.Id == 750123394237726847 || ctx.Channel.Id == 935200150710808626 || ctx.Channel.Id == 946835035372257320 || ctx.Channel.Id == 751534710068477953)
                {
                    await ctx.TriggerTypingAsync();
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
    }
}