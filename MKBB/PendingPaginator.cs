using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using System.Collections.Generic;

namespace MKBB.Commands
{
    public class PendingPaginator
    {
        public ulong MessageId { get; set; }
        public InteractionContext Context { get; set; }
        public List<DiscordEmbedBuilder> Pages { get; set; }
        public int CurrentPage { get; set; }
    }
}