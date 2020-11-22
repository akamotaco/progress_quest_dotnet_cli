using System;

namespace pq_dotnet
{
    class Program
    {
        private static Random global_rand;

        static void Main(string[] args)
        {
            Console.WriteLine("Progress Quest");
            
            var character = new Character();
            var config = new GameConfig();
#region character part
            RollCharacter(ref character, config);
            character.Sold(config);

            Console.WriteLine(character);
#endregion
            var gameState = new GameState(config);

            for(int i=0;i<10;++i) {
                //timer loop
                int key = GameSystem.ReadKey(1.0f);
                Console.Write(".");
                if(key == 'q')
                    break;
                GameSystem.Step(character, gameState, config);

                Console.WriteLine("TaskBar:"+gameState.TaskBar);
            }

        }

        private static void RollCharacter(ref Character character, GameConfig config)
        {
            global_rand = new Random(character.RandomSeed());

            int total = 0;
            var best = -1;

            foreach(string stat in config.PrimeStats) {
                total += Roll(ref character, stat);
                if (best < character.GetStat(stat)) {
                    best = character.GetStat(stat);
                    character.Best = stat;
                }
            }
            character.NewStat("HP Max", Random(8) + character.GetStat("CON")/6);
            character.NewStat("MP Max", Random(8) + character.GetStat("INT")/6);
        }

        private static int Roll(ref Character character, string stat)
        {
            character.NewStat(stat, 3 + Random(6) + Random(6) + Random(6));

            return character.GetStat(stat);
        }

        private static int Random(int v)
        {
            return global_rand.Next() % v;
        }
    }
}
