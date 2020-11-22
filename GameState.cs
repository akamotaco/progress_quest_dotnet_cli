using System;
using System.Collections.Generic;

namespace pq_dotnet
{
    internal class GameState
    {
        private readonly Random random;
        public string Task;
        public int Tasks;
        public string Kill;
        public int Act;

        public string QuestMonster;
        public int QuestMonsterIndex;
        public string BestQuest;

        public GameState(GameConfig config)
        {
            this.Task = "";
            this.random = new Random();
            this.Tasks = 0;
            this.Kill = "Loading....";
            this.Act = 0;
            this.QuestMonster = "";
            this.QuestMonsterIndex = -1;
            // [FormCreate]

            // AllLists = [Traits,Stats,Spells,Equips,Inventory,Plots,Quests];

            // storage.loadSheet(name, LoadGame);

            // StartTimer();

            this.TaskBar = new ProgressBar("TaskBar", "$percent%", config.TaskBar.max, config.TaskBar.position);
            this.QuestBar = new ProgressBar("QuestBar", "$percent% complete", config.QuestBar.max, config.QuestBar.position);
            this.PlotBar = new ProgressBar("PlotBar", "$time remaining", config.PlotBar.max, config.PlotBar.position);

            this.Quests = new List<(string, bool)>();
            this.Q = new Queue<string>();
        }

        public ProgressBar TaskBar { get; }
        public ProgressBar QuestBar { get; }
        public ProgressBar PlotBar { get; }
        public List<(string quest, bool completed)> Quests { get; }
        public Queue<string> Q { get; internal set; }

        internal int Random(int n)
        {
            return this.random.Next() % n;
        }

        internal int RandomLow(int below)
        {
            return Math.Min(Random(below), Random(below));
        }

        internal void CheckAll(List<(string quest, bool completed)> quests)
        {
            for(var i=0;i<quests.Count;++i) {
                quests[i] = (quests[i].quest, true);
            }
        }

        // internal delegate int RandomGenerator(int n);
    }
}