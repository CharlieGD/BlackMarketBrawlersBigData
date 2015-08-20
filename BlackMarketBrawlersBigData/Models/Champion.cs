using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BlackMarketBrawlersBigData.Models
{
    //Class for champions
    public class Champion
    {
        //Champions ID
        public uint ID { get; set; }

        //champions Name
        public string ChampionName { get; set; }

        //Enum to classify the names of the lanes as int
        public enum Lanes {
             TOP = 0,
             MIDDLE = 1,
             BOTTOM = 2,
             JUNGLE =3
        }

        //Lane ID with how many times the champion was in that lane
        public Dictionary<Lanes, int> lanePrefCount { get; set; }

        //how often the champion got banned 
        public uint banCount { get; set; }

        //how many games this champion played
        public uint gameCount { get; set; }

        //item id to hook up with the item so queries could be done on the items
        public Dictionary<uint, Items> itemStats { get; set; }

        // f(x) = (banCount)/(gameCount);
        public double banRate { get; set; }

        // f(x) = (totalKills)/(gameCount);
        public double killRate { get; set; }

        // f(x) = (totalDeaths)/(gameCount);
        public double deathRate { get; set; }

        // total number of kills the champion combined each game
        public uint totalKills { get; set; }

        // total number of deaths the champion combined each game
        public uint totalDeaths { get; set; }

        //  f(x) = (totalKills)/(totalDeaths);
        public double AverageKDA { get; set; }

        public double averageBanRate()
        {
            return this.banCount / (double)this.gameCount;
        }

        public double averageKillRate()
        {
            double averageKillRate;
            averageKillRate = this.totalKills / (double)this.gameCount;
            return averageKillRate;
        }

        public double averageDeathRate()
        {
            double averageDeathRate;
            averageDeathRate = (this.totalDeaths / (double)this.gameCount);
            return averageDeathRate;
        }

        public double averageKillDeathRate()
        {
            return averageKillRate() / averageDeathRate();
        }

    }

    //Public class depicting the match ID, which champions where invloved, and which champions where banned
    public class Games
    {
        public uint MatchID { get; set; }
        public List<Champion> championsInGame { get; set; }
        public List<Champion> championsBanned { get; set; }

    }

    //Public class of items by ID, thier name, imagePath, and how many times a champion used them
    public class Items
    {
        public uint ID { get; set; }
        public string itemName { get; set; }
        public string imagePath { get; set; }
        public uint numberOfTimesUsed { get; set; }
    }
}