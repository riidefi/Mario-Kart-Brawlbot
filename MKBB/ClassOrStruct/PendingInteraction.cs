using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using System.Collections.Generic;

namespace MKBB.Commands
{
    public class PendingPagesInteraction
    {
        public ulong MessageId { get; set; }
        public InteractionContext Context { get; set; }
        public List<List<DiscordEmbedBuilder>> Categories { get; set; }
        public List<DiscordEmbedBuilder> Pages { get; set; }
        public List<Track> CategoryNames { get; set; }
        public int CurrentCategory { get; set; }
        public int CurrentPage { get; set; }
    }
    public class PendingChannelConfigInteraction
    {
        public ulong MessageId { get; set; }
        public InteractionContext Context { get; set; }
    }
    public class PendingAdminPin
    {
        public DiscordMessage Message { get; set; }
        public DiscordMessage ThreadMessage { get; set; }
    }
}