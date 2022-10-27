using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using FluentScheduler;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MKBB.Commands
{
    public class Ghostbusters : ApplicationCommandModule
    {
        [SlashCommand("addnewtrack", "Adds a new track for Ghostbusters to set times on.")]
        public async Task AddNewTrack(InteractionContext ctx,
            [Option("track-name", "The name of the track to add.")] string track,
            [Option("track-id", "The id of the track (also known as the file hash).")] string trackId)
        {

        }
    }
}