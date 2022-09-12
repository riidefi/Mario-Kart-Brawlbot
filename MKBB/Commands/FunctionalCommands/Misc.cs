using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace MKBB.Commands
{
    public class Misc : ApplicationCommandModule
    {
        [SlashCommand("createtest", "Creates a test back for CTTP for CTGP track tests.")]
        [SlashRequireUserPermissions(Permissions.Administrator)]
        public async Task CreateTestPack(InteractionContext ctx)
        {
            try
            {
                await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder() { IsEphemeral = true });

                string serviceAccountEmail = "brawlbox@custom-track-testing-bot.iam.gserviceaccount.com";

                var certificate = new X509Certificate2(@"key.p12", "notasecret", X509KeyStorageFlags.Exportable);

                ServiceAccountCredential credential = new ServiceAccountCredential(
                   new ServiceAccountCredential.Initializer(serviceAccountEmail).FromCertificate(certificate));

                var service = new SheetsService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "Mario Kart Brawlbot",
                });

                var request = service.Spreadsheets.Values.Get("19CF8UP1ubGfkM31uHha6bxYFENnQ-k7L7AEtozENnvk", "'Track Pack Downloads'");
                var responseRaw = await request.ExecuteAsync();
                request.ValueRenderOption = SpreadsheetsResource.ValuesResource.GetRequest.ValueRenderOptionEnum.FORMULA;
                var response = await request.ExecuteAsync();

                foreach (var v in response.Values)
                {
                    while (v.Count < 10)
                    {
                        v.Add("");
                    }
                }

                string[] courseStrings = new[] { "Luigi Circuit",
            "Moo Moo Meadows",
            "Mushroom Gorge",
            "Toad's Factory",
            "Mario Circuit",
            "Coconut Mall",
            "DK Summit",
            "Wario's Gold Mine",
            "Daisy Circuit",
            "Koopa Cape",
            "Maple Treeway",
            "Grumble Volcano",
            "Dry Dry Ruins",
            "Moonview Highway",
            "Bowser's Castle",
            "Rainbow Road",
            "GCN Peach Beach",
            "DS Yoshi Falls",
            "SNES Ghost Valley 2",
            "N64 Mario Raceway",
            "N64 Sherbet Land",
            "GBA Shy Guy Beach",
            "DS Delfino Square",
            "GCN Waluigi Stadium",
            "DS Desert Hills",
            "GBA Bowser Castle 3",
            "N64 DK's Jungle Parkway",
            "GCN Mario Circuit",
            "SNES Mario Circuit 3",
            "DS Peach Gardens",
            "GCN DK Mountain",
            "N64 Bowser's Castle",
            "Block Plaza",
            "Delfino Pier",
            "Funky Stadium",
            "Chain Chomp Wheel",
            "Thwomp Desert",
            "SNES Battle Course 4",
            "GBA Battle Course 3",
            "N64 Skyscraper",
            "GCN Cookie Land",
            "DS Twilight House" };
                string[] courseMusicIds = new[] { "117",
            "119",
            "121",
            "123",
            "125",
            "127",
            "129",
            "131",
            "135",
            "133",
            "143",
            "139",
            "137",
            "141",
            "145",
            "147",
            "165",
            "173",
            "151",
            "159",
            "157",
            "149",
            "175",
            "169",
            "177",
            "155",
            "161",
            "167",
            "153",
            "179",
            "171",
            "163",
            "33",
            "32",
            "35",
            "34",
            "36",
            "39",
            "40",
            "41",
            "37",
            "38"};
                string[] courseSlotIds = new[] { "T11",
            "T12",
            "T13",
            "T14",
            "T21",
            "T22",
            "T23",
            "T24",
            "T31",
            "T32",
            "T33",
            "T34",
            "T41",
            "T42",
            "T43",
            "T44",
            "T51",
            "T52",
            "T53",
            "T54",
            "T61",
            "T62",
            "T63",
            "T64",
            "T71",
            "T72",
            "T73",
            "T74",
            "T81",
            "T82",
            "T83",
            "T84" };

                int[] trackSlots = new int[12];
                int[] musicSlots = new int[12];

                Directory.CreateDirectory(@"workdir/");
                Directory.CreateDirectory(@"workdir/input");
                Directory.CreateDirectory(@"workdir/output");
                Directory.CreateDirectory(@"output/");
                Directory.CreateDirectory(@"output/CTTP");
                Directory.CreateDirectory(@"output/CTTP/rel");
                Directory.CreateDirectory(@"output/CTTP/Scene");
                Directory.CreateDirectory(@"output/CTTP/Scene/YourMom");

                for (int i = 3; i < 15; i++)
                {
                    WebClient webClient = new WebClient();
                    await webClient.DownloadFileTaskAsync(response.Values[i][5].ToString().Split('"')[1], $@"workdir/input/{response.Values[i][2].ToString().Replace("???", "Secret")}.szs");
                    for (int j = 0; j < courseStrings.Length; j++)
                    {
                        if (response.Values[i][6].ToString().Split('/')[0].Remove(response.Values[i][6].ToString().Split('/')[0].Length - 1) == courseStrings[j])
                        {
                            trackSlots[i - 3] = j + 1;
                        }
                        if (response.Values[i][6].ToString().Split('/')[1].Substring(1) == courseStrings[j])
                        {
                            musicSlots[i - 3] = j + 1;
                        }
                        else if (musicSlots[i - 3] == 0)
                        {
                            musicSlots[i - 3] = trackSlots[i - 3];
                        }
                    }
                }

                List<CupInfo> cups = new List<CupInfo>(3);
                for (int i = 0; i < 3; i++)
                {
                    cups.Add(new CupInfo(response.Values[3 + i * 4][2].ToString(),
                        response.Values[4 + i * 4][2].ToString(),
                        response.Values[5 + i * 4][2].ToString(),
                        response.Values[6 + i * 4][2].ToString(),
                        response.Values[3 + i * 4][2].ToString(),
                        response.Values[4 + i * 4][2].ToString(),
                        response.Values[5 + i * 4][2].ToString(),
                        response.Values[6 + i * 4][2].ToString(),
                        trackSlots[0 + i * 4],
                        trackSlots[1 + i * 4],
                        trackSlots[2 + i * 4],
                        trackSlots[3 + i * 4],
                        musicSlots[0 + i * 4],
                        musicSlots[1 + i * 4],
                        musicSlots[2 + i * 4],
                        musicSlots[3 + i * 4]));
                }

                string fileName = @"workdir/config.txt";

                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }
                using (StreamWriter streamwriter = File.CreateText(fileName))
                {
                    streamwriter.WriteLine("#CT-CODE\n\n");
                    streamwriter.WriteLine("[RACING-TRACK-LIST]\n");
                    streamwriter.WriteLine("%LE-FLAGS  = 1\n");
                    streamwriter.WriteLine("%WIIMM-CUP = 0\n");
                    streamwriter.WriteLine("N N$SWAP | N$F_WII");

                    for (int i = 0; i < cups.Count; i++)
                    {
                        streamwriter.WriteLine($"\nC {i + 1} # {i + 12}");
                        streamwriter.WriteLine($"T {courseMusicIds[Convert.ToInt32(cups[i].Track1Music) - 1]}; {courseSlotIds[Convert.ToInt32(cups[i].Track1Slot) - 1]}; 0x00; {cups[i].Track1Name.Replace("???", "Secret")}; {cups[i].Track1Name}; ");
                        streamwriter.WriteLine($"T {courseMusicIds[Convert.ToInt32(cups[i].Track2Music) - 1]}; {courseSlotIds[Convert.ToInt32(cups[i].Track2Slot) - 1]}; 0x00; {cups[i].Track2Name.Replace("???", "Secret")}; {cups[i].Track2Name}; ");
                        streamwriter.WriteLine($"T {courseMusicIds[Convert.ToInt32(cups[i].Track3Music) - 1]}; {courseSlotIds[Convert.ToInt32(cups[i].Track3Slot) - 1]}; 0x00; {cups[i].Track3Name.Replace("???", "Secret")}; {cups[i].Track3Name}; ");
                        streamwriter.WriteLine($"T {courseMusicIds[Convert.ToInt32(cups[i].Track4Music) - 1]}; {courseSlotIds[Convert.ToInt32(cups[i].Track4Slot) - 1]}; 0x00; {cups[i].Track4Name.Replace("???", "Secret")}; {cups[i].Track4Name}; ");

                    }
                    streamwriter.Close();
                }

                File.WriteAllBytes(@"workdir/lecode-JAP.bin", Properties.Resources.lecode_JAP);
                File.WriteAllBytes(@"workdir/lecode-USA.bin", Properties.Resources.lecode_USA);
                File.WriteAllBytes(@"workdir/lecode-PAL.bin", Properties.Resources.lecode_PAL);

                File.WriteAllBytes(@"workdir/cygattr-1.dll", Properties.Resources.cygattr_1);
                File.WriteAllBytes(@"workdir/cygcrypto-1.1.dll", Properties.Resources.cygcrypto_1_1);
                File.WriteAllBytes(@"workdir/cygiconv-2.dll", Properties.Resources.cygiconv_2);
                File.WriteAllBytes(@"workdir/cygintl-8.dll", Properties.Resources.cygintl_8);
                File.WriteAllBytes(@"workdir/cygncursesw-10.dll", Properties.Resources.cygncursesw_10);
                File.WriteAllBytes(@"workdir/cygpcre-1.dll", Properties.Resources.cygpcre_1);
                File.WriteAllBytes(@"workdir/cygpng16-16.dll", Properties.Resources.cygpng16_16);
                File.WriteAllBytes(@"workdir/cygreadline7.dll", Properties.Resources.cygreadline7);
                File.WriteAllBytes(@"workdir/cygwin1.dll", Properties.Resources.cygwin1);
                File.WriteAllBytes(@"workdir/cygz.dll", Properties.Resources.cygz);

                File.WriteAllBytes(@"workdir/MenuSingle_E_reg.szs", Properties.Resources.MenuSingle_E_reg);
                File.WriteAllBytes(@"workdir/MenuSingle_E_mom.szs", Properties.Resources.MenuSingle_E_mom);

                var processInfo = new ProcessStartInfo();
                processInfo.FileName = @"wszst";
                processInfo.Arguments = @"extract MenuSingle_E_reg.szs";
                processInfo.WorkingDirectory = @"workdir/";
                processInfo.CreateNoWindow = true;
                processInfo.WindowStyle = ProcessWindowStyle.Hidden;
                processInfo.UseShellExecute = false;

                var process = new Process();
                process.StartInfo = processInfo;
                process.Start();

                process.WaitForExit();

                processInfo = new ProcessStartInfo();
                processInfo.FileName = @"wbmgt";
                processInfo.Arguments = @"decode Common.bmg";
                processInfo.WorkingDirectory = @"workdir/MenuSingle_E_reg.d/message/";
                processInfo.CreateNoWindow = true;
                processInfo.WindowStyle = ProcessWindowStyle.Hidden;
                processInfo.UseShellExecute = false;

                process = new Process();
                process.StartInfo = processInfo;
                process.Start();

                process.WaitForExit();

                StreamWriter sw = new StreamWriter(@"workdir/MenuSingle_E_reg.d/message/Common.txt", true);
                sw.WriteLine("\n\n#--- [7000:7fff] LE-CODE: track names");
                sw.WriteLine(@"  7000	= \c{blue1}Wii \c{off}Mario Circuit");
                sw.WriteLine(@"  7001	= \c{blue1}Wii \c{off}Moo Moo Meadows");
                sw.WriteLine(@"  7002	= \c{blue1}Wii \c{off}Mushroom Gorge");
                sw.WriteLine(@"  7003	= \c{blue1}Wii \c{off}Grumble Volcano");
                sw.WriteLine(@"  7004	= \c{blue1}Wii \c{off}Toad's Factory");
                sw.WriteLine(@"  7005	= \c{blue1}Wii \c{off}Coconut Mall");
                sw.WriteLine(@"  7006	= \c{blue1}Wii \c{off}DK's Snowboard Cross");
                sw.WriteLine(@"  7007	= \c{blue1}Wii \c{off}Wario's Gold Mine");
                sw.WriteLine(@"  7008	= \c{blue1}Wii \c{off}Luigi Circuit");
                sw.WriteLine(@"  7009	= \c{blue1}Wii \c{off}Daisy Circuit");
                sw.WriteLine(@"  700a	= \c{blue1}Wii \c{off}Moonview Highway");
                sw.WriteLine(@"  700b	= \c{blue1}Wii \c{off}Maple Treeway");
                sw.WriteLine(@"  700c	= \c{blue1}Wii \c{off}Bowser's Castle");
                sw.WriteLine(@"  700d	= \c{blue1}Wii \c{off}Rainbow Road");
                sw.WriteLine(@"  700e	= \c{blue1}Wii \c{off}Dry Dry Ruins");
                sw.WriteLine(@"  700f	= \c{blue1}Wii \c{off}Koopa Cape");
                sw.WriteLine(@"  7010	= \c{blue1}GCN \c{off}Peach Beach");
                sw.WriteLine(@"  7011	= \c{blue1}GCN \c{off}Mario Circuit");
                sw.WriteLine(@"  7012	= \c{blue1}GCN \c{off}Waluigi Stadium");
                sw.WriteLine(@"  7013	= \c{blue1}GCN \c{off}DK Mountain");
                sw.WriteLine(@"  7014	= \c{blue1}DS \c{off}Yoshi Falls");
                sw.WriteLine(@"  7015	= \c{blue1}DS \c{off}Desert Hills");
                sw.WriteLine(@"  7016	= \c{blue1}DS \c{off}Peach Gardens");
                sw.WriteLine(@"  7017	= \c{blue1}DS \c{off}Delfino Square");
                sw.WriteLine(@"  7018	= \c{blue1}SNES \c{off}Mario Circuit 3");
                sw.WriteLine(@"  7019	= \c{blue1}SNES \c{off}Ghost Valley 2");
                sw.WriteLine(@"  701a	= \c{blue1}N64 \c{off}Mario Raceway");
                sw.WriteLine(@"  701b	= \c{blue1}N64 \c{off}Sherbet Land");
                sw.WriteLine(@"  701c	= \c{blue1}N64 \c{off}Bowser's Castle");
                sw.WriteLine(@"  701d	= \c{blue1}N64 \c{off}DK's Jungle Parkway");
                sw.WriteLine(@"  701e	= \c{blue1}GBA \c{off}Bowser Castle 3");
                sw.WriteLine(@"  701f	= \c{blue1}GBA \c{off}Shy Guy Beach");
                sw.WriteLine(@"  7020	= \c{yor7}-----\c{off}");
                sw.WriteLine(@"  7021	= \c{yor7}-----\c{off}");
                sw.WriteLine(@"  7022	= \c{yor7}-----\c{off}");
                sw.WriteLine(@"  7023	= \c{yor7}-----\c{off}");
                sw.WriteLine(@"  7024	= \c{yor7}-----\c{off}");
                sw.WriteLine(@"  7025	= \c{yor7}-----\c{off}");
                sw.WriteLine(@"  7026	= \c{yor7}-----\c{off}");
                sw.WriteLine(@"  7027	= \c{yor7}-----\c{off}");
                sw.WriteLine(@"  7028	= \c{yor7}-----\c{off}");
                sw.WriteLine(@"  7029	= \c{yor7}-----\c{off}");
                sw.WriteLine(@"  702a	/");
                sw.WriteLine(@"  702b	/");
                sw.WriteLine(@"  702c	/");
                sw.WriteLine(@"  702d	/");
                sw.WriteLine(@"  702e	/");
                sw.WriteLine(@"  702f	/");
                sw.WriteLine(@"  7030	/");
                sw.WriteLine(@"  7031	/");
                sw.WriteLine(@"  7032	/");
                sw.WriteLine(@"  7033	/");
                sw.WriteLine(@"  7034	/");
                sw.WriteLine(@"  7035	/");
                sw.WriteLine(@"  7036	= Ring Mission");
                sw.WriteLine(@"  7037	= Winningrun Demo");
                sw.WriteLine(@"  7038	= Loser Demo");
                sw.WriteLine(@"  7039	= Draw Demo");
                sw.WriteLine(@"  703a	= Ending Demo");
                sw.WriteLine(@"  703b	/");
                sw.WriteLine(@"  703c	/");
                sw.WriteLine(@"  703d	/");
                sw.WriteLine(@"  703e	/");
                sw.WriteLine(@"  703f	/");
                sw.WriteLine(@"  7040	/");
                sw.WriteLine(@"  7041	/");
                sw.WriteLine(@"  7042	/");
                sw.WriteLine(@"  7043	=  Wiimms SZS Toolset v2.25a r8443");
                for (int i = 0; i < cups.Count; i++)
                {
                    sw.WriteLine($"  {28740 + i * 4:X}	= {cups[i].Track1BMG}");
                    sw.WriteLine($"  {28741 + i * 4:X}	= {cups[i].Track2BMG}");
                    sw.WriteLine($"  {28742 + i * 4:X}	= {cups[i].Track3BMG}");
                    sw.WriteLine($"  {28743 + i * 4:X}	= {cups[i].Track4BMG}");
                }
                sw.WriteLine(@"");
                sw.WriteLine(@" 18697	/");
                sw.WriteLine(@" 18698	/");
                sw.WriteLine(@" 18699	/");
                sw.WriteLine(@" 1869a	/");
                sw.WriteLine(@" 1869b	/");
                sw.WriteLine(@" 1869c	/");
                sw.WriteLine(@" 1869d	/");
                sw.WriteLine(@" 1869e	/");
                sw.Close();

                processInfo = new ProcessStartInfo();
                processInfo.FileName = @"wbmgt";
                processInfo.Arguments = @"encode Common.txt -o";
                processInfo.WorkingDirectory = @"workdir/MenuSingle_E_reg.d/message/";
                processInfo.CreateNoWindow = true;
                processInfo.WindowStyle = ProcessWindowStyle.Hidden;
                processInfo.UseShellExecute = false;

                process = new Process();
                process.StartInfo = processInfo;
                process.Start();

                process.WaitForExit();

                processInfo = new ProcessStartInfo();
                processInfo.FileName = @"wszst";
                processInfo.Arguments = @"create MenuSingle_E_reg.d -o";
                processInfo.WorkingDirectory = @"workdir/";
                processInfo.CreateNoWindow = true;
                processInfo.WindowStyle = ProcessWindowStyle.Hidden;
                processInfo.UseShellExecute = false;

                process = new Process();
                process.StartInfo = processInfo;
                process.Start();

                process.WaitForExit();

                processInfo = new ProcessStartInfo();
                processInfo.FileName = @"wszst";
                processInfo.Arguments = @"extract MenuSingle_E_mom.szs";
                processInfo.WorkingDirectory = @"workdir/";
                processInfo.CreateNoWindow = true;
                processInfo.WindowStyle = ProcessWindowStyle.Hidden;
                processInfo.UseShellExecute = false;

                process = new Process();
                process.StartInfo = processInfo;
                process.Start();

                process.WaitForExit();

                processInfo = new ProcessStartInfo();
                processInfo.FileName = @"wbmgt";
                processInfo.Arguments = @"decode Common.bmg";
                processInfo.WorkingDirectory = @"workdir/MenuSingle_E_mom.d/message/";
                processInfo.CreateNoWindow = true;
                processInfo.WindowStyle = ProcessWindowStyle.Hidden;
                processInfo.UseShellExecute = false;

                process = new Process();
                process.StartInfo = processInfo;
                process.Start();

                process.WaitForExit();

                sw = new StreamWriter(@"workdir/MenuSingle_E_mom.d/message/Common.txt", true);
                sw.WriteLine("\n\n#--- [7000:7fff] LE-CODE: track names");
                sw.WriteLine(@"  7000	= \c{blue1}Wii \c{off}Mario Circuit");
                sw.WriteLine(@"  7001	= \c{blue1}Wii \c{off}Moo Moo Meadows");
                sw.WriteLine(@"  7002	= \c{blue1}Wii \c{off}Mushroom Gorge");
                sw.WriteLine(@"  7003	= \c{blue1}Wii \c{off}Grumble Volcano");
                sw.WriteLine(@"  7004	= \c{blue1}Wii \c{off}Toad's Factory");
                sw.WriteLine(@"  7005	= \c{blue1}Wii \c{off}Coconut Mall");
                sw.WriteLine(@"  7006	= \c{blue1}Wii \c{off}DK's Snowboard Cross");
                sw.WriteLine(@"  7007	= \c{blue1}Wii \c{off}Wario's Gold Mine");
                sw.WriteLine(@"  7008	= \c{blue1}Wii \c{off}Luigi Circuit");
                sw.WriteLine(@"  7009	= \c{blue1}Wii \c{off}Daisy Circuit");
                sw.WriteLine(@"  700a	= \c{blue1}Wii \c{off}Moonview Highway");
                sw.WriteLine(@"  700b	= \c{blue1}Wii \c{off}Maple Treeway");
                sw.WriteLine(@"  700c	= \c{blue1}Wii \c{off}Bowser's Castle");
                sw.WriteLine(@"  700d	= \c{blue1}Wii \c{off}Rainbow Road");
                sw.WriteLine(@"  700e	= \c{blue1}Wii \c{off}Dry Dry Ruins");
                sw.WriteLine(@"  700f	= \c{blue1}Wii \c{off}Koopa Cape");
                sw.WriteLine(@"  7010	= \c{blue1}GCN \c{off}Peach Beach");
                sw.WriteLine(@"  7011	= \c{blue1}GCN \c{off}Mario Circuit");
                sw.WriteLine(@"  7012	= \c{blue1}GCN \c{off}Waluigi Stadium");
                sw.WriteLine(@"  7013	= \c{blue1}GCN \c{off}DK Mountain");
                sw.WriteLine(@"  7014	= \c{blue1}DS \c{off}Yoshi Falls");
                sw.WriteLine(@"  7015	= \c{blue1}DS \c{off}Desert Hills");
                sw.WriteLine(@"  7016	= \c{blue1}DS \c{off}Peach Gardens");
                sw.WriteLine(@"  7017	= \c{blue1}DS \c{off}Delfino Square");
                sw.WriteLine(@"  7018	= \c{blue1}SNES \c{off}Mario Circuit 3");
                sw.WriteLine(@"  7019	= \c{blue1}SNES \c{off}Ghost Valley 2");
                sw.WriteLine(@"  701a	= \c{blue1}N64 \c{off}Mario Raceway");
                sw.WriteLine(@"  701b	= \c{blue1}N64 \c{off}Sherbet Land");
                sw.WriteLine(@"  701c	= \c{blue1}N64 \c{off}Bowser's Castle");
                sw.WriteLine(@"  701d	= \c{blue1}N64 \c{off}DK's Jungle Parkway");
                sw.WriteLine(@"  701e	= \c{blue1}GBA \c{off}Bowser Castle 3");
                sw.WriteLine(@"  701f	= \c{blue1}GBA \c{off}Shy Guy Beach");
                sw.WriteLine(@"  7020	= \c{yor7}-----\c{off}");
                sw.WriteLine(@"  7021	= \c{yor7}-----\c{off}");
                sw.WriteLine(@"  7022	= \c{yor7}-----\c{off}");
                sw.WriteLine(@"  7023	= \c{yor7}-----\c{off}");
                sw.WriteLine(@"  7024	= \c{yor7}-----\c{off}");
                sw.WriteLine(@"  7025	= \c{yor7}-----\c{off}");
                sw.WriteLine(@"  7026	= \c{yor7}-----\c{off}");
                sw.WriteLine(@"  7027	= \c{yor7}-----\c{off}");
                sw.WriteLine(@"  7028	= \c{yor7}-----\c{off}");
                sw.WriteLine(@"  7029	= \c{yor7}-----\c{off}");
                sw.WriteLine(@"  702a	/");
                sw.WriteLine(@"  702b	/");
                sw.WriteLine(@"  702c	/");
                sw.WriteLine(@"  702d	/");
                sw.WriteLine(@"  702e	/");
                sw.WriteLine(@"  702f	/");
                sw.WriteLine(@"  7030	/");
                sw.WriteLine(@"  7031	/");
                sw.WriteLine(@"  7032	/");
                sw.WriteLine(@"  7033	/");
                sw.WriteLine(@"  7034	/");
                sw.WriteLine(@"  7035	/");
                sw.WriteLine(@"  7036	= Ring Mission");
                sw.WriteLine(@"  7037	= Winningrun Demo");
                sw.WriteLine(@"  7038	= Loser Demo");
                sw.WriteLine(@"  7039	= Draw Demo");
                sw.WriteLine(@"  703a	= Ending Demo");
                sw.WriteLine(@"  703b	/");
                sw.WriteLine(@"  703c	/");
                sw.WriteLine(@"  703d	/");
                sw.WriteLine(@"  703e	/");
                sw.WriteLine(@"  703f	/");
                sw.WriteLine(@"  7040	/");
                sw.WriteLine(@"  7041	/");
                sw.WriteLine(@"  7042	/");
                sw.WriteLine(@"  7043	=  Wiimms SZS Toolset v2.25a r8443");
                for (int i = 0; i < cups.Count; i++)
                {
                    sw.WriteLine($"  {28740 + i * 4:X}	= {cups[i].Track1BMG}");
                    sw.WriteLine($"  {28741 + i * 4:X}	= {cups[i].Track2BMG}");
                    sw.WriteLine($"  {28742 + i * 4:X}	= {cups[i].Track3BMG}");
                    sw.WriteLine($"  {28743 + i * 4:X}	= {(cups[i].Track4BMG.ToLowerInvariant() == "secret" ? @"\c{red}???\c{off}" : cups[i].Track4BMG)}");
                }
                sw.WriteLine(@"");
                sw.WriteLine(@" 18697	/");
                sw.WriteLine(@" 18698	/");
                sw.WriteLine(@" 18699	/");
                sw.WriteLine(@" 1869a	/");
                sw.WriteLine(@" 1869b	/");
                sw.WriteLine(@" 1869c	/");
                sw.WriteLine(@" 1869d	/");
                sw.WriteLine(@" 1869e	/");
                sw.Close();

                processInfo = new ProcessStartInfo();
                processInfo.FileName = @"wbmgt";
                processInfo.Arguments = @"encode Common.txt -o";
                processInfo.WorkingDirectory = @"workdir/MenuSingle_E_mom.d/message/";
                processInfo.CreateNoWindow = true;
                processInfo.WindowStyle = ProcessWindowStyle.Hidden;
                processInfo.UseShellExecute = false;

                process = new Process();
                process.StartInfo = processInfo;
                process.Start();

                process.WaitForExit();

                processInfo = new ProcessStartInfo();
                processInfo.FileName = @"wszst";
                processInfo.Arguments = @"create MenuSingle_E_mom.d -o";
                processInfo.WorkingDirectory = @"workdir/";
                processInfo.CreateNoWindow = true;
                processInfo.WindowStyle = ProcessWindowStyle.Hidden;
                processInfo.UseShellExecute = false;

                process = new Process();
                process.StartInfo = processInfo;
                process.Start();

                process.WaitForExit();

                processInfo = new ProcessStartInfo();
                processInfo.FileName = @"wlect";
                processInfo.Arguments = @"patch lecode-PAL.bin --le-define config.txt --custom-tt -o";
                processInfo.WorkingDirectory = @"workdir/";
                processInfo.CreateNoWindow = true;
                processInfo.WindowStyle = ProcessWindowStyle.Hidden;
                processInfo.UseShellExecute = false;

                process = new Process();
                process.StartInfo = processInfo;
                process.Start();

                process.WaitForExit();

                processInfo = new ProcessStartInfo();
                processInfo.FileName = @"wlect";
                processInfo.Arguments = @"patch lecode-USA.bin --le-define config.txt --custom-tt -o";
                processInfo.WorkingDirectory = @"workdir/";
                processInfo.CreateNoWindow = true;
                processInfo.WindowStyle = ProcessWindowStyle.Hidden;
                processInfo.UseShellExecute = false;

                process = new Process();
                process.StartInfo = processInfo;
                process.Start();

                process.WaitForExit();

                processInfo = new ProcessStartInfo();
                processInfo.FileName = @"wlect";
                processInfo.Arguments = @"patch lecode-JAP.bin --le-define config.txt --custom-tt -o";
                processInfo.WorkingDirectory = @"workdir/";
                processInfo.CreateNoWindow = true;
                processInfo.WindowStyle = ProcessWindowStyle.Hidden;
                processInfo.UseShellExecute = false;

                process = new Process();
                process.StartInfo = processInfo;
                process.Start();

                process.WaitForExit();

                processInfo = new ProcessStartInfo();
                processInfo.FileName = @"wlect";
                processInfo.Arguments = @"patch lecode-PAL.bin -od lecode-PAL.bin --le-define config.txt --track-dir output --copy-tracks input";
                processInfo.WorkingDirectory = @"workdir/";
                processInfo.CreateNoWindow = true;
                processInfo.WindowStyle = ProcessWindowStyle.Hidden;
                processInfo.UseShellExecute = false;
                processInfo.RedirectStandardOutput = true;

                process = new Process();
                process.StartInfo = processInfo;
                process.Start();
                process.WaitForExit();

                Directory.Move(@"workdir/output", @"output/CTTP/Course");
                File.Move(@"workdir/MenuSingle_E_reg.szs", @"output/CTTP/Scene/MenuSingle_E.szs");
                File.Move(@"workdir/MenuSingle_E_mom.szs", @"output/CTTP/Scene/YourMom/MenuSingle_E.szs");
                File.Move(@"workdir/lecode-PAL.bin", @"output/CTTP/rel/lecode-PAL.bin");
                File.Move(@"workdir/lecode-USA.bin", @"output/CTTP/rel/lecode-USA.bin");
                File.Move(@"workdir/lecode-JAP.bin", @"output/CTTP/rel/lecode-JAP.bin");
                Directory.Delete(@"workdir/", true);

                string dateString = responseRaw.Values[1][1].ToString();
                var date = DateTime.Parse(dateString);

                ZipFile.CreateFromDirectory(@"output/", $"{(date.Year + "-" + date.Month + "-" + date.Day).Replace(",", string.Empty).Replace(' ', '_')}_Track_Test.zip");

                processInfo = new ProcessStartInfo();
                processInfo.FileName = @"sudo";
                processInfo.Arguments = $"cp '{(date.Year + "-" + date.Month + "-" + date.Day).Replace(",", string.Empty).Replace(' ', '_')}_Track_Test.zip' /var/www/brawlbox/";
                processInfo.CreateNoWindow = true;
                processInfo.WindowStyle = ProcessWindowStyle.Hidden;
                processInfo.UseShellExecute = false;
                processInfo.RedirectStandardOutput = true;

                process = new Process();
                process.StartInfo = processInfo;
                process.Start();
                process.WaitForExit();

                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"https://brawlbox.xyz/{(date.Year + "-" + date.Month + "-" + date.Day).Replace(",", string.Empty).Replace(' ', '_')}_Track_Test.zip"));

                File.Delete($"{(date.Year + "-" + date.Month + "-" + date.Day).Replace(",", string.Empty).Replace(' ', '_')}_Track_Test.zip");
                Directory.Delete(@"output/", true);
            }
            catch (Exception ex)
            {
                var embed = new DiscordEmbedBuilder
                {
                    Color = new DiscordColor("#FF0000"),
                    Title = "__**Error:**__",
                    Description = $"*{ex.Message}*",
                    Footer = new DiscordEmbedBuilder.EmbedFooter
                    {
                        Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                    }
                };
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));

                Console.WriteLine(ex.ToString());

                if (Directory.Exists(@"workdir/"))
                {
                    Directory.Delete(@"workdir/", true);
                }

                if (Directory.Exists(@"output/"))
                {
                    Directory.Delete(@"output/", true);
                }
            }
        }

        [SlashCommand("dmrole", "Direct messages members of a role specified.")]
        [SlashRequireUserPermissions(Permissions.Administrator)]
        public async Task DMRole(InteractionContext ctx,
            [Option("role", "The member role you want to send a direct message to.")] string role,
            [Option("message", "The message you would like to send in the direct message.")] string message)
        {
            try
            {
                await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder() { IsEphemeral = true });

                DiscordRole discordRole = null;
                foreach (var r in ctx.Guild.Roles.Values)
                {
                    if (r.Id.ToString() == role.Replace("<@&", string.Empty).Replace(">", string.Empty))
                    {
                        discordRole = r;
                    }
                }
                if (discordRole == null)
                {
                    var embed = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = "__**Error:**__",
                        Description = $"*{role} could not be found in the server.*",
                        Footer = new DiscordEmbedBuilder.EmbedFooter
                        {
                            Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                        }
                    };
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
                }
                else
                {
                    var members = ctx.Guild.GetAllMembersAsync();
                    foreach (var member in members.Result)
                    {
                        foreach (var r in member.Roles)
                        {
                            if (r == discordRole)
                            {
                                try
                                {
                                    await member.SendMessageAsync(message).ConfigureAwait(false);
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine(ex.Message);
                                    Console.WriteLine("DMs are likely closed.");
                                }
                            }
                        }
                    }

                    var embed = new DiscordEmbedBuilder
                    {
                        Color = new DiscordColor("#FF0000"),
                        Title = "__**Success:**__",
                        Description = $"*Message was sent to {role} successfully.*",
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
                var embed = new DiscordEmbedBuilder
                {
                    Color = new DiscordColor("#FF0000"),
                    Title = "__**Error:**__",
                    Description = $"*{ex.Message}*",
                    Footer = new DiscordEmbedBuilder.EmbedFooter
                    {
                        Text = $"Last Updated: {File.ReadAllText("lastUpdated.txt")}"
                    }
                };
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));

                Console.WriteLine(ex.ToString());
            }
        }
    }
}
