using DSharpPlus.CommandsNext;
using System;
using FluentScheduler;
using System.IO;
using Newtonsoft.Json;
using System.Text;
using IronPython.Runtime;
using DSharpPlus.Entities;
using System.Collections.Generic;
using DSharpPlus;
using static IronPython.Modules.PythonRegex;
using DSharpPlus.SlashCommands;
using System.Threading.Tasks;
using System.Threading.Channels;
using System.Runtime.Remoting.Channels;

namespace MKBB.Commands
{
    public class Util : ApplicationCommandModule
    {
        public enum CheckType : byte
        {
            STRING_MATCH = 0x0,
            INCOMPLETE_STRING = 0x1,
            ABBREVIATION = 0x2,
            LEVENSHTEIN = 0x3,
            NO_MATCH = 0x4
        }

        public static string councilUrl;

        public static char[] strAlpha = {
            (char)65,
            (char)66,
            (char)67,
            (char)68,
            (char)69,
            (char)70,
            (char)71,
            (char)72,
            (char)73,
            (char)74,
            (char)75,
            (char)76,
            (char)77,
            (char)78,
            (char)79,
            (char)80,
            (char)81,
            (char)82,
            (char)83,
            (char)84,
            (char)85,
            (char)86,
            (char)87,
            (char)88,
            (char)89,
            (char)90
        };

        public static Scrape Scraper = new Scrape();

        public static Registry ScheduleRegister = new Registry();

        public static bool CompareStrings(string arg1, string arg2)
        {
            if (arg1.Replace(".", string.Empty).Replace("_", " ").Replace("`", string.Empty).Replace("'", string.Empty).ToLowerInvariant() == arg2.Replace(".", string.Empty).Replace("_", " ").Replace("'", string.Empty).ToLowerInvariant())
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool CompareIncompleteStrings(string arg1, string arg2)
        {
            if (arg1.Replace(".", string.Empty).Replace("_", " ").Replace("`", string.Empty).Replace("'", string.Empty).ToLowerInvariant().Contains(arg2.Replace(".", string.Empty).Replace("_", " ").Replace("'", string.Empty).ToLowerInvariant()))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool CompareStringsLevenshteinDistance(string arg1, string arg2)
        {
            arg1 = arg1.ToLowerInvariant();
            arg2 = arg2.ToLowerInvariant();

            var bounds = new { Height = arg1.Length + 1, Width = arg2.Length + 1 };

            int[,] matrix = new int[bounds.Height, bounds.Width];

            for (int height = 0; height < bounds.Height; height++) { matrix[height, 0] = height; };
            for (int width = 0; width < bounds.Width; width++) { matrix[0, width] = width; };

            for (int height = 1; height < bounds.Height; height++)
            {
                for (int width = 1; width < bounds.Width; width++)
                {
                    int cost = (arg1[height - 1] == arg2[width - 1]) ? 0 : 1;
                    int insertion = matrix[height, width - 1] + 1;
                    int deletion = matrix[height - 1, width] + 1;
                    int substitution = matrix[height - 1, width - 1] + cost;

                    int distance = Math.Min(insertion, Math.Min(deletion, substitution));

                    if (height > 1 && width > 1 && arg1[height - 1] == arg2[width - 2] && arg1[height - 2] == arg2[width - 1])
                    {
                        distance = Math.Min(distance, matrix[height - 2, width - 2] + cost);
                    }

                    matrix[height, width] = distance;
                }
            }

            if (matrix[bounds.Height - 1, bounds.Width - 1] > 2)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public static bool CompareStringAbbreviation(string abbr, string full)
        {
            string[] fullArray = full.Split(' ');

            string abbreviation = string.Empty;

            if (full.Contains(" ") && (fullArray[0] == "SNES" ||
                fullArray[0] == "N64" ||
                fullArray[0] == "GBA" ||
                fullArray[0] == "GCN" ||
                fullArray[0] == "DS" ||
                fullArray[0] == "3DS"))
            {
                abbreviation = $"{fullArray[0]} ";
                fullArray[0] = "";
            }

            for (int i = 0; i < fullArray.Length; i++)
            {
                if (fullArray[i].Length != 0)
                {
                    abbreviation += fullArray[i].Substring(0, 1);
                }
            }
            if (abbreviation.ToLowerInvariant() == abbr.ToLowerInvariant())
            {
                return true;
            }

            return false;
        }

        public static string RankNumber(string number)
        {
            if (number.Length > 1 && number[number.Length - 2] == '1')
            {
                return number + "th";
            }
            else if (number[number.Length - 1] == '1')
            {
                return number + "st";
            }
            else if (number[number.Length - 1] == '2')
            {
                return number + "nd";
            }
            else if (number[number.Length - 1] == '3')
            {
                return number + "rd";
            }
            return number + "th";
        }

        public static string GetCouncilUrl()
        {
            using (var fs = File.OpenRead("config.json"))
            using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                return JsonConvert.DeserializeObject<ConfigJson>(sr.ReadToEnd()).CouncilUrl;
        }

        public static List<CouncilMember> GetCouncilJsonList()
        {
            string json;
            using (var fs = File.OpenRead("council.json"))
            using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                json = sr.ReadToEnd();
            return JsonConvert.DeserializeObject<List<CouncilMember>>(json);
        }

        public static bool CheckItemInList(string item1, string item2)
        {
            return true;
        }

        public static DiscordLinkButtonComponent GetBackroomLinkButton()
        {
            return new DiscordLinkButtonComponent("https://docs.google.com/spreadsheets/d/1xwhKoyypCWq5tCRTI69ijJoDiaoAVsvYAxz-q4UBNqM/edit#gid=1751905284", "Backroom Page");
        }

        public static DiscordLinkButtonComponent GetTestingNotesLinkButton()
        {
            return new DiscordLinkButtonComponent("https://docs.google.com/spreadsheets/d/19CF8UP1ubGfkM31uHha6bxYFENnQ-k7L7AEtozENnvk/edit#gid=1807035740", "Testing Notes Page");
        }

        public static DiscordLinkButtonComponent GetCouncilLinkButton()
        {
            return new DiscordLinkButtonComponent(GetCouncilUrl(), "Council Page");
        }

        public static DiscordButtonComponent[] GeneratePageArrows(InteractionContext ctx)
        {
            var leftButton = new DiscordButtonComponent(ButtonStyle.Primary, "leftButton", "", false, new DiscordComponentEmoji(DiscordEmoji.FromName(ctx.Client, ":arrow_backward:")));
            var rightButton = new DiscordButtonComponent(ButtonStyle.Primary, "rightButton", "", false, new DiscordComponentEmoji(DiscordEmoji.FromName(ctx.Client, ":arrow_forward:")));

            return new DiscordButtonComponent[] { leftButton, rightButton };
        }

        public static DiscordButtonComponent[] GeneratePageArrows(DiscordClient client)
        {
            var leftButton = new DiscordButtonComponent(ButtonStyle.Primary, "leftButton", "", false, new DiscordComponentEmoji(DiscordEmoji.FromName(client, ":arrow_backward:")));
            var rightButton = new DiscordButtonComponent(ButtonStyle.Primary, "rightButton", "", false, new DiscordComponentEmoji(DiscordEmoji.FromName(client, ":arrow_forward:")));

            return new DiscordButtonComponent[] { leftButton, rightButton };
        }

        public static List<PendingPaginator> PendingInteractions = new List<PendingPaginator>();

        public static int ListNameCheck(List<Track> list, string comparer, int startIx = 0)
        {
            int index = -1;
            CheckType type = CheckType.NO_MATCH;
            for (int i = startIx; i < list.Count; i++)
            {
                if (CompareStrings(list[i].Name, comparer))
                {
                    type = CheckType.STRING_MATCH;
                    index = i;
                    break;
                }
                else if (CompareIncompleteStrings(list[i].Name, comparer))
                {
                    if (type > CheckType.INCOMPLETE_STRING)
                    {
                        type = CheckType.INCOMPLETE_STRING;
                        index = i;
                    }
                }
                else if (CompareStringAbbreviation(comparer, list[i].Name))
                {
                    if (type > CheckType.ABBREVIATION)
                    {
                        type = CheckType.ABBREVIATION;
                        index = i;
                    }
                }
                else if (CompareStringsLevenshteinDistance(list[i].Name, comparer))
                {
                    if (type == CheckType.NO_MATCH)
                    {
                        type = CheckType.LEVENSHTEIN;
                        index = i;
                    }
                }
            }
            return index;
        }

        public static int ListNameCheck(IList<object> list, string comparer, int startIx = 0)
        {
            int index = -1;
            CheckType type = CheckType.NO_MATCH;
            for (int i = startIx; i < list.Count; i++)
            {
                if (CompareStrings(list[i].ToString(), comparer))
                {
                    type = CheckType.STRING_MATCH;
                    index = i;
                    break;
                }
                else if (CompareIncompleteStrings(list[i].ToString(), comparer))
                {
                    if (type > CheckType.INCOMPLETE_STRING)
                    {
                        type = CheckType.INCOMPLETE_STRING;
                        index = i;
                    }
                }
                else if (CompareStringAbbreviation(comparer, list[i].ToString()))
                {
                    if (type > CheckType.ABBREVIATION)
                    {
                        type = CheckType.ABBREVIATION;
                        index = i;
                    }
                }
                else if (CompareStringsLevenshteinDistance(list[i].ToString(), comparer))
                {
                    if (type == CheckType.NO_MATCH)
                    {
                        type = CheckType.LEVENSHTEIN;
                        index = i;
                    }
                }
            }
            return index;
        }

        public static int ListNameCheck(IList<IList<object>> list, string comparer, int startIx = 0, int ix1 = -1, int ix2 = -1)
        {
            int index = -1;
            CheckType type = CheckType.NO_MATCH;
            if (ix1 > -1)
            {
                for (int i = startIx; i < list.Count; i++)
                {
                    if (CompareStrings(list[ix1][i].ToString(), comparer))
                    {
                        type = CheckType.STRING_MATCH;
                        index = i;
                        break;
                    }
                    else if (CompareIncompleteStrings(list[ix1][i].ToString(), comparer))
                    {
                        if (type > CheckType.INCOMPLETE_STRING)
                        {
                            type = CheckType.INCOMPLETE_STRING;
                            index = i;
                        }
                    }
                    else if (CompareStringAbbreviation(comparer, list[ix1][i].ToString()))
                    {
                        if (type > CheckType.ABBREVIATION)
                        {
                            type = CheckType.ABBREVIATION;
                            index = i;
                        }
                    }
                    else if (CompareStringsLevenshteinDistance(list[ix1][i].ToString(), comparer))
                    {
                        if (type == CheckType.NO_MATCH)
                        {
                            type = CheckType.LEVENSHTEIN;
                            index = i;
                        }
                    }
                }
            }
            else if (ix2 > -1)
            {
                for (int i = startIx; i < list.Count; i++)
                {
                    if (CompareStrings(list[i][ix2].ToString(), comparer))
                    {
                        type = CheckType.STRING_MATCH;
                        index = i;
                        break;
                    }
                    else if (CompareIncompleteStrings(list[i][ix2].ToString(), comparer))
                    {
                        if (type > CheckType.INCOMPLETE_STRING)
                        {
                            type = CheckType.INCOMPLETE_STRING;
                            index = i;
                        }
                    }
                    else if (CompareStringAbbreviation(comparer, list[i][ix2].ToString()))
                    {
                        if (type > CheckType.ABBREVIATION)
                        {
                            type = CheckType.ABBREVIATION;
                            index = i;
                        }
                    }
                    else if (CompareStringsLevenshteinDistance(list[i][ix2].ToString(), comparer))
                    {
                        if (type == CheckType.NO_MATCH)
                        {
                            type = CheckType.LEVENSHTEIN;
                            index = i;
                        }
                    }
                }
            }
            return index;
        }

        public static async Task ThrowError(InteractionContext ctx, Exception ex)
        {
            var embed = new DiscordEmbedBuilder
            {
                Color = new DiscordColor("#FF0000"),
                Title = $"__**Error:**__",
                Description = $"*{ex.Message}*",
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                }
            };
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));

            DiscordChannel channel = ctx.Channel;

            foreach (var c in ctx.Guild.Channels)
            {
                if (c.Value.Id == 1019149329556062278)
                {
                    channel = c.Value;
                }
            }

            string options = "";

            if (ctx.Interaction.Data.Options != null)
            {
                foreach (var option in ctx.Interaction.Data.Options)
                {
                    options += $" {option.Name}: *{option.Value}*";
                }
            }

            embed = new DiscordEmbedBuilder
            {
                Color = new DiscordColor("#FF0000"),
                Title = $"__**Error:**__",
                Description = $"'/{ctx.Interaction.Data.Name}{options}' was used by <@{ctx.User.Id}>." +
                $"\n\n{ex}",
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = $"Server Time: {DateTime.Now}"
                }
            };
            await channel.SendMessageAsync(embed);

            Console.WriteLine(ex.ToString());
        }

        public static async Task ThrowInteractionlessError(InteractionContext ctx, Exception ex)
        {
            DiscordChannel channel = ctx.Channel;

            foreach (var c in ctx.Guild.Channels)
            {
                if (c.Value.Id == 1019149329556062278)
                {
                    channel = c.Value;
                }
            }

            var embed = new DiscordEmbedBuilder
            {
                Color = new DiscordColor("#FF0000"),
                Title = $"__**Error:**__",
                Description = $"\n\n{ex}",
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = $"Server Time: {DateTime.Now}"
                }
            };
            await channel.SendMessageAsync(embed);
        }
    }
}
