using System;


namespace Game
{
    public class NewInstance : ECS.Entity
    {
        Random rand;
        private Character character;
        private GameConfig config;

        public NewInstance(GameConfig config) {
            rand = new Random();

            this.character = new Character(this);
            this.config = config;
        }

        public void RollCharacter() {
            RollCharacter(ref character, this.config);
        }

        internal override void OnCreate() {
#region character part
            character.Sold(config, this.rand);
            Console.WriteLine(character);
#endregion
            var gameState = new GameState(rand, config, this);
#region add components
            this._collection.Add(character);
            this._collection.Add(gameState);
#endregion
            base.OnCreate();
        }

        private void RollCharacter(ref Character character, GameConfig config)
        {
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

        private int Roll(ref Character character, string stat)
        {
            character.NewStat(stat, 3 + Random(6) + Random(6) + Random(6));

            return character.GetStat(stat);
        }

        private int Random(int v)
        {
            return this.rand.Next() % v;
        }
    }
}