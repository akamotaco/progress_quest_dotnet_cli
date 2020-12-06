using System;
using System.Linq;

namespace pq_dotnet
{
    class Program
    {
        static void Main(string[] args)
        {
            int UiMode = 0;

            Console.WriteLine("Progress Quest");
            var config = new Game.GameConfig();

            var hub = new ECS.SystemHub();

#region add entities    
            var newGame = new Game.NewInstance(config);
            newGame.RollCharacter();
            hub.Add(newGame);
#endregion

#region add systems
            var game = new Game.Game(config);
            hub.AddSystem(game);

            switch(UiMode) {
                case 0:
                    hub.AddSystem(new Game.BasicLog());
                    break;
                case 1:
                    hub.AddSystem(new Game.BrailleLog(80, 26));
                    break;
                default:
                    throw new Exception("unknown UiMode.");
            }
#endregion

            while(game.IsLoop) {
                hub.Step(1000);
            }

        }
    }
}
