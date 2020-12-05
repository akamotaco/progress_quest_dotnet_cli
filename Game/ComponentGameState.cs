using System;
using System.Collections.Generic;
using pq_dotnet;

namespace Game
{
    public class GameState : ECS.Component
    {
        private readonly Random random;
        public string Task;
        public int Tasks;
        public string Kill;
        public int Act;

        public string QuestMonster;
        public int QuestMonsterIndex;
        public string BestQuest;

        public GameState(Random instance, GameConfig config, ECS.Entity parent) : base(parent) {
            this.Task = "";
            if(instance == null)
                this.random = new Random();
            else
                this.random = instance;
            this.Tasks = 0;
            this.Kill = "Loading....";
            this.Act = 0;
            this.QuestMonster = "";
            this.QuestMonsterIndex = -1;
            this.BestPlot = "Prologue";
            // [FormCreate]

            // AllLists = [Traits,Stats,Spells,Equips,Inventory,Plots,Quests];

            // storage.loadSheet(name, LoadGame);

            // StartTimer();

            this.TaskBar = new ProgressBar("TaskBar", "$percent%", config.TaskBar.max, config.TaskBar.position);
            this.QuestBar = new ProgressBar("QuestBar", "$percent% complete", config.QuestBar.max, config.QuestBar.position);
            this.PlotBar = new ProgressBar("PlotBar", "$time remaining", config.PlotBar.max, config.PlotBar.position);

            this.Quests = new List<(string, bool)>();
            this.Plots = new List<(string, bool)>();
            this.Plots.Add(("Prologue", false));
            this.Q = new Queue<string>();
            var pre_queue = new string[] {
                "task|10|Experiencing an enigmatic and foreboding night vision",
                "task|6|Much is revealed about that wise old bastard you'd underestimated",
                "task|6|A shocking series of events leaves you alone and bewildered, but resolute",
                "task|4|Drawing upon an unrealized reserve of determination, you set out on a long and dangerous journey",
                "plot|2|Loading"
                };
            foreach(var queue in pre_queue)
                this.Q.Enqueue(queue);
        }

        public ProgressBar TaskBar { get; }
        public ProgressBar QuestBar { get; }
        public ProgressBar PlotBar { get; }
        public List<(string quest, bool completed)> Quests { get; }
        public List<(string quest, bool completed)> Plots { get; }
        public Queue<string> Q { get; internal set; }
        public string BestPlot { get; internal set; }

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