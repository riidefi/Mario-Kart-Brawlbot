using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using MKBB.Class;
using MKBB.Data;
using Newtonsoft.Json;
using static Google.Apis.Sheets.v4.SpreadsheetsResource.ValuesResource;
using System.Net;
using System.Runtime.InteropServices.JavaScript;
using System.Xml.Linq;

namespace MKBB
{
    public class Interactions
    {
        public async Task AssignAllInteractions()
        {
            Bot.Client.InteractionCreated += LogInteractions;

            Bot.Client.ComponentInteractionCreated += async (s, e) =>
            {
                foreach (PendingPagesInteraction p in Util.PendingPageInteractions)
                {
                    if (e.Id == "leftButton") await PressLeftButton(s, e, p);
                    if (e.Id == "rightButton") await PressRightButton(s, e, p);
                    if (e.Id == "category") await OnCategoryChange(s, e, p);
                }
                foreach (PendingChannelConfigInteraction p in Util.PendingChannelConfigInteractions)
                {
                    await OnBotChannelChange(s, e, p);
                };
                if (e.Id == "gbsubmission")
                {
                    await GBModal(s, e);
                }
            };

            Bot.Client.ModalSubmitted += async (s, e) =>
            {
                if (e.Interaction.Data.CustomId == "gbsubmissionmodal") await GBModalSubmit(s, e);
            };

            await Task.CompletedTask;
        }
        private async Task LogInteractions(DiscordClient s, InteractionCreateEventArgs e)
        {
            DiscordChannel channel = Bot.Client.GetGuildAsync(1095401690120851558).Result.GetChannel(1095402077338996846);

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
        }
        private async Task PressLeftButton(DiscordClient s, ComponentInteractionCreateEventArgs e, PendingPagesInteraction p)
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
        private async Task PressRightButton(DiscordClient s, ComponentInteractionCreateEventArgs e, PendingPagesInteraction p)
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
        private async Task OnCategoryChange(DiscordClient s, ComponentInteractionCreateEventArgs e, PendingPagesInteraction p)
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
        private async Task OnBotChannelChange(DiscordClient s, ComponentInteractionCreateEventArgs e, PendingChannelConfigInteraction p)
        {

            if (e.Message.Id == p.MessageId)
            {
                using MKBBContext dbCtx = new();
                List<ServerData> servers = dbCtx.Servers.ToList();
                string ids = "";
                foreach (string value in e.Values)
                {
                    ids += $"{value},,";
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
                    channelsDisplay += $"<#{id}>\n";
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
        private async Task GBModal(DiscordClient s, ComponentInteractionCreateEventArgs e)
        {
            await e.Interaction.CreateResponseAsync(InteractionResponseType.Modal, new DiscordInteractionResponseBuilder()
                .WithTitle("Ghostbusters Submission")
                .WithCustomId("gbsubmissionmodal")
                .AddComponents(new TextInputComponent(label: "Chadsoft Link", customId: "link", placeholder: "https://chadsoft.co.uk/time-trials/rkgd/60/BD/59C361AB44B3542B99349F815D3AA5090758.html", min_length: 87, max_length: 87))
                .AddComponents(new TextInputComponent(label: "Comments", customId: "comments", placeholder: "", required: false, style: TextInputStyle.Paragraph))
            );
        }
        private async Task GBModalSubmit(DiscordClient s, ModalSubmitEventArgs e)
        {
            await e.Interaction.DeferAsync(true);

            string ghostUrl = e.Values.First(x => x.Key == "link").Value;
            DiscordUser user = e.Interaction.User;
            string comments = e.Values.First(x => x.Key == "comments").Value;

            if (ghostUrl.Contains("rkg"))
            {
                WebClient webClient = new();
                Ghost ghostData = new();
                try
                {
                    ghostData = JsonConvert.DeserializeObject<Ghost>(await webClient.DownloadStringTaskAsync(ghostUrl[..^4] + "json"));
                }
                catch
                {
                    DiscordEmbedBuilder embed = new()
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = $"__*Error:*__",
                        Description = "*Invalid ghost link.*",
                        Footer = new DiscordEmbedBuilder.EmbedFooter
                        {
                            Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                        }
                    };
                    await e.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
                    return;
                }

                bool found = false;
                using MKBBContext dbCtx = new();

                foreach (var track in dbCtx.GBTracks)
                {
                    if (track.SHA1s.Contains(ghostData.TrackID))
                    {
                        found = true;
                        dbCtx.GBTimes.Add(new()
                        {
                            TrackSHA1 = ghostData.TrackID,
                            User = user.Mention,
                            URL = ghostUrl,
                            Comments = comments != null ? comments : null
                        });
                        await dbCtx.SaveChangesAsync();
                        break;
                    }
                }

                if (found)
                {

                    await e.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().AddEmbed(new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = $"__*Success:*__",
                        Description = $"*The time was added successfully.*",
                        Footer = new DiscordEmbedBuilder.EmbedFooter
                        {
                            Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                        }
                    }));
                }
                else
                {

                    await e.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().AddEmbed(new DiscordEmbedBuilder
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
                await e.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().AddEmbed(new DiscordEmbedBuilder
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
    }
}
