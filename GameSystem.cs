using System;
using System.Threading;

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
    }
}