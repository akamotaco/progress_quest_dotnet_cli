using System;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;

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

            // Thread.Sleep((int)(seconds*1000));
            // return -1;
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

                // advance plot
                if (gain || gameState.Act == 0) {
                    if (gameState.PlotBar.Done())
                        InterplotCinematic(character, gameState, config);
                    else
                        gameState.PlotBar.Increment(gameState.TaskBar.Max / 1000);
                }

                Dequeue(character, gameState, config);
            }
            else {
                // var elapsed = timeGetTime() - lasttick;
                // if (elapsed > 100) elapsed = 100;
                // if (elapsed < 0) elapsed = 0;
                // TaskBar.increment(elapsed);
                gameState.TaskBar.Increment(1000); // 1000 msec == 1 sec
            }
        }

        private static void Dequeue(Character character, GameState gameState, GameConfig config)
        {
            while(TaskDone(gameState)) {
                if(Split(gameState.Task, 0) == "kill") {
                    if(Split(gameState.Task, 3) == "*") {
                        WinItem(ref character, gameState, config);
                    } else if(Split(gameState.Task, 3) != "") {
                        AddInventory(character, character.Inventory, LowerCase(Split(gameState.Task,1) + " " +
                                                            ProperCase(Split(gameState.Task,3))),1);
                    }
                } else if(gameState.Task == "buying") {
                    AddInventory(character, character.Inventory, "Gold", -EquipPrice(character));
                    WinEquip(ref character, gameState, config);
                } else if ((gameState.Task == "market") || (gameState.Task == "sell")) {
                    if (gameState.Task == "sell") {
                        var firstItem = character.Inventory.ToArray()[1];
                        var amt = firstItem.Value * character.Traits.Level;
                        if (Pos(" of ", firstItem.Key) > 0)
                            amt *= (1+gameState.RandomLow(10)) * (1+gameState.RandomLow(character.Traits.Level));
                        character.Inventory.Remove(firstItem.Key);
                        AddInventory(character, character.Inventory, "Gold", amt);
                    }
                    if (character.Inventory.Count > 1) {
                        // character.Inventory.scrollToTop();
                        // Debug.Assert(firstItem == "");
                        var firstItem = character.Inventory.ToArray()[1];
                        Task("Selling " + Indefinite(firstItem.Key, firstItem.Value),
                            1 * 1000, gameState);
                        gameState.Task = "sell";
                        break;
                    }
                }

                var old = gameState.Task;
                gameState.Task = "";
                if (gameState.Q.Count > 0) {
                    var a = Split(gameState.Q.Peek(),0);
                    var n = StrToInt(Split(gameState.Q.Peek(),1));
                    var s = Split(gameState.Q.Peek(),2);
                    if (a == "task" || a == "plot") {
                        gameState.Q.Dequeue();
                        if (a == "plot") {
                            CompleteAct(character, gameState, config);
                            s = "Loading " + gameState.BestPlot;
                        }
                        Task(s, n * 1000, gameState);
                    } else {
                        throw new Exception("bah!" + a);
                    }
                } else if (character.EncumBar.Done()) {
                    Task("Heading to market to sell loot",4 * 1000, gameState);
                    gameState.Task = "market";
                } else if ((Pos("kill|",old) <= 0) && (old != "heading")) {
                    if (character.Inventory["Gold"] > EquipPrice(character)) {
                        Task("Negotiating purchase of better equipment", 5 * 1000, gameState);
                        gameState.Task = "buying";
                    } else {
                        Task("Heading to the killing fields", 4 * 1000, gameState);
                        gameState.Task = "heading";
                    }
                } else {
                    var nn = character.Traits.Level;
                    var t = MonsterTask(nn, gameState, config);
                    var InventoryLabelAlsoGameStyleTag = 3;
                    // nn = Math.floor((2 * InventoryLabelAlsoGameStyleTag * t.level * 1000) / nn);
                    nn = (2 * InventoryLabelAlsoGameStyleTag * t.level * 1000) / nn;
                    Task("Executing " + t.description, nn, gameState);
                }
            }
        }

        private static (string description, int level) MonsterTask(int level, GameState gameState, GameConfig config)
        {
            var definite = false;
            int i;
            for(i=level; i >=1; --i) {
                if(Odds(2,5, gameState))
                    level += RandSign(gameState);
            }
            if(level < 1) level = 1;
            // level = level of puissance of opponent(s) we'll return

            string monster = "";
            int lev;
            if(Odds(1,25, gameState)) {
                // Use an NPC every once in a while
                monster = " " + Split(Pick(config.Races, gameState), 0);
                if(Odds(1,2, gameState)) {
                    monster = "passing" + monster + " " + Split(Pick(config.Klasses, gameState), 0);
                } else {
                    monster = PickLow(config.Titles, gameState) + " " + config.GenerateName() + " the" + monster;
                    definite = true;
                }
                lev = level;
                monster = monster + "|" + IntToStr(level) + "|*";
            } else if(gameState.QuestMonster != "" && Odds(1,4, gameState)) {
                // Use the quest monster
                monster = config.Monsters[gameState.QuestMonsterIndex];
                lev = StrToInt(Split(monster, 1));
            } else {
                // Pick the monster out of so many random ones closest to the level we want
                monster = Pick(config.Monsters, gameState);
                lev = StrToInt(Split(monster, 1));
                for(var ii=0;ii<5;++ii) {
                    var m1 = Pick(config.Monsters, gameState);
                    if(Math.Abs(level-StrToInt(Split(m1,1))) < Math.Abs(level-lev)) {
                        monster = m1;
                        lev = StrToInt(Split(monster, 1));
                    }
                }
            }

            var result = Split(monster, 0);
            gameState.Task = "kill|" + monster;

            var qty = 1;
            if(level-lev > 10) {
                // lev is too low. multiply...
                // qty = MathF.Floor((level + gameState.Random(Math.Max(lev,1))) / Math.Max(lev,1));
                qty = (level + gameState.Random(Math.Max(lev,1))) / Math.Max(lev,1);
                if(qty < 1) qty = 1;
                // level = Math.Floor(level / qty);
                level = level / qty;
            }

            if((level - lev) <= -10) {
                result = "imaginary " + result;
            } else if((level-lev) < -5) {
                i = 10+(level-lev);
                i = 5-gameState.Random(i+1);
                result = Sick(i,Young((lev-level)-i,result));
            } else if (((level-lev) < 0) && (gameState.Random(2) == 1)) {
                result = Sick(level-lev,result);
            } else if (((level-lev) < 0)) {
                result = Young(level-lev,result);
            } else if ((level-lev) >= 10) {
                result = "messianic " + result;
            } else if ((level-lev) > 5) {
                i = 10-(level-lev);
                i = 5-gameState.Random(i+1);
                result = Big(i,Special((level-lev)-i,result));
            } else if (((level-lev) > 0) && (gameState.Random(2) == 1)) {
                result = Big(level-lev,result);
            } else if (((level-lev) > 0)) {
                result = Special(level-lev,result);
            }

            lev = level;
            level = lev * qty;

            if (!definite) result = Indefinite(result, qty);
            return (result, level);
        }

        private static string Special(int m, string s)
        {
            if(Pos(" ",s) > 0)
                return prefix(new string[]{"veteran","cursed","warrior","undead","demon"}, m, s);
            else
                return prefix(new string[]{"Battle-","cursed ","Were-","undead ","demon "}, m, s, "");
        }

        private static string Big(int m, string s)
        {
            return prefix(new string[]{"greater","massive","enormous","giant","titanic"}, m ,s);
        }

        private static string Young(int m, string s)
        {
            m = 6 - Math.Abs(m);
            return prefix(new string[]{"foetal","baby","preadolescent","teenage","underage"}, m, s);
        }

        private static string Sick(int m, string s)
        {
            m = 6 - Math.Abs(m);
            return prefix(new string[]{"dead","comatose","crippled","sick","undernourished"}, m, s);
        }

        private static string prefix(string[] a, int m, string s, string sep = " ")
        {
            m = Math.Abs(m);
            if(m < 1 || m > a.Length) return s;
            return a[m-1] + sep + s;
        }

        private static string PickLow(string[] s, GameState gameState)
        {
            return s[gameState.RandomLow(s.Length)];
        }

        private static int RandSign(GameState gameState)
        {
            return gameState.Random(2) * 2 - 1;
        }

        private static void CompleteAct(Character character, GameState gameState, GameConfig config)
        {
            gameState.CheckAll(gameState.Plots);
            gameState.Act += 1;
            gameState.PlotBar.Reset(60 * 60 * (1 + 5 * gameState.Act)); // 1 hr + 5/Act
            //Plots.AddUI((game.bestplot = 'Act ' + toRoman(game.act)));
            gameState.BestPlot = "Act " + ToRoman(gameState.Act);
            gameState.Plots.Add((gameState.BestPlot, false));

            if(gameState.Act > 1) {
                WinItem(ref character, gameState, config);
                WinEquip(ref character, gameState, config);
            }

            // Brag("act");
        }

        private static void Task(string caption, int msec, GameState gameState)
        {
            gameState.Kill = caption + "...";
            // if (Kill)
            //     Kill.text(game.kill);
            // Log(game.kill);
            gameState.TaskBar.Reset(msec);
        }

        private static int EquipPrice(Character character)
        {
            return 5 * character.Traits.Level * character.Traits.Level +
                10 * character.Traits.Level +
                20;
        }

        private static string ProperCase(string s)
        {
            return s.Substring(0,1).ToUpper() + s.Substring(1);
        }

        private static string LowerCase(string s)
        {
            return s.ToLower();
        }

        private static bool TaskDone(GameState gameState)
        {
            return gameState.TaskBar.Done();
        }

        private static void InterplotCinematic(Character character, GameState gameState, GameConfig config)
        {
            switch(gameState.Random(3)) {
                case 0:
                    Q(character, gameState, config, "task|1|Exhausted, you arrive at a friendly oasis in a hostile land");
                    Q(character, gameState, config, "task|2|You greet old friends and meet new allies");
                    Q(character, gameState, config, "task|2|You are privy to a council of powerful do-gooders");
                    Q(character, gameState, config, "task|1|There is much to be done. You are chosen!");
                    break;
                case 1:
                    Q(character, gameState, config, "task|1|Your quarry is in sight, but a mighty enemy bars your path!");
                    var nemesis = NamedMonster(character.Traits.Level+3, gameState, config);
                    Q(character, gameState, config, "task|4|A desperate struggle commences with " + nemesis);
                    var s = gameState.Random(3);
                    for (var i = 1; i <= gameState.Random(1 + gameState.Act + 1); ++i) {
                    s += 1 + gameState.Random(2);
                    switch (s % 3) {
                    case 0: Q(character, gameState, config, "task|2|Locked in grim combat with " + nemesis); break;
                    case 1: Q(character, gameState, config, "task|2|" + nemesis + " seems to have the upper hand"); break;
                    case 2: Q(character, gameState, config, "task|2|You seem to gain the advantage over " + nemesis); break;
                    }
                    }
                    Q(character, gameState, config, "task|3|Victory! " + nemesis + " is slain! Exhausted, you lose conciousness");
                    Q(character, gameState, config, "task|2|You awake in a friendly place, but the road awaits");
                    break;
                case 2:
                    var nemesis2 = ImpressiveGuy(gameState, config);
                    Q(character, gameState, config, "task|2|Oh sweet relief! You've reached the protection of the good " + nemesis2);
                    Q(character, gameState, config, "task|3|There is rejoicing, and an unnerving encouter with " + nemesis2 + " in private");
                    Q(character, gameState, config, "task|2|You forget your " + BoringItem(gameState, config) + " and go back to get it");
                    Q(character, gameState, config, "task|2|What's this!? You overhear something shocking!");
                    Q(character, gameState, config, "task|2|Could " + nemesis2 + " be a dirty double-dealer?");
                    Q(character, gameState, config, "task|3|Who can possibly be trusted with this news!? ... Oh yes, of course");
                    break;
                }
            Q(character, gameState, config, "plot|1|Loading");
        }

        private static object ImpressiveGuy(GameState gameState, GameConfig config)
        {
            return Pick(config.ImpressiveTitles, gameState) +
                (gameState.Random(2) == 1 ? " of the " + Pick(config.Races, gameState) : " of " + config.GenerateName());
        }

        private static string NamedMonster(int level, GameState gameState, GameConfig config)
        {
            var lev = 0;
            var result = "";
            for(var i=0;i<5;++i) {
                var m = Pick(config.Monsters, gameState);
                if(result=="" || (Math.Abs(level-StrToInt(Split(m, 1))) < Math.Abs(level-lev))) {
                    result = Split(m, 0);
                    lev = StrToInt(Split(m, 1));
                }
            }

            return config.GenerateName() + " the " + result;
        }

        private static void Q(Character character, GameState gameState, GameConfig config, string s)
        {
            gameState.Q.Enqueue(s);
            Dequeue(character, gameState, config);
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
            // Debug.Assert(caption == "");
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
            AddInventory(character, character.Inventory, SpecialItem(gameState, config), 1);
        }

        private static void AddInventory(Character character, Dictionary<string, int> inventory, string key, int value)
        {
            var base_value = 0;
            inventory.TryGetValue(key, out base_value);

            inventory[key] = value + base_value;

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

            var cubits = 0;
            var itemList = inventory.Keys.ToArray();
            for(var i=1;i<itemList.Length;++i) {
                cubits += inventory[itemList[i]];
            }
            character.EncumBar.Reposition(cubits);
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
            var qual = StrToInt(Split(name, 1));
            name = Split(name, 0);
            var plus = character.Traits.Level - qual;
            if(plus < 0) better = worse;
            var count = 0;
            while(count < 2 && plus != 0) {
                var modifier = Pick(better, gameState);
                qual = StrToInt(Split(modifier, 1));
                modifier = Split(modifier, 0);
                if(Pos(modifier, name) > 0) break; // no repeats
                if(Math.Abs(plus) < Math.Abs(plus)) break; // too much
                name = modifier + " " + name;
                plus -= qual;
                ++count;
            }
            if(plus != 0) name = plus + " " + name;
            if(plus > 0) name = "+" + name;

            // character.Equips[posn] = name;
            var keys = character.Equips.Keys.ToArray();
            var values = character.Equips.Values.ToArray();
            character.Equips[keys[posn]] = name;
            character.BestEquip = name;
            if(posn > 1) character.BestEquip += " " + values[posn];
        }

        private static string LPick(string[] list, int goal, GameState gameState)
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
            AddR(character.Spells, config.Spells[gameState.RandomLow(Math.Min(character.GetStat("WIS") + character.Traits.Level, config.Spells.Length))], 1);
        }

        private static void AddR(Dictionary<string, string> dict, string key, int value)
        {
            string currentLevelStr;
            int currentLevel = 0;
            if(dict.TryGetValue(key, out currentLevelStr))
                currentLevel = ToArabic(currentLevelStr);

            dict[key] = ToRoman(value + currentLevel);
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

            // Debug.Assert(i == "");
            
            var value = character.GetStat(i) + 1;
            character.SetStat(i, value);
            if(i == "STR") {
                character.EncumBar.Reset(10 + value, character.EncumBar.Position);
            }
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