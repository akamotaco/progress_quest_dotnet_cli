using System;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;

namespace pq_dotnet
{
    public class GameSystem
    {
        public static int ReadKey(float seconds, int term=250)
        {
            DateTime beginWait = DateTime.Now;
            while (!Console.KeyAvailable && DateTime.Now.Subtract(beginWait).TotalSeconds < seconds)
                Thread.Sleep(term);

            if(Console.KeyAvailable)
                return Console.ReadKey(true).KeyChar;
            return -1;
        }

        static public int Pos(string needle, string haystack) {
            return haystack.IndexOf(needle) + 1;
        }

        internal static void Step(Character character, GameState gameState, GameConfig config)
        {
            if(gameState.TaskBar.Done()) {
                gameState.Tasks += 1;
                // game.elapsed += TaskBar.Max().div(1000);

                // ClearAllSelections();

                if (gameState.Kill == "Loading....")
                gameState.TaskBar.Reset(0);  // Not sure if this is still the ticket

                // gain XP / level up
                var gain = Pos("kill|", gameState.Task) == 1;
                if (gain) {
                if (character.ExpBar.Done())
                    LevelUp(ref character, gameState, config);
                else
                    character.ExpBar.Increment(gameState.TaskBar.Max / 1000);
                }

                // // advance quest
                // if (gain && game.act >= 1) {
                // if (QuestBar.done() || !Quests.length()) {
                //     CompleteQuest();
                // } else {
                //     QuestBar.increment(TaskBar.Max() / 1000);
                // }
                // }

                // // advance plot
                // if (gain || !game.act) {
                // if (PlotBar.done())
                //     InterplotCinematic();
                // else
                //     PlotBar.increment(TaskBar.Max() / 1000);
                // }

                // Dequeue();
            }
            else {
                // var elapsed = timeGetTime() - lasttick;
                // if (elapsed > 100) elapsed = 100;
                // if (elapsed < 0) elapsed = 0;
                // TaskBar.increment(elapsed);
                gameState.TaskBar.Increment(100);
            }
        }

        private static void LevelUp(ref Character character, GameState gameState, GameConfig config)
        {
            character.Traits.Level += 1;
            character.SetStat("HP Max", character.GetStat("CON") / 3 + 1 + gameState.Random(4));
            character.SetStat("MP Max", character.GetStat("INT") / 3 + 1 + gameState.Random(4));
            WinStat(ref character, gameState, config);
            WinStat(ref character, gameState, config);
            WinSpell(ref character, gameState, config);
            character.ExpBar.Reset(config.LevelUpTime(character.Traits.Level));
            // Brag('level');
        }

        private static void WinSpell(ref Character character, GameState gameState, GameConfig config)
        {
            AddR(character.Spells, config.Spells[gameState.RandomLow(Math.Min(character.GetStat("WiS") + character.Traits.Level, config.Spells.Length))], 1);
        }

        private static void AddR(Dictionary<string, string> dict, string key, int value)
        {
            dict[key] = ToRoman(value + ToArabic(dict[key]));
        }

        private static string ToRoman(int n)
        {
            // if (!n) return "N";
            string s = "";
            if (n < 0) {
                s = "-";
                n = -n;
            }
            while(_rome(ref n, ref s, 1000,"M"));
            _rome(ref n, ref s, 900,"CM");
            _rome(ref n, ref s, 500,"D");
            _rome(ref n, ref s, 400,"CD");
            while (_rome(ref n, ref s, 100,"C"));
            _rome(ref n, ref s, 90,"XC");
            _rome(ref n, ref s, 50,"L");
            _rome(ref n, ref s, 40,"XL");
            while (_rome(ref n, ref s, 10,"X"));
            _rome(ref n, ref s, 9,"IX");
            _rome(ref n, ref s, 5,"V");
            _rome(ref n, ref s, 4,"IV");
            while (_rome(ref n, ref s, 1,"I"));
            return s;
        }

        private static bool _rome(ref int n, ref string s, int dn, string ds)
        {
            if(n >= dn) {
                n -= dn;
                s += ds;
                return true;
            }
            else return false;
        }

        private static int ToArabic(string s)
        {
            int n = 0;
            s = s.ToUpper();

            while(_arab(ref s, ref n, "M", 1000));
            _arab(ref s, ref n, "CM",900);
            _arab(ref s, ref n, "D",500);
            _arab(ref s, ref n, "CD",400);
            while (_arab(ref s, ref n, "C",100));
            _arab(ref s, ref n, "XC",90);
            _arab(ref s, ref n, "L",50);
            _arab(ref s, ref n, "XL",40);
            while (_arab(ref s, ref n, "X",10));
            _arab(ref s, ref n, "IX",9);
            _arab(ref s, ref n, "V",5);
            _arab(ref s, ref n, "IV",4);
            while (_arab(ref s, ref n, "I",1));
            return n;
        }

        private static bool _arab(ref string s, ref int n, string ds, int dn)
        {
            if(!Starts(s, ds)) return false;
            s = s.Substring(ds.Length);
            n += dn;
            return true;
        }

        private static bool Starts(string s, string pre)
        {
            return 0 == s.IndexOf(pre);
        }

        private static void WinStat(ref Character character, GameState gameState, GameConfig config)
        {
            string i = "";
            if(Odds(1,2, gameState)) {
                i = Pick(config.Stats, gameState);
            } else {
                // Favor the best stat so it will tend to clump
                int t = 0;
                foreach(var key in config.Stats) {
                    t += Square(character.GetStat(key));
                }
                t = gameState.Random(t);
                foreach(var key in config.Stats) {
                    i = key;
                    t -= Square(character.GetStat(key));
                    if(t<0) break;
                }
            }

            Debug.Assert(i == "");
            
            character.SetStat(i, character.GetStat(i) + 1);
        }

        private static int Square(int x)
        {
            return x*x;
        }

        private static string Pick(string[] stats, GameState gameState)
        {
            return stats[gameState.Random(stats.Length)];
        }

        private static bool Odds(int chance, int outof, GameState gameState)
        {
            return gameState.Random(outof) < chance;
        }
    }
}