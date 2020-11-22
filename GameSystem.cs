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

                // advance quest
                if (gain && gameState.Act >= 1) {
                if (gameState.QuestBar.Done() || gameState.Quests.Count == 0) {
                    CompleteQuest(ref character, ref gameState, config);
                } else {
                    gameState.QuestBar.Increment(gameState.TaskBar.Max / 1000);
                }
                }

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
                gameState.TaskBar.Increment(1000); // 1000 msec == 1 sec
            }
        }

        private static void CompleteQuest(ref Character character, ref GameState gameState, GameConfig config)
        {
            gameState.QuestBar.Reset(50 + gameState.RandomLow(1000));
            if(gameState.Quests.Count != 0) {
                // Log("Quest complete: " + gameState.BestQuest);
                gameState.CheckAll(gameState.Quests);

                switch(gameState.Random(4)) {
                    case 0: WinSpell(ref character, gameState, config); break;
                    case 1: WinEquip(ref character, gameState, config); break;
                    case 2: WinStat(ref character, gameState, config); break;
                    case 3: WinItem(ref character, gameState, config); break;
                }
            }

            while (gameState.Quests.Count > 99)
                gameState.Quests.RemoveAt(0);

            gameState.QuestMonster = "";

            string caption = "";
            switch (gameState.Random(5)) {
                case 0:
                    var level = character.Traits.Level;
                    var lev = 0;
                    for (var i = 1; i <= 4; ++i) {
                        var montag = gameState.Random(config.Monsters.Length);
                        var m = config.Monsters[montag];
                        var l = StrToInt(Split(m,1));
                        if (i == 1 || Math.Abs(l - level) < Math.Abs(lev - level)) {
                            lev = l;
                            gameState.QuestMonster = m;
                            gameState.QuestMonsterIndex = montag;
                        }
                    }
                    caption = "Exterminate " + Definite(Split(gameState.QuestMonster,0),2);
                    break;
                case 1:
                    caption = "Seek " + Definite(InterestingItem(gameState, config), 1);
                    break;
                case 2:
                    caption = "Deliver this " + BoringItem(gameState, config);
                    break;
                case 3:
                    caption = "Fetch me " + Indefinite(BoringItem(gameState, config), 1);
                    break;
                case 4:
                    var mlev = 0;
                    level = character.Traits.Level;
                    for (var ii = 1; ii <= 2; ++ii) {
                        var montag = gameState.Random(config.Monsters.Length);
                        var m = config.Monsters[montag];
                        var l = StrToInt(Split(m,1));
                        if ((ii == 1) || (Math.Abs(l - level) < Math.Abs(mlev - level))) {
                            mlev = l;
                            gameState.QuestMonster = m;
                        }
                    }
                    caption = "Placate " + Definite(Split(gameState.QuestMonster,0),2);
                    gameState.QuestMonster = "";  // We're trying to placate them, after all
                    break;
            }

            // if (!game.Quests) game.Quests = [];
            while (gameState.Quests.Count > 99) gameState.Quests.RemoveAt(0); // shift();
            Debug.Assert(caption == "");
            gameState.Quests.Add((caption, false));
            gameState.BestQuest = caption;
            // Quests.AddUI(caption);


            // Log('Commencing quest: ' + caption);

            // SaveGame();
        }

        private static string Indefinite(string s, int qty)
        {
            if(qty == 1) {
                if(Pos(s[0].ToString(), "AEIOU�aeiou�") > 0)
                    return "an " + s;
                else
                    return "a " + s;
            } else {
                return IntToStr(qty) + " " + Plural(s);
            }
        }

        private static string IntToStr(int qty)
        {
            return qty.ToString();
        }

        private static string BoringItem(GameState gameState, GameConfig config)
        {
            return Pick(config.BoringItems, gameState);
        }

        private static string Definite(string s, int qty)
        {
            if(qty > 1)
                s = Plural(s);
            return "the " + s;
        }

        private static string Plural(string s)
        {
            if(Ends(s, "y"))
                return s.Substring(0,s.Length-1) + "ies";
            else if(Ends(s, "us"))
                return s.Substring(0,s.Length-2) + "i";
            else if(Ends(s,"ch") || Ends(s,"x") || Ends(s,"s") || Ends(s, "sh"))
                return s + "es";
            else if(Ends(s,"f"))
                return s.Substring(s.Length-1) + "ves";
            else if(Ends(s,"man") || Ends(s,"Man"))
                return s.Substring(s.Length-2) + "en";
            else return s + "s";
        }

        private static bool Ends(string s, string e)
        {
            return s.Substring(0,s.Length-e.Length) == e;
        }

        private static void WinItem(ref Character character, GameState gameState, GameConfig config)
        {
            Add(character.Inventory, SpecialItem(gameState, config), 1);
        }

        private static void Add(Dictionary<string, int> dict, string key, int value)
        {
            var base_value = 0;
            dict.TryGetValue(key, out base_value);

            dict[key] = value + base_value;

            /*$IFDEF LOGGING*/
            if (value == 0) return;
            var line = (value > 0) ? "Gained" : "Lost";
            if (key == "Gold") {
                key = "gold piece";
                line = (value > 0) ? "Got paid" : "Spent";
            }
            if (value < 0) value = -value;
            // line = line + ' ' + Indefinite(key, value);
            // Log(line);
            /*$ENDIF*/
        }

        private static string SpecialItem(GameState gameState, GameConfig config)
        {
            return InterestingItem(gameState, config) + " of " + Pick(config.ItemOfs, gameState);
        }

        private static string InterestingItem(GameState gameState, GameConfig config)
        {
            return Pick(config.ItemAttrib, gameState) + " " + Pick(config.Specials, gameState);
        }

        private static void WinEquip(ref Character character, GameState gameState, GameConfig config)
        {
            var posn = gameState.Random(character.Equips.Count);
            string[] stuff;
            string[] better;
            string[] worse;

            if(posn == 0) {
                stuff = config.Weapons;
                better = config.OffenseAttrib;
                worse = config.OffenseBad;
            } else {
                better = config.DefenseAttrib;
                worse = config.DefenseBad;
                stuff = (posn == 1) ? config.Shields : config.Armors;
            }

            var name = LPick(stuff, character.Traits.Level, gameState);
        }

        private static object LPick(string[] list, int goal, GameState gameState)
        {
            var result = Pick(list, gameState);
            for(var i=1; i<=5; ++i) {
                var best = StrToInt(Split(result, 1));
                var s = Pick(list, gameState);
                var b1 = StrToInt(Split(s,1));
                if(Math.Abs(goal-best) > Math.Abs(goal-b1))
                    result = s;
            }
            return result;
        }

        private static int StrToInt(string s)
        {
            return Int32.Parse(s);
        }

        private static string Split(string s, int field, string separator = null)
        {
            if(separator == null) {
                return s.Split("|")[field];
            }
            return s.Split(separator + "|")[field];
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