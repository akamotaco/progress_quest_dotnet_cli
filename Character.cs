using System;
using System.Collections.Generic;

namespace pq_dotnet
{
    internal class Character
    {
        private Dictionary<string, int> stats;
        private int seed;

        public Traits Traits { get; private set; }

        private DateTime birthday;
        private DateTime birthstamp;
        // private int elapsed;
        public string BestEquip;

        public Dictionary<string, string> Equips { get; private set; }
        public Dictionary<string, string> Spells { get; private set; }

        public ProgressBar ExpBar { get; private set; }
        public ProgressBar EncumBar { get; private set; }

        private DateTime date;
        private DateTime stamp;

        public string Best { get; internal set; }
        public Dictionary<string, int> Inventory { get; internal set; }

        public Character()
        {
            this.stats = new Dictionary<string, int>();
        }

        internal bool hasStat(string stat)
        {
            return this.stats.ContainsKey(stat);
        }

        internal void Sold(GameConfig config)
        {
            this.Traits = new Traits();
            this.birthday = new DateTime();
            this.birthstamp = new DateTime();
            this.BestEquip = "Sharp Rock";
            this.Equips = new Dictionary<string, string>();
            this.Inventory = new Dictionary<string, int>();
            this.Inventory["Gold"] = 0;
            this.Spells = new Dictionary<string, string>();
            this.ExpBar = new ProgressBar("ExpBar", "$remaining XP needed for next level", config.LevelUpTime(1), 0);
            this.EncumBar = new ProgressBar("EncumBar", "$position/$max cubits", GetStat("STR") + 10, 0);

            this.Traits.Name = config.GenerateName();
            this.Traits.Race = config.RandomRace();
            this.Traits.Class = config.RandomClass();
            this.Traits.Level = 1;

            this.date = this.birthday;
            this.stamp = this.birthstamp;

            foreach(var equip in this.Equips.Keys) { this.Equips[equip] = ""; }
            this.Equips["Weapon"] = this.BestEquip;
            this.Equips["Hauberk"] = "-3 Burlap";

        }

        internal bool SetStat(string stat, int value)
        {
            if(hasStat(stat)) {
                this.stats[stat] = value;
                return true;
            }
            return false;
        }

        internal int RandomSeed()
        {
            this.seed = System.Environment.TickCount;
            return this.seed;
        }

        internal void NewStat(string stat, int value)
        {
            if(hasStat(stat))
                throw new Exception($"{stat} is already exist");
            this.stats[stat] = value;
        }

        internal int GetStat(string stat)
        {
            int value;
            if( this.stats.TryGetValue(stat, out value) ) {
                return value;
            }
            throw new Exception($"{stat} is not stat");
        }

        public override string ToString()
        {
            var report = $"[Character:{this.Traits.Name} / {this.Traits.Race} / {this.Traits.Class}]\n";
            foreach(var key in this.stats.Keys) {
                report += $"[{key}:{this.stats[key]}]\n";
            }
            report += $"[Best:{this.Best}]\n";
            report += $"[Seed:{this.seed}]\n";
            return report;
        }
    }
}