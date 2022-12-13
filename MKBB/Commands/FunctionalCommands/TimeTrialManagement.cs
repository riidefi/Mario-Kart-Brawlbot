﻿using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace MKBB.Commands
{
    public class TimeTrialManagement : ApplicationCommandModule
    {
        [SlashCommand("register", "Register your Chadsoft account with the bot.")]
        public async Task RegisterNewPlayerInit(InteractionContext ctx,
            [Option("player-id", "The player ID on your Chadsoft player page.")] string playerId)
        {
            try
            {
                await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder() { IsEphemeral = !(ctx.Channel.Id == 908709951411716166 || ctx.Channel.ParentId == 755509221394743467) });

                bool fail = false;
                string playerUrl = $"https://www.chadsoft.co.uk/time-trials/players/{playerId.Substring(0, 2)}/{playerId.Substring(2)}.json";
                var player = new Player();
                try
                {
                    var webClient = new WebClient();
                    player = JsonConvert.DeserializeObject<Player>(await webClient.DownloadStringTaskAsync(playerUrl));
                    player.PlayerLink = playerUrl;
                    player.DiscordId = ctx.Member.Id;
                    player.Ghosts = null;
                }
                catch
                {
                    fail = true;
                }
                if (fail)
                {
                    var embed = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = "__**Error:**__",
                        Description = "*Player ID was invalid or doesn't exist. To get your player ID, go to [Chadsoft](https://www.chadsoft.co.uk/time-trials/players.html) and search for your player page, which will have your player ID.*",
                        Footer = new DiscordEmbedBuilder.EmbedFooter
                        {
                            Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                        }
                    };
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
                }
                else
                {
                    string playerListJson = File.ReadAllText("players.json");
                    List<Player> playerList = JsonConvert.DeserializeObject<List<Player>>(playerListJson);

                    int ix = playerList.FindIndex(x => x.DiscordId == player.DiscordId);

                    if (ix < 0)
                    {
                        playerList.Add(player);

                        playerListJson = JsonConvert.SerializeObject(playerList);
                        File.WriteAllText("players.json", playerListJson);

                        var embed = new DiscordEmbedBuilder
                        {
                            Color = new DiscordColor("#FF0000"),
                            Title = "__**Success:**__",
                            Description = $"*Player ID has been registered.*",
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
                            Title = "__**Notice:**__",
                            Description = $"*This player ID has already been registered. If you think this is a mistake, please contact <@105742694730457088>.*",
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

        [SlashCommand("stars", "Gets the stars of the user specified.")]
        public async Task DisplayStars(InteractionContext ctx,
            [Option("player", "Will display the stars of the player requested.")] DiscordUser user)
        {
            try
            {
                await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder() { IsEphemeral = !(ctx.Channel.Id == 908709951411716166 || ctx.Channel.ParentId == 755509221394743467) });

                string playerListJson = File.ReadAllText("players.json");
                List<Player> playerList = JsonConvert.DeserializeObject<List<Player>>(playerListJson);

                int ix = playerList.FindIndex(x => x.DiscordId == user.Id);

                if (ix < 0)
                {
                    var embed = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = "__**Error:**__",
                        Description = $"*Player ID has not yet been registered. Please use /register to register your player ID if you haven't already. To get your player ID, go to [Chadsoft](https://www.chadsoft.co.uk/time-trials/players.html) and search for your player page, which will have your player ID.*",
                        Footer = new DiscordEmbedBuilder.EmbedFooter
                        {
                            Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                        }
                    };
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
                }
                else
                {
                    var webClient = new WebClient();
                    var starsJson = JsonConvert.DeserializeObject<GetStars>(await webClient.DownloadStringTaskAsync(playerList[ix].PlayerLink));

                    var embed = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = $"__**Stars of {playerList[ix].MiiName} ({user.Username}):**__",
                        Description = $"<:goldstar:1021692357659332689> - {starsJson.Stars.Gold}\n" +
                        $"<:silverstar:1021692359202836480> - {starsJson.Stars.Silver}\n" +
                        $"<:bronzestar:1021692360956059688> - {starsJson.Stars.Bronze}",
                        Url = $"{playerList[ix].PlayerLink.Substring(0, playerList[ix].PlayerLink.Length - 5)}.html",
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

        [SlashCommand("pb", "Gets the personal best(s) on the track and player specified.")]
        public async Task GetPBs(InteractionContext ctx,
            [Option("track-name", "The track you want to display the personal bests of.")] string track,
            [Option("player", "The Discord user of the player you want to the display the personal bests of.")] DiscordUser user,
            [Choice("150cc", "")]
            [Choice("200cc", "200")]
            [Option("engine-class", "The engine class of the personal best you want to find .")] string cc = "")
        {
            try
            {
                await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder() { IsEphemeral = !(ctx.Channel.Id == 908709951411716166 || ctx.Channel.ParentId == 755509221394743467) });
                List<Player> playerList = JsonConvert.DeserializeObject<List<Player>>(File.ReadAllText("players.json"));
                int playerIx = playerList.FindIndex(x => x.DiscordId == user.Id);

                List<Track> trackList = JsonConvert.DeserializeObject<List<Track>>(File.ReadAllText($"rts{cc}.json"));
                foreach (var t in JsonConvert.DeserializeObject<List<Track>>(File.ReadAllText($"cts{cc}.json")))
                {
                    trackList.Add(t);
                }
                int trackIx = Util.ListNameCheck(trackList, track);
                if (playerIx < 0)
                {
                    var embed = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = $"__**Error:**__",
                        Description = $"*{user.Username} has not been registered. To register, use /register with your Chadsoft player ID. To get your player ID, go to [Chadsoft](https://www.chadsoft.co.uk/time-trials/players.html) and search for your player page, which will have your player ID.*",
                        Footer = new DiscordEmbedBuilder.EmbedFooter
                        {
                            Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                        }
                    };
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
                }
                else if (trackIx < 0)
                {
                    var embed = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = $"__**Error:**__",
                        Description = $"*{track} could not be found.*",
                        Footer = new DiscordEmbedBuilder.EmbedFooter
                        {
                            Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                        }
                    };
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
                }
                else
                {
                    var webClient = new WebClient();
                    Track foundTrack = trackList[trackIx];
                    List<Track> allTrackCategories = trackList.Where(x => x.SHA1 == foundTrack.SHA1).ToList();
                    Player player = JsonConvert.DeserializeObject<Player>(await webClient.DownloadStringTaskAsync(playerList[playerIx].PlayerLink));
                    List<Ghost> applicableGhosts = player.Ghosts.Where(x => x.TrackID == foundTrack.SHA1 && x.IsPB && x.Is200cc == (cc == "" ? false : true)).ToList();
                    if (applicableGhosts.Count == 0)
                    {
                        var embed = new DiscordEmbedBuilder
                        {
                            Color = new DiscordColor("#FF0000"),
                            Title = $"__**Error:**__",
                            Description = $"*No record was found on {trackList[trackIx].Name}.*",
                            Footer = new DiscordEmbedBuilder.EmbedFooter
                            {
                                Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                            }
                        };
                        await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
                    }
                    else
                    {
                        List<DiscordEmbedBuilder> embeds = new List<DiscordEmbedBuilder>();
                        allTrackCategories.OrderBy(x => x.Category);
                        applicableGhosts.OrderBy(x => x.Category);

                        foreach (var ghost in applicableGhosts)
                        {
                            ghost.ExtraInfo = JsonConvert.DeserializeObject<ExtraInfo>(await webClient.DownloadStringTaskAsync($"https://www.chadsoft.co.uk/time-trials{ghost.LinkContainer.Href.URL}"));
                            ghost.CategoryName = allTrackCategories.Find(x => x.Category == ghost.Category).CategoryName;

                            var embed = new DiscordEmbedBuilder
                            {
                                Color = new DiscordColor("#FF0000"),
                                Title = $"__**{trackList[trackIx].Name} - {ghost.CategoryName} {(cc == "" ? "(150cc)" : "(200cc)")}:**__",
                                Description = $"{user.Username}'s fastest time on {trackList[trackIx].Name}:\n\n" +
                                $"**Time:** {ghost.FinishTimeSimple}\n\n" +
                                $"**Splits:** {string.Join(" - ", ghost.ExtraInfo.SimpleSplits.ToArray())}\n\n" +
                                $"**Combo:** {Util.Characters[ghost.DriverID]} on {Util.Vehicles[ghost.VehicleID]}\n\n" +
                                $"**Date Set:** {ghost.DateSet.Split('T')[0]}\n\n" +
                                $"**Controller:**\n{Util.Controllers[ghost.ControllerID]}\n\n" +
                                $"**Extra Details:**\n" +
                                $"*Exact Finish Time: {ghost.FinishTime}*\n\n" +
                                $"*Exact Splits: {string.Join(" - ", ghost.ExtraInfo.Splits.ToArray())}*",
                                Url = $"https://www.chadsoft.co.uk/time-trials{ghost.LinkContainer.Href.URL.Substring(0, ghost.LinkContainer.Href.URL.Length - 4)}html",
                                Footer = new DiscordEmbedBuilder.EmbedFooter
                                {
                                    Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                                }
                            };

                            embeds.Add(embed);
                        }

                        var messageBuilder = new DiscordWebhookBuilder().AddEmbed(embeds[0]);

                        if (embeds.Count() > 1)
                        {
                            messageBuilder.AddComponents(Util.GeneratePageArrows(ctx));
                        }

                        var message = await ctx.EditResponseAsync(messageBuilder);

                        if (embeds.Count() > 1)
                        {
                            PendingInteraction pending = new PendingInteraction() { CurrentPage = 0, MessageId = message.Id, Context = ctx, Pages = embeds };

                            Util.PendingInteractions.Add(pending);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await Util.ThrowError(ctx, ex);
            }
        }

        [SlashCommand("top10", "Gets the top 10 of the track specified.")]
        public async Task GetTop10(InteractionContext ctx,
            [Option("track-name", "The track you want to display the leaderboard of.")] string track,
            [Choice("150cc", "")]
            [Choice("200cc", "200")]
            [Option("engine-class", "The engine class of the personal best you want to find .")] string cc = "")
        {
            try
            {
                await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder() { IsEphemeral = !(ctx.Channel.Id == 908709951411716166 || ctx.Channel.ParentId == 755509221394743467) });

                List<Track> trackList = JsonConvert.DeserializeObject<List<Track>>(File.ReadAllText($"rts{cc}.json"));
                foreach (var t in JsonConvert.DeserializeObject<List<Track>>(File.ReadAllText($"cts{cc}.json")))
                {
                    trackList.Add(t);
                }
                int trackIx = Util.ListNameCheck(trackList, track);
                if (trackIx < 0)
                {
                    var embed = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = $"__**Error:**__",
                        Description = $"*{track} could not be found.*",
                        Footer = new DiscordEmbedBuilder.EmbedFooter
                        {
                            Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                        }
                    };
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
                }
                else
                {
                    var webClient = new WebClient();
                    Track foundTrack = trackList[trackIx];
                    List<Track> allTrackCategories = trackList.Where(x => x.SHA1 == foundTrack.SHA1).ToList();
                    allTrackCategories.OrderBy(x => x.Category);

                    List<List<DiscordEmbedBuilder>> categories = new List<List<DiscordEmbedBuilder>>();

                    for (int i = 0; i < allTrackCategories.Count(); i++)
                    {
                        GhostList leaderboard = JsonConvert.DeserializeObject<GhostList>(await webClient.DownloadStringTaskAsync($"https://www.chadsoft.co.uk/time-trials{allTrackCategories[i].LeaderboardLink}?limit=1000"));

                        leaderboard.List.RemoveAll(x => !x.IsPB);
                        try
                        {
                            leaderboard.List.RemoveRange(10, leaderboard.List.Count() - 10);
                        }
                        catch
                        {
                            Console.WriteLine("Already <=10.");
                        }

                        leaderboard.List.OrderBy(x => x.Category);

                        List<DiscordEmbedBuilder> embeds = new List<DiscordEmbedBuilder>();

                        foreach (var ghost in leaderboard.List)
                        {
                            var ghostJson = await webClient.DownloadStringTaskAsync($"https://www.chadsoft.co.uk/time-trials{ghost.LinkContainer.Href.URL}");
                            ghost.Category = JsonConvert.DeserializeObject<Ghost>(ghostJson).Category;
                            ghost.CategoryName = JsonConvert.DeserializeObject<Ghost>(ghostJson).CategoryName;
                            ghost.ExtraInfo = JsonConvert.DeserializeObject<ExtraInfo>(ghostJson);
                            ghost.CategoryName = allTrackCategories.Find(x => x.Category == ghost.Category).CategoryName;

                            var embed = new DiscordEmbedBuilder
                            {
                                Color = new DiscordColor("#FF0000"),
                                Title = $"__**{leaderboard.List.FindIndex(x => x.LinkContainer.Href.URL == ghost.LinkContainer.Href.URL) + 1}) {trackList[trackIx].Name} - {ghost.CategoryName} {(cc == "" ? "(150cc)" : "(200cc)")}:**__",
                                Description = $"{ghost.ExtraInfo.MiiName}'s fastest time on {trackList[trackIx].Name}:\n\n" +
                                $"**Time:** {ghost.FinishTimeSimple}\n\n" +
                                $"**Splits:** {string.Join(" - ", ghost.ExtraInfo.SimpleSplits.ToArray())}\n\n" +
                                $"**Combo:** {Util.Characters[ghost.DriverID]} on {Util.Vehicles[ghost.VehicleID]}\n\n" +
                                $"**Date Set:** {ghost.DateSet.Split('T')[0]}\n\n" +
                                $"**Controller:**\n{Util.Controllers[ghost.ControllerID]}\n\n" +
                                $"**Extra Details:**\n" +
                                $"*Exact Finish Time: {ghost.FinishTime}*\n\n" +
                                $"*Exact Splits: {string.Join(" - ", ghost.ExtraInfo.Splits.ToArray())}*",
                                Url = $"https://www.chadsoft.co.uk/time-trials{ghost.LinkContainer.Href.URL.Substring(0, ghost.LinkContainer.Href.URL.Length - 4)}html",
                                Footer = new DiscordEmbedBuilder.EmbedFooter
                                {
                                    Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                                }
                            };

                            embeds.Add(embed);
                        }

                        categories.Add(embeds);
                    }

                    var messageBuilder = new DiscordWebhookBuilder().AddEmbed(categories[0][0]);

                    if (categories.Count() > 1)
                    {
                        messageBuilder.AddComponents(Util.GenerateCategorySelectMenu(allTrackCategories, 0));
                    }
                    if (categories[0].Count() > 1)
                    {
                        messageBuilder.AddComponents(Util.GeneratePageArrows(ctx));
                    }

                    var message = await ctx.EditResponseAsync(messageBuilder);

                    if (categories[0].Count() > 1)
                    {
                        PendingInteraction pending = new PendingInteraction() { CurrentPage = 0, CurrentCategory = 0, MessageId = message.Id, Context = ctx, Categories = categories, Pages = categories[0], CategoryNames = allTrackCategories };

                        Util.PendingInteractions.Add(pending);
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