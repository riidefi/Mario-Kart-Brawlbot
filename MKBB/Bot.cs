using MKBB.Commands;
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
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace MKBB
{
    public class Bot
    {
        public DiscordClient Client { get; private set; }
        public CommandsNextExtension Commands { get; private set; }
        public SlashCommandsExtension SlashCommands { get; private set; }
        public InteractivityExtension Interactivity { get; private set; }

        private async Task Events(SlashCommandsExtension slash)
        {
            slash.SlashCommandErrored += async (s, e) =>
            {
                if (e.Exception is SlashExecutionChecksFailedException slex)
                {
                    foreach (var check in slex.FailedChecks)
                    {
                        if (check is SlashRequireUserPermissionsAttribute att)
                        {
                            await e.Context.CreateResponseAsync($"Only members with {att.Permissions} can run this command!", true);
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

        private async Task Interactions(DiscordClient client)
        {
            client.InteractionCreated += async (s, e) =>
            {
                DiscordChannel channel = e.Interaction.Channel;

                foreach (var c in e.Interaction.Guild.Channels)
                {
                    if (c.Value.Id == 1019149329556062278)
                    {
                        channel = c.Value;
                    }
                }

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
                    Description = $"'/{e.Interaction.Data.Name}{options}' was used by <@{e.Interaction.User.Id}>.",
                    Footer = new DiscordEmbedBuilder.EmbedFooter
                    {
                        Text = $"Server Time: {DateTime.Now}"
                    }
                };
                await channel.SendMessageAsync(embed);
            };

            client.ComponentInteractionCreated += async (s, e) =>
            {
                foreach (var p in Util.PendingInteractions)
                {
                    if (e.Id == "rightButton")
                    {
                        if (e.Message.Id == p.MessageId)
                        {
                            p.CurrentPage = (p.CurrentPage + 1) % p.Pages.Count;
                            await e.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage,
                                new DiscordInteractionResponseBuilder()
                                .AddEmbed(p.Pages[p.CurrentPage])
                                .AddComponents(Util.GeneratePageArrows(Client)));
                        }
                    }
                    else if (e.Id == "leftButton")
                    {
                        if (e.Message.Id == p.MessageId)
                        {
                            p.CurrentPage = p.CurrentPage - 1 == -1 ? p.Pages.Count - 1 : p.CurrentPage - 1;
                            await e.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage,
                                new DiscordInteractionResponseBuilder()
                                .AddEmbed(p.Pages[p.CurrentPage])
                                .AddComponents(Util.GeneratePageArrows(Client)));
                        }
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

            //SlashCommands.RegisterCommands<Testing>();

            SlashCommands.RegisterCommands<TextCommands>();
            SlashCommands.RegisterCommands<Update>();
            SlashCommands.RegisterCommands<Homework>();
            SlashCommands.RegisterCommands<Misc>();
            SlashCommands.RegisterCommands<Info>();
            SlashCommands.RegisterCommands<Issues>();
            SlashCommands.RegisterCommands<Threads>();

            await Events(SlashCommands);

            await Interactions(Client);

            await Client.ConnectAsync();

            await Task.Delay(-1);
        }

        private Task OnClientReady(DiscordClient sender, ReadyEventArgs e)
        {
            return Task.CompletedTask;
        }
    }
}
