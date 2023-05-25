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
                foreach (PendingAdminPin papr in Util.PendingAdminPinReactions)
                {
                    if (papr.Message.Id == e.Message.Id)
                    {
                        if (!e.User.IsBot)
                        {
                            if (e.Emoji.Id == 1088583759844081664)
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
                            foreach (DiscordEmoji emoji in Util.GenerateTickAndCrossEmojis())
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
                    DiscordMessage opPost = thread.GetMessagesAsync(1).Result[0];
                    if (opPost.Attachments.Count > 0 &&
                    opPost.Attachments.Any(x => x.FileName.EndsWith(".szs")))
                    {
                        DiscordChannel channel = s.GetGuildAsync(180306609233330176).Result.GetChannel(1071555342829363281);
                        DiscordMessage message = await channel.SendMessageAsync(
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
                        foreach (DiscordEmoji emoji in Util.GenerateTickAndCrossEmojis())
                        {
                            await message.CreateReactionAsync(emoji);
                        }
                        Util.PendingAdminPinReactions.Add(new PendingAdminPin { Message = message, ThreadMessage = e.Message });
                    }
                }
            };

            Client.GuildCreated += async (s, e) =>
                {
                    using MKBBContext dbCtx = new();
                    dbCtx.Servers.Add(new ServerData()
                    {
                        Name = e.Guild.Name,
                        ServerID = e.Guild.Id
                    });
                    await dbCtx.SaveChangesAsync();
                };

            Client.GuildDeleted += async (s, e) =>
                    {
                        using MKBBContext dbCtx = new();
                        List<ServerData> servers = dbCtx.Servers.ToList();
                        for (int i = 0; i < servers.Count; i++)
                        {
                            if (servers[i].ServerID == e.Guild.Id)
                            {
                                dbCtx.Servers.Remove(servers[i]);
                                break;
                            }
                        }
                        await dbCtx.SaveChangesAsync();
                    };

            SlashCommands.SlashCommandErrored += async (s, e) =>
            {
                if (e.Exception is SlashExecutionChecksFailedException slex)
                {
                    foreach (SlashCheckBaseAttribute check in slex.FailedChecks)
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

        private static async Task Interactions()
        {
            Client.InteractionCreated += async (s, e) =>
            {
                DiscordChannel channel = Client.GetGuildAsync(1095401690120851558).Result.GetChannel(1095402077338996846);

                string options = "";

                if (e.Interaction.Data.Options != null)
                {
                    foreach (DiscordInteractionDataOption option in e.Interaction.Data.Options)
                    {
                        options += $" {option.Name}: *{option.Value}*";
                    }
                }

                DiscordEmbedBuilder embed = new()
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
                foreach (PendingPagesInteraction p in Util.PendingPageInteractions)
                {
                    if (e.Id == "rightButton")
                    {
                        if (e.Message.Id == p.MessageId)
                        {
                            p.CurrentPage = (p.CurrentPage + 1) % p.Pages.Count;
                            DiscordInteractionResponseBuilder responseBuilder = new();
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
                            DiscordInteractionResponseBuilder responseBuilder = new();
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

                            DiscordInteractionResponseBuilder responseBuilder = new();
                            responseBuilder.AddComponents(Util.GenerateCategorySelectMenu(p.CategoryNames, p.CurrentCategory));
                            responseBuilder.AddEmbed(p.Pages[p.CurrentPage]).AddComponents(Util.GeneratePageArrows());
                            await e.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, responseBuilder);
                        }
                    }
                }
                foreach (PendingChannelConfigInteraction p in Util.PendingChannelConfigInteractions)
                {
                    if (e.Message.Id == p.MessageId)
                    {
                        using MKBBContext dbCtx = new();
                        List<ServerData> servers = dbCtx.Servers.ToList();
                        string ids = "";
                        foreach (string value in e.Values)
                        {
                            ids += $"{value}, ";
                        }
                        ids = ids.Remove(ids.Length - 2, 2);
                        foreach (ServerData server in servers)
                        {
                            if (server.ServerID == e.Guild.Id)
                            {
                                server.BotChannelIDs = ids;
                                break;
                            }
                        }
                        await dbCtx.SaveChangesAsync();
                        string channelsDisplay = "";
                        foreach (string id in ids.Split(',').ToList())
                        {
                            channelsDisplay += $"<#{id.Trim(' ')}>\n";
                        }

                        DiscordEmbedBuilder embed = new()
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
            string json = string.Empty;

            using (FileStream fs = File.OpenRead("config.json"))
            using (StreamReader sr = new(fs, new UTF8Encoding(false)))
                json = await sr.ReadToEndAsync().ConfigureAwait(false);

            ConfigJson configJson = JsonConvert.DeserializeObject<ConfigJson>(json);

            DiscordConfiguration config = new()
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

            CommandsNextConfiguration commandsConfig = new()
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
            SlashCommands.RegisterCommands<Info>();

            SlashCommands.RegisterCommands<Update>(180306609233330176);
            SlashCommands.RegisterCommands<Council>(180306609233330176);
            SlashCommands.RegisterCommands<Misc>(180306609233330176);
            SlashCommands.RegisterCommands<Ghostbusters>(180306609233330176);

            await Events();

            await Interactions();

            DiscordActivity activity = new()
            {
                Name = $"Bot is currently under maintenance. Please be patient :)"
            };

            await Client.ConnectAsync(activity);

            Update update = new();

            await Update.StartTimers(null);

            using MKBBContext dbCtx = new();

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
