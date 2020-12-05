using System.Collections.Generic;
using ECS;
using System;
using System.Linq;

namespace Game
{
    public class BasicLog : ECS.System
    {
        public BasicLog() : base(typeof(Character), typeof(GameState)) {}
        protected override void Proc(int step, List<List<Component>> allComponents)
        {
            foreach(var components in allComponents) {
                var character = components[0] as Character;
                var gameState = components[1] as GameState;

                PrintAll(character, gameState);
            }
        }

        private void PrintAll(Character character, GameState gameState)
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
                Console.Write($"{gameState.Quests[i].quest}[{(gameState.Quests[i].completed ? "X" : " ")}],");
            }
            if(gameState.Quests.Count >= printLimit)
                Console.WriteLine("...");
            
            Console.Write("Plot:");
            for(var i=0;i<printLimit;++i) {
                if(i >= gameState.Plots.Count) {
                    Console.WriteLine("");
                    break;
                }
                Console.Write($"{gameState.Plots[i].quest}[{(gameState.Plots[i].completed ? "X" : " ")}],");
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


    }
}