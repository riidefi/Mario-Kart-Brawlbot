using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using static Google.Apis.Sheets.v4.SpreadsheetsResource.ValuesResource;

namespace MKBB.Commands
{
    public class Ghostbusters : ApplicationCommandModule
    {
        [SlashCommand("gbaddtrack", "Adds a new track for Ghostbusters to set times on.")]
        [SlashRequireUserPermissions(Permissions.ManageGuild)]
        public async Task AddNewTrack(InteractionContext ctx,
            [Option("track-name", "The name of the track to add.")] string track,
            [Option("track-id", "The id of the track (also known as the SHA1).")] string trackId)
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

                var request = service.Spreadsheets.Values.Get("1lEn_ex9LtSZhNQ6_T-t13dBGgcWhN8XZXcCtzc49HEY", "'Ghostbusters Submissions'");
                var response = await request.ExecuteAsync();

                foreach (var t in response.Values)
                {
                    while (t.Count < response.Values[response.Values.Count - 1].Count)
                    {
                        t.Add("");
                    }
                }

                for (int i = 0; i < response.Values[response.Values.Count - 1].Count / 5; i++)
                {
                    if (response.Values[0][5 * i].ToString() == track)
                    {
                        await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(new DiscordEmbedBuilder
                        {
                            Color = new DiscordColor("#FF0000"),
                            Title = $"__*Error:*__",
                            Description = "*The track is already added.*",
                            Footer = new DiscordEmbedBuilder.EmbedFooter
                            {
                                Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                            }
                        }));
                        return;
                    }
                    else if (response.Values[0][5 * i].ToString() == "")
                    {
                        response.Values[0][5 * i] = track;
                        response.Values[1][5 * i] = "SHA1s";
                        response.Values[2][5 * i] = trackId.ToUpperInvariant();
                        response.Values[1][5 * i + 1] = "Players";
                        response.Values[1][5 * i + 2] = "Times";
                        response.Values[1][5 * i + 3] = "Combo";
                        response.Values[1][5 * i + 4] = "Comments";
                        break;
                    }
                }
                var updateRequest = service.Spreadsheets.Values.Update(response, "1lEn_ex9LtSZhNQ6_T-t13dBGgcWhN8XZXcCtzc49HEY", $"'Ghostbusters Submissions'!{Util.ConvertToSheetRange(0, 0, response.Values.Count - 1, response.Values[0].Count - 1)}");
                updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;
                var update = await updateRequest.ExecuteAsync();
                var embed = new DiscordEmbedBuilder
                {
                    Color = new DiscordColor("#FF0000"),
                    Title = $"__*Success:*__",
                    Description = $"*{track} has been added successfully.*",
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

        [SlashCommand("gbaddsha1", "Edits the information for a track on the Ghostbusters sheet.")]
        [SlashRequireUserPermissions(Permissions.ManageGuild)]
        public async Task EditNewTrack(InteractionContext ctx,
            [Option("track-name", "The name of the track you want to edit.")] string track,
            [Option("track-id", "The new id of the track (also known as the SHA1).")] string newTrackId)
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

                var request = service.Spreadsheets.Values.Get("1lEn_ex9LtSZhNQ6_T-t13dBGgcWhN8XZXcCtzc49HEY", "'Ghostbusters Submissions'");
                var response = await request.ExecuteAsync();

                foreach (var t in response.Values)
                {
                    while (t.Count < response.Values[response.Values.Count - 1].Count)
                    {
                        t.Add("");
                    }
                }

                for (int i = 0; i < response.Values[response.Values.Count - 1].Count / 5; i++)
                {
                    if (Util.CompareStrings(response.Values[0][5 * i].ToString(), track))
                    {
                        for (int j = 2; j < response.Values.Count; j++)
                        {
                            if (response.Values[j][5 * i].ToString() == "")
                            {
                                response.Values[j][5 * i] = newTrackId.ToUpperInvariant();
                                break;
                            }
                        }
                        break;
                    }
                    else if (response.Values[0][5 * i].ToString() == "")
                    {
                        await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(new DiscordEmbedBuilder
                        {
                            Color = new DiscordColor("#FF0000"),
                            Title = $"__*Error:*__",
                            Description = $"*{track} could not be found.*",
                            Footer = new DiscordEmbedBuilder.EmbedFooter
                            {
                                Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                            }
                        }));
                        return;
                    }
                }

                var updateRequest = service.Spreadsheets.Values.Update(response, "1lEn_ex9LtSZhNQ6_T-t13dBGgcWhN8XZXcCtzc49HEY", $"'Ghostbusters Submissions'!{Util.ConvertToSheetRange(0, 0, response.Values.Count - 1, response.Values[0].Count - 1)}");
                updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;
                var update = await updateRequest.ExecuteAsync();
                var embed = new DiscordEmbedBuilder
                {
                    Color = new DiscordColor("#FF0000"),
                    Title = $"__*Success:*__",
                    Description = "*The track ID was added successfully.*",
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

        [SlashCommand("gbremovetrack", "Removes a track for Ghostbusters.")]
        [SlashRequireUserPermissions(Permissions.ManageGuild)]
        public async Task RemoveNewTrack(InteractionContext ctx,
            [Option("track-name", "The name of the track you want to edit.")] string track)
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

                var request = service.Spreadsheets.Values.Get("1lEn_ex9LtSZhNQ6_T-t13dBGgcWhN8XZXcCtzc49HEY", "'Ghostbusters Submissions'");
                var response = await request.ExecuteAsync();

                foreach (var t in response.Values)
                {
                    while (t.Count < response.Values[response.Values.Count - 1].Count)
                    {
                        t.Add("");
                    }
                }

                for (int i = 0; i < response.Values[response.Values.Count - 1].Count / 5; i++)
                {
                    if (Util.CompareStrings(response.Values[0][5 * i].ToString(), track))
                    {
                        for (int j = 0; j < response.Values.Count; j++)
                        {
                            response.Values[j].RemoveAt(5 * i);
                            response.Values[j].RemoveAt(5 * i);
                            response.Values[j].RemoveAt(5 * i);
                            response.Values[j].RemoveAt(5 * i);
                            response.Values[j].RemoveAt(5 * i);
                            response.Values[j].Add("");
                            response.Values[j].Add("");
                            response.Values[j].Add("");
                            response.Values[j].Add("");
                            response.Values[j].Add("");
                        }
                        response.Values[response.Values.Count - 1][response.Values[response.Values.Count - 1].Count - 6] = "";
                        response.Values[response.Values.Count - 1][response.Values[response.Values.Count - 1].Count - 1] = "End";
                        break;
                    }
                    else if (response.Values[0][5 * i].ToString() == "")
                    {
                        await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(new DiscordEmbedBuilder
                        {
                            Color = new DiscordColor("#FF0000"),
                            Title = $"__*Error:*__",
                            Description = $"*{track} could not be found.*",
                            Footer = new DiscordEmbedBuilder.EmbedFooter
                            {
                                Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                            }
                        }));
                        return;
                    }
                }

                var updateRequest = service.Spreadsheets.Values.Update(response, "1lEn_ex9LtSZhNQ6_T-t13dBGgcWhN8XZXcCtzc49HEY", $"'Ghostbusters Submissions'!{Util.ConvertToSheetRange(0, 0, response.Values.Count - 1, response.Values[0].Count - 1)}");
                updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;
                var update = await updateRequest.ExecuteAsync();
                var embed = new DiscordEmbedBuilder
                {
                    Color = new DiscordColor("#FF0000"),
                    Title = $"__*Success:*__",
                    Description = "*The track was removed successfully.*",
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

        [SlashCommand("gbsubmittime", "Submits a time for a new track.")]
        public async Task SubmitGBTime(InteractionContext ctx,
            [Option("ghost-url", "The Chadsoft URL of the time you would like to submit.")] string ghost,
            [Option("comments", "Comments you would like to make about your ghost e.g. how long it took to set.")] string comments = "")
        {
            try
            {
                await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder() { IsEphemeral = ctx.Guild.Id == 180306609233330176 ? !(ctx.Channel.ParentId == 755509221394743467 || !Util.CheckEphemeral(ctx)) : Util.CheckEphemeral(ctx) });

                string serviceAccountEmail = "brawlbox@custom-track-testing-bot.iam.gserviceaccount.com";

                var certificate = new X509Certificate2(@"key.p12", "notasecret", X509KeyStorageFlags.Exportable);

                ServiceAccountCredential credential = new ServiceAccountCredential(
                   new ServiceAccountCredential.Initializer(serviceAccountEmail).FromCertificate(certificate));

                var service = new SheetsService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "Mario Kart Brawlbot",
                });

                var request = service.Spreadsheets.Values.Get("1lEn_ex9LtSZhNQ6_T-t13dBGgcWhN8XZXcCtzc49HEY", "'Ghostbusters Submissions'");
                var response = await request.ExecuteAsync();

                foreach (var t in response.Values)
                {
                    while (t.Count < response.Values[response.Values.Count - 1].Count)
                    {
                        t.Add("");
                    }
                }

                if (ghost.Contains("rkg"))
                {
                    WebClient webClient = new WebClient();
                    Ghost ghostData = new Ghost();
                    try
                    {
                        ghostData = JsonConvert.DeserializeObject<Ghost>(await webClient.DownloadStringTaskAsync(ghost.Substring(0, ghost.Length - 4) + "json"));
                    }
                    catch
                    {
                        var embed = new DiscordEmbedBuilder
                        {
                            Color = new DiscordColor("#FF0000"),
                            Title = $"__*Error:*__",
                            Description = "*Invalid ghost link.*",
                            Footer = new DiscordEmbedBuilder.EmbedFooter
                            {
                                Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                            }
                        };
                        await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
                        return;
                    }

                    bool found = false;
                    string track = "";

                    for (int i = 0; i < response.Values[response.Values.Count - 1].Count / 5; i++)
                    {
                        for (int j = 2; j < response.Values.Count; j++)
                        {
                            if (Util.CompareStrings(response.Values[j][5 * i].ToString(), ghostData.TrackID))
                            {
                                found = true;
                                track = response.Values[0][5 * i].ToString();
                                break;
                            }
                        }
                        if (found)
                        {
                            for (int j = 2; j < response.Values.Count; j++)
                            {
                                if (response.Values[j][5 * i + 1].ToString() == "")
                                {
                                    response.Values[j][5 * i + 1] = ctx.Member.Username;
                                    response.Values[j][5 * i + 2] = ghostData.FinishTimeSimple;
                                    response.Values[j][5 * i + 3] = $"{Util.Characters[ghostData.DriverID]} + {Util.Vehicles[ghostData.VehicleID]}";
                                    response.Values[j][5 * i + 4] = comments;
                                    break;
                                }
                            }
                            break;
                        }
                    }

                    if (found)
                    {
                        var updateRequest = service.Spreadsheets.Values.Update(response, "1lEn_ex9LtSZhNQ6_T-t13dBGgcWhN8XZXcCtzc49HEY", $"'Ghostbusters Submissions'!{Util.ConvertToSheetRange(0, 0, response.Values.Count - 1, response.Values[0].Count - 1)}");
                        updateRequest.ValueInputOption = UpdateRequest.ValueInputOptionEnum.USERENTERED;
                        await updateRequest.ExecuteAsync();

                        await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(new DiscordEmbedBuilder
                        {
                            Color = new DiscordColor("#FF0000"),
                            Title = $"__*Success:*__",
                            Description = $"*The time was added successfully.*" +
                            $"\nTrack: *{track}*" +
                            $"\nTime: *{ghostData.FinishTimeSimple}*" +
                            $"\nCombo: *{Util.Characters[ghostData.DriverID]} + {Util.Vehicles[ghostData.VehicleID]}*",
                            Footer = new DiscordEmbedBuilder.EmbedFooter
                            {
                                Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                            }
                        }));
                    }
                    else
                    {
                        await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(new DiscordEmbedBuilder
                        {
                            Color = new DiscordColor("#FF0000"),
                            Title = $"__*Error:*__",
                            Description = $"*The track SHA1 of the ghost link was not found on any new tracks.*",
                            Footer = new DiscordEmbedBuilder.EmbedFooter
                            {
                                Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                            }
                        }));
                    }
                }
                else
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = $"__*Error:*__",
                        Description = $"*This is not a valid ghost link.*",
                        Footer = new DiscordEmbedBuilder.EmbedFooter
                        {
                            Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                        }
                    }));
                }
            }

            catch (Exception ex)
            {
                await Util.ThrowError(ctx, ex);
            }
        }

        [SlashCommand("gblist", "Gets the list of all the tracks current being reviewed by Ghostbusters.")]
        public async Task GetGBList(InteractionContext ctx)
        {
            try
            {
                await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder() { IsEphemeral = ctx.Guild.Id == 180306609233330176 ? !(ctx.Channel.ParentId == 755509221394743467 || !Util.CheckEphemeral(ctx)) : Util.CheckEphemeral(ctx) });

                string serviceAccountEmail = "brawlbox@custom-track-testing-bot.iam.gserviceaccount.com";

                var certificate = new X509Certificate2(@"key.p12", "notasecret", X509KeyStorageFlags.Exportable);

                ServiceAccountCredential credential = new ServiceAccountCredential(
                   new ServiceAccountCredential.Initializer(serviceAccountEmail).FromCertificate(certificate));

                var service = new SheetsService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "Mario Kart Brawlbot",
                });

                var request = service.Spreadsheets.Values.Get("1lEn_ex9LtSZhNQ6_T-t13dBGgcWhN8XZXcCtzc49HEY", "'Ghostbusters Submissions'");
                var response = await request.ExecuteAsync();

                foreach (var t in response.Values)
                {
                    while (t.Count < response.Values[response.Values.Count - 1].Count)
                    {
                        t.Add("");
                    }
                }

                List<string> tracks = new List<string>();

                for (int i = 0; i < response.Values[response.Values.Count - 1].Count / 5; i++)
                {
                    if (response.Values[0][5 * i].ToString() == "")
                    {
                        break;
                    }
                    else
                    {
                        tracks.Add(response.Values[0][5 * i].ToString());
                    }
                }
                string description = "";

                if (tracks.Count > 0)
                {
                    foreach (var track in tracks)
                    {
                        description += $"*{track}*\n";
                    }
                }
                else
                {
                    description = "*No tracks currently listed.*";
                }

                var embed = new DiscordEmbedBuilder
                {
                    Color = new DiscordColor("#FF0000"),
                    Title = $"__**Current Ghostbusters Tracks:**__",
                    Description = description,
                    Footer = new DiscordEmbedBuilder.EmbedFooter
                    {
                        Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                    }
                };
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed).AddComponents(Util.GetBackroomLinkButton()));
            }

            catch (Exception ex)
            {
                await Util.ThrowError(ctx, ex);
            }
        }
    }
}