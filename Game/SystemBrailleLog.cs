using System.Collections.Generic;
using ECS;
using System;
using System.Linq;
using System.Text;

namespace Game
{
    public class BrailleLog : ECS.System
    {
        private ConsoleCanvas canvas;

        public BrailleLog(int cols, int rows) : base(typeof(Character), typeof(GameState)) {
            this.canvas = new ConsoleCanvas(cols,rows);
        }
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
            this.canvas.Clear();
            this.canvas.Print(2,24,"[Q]uit key...");
            
            this.canvas.Print(2,2, $"Act:{gameState.Act}\t[{gameState.Kill}]");

            int printLimit = 5;
            this.canvas.Print(2,4, "[Quests]");
            for(var i=0;i<printLimit;++i) {
                if(i >= gameState.Quests.Count) {
                    break;
                }
                this.canvas.Print(2,4+1+i,$"{gameState.Quests[i].quest}[{(gameState.Quests[i].completed ? "X" : " ")}]");
            }
            if(gameState.Quests.Count >= printLimit)
                this.canvas.Print(2,4+1+printLimit, "...");
            
            this.canvas.Print(2, 13, "[Plot]");
            for(var i=0;i<printLimit;++i) {
                if(i >= gameState.Plots.Count) {
                    break;
                }
                this.canvas.Print(2, 13+1+i, $"{gameState.Plots[i].quest}[{(gameState.Plots[i].completed ? "X" : " ")}]");
            }
            if(gameState.Plots.Count >= printLimit)
                this.canvas.Print(2, 10+1+printLimit, "...");

            this.canvas.Print(30,4, "[Inventory]");
            var inv_k = character.Inventory.Keys.ToArray();
            var inv_v = character.Inventory.Values.ToArray();
            for(var i=0;i<printLimit;++i) {
                if(i >= character.Inventory.Count) {
                    break;
                }
                this.canvas.Print(30, 4+1+i, $"{inv_k[i]}:{inv_v[i]}");
            }
            if(character.Inventory.Count >= printLimit)
                this.canvas.Print(30, 4+1+printLimit, "...");

            this.canvas.Print(2,  22, $"Exp:{character.ExpBar}");
            this.canvas.Print(20, 22, $"Encumbrance:{character.EncumBar}");
            this.canvas.Print(42, 22, $"Progress:{gameState.TaskBar}");
            this.canvas.Print(2, 23, $"Plot:{gameState.PlotBar}");
            this.canvas.Print(20, 23, $"Quest:{gameState.QuestBar}");
            this.canvas.Print(42, 23, $"Task:{gameState.Task}");

            this.canvas.Draw();
        }


    }

    public class ConsoleCanvas
    {
        public int Cols {get; private set;}
        public int Rows {get; private set;}
        private char[,] _buffer;

        public ConsoleCanvas(int cols, int rows) {
            Cols = cols;
            Rows = rows;
            _buffer = new char[rows, cols];
        }

        public void Draw() {
            Console.SetCursorPosition(0, 0);
            var endline = Rows-1;
            for(var y=0;y<endline;++y) {
                for(var x=0;x<Cols;++x)
                    Console.Write(_buffer[y, x]);
                Console.WriteLine();
            }
            for(var x=0;x<Cols;++x)
                Console.Write(_buffer[endline, x]);
        }

        public void Print(int x, int y, string content) {
            byte[] bytes = Encoding.ASCII.GetBytes(content);
            for(var i=0;i<bytes.Length;++i) {
                if(x+i >= Cols)
                    continue;
                _buffer[y, x+i] = (char)bytes[i];
            }
        }

        internal void Clear()
        {
            for(var y=0;y<Rows;++y) for(var x=0;x<Cols;++x)
                _buffer[y, x] = ' ';
        }
    }
}