﻿using MKBB.Commands;
using DSharpPlus.CommandsNext;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MKBB
{
    static class MyLinqExtensions
    {
        public static IEnumerable<IEnumerable<T>> Batch<T>(
            this IEnumerable<T> source, int batchSize)
        {
            using (var enumerator = source.GetEnumerator())
                while (enumerator.MoveNext())
                    yield return YieldBatchElements(enumerator, batchSize - 1);
        }

        private static IEnumerable<T> YieldBatchElements<T>(
            IEnumerator<T> source, int batchSize)
        {
            yield return source.Current;
            for (int i = 0; i < batchSize && source.MoveNext(); i++)
                yield return source.Current;
        }
    }

    public class Scrape
    {
        public async Task WiimmfiScrape(string rtJson, string rt200Json, string ctwwDl1, string ctwwDl2, string ctwwDl3, string wwDl, List<Track> trackListRt, List<Track> trackListRt200, List<Track> trackList, List<Track> trackList200, List<Track> trackListNc, List<Track> trackList200Nc)
        {
            List<Track> oldJson = JsonConvert.DeserializeObject<List<Track>>(File.ReadAllText($"cts.json"));
            for (int i = 0; i < oldJson.Count; i++)
            {
                if (oldJson[i].Category % 16 != 0 || oldJson[i].Category != 4)
                {
                    oldJson.RemoveAt(i);
                    i--;
                }
            }
            HtmlDocument ctwwHtml1 = new HtmlDocument();
            HtmlDocument ctwwHtml2 = new HtmlDocument();
            HtmlDocument ctwwHtml3 = new HtmlDocument();
            ctwwHtml1.LoadHtml(ctwwDl1);
            var bodyNode1 = ctwwHtml1.DocumentNode.SelectNodes("//td[contains(@class, 'LL')]");
            var innerText1 = ctwwHtml1.DocumentNode.SelectNodes("//tr[contains(@id, 'p0-')]/td");
            var m1s = new List<string>();
            var m2s = new List<string>();
            var m3s = new List<string>();
            var m6s = new List<string>();
            var m9s = new List<string>();
            var m12s = new List<string>();
            for (int i = 0; i < innerText1.Count; i++)
            {
                if (i % 11 - 3 == 0)
                {
                    m1s.Add(innerText1[i].InnerHtml == "–" ? "0" : innerText1[i].InnerHtml);
                }
                if (i % 11 - 4 == 0)
                {
                    m2s.Add(innerText1[i].InnerHtml == "–" ? "0" : innerText1[i].InnerHtml);
                }
                if (i % 11 - 5 == 0)
                {
                    m3s.Add(innerText1[i].InnerHtml == "–" ? "0" : innerText1[i].InnerHtml);
                }
                if (i % 11 - 6 == 0)
                {
                    m6s.Add(innerText1[i].InnerHtml == "–" ? "0" : innerText1[i].InnerHtml);
                }
                if (i % 11 - 7 == 0)
                {
                    m9s.Add(innerText1[i].InnerHtml == "–" ? "0" : innerText1[i].InnerHtml);
                }
                if (i % 11 - 8 == 0)
                {
                    m12s.Add(innerText1[i].InnerHtml == "–" ? "0" : innerText1[i].InnerHtml);
                }
            }
            Console.WriteLine("Downloading 1st Wiimmfi Page");
            ctwwHtml2.LoadHtml(ctwwDl2);
            var bodyNode2 = ctwwHtml2.DocumentNode.SelectNodes("//td[contains(@class, 'LL')]");
            var innerText2 = ctwwHtml2.DocumentNode.SelectNodes("//tr[contains(@id, 'p0-')]/td");
            for (int i = 0; i < innerText2.Count; i++)
            {
                if (i % 11 - 3 == 0)
                {
                    m1s.Add(innerText2[i].InnerHtml == "–" ? "0" : innerText2[i].InnerHtml);
                }
                if (i % 11 - 4 == 0)
                {
                    m2s.Add(innerText2[i].InnerHtml == "–" ? "0" : innerText2[i].InnerHtml);
                }
                if (i % 11 - 5 == 0)
                {
                    m3s.Add(innerText2[i].InnerHtml == "–" ? "0" : innerText2[i].InnerHtml);
                }
                if (i % 11 - 6 == 0)
                {
                    m6s.Add(innerText2[i].InnerHtml == "–" ? "0" : innerText2[i].InnerHtml);
                }
                if (i % 11 - 7 == 0)
                {
                    m9s.Add(innerText2[i].InnerHtml == "–" ? "0" : innerText2[i].InnerHtml);
                }
                if (i % 11 - 8 == 0)
                {
                    m12s.Add(innerText2[i].InnerHtml == "–" ? "0" : innerText2[i].InnerHtml);
                }
            }
            Console.WriteLine("Downloading 2nd Wiimmfi Page");
            ctwwHtml3.LoadHtml(ctwwDl3);
            var bodyNode3 = ctwwHtml3.DocumentNode.SelectNodes("//td[contains(@class, 'LL')]");
            var innerText3 = ctwwHtml3.DocumentNode.SelectNodes("//tr[contains(@id, 'p0-')]/td");
            for (int i = 0; i < innerText3.Count; i++)
            {
                if (i % 11 - 3 == 0)
                {
                    m1s.Add(innerText3[i].InnerHtml == "–" ? "0" : innerText3[i].InnerHtml);
                }
                if (i % 11 - 4 == 0)
                {
                    m2s.Add(innerText3[i].InnerHtml == "–" ? "0" : innerText3[i].InnerHtml);
                }
                if (i % 11 - 5 == 0)
                {
                    m3s.Add(innerText3[i].InnerHtml == "–" ? "0" : innerText3[i].InnerHtml);
                }
                if (i % 11 - 6 == 0)
                {
                    m6s.Add(innerText3[i].InnerHtml == "–" ? "0" : innerText3[i].InnerHtml);
                }
                if (i % 11 - 7 == 0)
                {
                    m9s.Add(innerText3[i].InnerHtml == "–" ? "0" : innerText3[i].InnerHtml);
                }
                if (i % 11 - 8 == 0)
                {
                    m12s.Add(innerText3[i].InnerHtml == "–" ? "0" : innerText3[i].InnerHtml);
                }
            }
            Console.WriteLine("Downloading 3rd Wiimmfi Page");

            int j = 0;

            j = await ScrapeBodyNode(trackListNc, oldJson, m1s, m2s, m3s, m6s, m9s, m12s, bodyNode1, j);
            j = await ScrapeBodyNode(trackListNc, oldJson, m1s, m2s, m3s, m6s, m9s, m12s, bodyNode2, j);
            await ScrapeBodyNode(trackListNc, oldJson, m1s, m2s, m3s, m6s, m9s, m12s, bodyNode3, j);

            for (int i = 0; i < trackList.Count; i++)
            {
                foreach (var t in trackListNc)
                {
                    if (t.Name == trackList[i].Name)
                    {
                        trackList[i].M1 = t.M1;
                        trackList[i].M2 = t.M2;
                        trackList[i].M3 = t.M3;
                        trackList[i].M6 = t.M6;
                        trackList[i].M9 = t.M9;
                        trackList[i].M12 = t.M12;
                        trackList[i].WiimmfiName = t.WiimmfiName;
                    }
                }
            }
            for (int i = 0; i < trackList200.Count; i++)
            {
                foreach (var t in trackList200Nc)
                {
                    if (t.Name == trackList200[i].Name)
                    {
                        trackList200[i].M1 = t.M1;
                        trackList200[i].M2 = t.M2;
                        trackList200[i].M3 = t.M3;
                        trackList200[i].M6 = t.M6;
                        trackList200[i].M9 = t.M9;
                        trackList200[i].M12 = t.M12;
                        trackList200[i].WiimmfiName = t.WiimmfiName;
                    }
                }
            }

            List<Track> trackListRTNc = JsonConvert.DeserializeObject<List<Track>>(rtJson);
            List<Track> trackListRT200Nc = JsonConvert.DeserializeObject<List<Track>>(rt200Json);
            for (int i = 0; i < trackListRTNc.Count; i++)
            {
                if (trackListRTNc[i].Category % 16 != 0)
                {
                    trackListRTNc.RemoveAt(i);
                    i--;
                }
            }
            for (int i = 0; i < trackListRT200Nc.Count; i++)
            {
                if (trackListRT200Nc[i].Category % 16 != 0 && trackListRT200Nc[i].Category != 4)
                {
                    trackListRT200Nc.RemoveAt(i);
                    i--;
                }
            }
            HtmlDocument wwHtml = new HtmlDocument();
            wwHtml.LoadHtml(wwDl);
            innerText1 = wwHtml.DocumentNode.SelectNodes("//tr[contains(@id, 'p0-')]/td");
            m1s = new List<string>();
            m2s = new List<string>();
            m3s = new List<string>();
            m6s = new List<string>();
            m9s = new List<string>();
            m12s = new List<string>();
            var names = new List<string>();
            for (int i = 0; i < 32 * 11; i++)
            {
                if (i % 11 - 2 == 0)
                {
                    names.Add(innerText1[i].InnerHtml == "–" ? "0" : innerText1[i].InnerHtml);
                }
                if (i % 11 - 3 == 0)
                {
                    m1s.Add(innerText1[i].InnerHtml == "–" ? "0" : innerText1[i].InnerHtml);
                }
                if (i % 11 - 4 == 0)
                {
                    m2s.Add(innerText1[i].InnerHtml == "–" ? "0" : innerText1[i].InnerHtml);
                }
                if (i % 11 - 5 == 0)
                {
                    m3s.Add(innerText1[i].InnerHtml == "–" ? "0" : innerText1[i].InnerHtml);
                }
                if (i % 11 - 6 == 0)
                {
                    m6s.Add(innerText1[i].InnerHtml == "–" ? "0" : innerText1[i].InnerHtml);
                }
                if (i % 11 - 7 == 0)
                {
                    m9s.Add(innerText1[i].InnerHtml == "–" ? "0" : innerText1[i].InnerHtml);
                }
                if (i % 11 - 8 == 0)
                {
                    m12s.Add(innerText1[i].InnerHtml == "–" ? "0" : innerText1[i].InnerHtml);
                }
            }
            for (int i = 0; i < names.Count; i++)
            {
                foreach (var t in trackListRTNc)
                {
                    if (names[i].Contains(t.Name))
                    {
                        t.M1 = int.Parse(m1s[i]);
                        t.M2 = int.Parse(m2s[i]);
                        t.M3 = int.Parse(m3s[i]);
                        t.M6 = int.Parse(m6s[i]);
                        t.M9 = int.Parse(m9s[i]);
                        t.M12 = int.Parse(m12s[i]);
                        t.WiimmfiName = names[i];
                    }
                }
                foreach (var t in trackListRT200Nc)
                {
                    if (names[i].Contains(t.Name))
                    {
                        t.M1 = int.Parse(m1s[i]);
                        t.M2 = int.Parse(m2s[i]);
                        t.M3 = int.Parse(m3s[i]);
                        t.M6 = int.Parse(m6s[i]);
                        t.M9 = int.Parse(m9s[i]);
                        t.M12 = int.Parse(m12s[i]);
                        t.WiimmfiName = names[i];
                    }
                }
            }

            for (int i = 0; i < trackListRt.Count; i++)
            {
                foreach (var t in trackListRTNc)
                {
                    if (t.Name == trackListRt[i].Name)
                    {
                        trackListRt[i].M1 = t.M1;
                        trackListRt[i].M2 = t.M2;
                        trackListRt[i].M3 = t.M3;
                        trackListRt[i].M6 = t.M6;
                        trackListRt[i].M9 = t.M9;
                        trackListRt[i].M12 = t.M12;
                        trackListRt[i].WiimmfiName = t.WiimmfiName;
                    }
                }
            }
            for (int i = 0; i < trackListRt200.Count; i++)
            {
                foreach (var t in trackListRT200Nc)
                {
                    if (t.Name == trackListRt200[i].Name)
                    {
                        trackListRt200[i].M1 = t.M1;
                        trackListRt200[i].M2 = t.M2;
                        trackListRt200[i].M3 = t.M3;
                        trackListRt200[i].M6 = t.M6;
                        trackListRt200[i].M9 = t.M9;
                        trackListRt200[i].M12 = t.M12;
                        trackListRt200[i].WiimmfiName = t.WiimmfiName;
                    }
                }
            }
        }

        private async Task<int> ScrapeBodyNode(List<Track> trackListNc,
            List<Track> oldJson,
            List<string> m1s,
            List<string> m2s,
            List<string> m3s,
            List<string> m6s,
            List<string> m9s,
            List<string> m12s,
            HtmlNodeCollection bodyNode,
            int j)
        {
            WebClient webClient = new WebClient();

            int g = 0;
            bool check;
            foreach (var t in bodyNode)
            {
                check = true;
                if (t.InnerHtml.Contains("a href="))
                {
                    foreach (var track in oldJson)
                    {
                        if (t.InnerText.Contains(track.Name))
                        {
                            g = trackListNc.FindIndex(ix => ix.Name.Contains(track.Name));
                            trackListNc[g].M1 = int.Parse(m1s[j]);
                            trackListNc[g].M2 = int.Parse(m2s[j]);
                            trackListNc[g].M3 = int.Parse(m3s[j]);
                            trackListNc[g].M6 = int.Parse(m6s[j]);
                            trackListNc[g].M9 = int.Parse(m9s[j]);
                            trackListNc[g].M12 = int.Parse(m12s[j]);
                            trackListNc[g].WiimmfiName = t.InnerText;
                            check = false;
                        }
                    }
                    if (check)
                    {
                        var dl = await webClient.DownloadStringTaskAsync($"{t.InnerHtml.Split('"')[1]}?m=json");
                        var temp = new HtmlDocument();
                        temp.LoadHtml(dl);
                        var tts = temp.DocumentNode.SelectNodes("//tr/td/tt");
                        foreach (var tt in tts)
                        {
                            for (int i = 0; i < trackListNc.Count; i++)
                            {
                                if (tt.InnerText.ToLowerInvariant().Contains(trackListNc[i].SHA1.ToLowerInvariant()))
                                {
                                    trackListNc[i].M1 = int.Parse(m1s[j]);
                                    trackListNc[i].M2 = int.Parse(m2s[j]);
                                    trackListNc[i].M3 = int.Parse(m3s[j]);
                                    trackListNc[i].M6 = int.Parse(m6s[j]);
                                    trackListNc[i].M9 = int.Parse(m9s[j]);
                                    trackListNc[i].M12 = int.Parse(m12s[j]);
                                    trackListNc[i].WiimmfiName = t.InnerText;
                                }
                            }
                        }
                    }
                }
                else if (t.InnerHtml.Contains("SHA1"))
                {
                    for (int i = 0; i < trackListNc.Count; i++)
                    {
                        if (t.InnerText.Split(':')[1].Split(' ')[1].ToLowerInvariant().Contains(trackListNc[i].SHA1.ToLowerInvariant()))
                        {
                            trackListNc[i].M1 = int.Parse(m1s[j]);
                            trackListNc[i].M2 = int.Parse(m2s[j]);
                            trackListNc[i].M3 = int.Parse(m3s[j]);
                            trackListNc[i].M6 = int.Parse(m6s[j]);
                            trackListNc[i].M9 = int.Parse(m9s[j]);
                            trackListNc[i].M12 = int.Parse(m12s[j]);
                        }
                    }
                }
                j++;
            }
            return j;
        }

        public async Task GetBKTLeaderboards(List<Track> trackListRt, List<Track> trackListRt200, List<Track> trackList, List<Track> trackList200)
        {
            Task.WaitAll(DownloadLeaderboard(trackList),
                DownloadLeaderboard(trackListRt),
                DownloadLeaderboard(trackListRt200),
                DownloadLeaderboard(trackList200));

            Task.WaitAll(DownloadBKTInfo(trackList, "cts"),
                 DownloadBKTInfo(trackListRt, "rts"),
                 DownloadBKTInfo(trackListRt200, "rts200"),
                 DownloadBKTInfo(trackList200, "cts200"));
            await Task.CompletedTask;
        }

        private async Task DownloadLeaderboard(List<Track> trackList)
        {
            foreach (var track in trackList)
            {
                await DownloadLeaderboardTask(track);
            }
            await Task.CompletedTask;
        }

        private async Task DownloadLeaderboardTask(Track t)
        {
            WebClient webClient = new WebClient();
            try
            {
                var ghostJson = JsonConvert.DeserializeObject<BKTList>(await webClient.DownloadStringTaskAsync($"http://tt.chadsoft.co.uk{t.LeaderboardLink}?limit=1"));
                var ghostList = ghostJson.List;
                if (ghostList.Count > 0)
                {
                    t.BKTLink = ghostList[0].LinkContainer.Href.URL;
                    t.BKTHolder = ghostList[0].BKTHolder;
                    t.BKTUploadTime = ghostList[0].DateSet;
                }
                Console.WriteLine($"{t.Name} has been downloaded.");
                webClient.Dispose();
            }
            catch
            {
                Console.WriteLine($"{t.Name} download failed. Skipping...");
                webClient.Dispose();
            }
        }

        private async Task DownloadBKTInfo(List<Track> trackList, string type)
        {
            WebClient webClient = new WebClient();
            HtmlDocument document = new HtmlDocument();
            int l = 0;
        retry:
            l++;
            try
            {
                string html = string.Empty;
                if (type == "rts")
                {
                    html = await webClient.DownloadStringTaskAsync("https://www.chadsoft.co.uk/time-trials/original-track-leaderboards.html");
                }
                else if (type == "cts")
                {
                    html = await webClient.DownloadStringTaskAsync("https://www.chadsoft.co.uk/time-trials/ctgp-leaderboards.html");
                }
                else if (type == "rts200")
                {
                    html = await webClient.DownloadStringTaskAsync("https://www.chadsoft.co.uk/time-trials/original-track-leaderboards-200cc.html");
                }
                else
                {
                    html = await webClient.DownloadStringTaskAsync("https://www.chadsoft.co.uk/time-trials/ctgp-leaderboards-200cc.html");
                }
                document.LoadHtml(html);
            }
            catch
            {
                if (l > 10)
                {
                    Console.WriteLine("Failed to download leaderboard. Retrying...");
                    Thread.Sleep(5000);
                    goto retry;
                }
                else
                {
                    Console.WriteLine("Leaderboard download failed.");
                }
            }
            Parallel.ForEach(trackList, track =>
            {
                Task.WaitAll(DownloadBKTInfoTask(track, document));
            });
            await Task.CompletedTask;

        }

        private async Task DownloadBKTInfoTask(Track t, HtmlDocument document)
        {
            var hrefs = document.DocumentNode.SelectNodes("//td/a");
            foreach (var href in hrefs)
            {
                if (href.OuterHtml.Split('"')[1] == $".{t.LeaderboardLink.Split('.')[0]}.html")
                {
                    if (!href.InnerText.Contains("Normal") && !href.InnerText.Contains("No-shortcut") && !href.InnerText.Contains("Shortcut") && !href.InnerText.Contains("Glitch"))
                    {
                        t.CategoryName = "Normal";
                    }
                    else
                    {
                        t.CategoryName = href.InnerText.Split(' ')[href.InnerText.Split(' ').Count() - 1];
                    }
                }
            }

            Console.WriteLine($"Downloaded BKT info for {t.Name}");
            await Task.CompletedTask;
        }

        public async Task Dl200ccBKT(Track t)
        {
            string categoryName = string.Empty;
            if (t.Category == 4)
            {
                categoryName = "Shortcut";
            }
            if (t.Category == 5)
            {
                categoryName = "Glitch";
            }
            if (t.Category == 6 || t.Category == 0)
            {
                categoryName = "No Shortcut";
            }
            try
            {
                var webClient = new WebClient();

                webClient.DownloadFile(new Uri($"http://tt.chadsoft.co.uk{t.BKTLink.Split('.')[0]}.rkg"),
                    $"rkgs/200/{String.Join("", t.Name.Split('\\', '/', ':', '*', '?', '"', '<', '>', '|'))} - {categoryName} (200cc).rkg");
                Console.WriteLine($"{t.Name} 200cc {categoryName} has been downloaded");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"No ghosts found for {t.Name} 200cc {categoryName}");
                Console.WriteLine(ex.ToString());
            }

            await Task.CompletedTask;
        }

        public async Task Dl150ccBKT(Track t)
        {
            string categoryName = string.Empty;
            if (t.Name == "Mushroom Gorge" ||
                t.Name == "Toad's Factory" ||
                t.Name == "Coconut Mall" ||
                t.Name == "Grumble Volcano" ||
                t.Name == "Bowser's Castle" ||
                t.Name == "DS Desert Hills" ||
                t.Name == "GBA Bowser Castle 3" ||
                t.Name == "N64 DK's Jungle Parkway" ||
                t.Name == "GCN DK Mountain" ||
                t.Name == "N64 Bowser's Castle")
            {
                if (t.Category == 16)
                {
                    categoryName = "Shortcut";
                }
                if (t.Category == 1)
                {
                    categoryName = "Glitch";
                }
                if (t.Category == 2)
                {
                    categoryName = "No Shortcut";
                }
            }
            else
            {
                if (t.Category == 0)
                {
                    categoryName = "No Shortcut";
                }
                if (t.Category == 1)
                {
                    categoryName = "Glitch";
                }
                if (t.Category == 2)
                {
                    categoryName = "Shortcut";
                }
            }
            try
            {
                var webClient = new WebClient();

                webClient.DownloadFile(new Uri($"http://tt.chadsoft.co.uk{t.BKTLink.Split('.')[0]}.rkg"),
                    $"rkgs/150/{String.Join("", t.Name.Split('\\', '/', ':', '*', '?', '"', '<', '>', '|'))} - {categoryName} (150cc).rkg");
                Console.WriteLine($"{t.Name} 150cc {categoryName} has been downloaded");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"No ghosts found for {t.Name} 150cc {categoryName}");
                Console.WriteLine(ex.ToString());
            }

            await Task.CompletedTask;
        }

        public async Task GetSlotIds(List<Track> trackListRt, List<Track> trackListRt200, List<Track> trackList, List<Track> trackList200)
        {
            Task.WaitAll(ScrapeWikiPage(trackList),
                ScrapeWikiPage(trackListRt),
                ScrapeWikiPage(trackListRt200),
                ScrapeWikiPage(trackList200));
            await Task.CompletedTask;
        }

        public async Task ScrapeWikiPage(List<Track> trackList)
        {
            WebClient webClient = new WebClient();
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(await webClient.DownloadStringTaskAsync("https://wiki.tockdom.com/wiki/CTGP_Revolution"));
            var tds = document.DocumentNode.SelectNodes("//table[@class='textbox grid sortable center'][1]/tbody/tr/td");
            for (int i = 0; i < tds.Count; i++)
            {
                foreach (var track in trackList)
                {
                    if (tds[i].InnerText.Contains(track.Name))
                    {
                        track.SlotID = tds[i + 6].InnerText.Replace("\n", string.Empty);
                        Console.WriteLine($"{track.Name} Slot ID: {track.SlotID.Replace("\n", string.Empty)}");
                    }
                }
            }
            await Task.CompletedTask;
        }

        public async Task GetPlayerInfo(List<Player> playerList)
        {
            Parallel.ForEach(playerList, players =>
            {
                Task.WaitAll(GetPlayerInfoTask(players));
            });
            await Task.CompletedTask;
        }

        private async Task GetPlayerInfoTask(Player player)
        {
            try
            {
                WebClient webClient = new WebClient();
                var playerJson = JsonConvert.DeserializeObject<Player>(await webClient.DownloadStringTaskAsync(player.PlayerLink));
                player.MiiName = playerJson.MiiName;
                player.Ghosts = null;
                Console.WriteLine($"Updating data for {player.MiiName}.");
            }
            catch
            {
                Console.WriteLine($"Download failed for {player.MiiName}.");
            }
            await Task.CompletedTask;
        }
    }
}
