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

            RollCharacter(ref character, config);
            character.Sold(config);

            Console.WriteLine(character);
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
