using System;
using System.Linq;

namespace pq_dotnet
{
    class Program
    {
        private static Random global_rand;

        static void Main(string[] args)
        {
            // brille.Program.Test1();
            // brille.Program.Test2();
            // brille.Program.Test3();
            // return;

            Console.WriteLine("Progress Quest");
            var config = new Game.GameConfig(new Random());

            var hub = new ECS.SystemHub();

#region add entities    
            var newGame = new Game.NewInstance(config);
            hub.Add(newGame);
#endregion

#region add systems
            var game = new Game.Game(config);
            hub.AddSystem(game);
            hub.AddSystem(new Game.BasicLog());
#endregion

            while(game.IsLoop) {
                hub.Step(1);
            }

        }
    }
}
