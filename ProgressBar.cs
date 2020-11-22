using System;
using System.Text.RegularExpressions;

namespace pq_dotnet
{
    internal class ProgressBar
    {
        private string id;
        private string template;
        public int Max { get; private set;}
        public int Position { get; private set;}
        public int Percent { get; private set; }
        public float Remaining { get; private set; }

        public ProgressBar(string id, string tmpl, int max, int position = 0)
        {
            this.id = id;
            this.template = tmpl;
            this.Max = Math.Max(max, 1);
            this.Position = Math.Max(position, 0);
        }

        internal void Reset(int newMax, int newPosition = 0) {
            this.Max = newMax;
            this.Reposition(newPosition);
        }

        private void Reposition(int newPosition)
        {
            this.Position = Math.Min(newPosition, this.Max);

            this.Percent = (100*this.Position) / this.Max;
            this.Remaining = MathF.Floor(this.Max - this.Position);
            // this.Time = RoughTime(this.Max - this.Position)
            // this.Hint = template();
        }

        internal void Increment (int inc) {
            this.Reposition(this.Position + inc);
        }
        internal bool Done()
        {
            return this.Position >= this.Max;
        }

        public override string ToString()
        {
            return $"[{this.Position}/{this.Max}]";
        }
    }
}