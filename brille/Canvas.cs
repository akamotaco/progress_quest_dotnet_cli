using System;
using System.Text;

namespace brille
{
    class Canvas {
        public Canvas(int width, int height) {
            
        }
    }
    class Program
    {
        static public void Test1()
        {
            for(var i=0x2800;i<0x28FF;++i) {
                Console.Write(Convert.ToChar(i));
            }

            Console.WriteLine("Hello World!");
        }

        static public void Test2()
        {
            var tile = new bool[8,10] {
                // {true, false, false, false, false, true, false, false, false, false},
                // {true, false, false, false, false, true, false, false, false, false},
                // {true, false, false, false, false, true, false, false, false, false},
                // {true, false, false, false, false, true, false, false, false, false},
                // {true, false, false, false, false, true, false, false, false, false},
                // {true, false, false, false, false, true, false, false, false, false},
                // {true, false, false, false, false, true, false, false, false, false},
                // {true, false, false, false, false, true, false, false, false, false},

                {true, false, false, false, false, true, false, false, false, false},
                {true, true, false, false, false, true, false, false, false, false},
                {true, false, true, false, false, true, false, false, false, false},
                {true, false, false, true, false, true, false, false, false, false},
                {true, false, false, false, true, true, false, false, false, false},
                {true, false, false, false, false, true, true, false, false, false},
                {true, false, false, false, false, true, false, true, false, true},
                {true, false, false, false, false, true, false, false, true, false},

                // {true, true, true, true, true, true, true, true, true, true},
                // {true, false, false, false, false, false, false, false, false, true},
                // {true, false, false, false, false, false, false, false, false, true},
                // {true, false, false, false, false, false, false, false, false, true},
                // {true, false, false, false, false, false, false, false, false, true},
                // {true, false, false, false, false, false, false, false, false, true},
                // {true, false, false, false, false, false, false, false, false, true},
                // {true, true, true, true, true, true, true, true, true, true},
            };

            PrintTile(tile);
        }

        static public void Test3()
        {
            int height=8*3, width=10*3;
            var map = new bool[height,width];

            int px=2, py=2;

            for(var y=0;y<height;++y) {map[y,0] = true;map[y,width-1] = true;}
            for(var x=0;x<width;++x) {map[0,x] = true;map[height-1,x] = true;}
            map[py,px] = true;

            var time_old = DateTime.Now;

            while(true)
            {
                if (Console.KeyAvailable)
                {
                    ConsoleKeyInfo key = Console.ReadKey(true);
                    switch (key.Key)
                    {
                        case ConsoleKey.F1:
                            Console.WriteLine("You pressed F1!");
                            break;
                        case ConsoleKey.Q:
                            Console.WriteLine("end");
                            return;
                        case ConsoleKey.A:
                            Console.WriteLine("A");
                            break;
                        case ConsoleKey.RightArrow:
                            if(px<width-(1+1)) px += 1;
                            break;
                        case ConsoleKey.LeftArrow:
                            if(px>1) px -= 1;
                            break;
                        case ConsoleKey.UpArrow:
                            if(py>1) py -= 1;
                            break;
                        case ConsoleKey.DownArrow:
                            if(py<height-(1+1))py += 1;
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    var time_new = DateTime.Now;
                    var interval = (time_new - time_old);

                    double q = interval.TotalMilliseconds / 1000;

                    if(q < 0.16)
                        continue;

                    time_old = time_new;
                    
                    Console.WriteLine(interval.TotalMilliseconds);

#region draw
                    for(var y=0;y<height;++y) for(var x=0;x<width;++x) map[y,x] = false;
                    for(var y=0;y<height;++y) {map[y,0] = true;map[y,width-1] = true;}
                    for(var x=0;x<width;++x) {map[0,x] = true;map[height-1,x] = true;}
                    map[py,px] = true;
#endregion

                    PrintTile(map);

                    // Console.WriteLine("beep");
                }
            }
        }

        // static void Main(string[] args)
        // {
        //     // Console.OutputEncoding = System.Text.Encoding.UTF8;

        //     Test1();
        //     Test2();
        //     Test3();
        // }

        static public void PrintTile(bool[,] tile)
        {
            var height = tile.GetLength(0);
            var width = tile.GetLength(1);

            if(height%4 !=0)
                throw new ArgumentException($"{height} % 4 should be zero");
            if(width%2 != 0)
                throw new ArgumentException($"{width} % 2 should be zero");
            
            int p_width = width/2;
            int p_height = height/4;

            for(var y=0;y<p_height;++y) {
                for(var x=0;x<p_width;++x) {
                    var wchr = GetWchr(tile[(y*4)+0,(x*2)+0],tile[(y*4)+1,(x*2)+0],tile[(y*4)+2,(x*2)+0],tile[(y*4)+3,(x*2)+0],
                                       tile[(y*4)+0,(x*2)+1],tile[(y*4)+1,(x*2)+1],tile[(y*4)+2,(x*2)+1],tile[(y*4)+3,(x*2)+1]);
                    Console.Write(wchr);
                }
                Console.WriteLine("");
            }
        }

        static public char GetWchr(bool p11, bool p21, bool p31, bool p41, bool p12, bool p22, bool p32, bool p42)
        {
            var i = 0x2800;

            if(p11) i += 0x01;
            if(p21) i += 0x02;
            if(p31) i += 0x04;
            if(p41) i += 0x40;

            if(p12) i += 0x08;
            if(p22) i += 0x10;
            if(p32) i += 0x20;
            if(p42) i += 0x80;

            return Convert.ToChar(i);
        }
    }
}
