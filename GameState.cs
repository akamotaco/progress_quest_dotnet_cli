using System;

namespace pq_dotnet
{
    internal class GameState
    {
        private readonly Random random;
        public string Task;
        public int Tasks;
        public string Kill;

        public GameState(GameConfig config)
        {
            this.Task = "";
            this.random = new Random();
            this.Tasks = 0;
            this.Kill = "Loading....";
            // [FormCreate]
            // PlotBar =  new ProgressBar("PlotBar", "$time remaining");
            // QuestBar = new ProgressBar("QuestBar", "$percent% complete");
            // TaskBar =  new ProgressBar("TaskBar", "$percent%");

            // AllLists = [Traits,Stats,Spells,Equips,Inventory,Plots,Quests];

            // storage.loadSheet(name, LoadGame);

            // StartTimer();

            TaskBar = new ProgressBar("QuestBar", "$percent% complete", config.TaskBar.max, config.TaskBar.position);
        }

        public ProgressBar TaskBar { get; }

        internal int Random(int n)
        {
            return this.random.Next() % n;
        }

        internal int RandomLow(int below)
        {
            return Math.Min(Random(below), Random(below));
        }

        // internal delegate int RandomGenerator(int n);
    }
}