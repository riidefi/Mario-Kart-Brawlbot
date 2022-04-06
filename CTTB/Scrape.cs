using CTTB.Commands;
using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CTTB
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
            WebClient webClient = new WebClient();

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
            HtmlDocument temp = new HtmlDocument();
            ctwwHtml1.LoadHtml(ctwwDl1);
            var bodyNode1 = ctwwHtml1.DocumentNode.SelectNodes("//td[contains(@class, 'LL')]");
            var innerText1 = ctwwHtml1.DocumentNode.SelectNodes("//tr[contains(@id, 'p0-')]/td");
            var m3s = new List<string>();
            for (int i = 0; i < innerText1.Count; i++)
            {
                if (i % 10 - 4 == 0)
                {
                    m3s.Add(innerText1[i].InnerHtml);
                }
            }
            Console.WriteLine("Downloading 1st Wiimmfi Page");
            ctwwHtml2.LoadHtml(ctwwDl2);
            var bodyNode2 = ctwwHtml2.DocumentNode.SelectNodes("//td[contains(@class, 'LL')]");
            var innerText2 = ctwwHtml2.DocumentNode.SelectNodes("//tr[contains(@id, 'p0-')]/td");
            for (int i = 0; i < innerText2.Count; i++)
            {
                if (i % 10 - 4 == 0)
                {
                    m3s.Add(innerText2[i].InnerHtml);
                }
            }
            Console.WriteLine("Downloading 2nd Wiimmfi Page");
            ctwwHtml3.LoadHtml(ctwwDl3);
            var bodyNode3 = ctwwHtml3.DocumentNode.SelectNodes("//td[contains(@class, 'LL')]");
            var innerText3 = ctwwHtml3.DocumentNode.SelectNodes("//tr[contains(@id, 'p0-')]/td");
            for (int i = 0; i < innerText3.Count; i++)
            {
                if (i % 10 - 4 == 0)
                {
                    m3s.Add(innerText3[i].InnerHtml);
                }
            }
            Console.WriteLine("Downloading 3rd Wiimmfi Page");

            int j = 0;

            j = await ScrapeBodyNode(trackListNc, oldJson, m3s, bodyNode1, j);
            j = await ScrapeBodyNode(trackListNc, oldJson, m3s, bodyNode2, j);
            await ScrapeBodyNode(trackListNc, oldJson, m3s, bodyNode3, j);

            for (int i = 0; i < trackList.Count; i++)
            {
                foreach (var t in trackListNc)
                {
                    if (t.Name == trackList[i].Name)
                    {
                        trackList[i].WiimmfiScore = t.WiimmfiScore;
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
                        trackList200[i].WiimmfiScore = t.WiimmfiScore;
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
            bodyNode1 = wwHtml.DocumentNode.SelectNodes("//td[contains(@class, 'LL')]");
            innerText1 = wwHtml.DocumentNode.SelectNodes("//tr[contains(@id, 'p0-')]/td");
            m3s = new List<string>();
            var m3Names = new List<string>();
            for (int i = 0; i < 320; i++)
            {
                if (i % 10 - 2 == 0)
                {
                    m3Names.Add(innerText1[i].InnerHtml);
                }
                if (i % 10 - 4 == 0)
                {
                    m3s.Add(innerText1[i].InnerHtml);
                }
            }
            for (int i = 0; i < m3Names.Count; i++)
            {
                foreach (var t in trackListRTNc)
                {
                    if (m3Names[i].Contains(t.Name))
                    {
                        t.WiimmfiScore = int.Parse(m3s[i]);
                        t.WiimmfiName = m3Names[i];
                    }
                }
                foreach (var t in trackListRT200Nc)
                {
                    if (m3Names[i].Contains(t.Name))
                    {
                        t.WiimmfiScore = int.Parse(m3s[i]);
                        t.WiimmfiName = m3Names[i];
                    }
                }
            }

            for (int i = 0; i < trackListRt.Count; i++)
            {
                foreach (var t in trackListRTNc)
                {
                    if (t.Name == trackListRt[i].Name)
                    {
                        trackListRt[i].WiimmfiScore = t.WiimmfiScore;
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
                        trackListRt200[i].WiimmfiScore = t.WiimmfiScore;
                        trackListRt200[i].WiimmfiName = t.WiimmfiName;
                    }
                }
            }
        }

        private async Task<int> ScrapeBodyNode(List<Track> trackListNc, List<Track> oldJson, List<string> m3s, HtmlNodeCollection bodyNode, int j)
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
                        track.Name = t.Name.Replace('_', ' ');

                        if (t.InnerText.Contains(track.Name))
                        {
                            g = trackListNc.FindIndex(ix => ix.Name.Contains(track.Name));
                            trackListNc[g].WiimmfiScore = int.Parse(m3s[j]);
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
                                    trackListNc[i].WiimmfiScore = int.Parse(m3s[j]);
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
                            trackListNc[i].WiimmfiScore = int.Parse(m3s[j]);
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
        }

        private async Task DownloadLeaderboard(List<Track> trackList)
        {
            foreach (var trackBatch in trackList.Batch(5))
            {
                List<Task> tasks = new List<Task>();
                foreach (var track in trackBatch)
                {
                    tasks.Add(DownloadLeaderboardTask(track));
                }

                Task.WaitAll(tasks.ToArray());
            }
        }

        private async Task DownloadLeaderboardTask(Track t)
        {
            WebClient webClient = new WebClient();
            t.Name = t.Name.Replace('_', ' ');

            var ghostJson = JsonConvert.DeserializeObject<GhostList>(await webClient.DownloadStringTaskAsync($"http://tt.chadsoft.co.uk{t.LeaderboardLink}?start=16&limit=1&times=pb"));
            var ghostList = ghostJson.List;
            if (ghostList.Count > 0)
            {
                t.BKTLink = ghostList[0].Link.Href.LeaderboardLink;
            }
            Console.WriteLine($"Downloaded {t.Name} leaderboard...");
        }
    }
}
