using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using Microsoft.Extensions.Logging;
using MKBB.Class;
using MKBB.Commands;
using MKBB.Data;
using Newtonsoft.Json;
using System.Text;

namespace MKBB
{
    public class Bot
    {
        public static DiscordClient Client { get; private set; }
        public CommandsNextExtension Commands { get; private set; }
        public SlashCommandsExtension SlashCommands { get; private set; }
        public InteractivityExtension Interactivity { get; private set; }

        private async Task Events()
        {
            Client.MessageReactionAdded += async (s, e) =>
            {
                foreach (var papr in Util.PendingAdminPinReactions)
                {
                    if (papr.Message.Id == e.Message.Id)
                    {
                        if (!e.User.IsBot)
                        {
                            if (e.Emoji.Id == 670020961491222531)
                            {
                                await papr.Message.ModifyAsync(
                                    (DiscordEmbed)new DiscordEmbedBuilder
                                    {
                                        Color = new DiscordColor("#FF0000"),
                                        Title = $"__**Pin Accepted:**__",
                                        Description = $"{papr.ThreadMessage.Channel.Name}" +
                                        $"\n{papr.ThreadMessage.JumpLink}",
                                        Footer = new DiscordEmbedBuilder.EmbedFooter
                                        {
                                            Text = $"Server Time: {DateTime.Now}"
                                        }
                                    }
                                );
                                await papr.ThreadMessage.PinAsync();
                            }
                            else
                            {
                                await papr.Message.ModifyAsync(
                                    (DiscordEmbed)new DiscordEmbedBuilder
                                    {
                                        Color = new DiscordColor("#FF0000"),
                                        Title = $"__**Pin Rejected:**__",
                                        Description = $"{papr.ThreadMessage.Channel.Name}" +
                                        $"\n{papr.ThreadMessage.JumpLink}",
                                        Footer = new DiscordEmbedBuilder.EmbedFooter
                                        {
                                            Text = $"Server Time: {DateTime.Now}"
                                        }
                                    }
                                );
                            }
                            foreach (var emoji in Util.GenerateTickAndCrossEmojis())
                            {
                                await papr.Message.DeleteReactionsEmojiAsync(emoji);
                            }
                            Util.PendingAdminPinReactions.Remove(papr);
                        }
                    }
                }
            };

            Client.MessageCreated += async (s, e) =>
            {
                if (e.Guild.Id == 180306609233330176 &&
                e.Channel.ParentId == 1046936322574655578 || e.Channel.ParentId == 369281592407097345)
                {
                    DiscordThreadChannel thread = (DiscordThreadChannel)e.Channel;
                    var opPost = thread.GetMessagesAsync(1).Result[0];
                    if (opPost.Attachments.Count > 0 &&
                    opPost.Attachments.Any(x => x.FileName.Contains(".szs")))
                    {
                        DiscordChannel channel = s.GetGuildAsync(180306609233330176).Result.GetChannel(1071555342829363281);
                        var message = await channel.SendMessageAsync(
                            new DiscordEmbedBuilder
                            {
                                Color = new DiscordColor("#FF0000"),
                                Title = $"__**New Pending Pin:**__",
                                Description = $"{e.Channel.Name}" +
                                $"\n{e.Message.JumpLink}",
                                Footer = new DiscordEmbedBuilder.EmbedFooter
                                {
                                    Text = $"Server Time: {DateTime.Now}"
                                }
                            }
                        );
                        foreach (var emoji in Util.GenerateTickAndCrossEmojis())
                        {
                            await message.CreateReactionAsync(emoji);
                        }
                        Util.PendingAdminPinReactions.Add(new PendingAdminPin { Message = message, ThreadMessage = e.Message });
                    }
                }
            };

            Client.GuildCreated += async (s, e) =>
                {
                    using var dbCtx = new MKBBContext();
                    List<ServerData> servers = dbCtx.Servers.ToList();
                    servers.Add(new ServerData()
                    {
                        Name = e.Guild.Name,
                        ServerID = e.Guild.Id
                    });
                    await dbCtx.SaveChangesAsync();
                };

            Client.GuildDeleted += async (s, e) =>
                    {
                        using var dbCtx = new MKBBContext();
                        List<ServerData> servers = dbCtx.Servers.ToList();
                        for (int i = 0; i < servers.Count; i++)
                        {
                            if (servers[i].ServerID == e.Guild.Id)
                            {
                                servers.RemoveAt(i);
                                break;
                            }
                        }
                        await dbCtx.SaveChangesAsync();
                    };

            SlashCommands.SlashCommandErrored += async (s, e) =>
            {
                if (e.Exception is SlashExecutionChecksFailedException slex)
                {
                    foreach (var check in slex.FailedChecks)
                    {
                        if (check is SlashRequireUserPermissionsAttribute rqu)
                        {
                            await e.Context.CreateResponseAsync($"Only members with {rqu.Permissions} can run this command!", true);
                        }
                        else if (check is SlashRequireOwnerAttribute rqo)
                        {
                            await e.Context.CreateResponseAsync($"Only the owner <@105742694730457088> can run this command!", true);
                        }
                        else
                        {
                            await e.Context.CreateResponseAsync("An internal error has occured. Please report this to <@105742694730457088> with details of the error.", true);
                        }
                    }
                }
                else
                {
                    Console.WriteLine(e.Exception);
                }
                await Task.CompletedTask;
            };

            await Task.CompletedTask;
        }

        private async Task Interactions()
        {
            Client.InteractionCreated += async (s, e) =>
            {
                DiscordChannel channel = Client.GetGuildAsync(180306609233330176).Result.GetChannel(1019149329556062278);

                string options = "";

                if (e.Interaction.Data.Options != null)
                {
                    foreach (var option in e.Interaction.Data.Options)
                    {
                        options += $" {option.Name}: *{option.Value}*";
                    }
                }

                var embed = new DiscordEmbedBuilder
                {
                    Color = new DiscordColor("#FF0000"),
                    Title = $"__**Notice:**__",
                    Description = $"'/{e.Interaction.Data.Name}{options}' was used by <@{e.Interaction.User.Id}> in {e.Interaction.Guild.Name}.",
                    Footer = new DiscordEmbedBuilder.EmbedFooter
                    {
                        Text = $"Server Time: {DateTime.Now}"
                    }
                };
                await channel.SendMessageAsync(embed);
            };

            Client.ComponentInteractionCreated += async (s, e) =>
            {
                foreach (var p in Util.PendingPageInteractions)
                {
                    if (e.Id == "rightButton")
                    {
                        if (e.Message.Id == p.MessageId)
                        {
                            p.CurrentPage = (p.CurrentPage + 1) % p.Pages.Count;
                            var responseBuilder = new DiscordInteractionResponseBuilder();
                            if (p.CategoryNames != null)
                            {
                                if (p.CategoryNames.Count != 1)
                                {
                                    responseBuilder.AddComponents(Util.GenerateCategorySelectMenu(p.CategoryNames, p.CurrentCategory));
                                }
                            }
                            responseBuilder.AddEmbed(p.Pages[p.CurrentPage]).AddComponents(Util.GeneratePageArrows());
                            await e.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, responseBuilder);
                        }
                    }
                    else if (e.Id == "leftButton")
                    {
                        if (e.Message.Id == p.MessageId)
                        {
                            p.CurrentPage = p.CurrentPage - 1 == -1 ? p.Pages.Count - 1 : p.CurrentPage - 1;
                            var responseBuilder = new DiscordInteractionResponseBuilder();
                            if (p.CategoryNames != null)
                            {
                                if (p.CategoryNames.Count != 1)
                                {
                                    responseBuilder.AddComponents(Util.GenerateCategorySelectMenu(p.CategoryNames, p.CurrentCategory));
                                }
                            }
                            responseBuilder.AddEmbed(p.Pages[p.CurrentPage]).AddComponents(Util.GeneratePageArrows());
                            await e.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, responseBuilder);
                        }
                    }
                    else if (e.Id == "category")
                    {
                        if (e.Message.Id == p.MessageId)
                        {
                            p.CurrentPage = 0;
                            p.CurrentCategory = p.CategoryNames.FindIndex(x => x.CategoryName == e.Values[0]);
                            p.Pages = p.Categories[p.CurrentCategory];

                            var responseBuilder = new DiscordInteractionResponseBuilder();
                            responseBuilder.AddComponents(Util.GenerateCategorySelectMenu(p.CategoryNames, p.CurrentCategory));
                            responseBuilder.AddEmbed(p.Pages[p.CurrentPage]).AddComponents(Util.GeneratePageArrows());
                            await e.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, responseBuilder);
                        }
                    }
                }
                foreach (var p in Util.PendingChannelConfigInteractions)
                {
                    if (e.Message.Id == p.MessageId)
                    {
                        using var dbCtx = new MKBBContext();
                        List<ServerData> servers = dbCtx.Servers.ToList();
                        var ids = "";
                        foreach (var value in e.Values)
                        {
                            ids += $"{value}, ";
                        }
                        ids = ids.Remove(ids.Length - 2, 2);
                        foreach (var server in servers)
                        {
                            if (server.ServerID == e.Guild.Id)
                            {
                                server.BotChannelIDs = ids;
                                break;
                            }
                        }
                        await dbCtx.SaveChangesAsync();
                        string channelsDisplay = "";
                        foreach (var id in ids.Split(',').ToList())
                        {
                            channelsDisplay += $"<#{id.Trim(' ')}>\n";
                        }

                        var embed = new DiscordEmbedBuilder
                        {
                            Color = new DiscordColor("#FF0000"),
                            Title = $"__**Success:**__",
                            Description = $"*The server's bot channels have been set to the following:*\n" +
                            channelsDisplay,
                            Footer = new DiscordEmbedBuilder.EmbedFooter
                            {
                                Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                            }
                        };

                        await e.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder().AddEmbed(embed));
                    }
                }
            };

            await Task.CompletedTask;
        }

        public async Task RunAsync()
        {
            var json = string.Empty;

            using (var fs = File.OpenRead("config.json"))
            using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                json = await sr.ReadToEndAsync().ConfigureAwait(false);

            var configJson = JsonConvert.DeserializeObject<ConfigJson>(json);

            var config = new DiscordConfiguration
            {
                Token = configJson.Token,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                MinimumLogLevel = LogLevel.Debug
            };

            Client = new DiscordClient(config);

            Client.Ready += OnClientReady;

            Client.UseInteractivity(new InteractivityConfiguration
            {
                PollBehaviour = PollBehaviour.KeepEmojis,
                AckPaginationButtons = true,
                Timeout = TimeSpan.FromSeconds(60)
            });

            var commandsConfig = new CommandsNextConfiguration
            {
                StringPrefixes = new string[] { configJson.Prefix },
                EnableDms = false,
                EnableDefaultHelp = false,
                DmHelp = true
            };

            SlashCommands = Client.UseSlashCommands();
#if DEBUG
            SlashCommands.RegisterCommands<Testing>(180306609233330176);
#endif
            SlashCommands.RegisterCommands<Config>();
            SlashCommands.RegisterCommands<TimeTrialManagement>();
            SlashCommands.RegisterCommands<TextCommands>();
            SlashCommands.RegisterCommands<Update>(180306609233330176);
            SlashCommands.RegisterCommands<Council>(180306609233330176);
            SlashCommands.RegisterCommands<Misc>(180306609233330176);
            SlashCommands.RegisterCommands<Info>();
            SlashCommands.RegisterCommands<Ghostbusters>(180306609233330176);

            await Events();

            await Interactions();

            DiscordActivity activity = new DiscordActivity();
            activity.Name = $"Bot is currently under maintenance. Please be patient :)";

            await Client.ConnectAsync(activity);

            Update update = new Update();

            await update.StartTimers(null);

            using var dbCtx = new MKBBContext();

            //await dbCtx.Database.EnsureCreatedAsync();

            //await dbCtx.SaveChangesAsync();

            await Task.Delay(-1);
        }

        private Task OnClientReady(DiscordClient sender, ReadyEventArgs e)
        {
            return Task.CompletedTask;
        }
    }
}
