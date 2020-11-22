using System;
using System.Linq;

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

            while(true) {
                //timer loop
                int key = GameSystem.ReadKey(1.0f);
                // Console.Write(".");
                if(key == 'q')
                    break;
                GameSystem.Step(character, gameState, config);


                PrintAll(character, gameState);
            }

        }

        private static void PrintAll(Character character, GameState gameState)
        {
            Console.WriteLine("==========================================");
            Console.WriteLine($"Act:{gameState.Act}\t[{gameState.Kill}]");

            int printLimit = 5;
            Console.Write("Quests:");
            for(var i=0;i<printLimit;++i) {
                if(i >= gameState.Quests.Count) {
                    Console.WriteLine("");
                    break;
                }
                Console.Write(gameState.Quests[i].quest+",");
            }
            if(gameState.Quests.Count >= printLimit)
                Console.WriteLine("...");
            
            Console.Write("Plot:");
            for(var i=0;i<printLimit;++i) {
                if(i >= gameState.Plots.Count) {
                    Console.WriteLine("");
                    break;
                }
                Console.Write(gameState.Plots[i].quest+",");
            }
            if(gameState.Plots.Count >= printLimit)
                Console.WriteLine("...");

            Console.Write("Inventory:");
            var inv_k = character.Inventory.Keys.ToArray();
            var inv_v = character.Inventory.Values.ToArray();
            for(var i=0;i<printLimit;++i) {
                if(i >= character.Inventory.Count) {
                    Console.WriteLine("");
                    break;
                }
                Console.Write($"{inv_k[i]}:{inv_v[i]},");
            }
            if(character.Inventory.Count >= printLimit)
                Console.WriteLine("...");

            Console.WriteLine($"Exp:{character.ExpBar}\t Encumbrance:{character.EncumBar}");
            Console.WriteLine($"Quest:{gameState.QuestBar}\t Plot:{gameState.PlotBar}");
            Console.WriteLine($"Task:{gameState.Task}{gameState.TaskBar}");
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
