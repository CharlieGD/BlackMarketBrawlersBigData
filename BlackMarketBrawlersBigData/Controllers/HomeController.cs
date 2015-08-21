using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using System.IO;
using Newtonsoft.Json.Linq;
using BlackMarketBrawlersBigData.Models;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net;
using System.Drawing;
using System.Drawing.Imaging;

namespace BlackMarketBrawlersBigData.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult DataMining()
        {
            return View();
        }

        public ActionResult startDataMining()
        {
            //Variable Declaration
            string[] systemFiles;
            JArray dataArray = new JArray();
            int count = 1;
            Dictionary<uint, Champion> champions = new Dictionary<uint, Champion>();
            JsonSerializer serializer = new JsonSerializer();

            //Parse through Data set given by RiotGames for MatchID's
            using (StreamReader textReader = System.IO.File.OpenText(HttpRuntime.AppDomainAppPath + "JSON\\NA.json"))
            {
                using (JsonTextReader jsonReader = new JsonTextReader(textReader))
                {
                    dataArray = (JArray)JToken.ReadFrom(jsonReader)["DataSet"];
                }
            }

            //Foreach ID from dataset getMatch Data and store it in a Json File
            foreach (var data in dataArray)
            {
                //getAndWriteJsonFilesToIO(count, data);
            }

            //get all those files and set them in a string array
            systemFiles = Directory.GetFiles(HttpRuntime.AppDomainAppPath + "JSON\\DataSet\\");

            //go through each file and parse champion Data
            foreach (string s in systemFiles)
            {
                champions = parseThroughJsonFileForChampData(s, champions);
            }

            //Store Champ Json Files
            storeChampJsonData(champions);

            return Json("Done!", JsonRequestBehavior.AllowGet);
        }

        //Gets all the Champion Json Files and returns them to the front end to be manipulated
        public ActionResult getChampions()
        {
            List<Champion> championRows = new List<Champion>();
            string[] systemFiles = System.IO.Directory.GetFiles(HttpRuntime.AppDomainAppPath + "JSON\\Champions\\");
            foreach (string s in systemFiles)
            {
                using (StreamReader textReader = System.IO.File.OpenText(s))
                {
                    using (JsonTextReader jsonReader = new JsonTextReader(textReader))
                    {
                        Champion champ = JsonConvert.DeserializeObject<Champion>(JToken.ReadFrom(jsonReader).ToString());
                        champ.banRate = Math.Round(champ.banRate, 2);
                        champ.deathRate = Math.Round(champ.deathRate, 2);
                        champ.killRate = Math.Round(champ.killRate, 2);
                        champ.AverageKDA = Math.Round(champ.averageKillDeathRate(), 2);
                        championRows.Add(champ);
                    }
                }
            }
            string temp = JsonConvert.SerializeObject(championRows);
            JsonResult result = new JsonResult();
            result.Data = temp;
            result.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            result.MaxJsonLength = Int32.MaxValue;
            return result;
        }

        //Adds new champion to the Champions dictionary
        private Champion addNewChampion(uint ID, JToken item)
        {
            Champion Champ = new Champion();
            Champ.itemStats = new Dictionary<uint, Items>();
            Champ.ID = ID;
            Champ.totalDeaths += (uint)item["stats"]["deaths"];
            Champ.totalKills += (uint)item["stats"]["kills"];
            Champ.lanePrefCount = new Dictionary<Champion.Lanes, int>();
            for (int i = 0, max = 6; i < max; i++)
            {
                Items Item = new Items();
                Item.ID = (uint)item["stats"]["item" + i];
                if (Champ.itemStats.ContainsKey(Item.ID))
                {
                    Champ.itemStats[Item.ID].numberOfTimesUsed++;
                }
                else
                {
                    Item.numberOfTimesUsed++;
                    Champ.itemStats.Add(Item.ID, Item);
                }
            }
            switch ((string)item["timeline"]["lane"])
            {
                case "TOP":
                    {
                        Champ.lanePrefCount.Add(Champion.Lanes.TOP, 0);
                    }
                    break;
                case "JUNGLE":
                    {
                        Champ.lanePrefCount.Add(Champion.Lanes.JUNGLE, 0);
                    }
                    break;
                case "BOTTOM":
                    {
                        Champ.lanePrefCount.Add(Champion.Lanes.BOTTOM, 0);
                    }
                    break;
                case "MIDDLE":
                    {
                        Champ.lanePrefCount.Add(Champion.Lanes.MIDDLE, 0);
                    }
                    break;
            }
            Champ.gameCount++;
            return Champ;
        }

        //Updates a champion from the Champions Dictionary
        private Champion updateChampion(Dictionary<uint, Champion> champions, uint ID, JToken item)
        {
            for (int i = 0, max = 6; i < max; i++)
            {
                Items Item = new Items();
                Item.ID = (uint)item["stats"]["item" + i];
                if (champions[ID].itemStats.ContainsKey(Item.ID))
                {
                    champions[ID].itemStats[Item.ID].numberOfTimesUsed++;
                }
                else
                {
                    Item.numberOfTimesUsed++;
                    champions[ID].itemStats.Add(Item.ID, Item);
                }
            }
            champions[ID].totalDeaths += (uint)item["stats"]["deaths"];
            champions[ID].totalKills += (uint)item["stats"]["kills"];
            champions[ID].gameCount++;
            switch ((string)item["timeline"]["lane"])
            {
                case "TOP":
                    {
                        if (champions[ID].lanePrefCount.ContainsKey(Champion.Lanes.TOP))
                        {
                            champions[ID].lanePrefCount[Champion.Lanes.TOP]++;
                        }
                        else
                        {
                            champions[ID].lanePrefCount.Add(Champion.Lanes.TOP, 0);
                        }
                    }
                    break;
                case "JUNGLE":
                    {
                        if (champions[ID].lanePrefCount.ContainsKey(Champion.Lanes.JUNGLE))
                        {
                            champions[ID].lanePrefCount[Champion.Lanes.JUNGLE]++;
                        }
                        else
                        {
                            champions[ID].lanePrefCount.Add(Champion.Lanes.JUNGLE, 0);
                        }
                    }
                    break;
                case "BOTTOM":
                    {
                        if (champions[ID].lanePrefCount.ContainsKey(Champion.Lanes.BOTTOM))
                        {
                            champions[ID].lanePrefCount[Champion.Lanes.BOTTOM]++;
                        }
                        else
                        {
                            champions[ID].lanePrefCount.Add(Champion.Lanes.BOTTOM, 0);
                        }
                    }
                    break;
                case "MIDDLE":
                    {
                        if (champions[ID].lanePrefCount.ContainsKey(Champion.Lanes.MIDDLE))
                        {
                            champions[ID].lanePrefCount[Champion.Lanes.MIDDLE]++;
                        }
                        else
                        {
                            champions[ID].lanePrefCount.Add(Champion.Lanes.MIDDLE, 0);
                        }
                    }
                    break;
            }
            return champions[ID];
        }

        //Looks at banned characters and manipulates them
        private void workOnBans(Dictionary<uint, Champion> champions, JToken bans, Games newGame)
        {
            if (bans != null)
            {
                foreach (var ban in bans)
                {
                    uint ID = (uint)ban["championId"];
                    if (champions.ContainsKey(ID))
                    {
                        champions[ID].banCount++;
                    }
                    else
                    {
                        Champion dchamp = new Champion();
                        dchamp.ID = ID;
                        dchamp.banCount++;
                        newGame.championsBanned.Add(dchamp);
                    }
                }
            }
        }

        //Geta and writes Json files to IO from data set
        private void getAndWriteJsonFilesToIO(int count, JToken data)
        {
            RestfulServices rest = new RestfulServices();
            string[] systemFiles = Directory.GetFiles(HttpRuntime.AppDomainAppPath + "JSON\\DataSet\\");
            if (!systemFiles.Contains("C:\\Programming\\BlackMarketBrawlersBigData\\BlackMarketBrawlersBigData\\JSON\\DataSet\\" + (string)data + ".json"))
            {
                if (count % 10 == 0)
                {
                    System.Threading.Thread.Sleep(15000);
                    JObject temp = rest.getMatchHistory((int)data);
                    string json = JsonConvert.SerializeObject(temp);
                    System.IO.File.WriteAllText(HttpRuntime.AppDomainAppPath + "\\JSON\\DataSet\\" + (string)data + ".json", json);
                    count++;
                }
                else
                {
                    JObject temp = rest.getMatchHistory((int)data);
                    string json = JsonConvert.SerializeObject(temp);
                    System.IO.File.WriteAllText(HttpRuntime.AppDomainAppPath + "\\JSON\\DataSet\\" + (string)data + ".json", json);
                    count++;
                }
            }
        }

        //Parses through Match Json file for champion information
        private Dictionary<uint, Champion> parseThroughJsonFileForChampData(string s, Dictionary<uint, Champion> champions)
        {
            JObject Match = new JObject();
            using (StreamReader textReader = System.IO.File.OpenText(s))
            {
                using (JsonTextReader jsonReader = new JsonTextReader(textReader))
                {
                    Match = (JObject)JToken.ReadFrom(jsonReader);

                    Games newGame = new Games();
                    newGame.championsBanned = new List<Champion>();
                    newGame.championsInGame = new List<Champion>();
                    newGame.MatchID = (uint)Match["matchId"];

                    if (Match.HasValues)
                    {
                        var parTemp = Match.GetValue("participants");
                        foreach (var item in parTemp)
                        {
                            uint ID = (uint)item["championId"];
                            if (champions.ContainsKey(ID))
                            {
                                champions[ID] = updateChampion(champions, ID, item);
                            }
                            else
                            {
                                Champion newChamp = addNewChampion(ID, item);
                                champions.Add(newChamp.ID, newChamp);
                            }
                        }
                        var banChamps = Match.GetValue("teams");
                        foreach (var team in banChamps)
                        {
                            var bans = team["bans"];
                            workOnBans(champions, bans, newGame);
                        }
                    }
                }
            }
            return champions;
        }

        //Stores champions information in a Json file alongside image files for items
        private void storeChampJsonData(Dictionary<uint, Champion> champions) {
            RestfulServices rest = new RestfulServices();
            foreach (var champ in champions)
            {
                JObject champData = rest.getChampionName(champ.Key);
                champ.Value.ChampionName = (string)champData["name"];
                champ.Value.deathRate = Math.Round(champ.Value.averageDeathRate(), 2);
                champ.Value.killRate = Math.Round(champ.Value.averageKillRate(), 2);
                champ.Value.banRate = Math.Round(champ.Value.averageBanRate(), 2);
                champ.Value.AverageKDA = Math.Round(champ.Value.averageKillDeathRate(), 2);
                if (champ.Value.itemStats.ContainsKey(0))
                {
                    champ.Value.itemStats.Remove(0);
                }
                foreach (var item in champ.Value.itemStats)
                {
                        item.Value.itemName = (string)rest.getItemName(item.Value.ID)["name"];
                        item.Value.imagePath = rest.getImageFiles(item.Value.ID, item.Value.itemName);
                }
                string json = JsonConvert.SerializeObject(champ.Value);
                System.IO.File.WriteAllText(HttpRuntime.AppDomainAppPath + "\\JSON\\Champions\\" + champ.Value.ChampionName + ".json", json);
            }
        }

    }

    //Restful calls to different API's for Json files
    public class RestfulServices
    {
        private string matchUri = "https://na.api.pvp.net/api/lol/na/v2.2/match/{0}?includeTimeline=true&api_key={1}";
        private string champUri = "https://global.api.pvp.net/api/lol/static-data/na/v1.2/champion/{0}?champData=info&api_key={1}";
        private string imageUri = "http://ddragon.leagueoflegends.com/cdn/5.15.1/img/item/{0}.png";
        private string itemUri = "https://global.api.pvp.net/api/lol/static-data/na/v1.2/item/{0}?itemData=into&api_key={1}";

        public JObject getMatchHistory(int id)
        {
            JObject returnValue = new JObject();
            using (HttpClient httpClient = new HttpClient())
            {
                string temp = string.Format(matchUri, id, BlackMarketBrawlersBigData.Properties.Resources.API_KEY);
                httpClient.BaseAddress = new Uri(temp);
                httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                HttpResponseMessage response = httpClient.GetAsync(httpClient.BaseAddress).Result;
                if (response.IsSuccessStatusCode)
                {
                    JsonSerializer ser = new JsonSerializer();
                    StreamReader reader = new StreamReader(response.Content.ReadAsStreamAsync().Result);
                    TextReader read = reader;
                    JsonTextReader jReader = new JsonTextReader(read);
                    returnValue = ser.Deserialize<JObject>(jReader);
                }
            }
            return returnValue;
        }

        public JObject getChampionName(uint id)
        {
            JObject returnValue = new JObject();
            using (HttpClient httpClient = new HttpClient())
            {
                string temp = string.Format(champUri, id, BlackMarketBrawlersBigData.Properties.Resources.API_KEY);
                httpClient.BaseAddress = new Uri(temp);
                httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                HttpResponseMessage response = httpClient.GetAsync(httpClient.BaseAddress).Result;
                if (response.IsSuccessStatusCode)
                {
                    JsonSerializer ser = new JsonSerializer();
                    StreamReader reader = new StreamReader(response.Content.ReadAsStreamAsync().Result);
                    TextReader read = reader;
                    JsonTextReader jReader = new JsonTextReader(read);
                    returnValue = ser.Deserialize<JObject>(jReader);
                }
            }
            return returnValue;
        }

        public JObject getItemName(uint id)
        {
            JObject returnValue = new JObject();
            using (HttpClient httpClient = new HttpClient())
            {
                string temp = string.Format(itemUri, id, BlackMarketBrawlersBigData.Properties.Resources.API_KEY);
                httpClient.BaseAddress = new Uri(temp);
                httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                HttpResponseMessage response = httpClient.GetAsync(httpClient.BaseAddress).Result;
                if (response.IsSuccessStatusCode)
                {
                    JsonSerializer ser = new JsonSerializer();
                    StreamReader reader = new StreamReader(response.Content.ReadAsStreamAsync().Result);
                    TextReader read = reader;
                    JsonTextReader jReader = new JsonTextReader(read);
                    returnValue = ser.Deserialize<JObject>(jReader);
                }
            }
            return returnValue;
        }

        public string getImageFiles(uint id, string itemName)
        {
            WebClient wc = new WebClient();
            string temp = string.Format(imageUri, id);
            if (itemName != null && id != 0)
            {
                byte[] data = wc.DownloadData(temp);
                MemoryStream memstream = new MemoryStream(data);
                Image img = Image.FromStream(memstream);
                if (itemName != null)
                {
                    if (!System.IO.File.Exists(HttpRuntime.AppDomainAppPath + "\\Images\\ItemImages\\" + itemName.Replace(":", "") + ".png"))
                    {
                        img.Save(HttpRuntime.AppDomainAppPath + "\\Images\\ItemImages\\" + itemName.Replace(":", "") + ".png", ImageFormat.Png);
                    }
                    return HttpRuntime.AppDomainAppPath + "\\Images\\ItemImages\\" + itemName + ".png";
                }
                else
                {
                    return HttpRuntime.AppDomainAppPath + "\\Images\\ItemImages\\failed.png";
                }
            }
            else
            {
                return "Failed";
            }
        }

    }
}
