using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using DSharpPlus.SlashCommands.EventArgs;
using MKBB.Class;
using MKBB.Data;

namespace MKBB
{
    public class Events
    {
        public async Task AssignAllEvents()
        {
            Bot.Client.MessageReactionAdded += HandlePendingPins;
            Bot.Client.MessageCreated += GeneratePendingPins;
            Bot.Client.GuildCreated += AddGuild;
            Bot.Client.GuildDeleted += RemoveGuild;
            Bot.SlashCommands.SlashCommandErrored += SlashCommandErrorHandler;
            await Task.CompletedTask;
        }

        private async Task HandlePendingPins(DiscordClient s, MessageReactionAddEventArgs e)
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
        }
        private async Task GeneratePendingPins(DiscordClient s, MessageCreateEventArgs e)
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
        }
        private async Task AddGuild(DiscordClient s, GuildCreateEventArgs e)
        {
            using MKBBContext dbCtx = new();
            dbCtx.Servers.Add(new ServerData()
            {
                Name = e.Guild.Name,
                ServerID = e.Guild.Id
            });
            await dbCtx.SaveChangesAsync();
        }
        private async Task RemoveGuild(DiscordClient s, GuildDeleteEventArgs e)
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
        }
        private async Task SlashCommandErrorHandler(SlashCommandsExtension s, SlashCommandErrorEventArgs e)
        {
            Bot.SlashCommands.SlashCommandErrored += async (s, e) =>
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
                            await e.Context.EditResponseAsync(new DiscordWebhookBuilder().WithContent("An internal error has occured. Please report this to <@105742694730457088> with details of the error."));
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
    }
}
