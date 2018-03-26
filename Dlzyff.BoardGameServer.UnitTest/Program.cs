using Dlzyff.BoardGame.BottomServer.Concurrents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dlzyff.BoardGameServer.UnitTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Random r = new Random(0);

            #region 测试代码
            //int enterCode1 = r.Next(10000, 40000);
            //int enterCode2 = r.Next(10000, 40000);
            //int enterCode3 = r.Next(10000, 40000);
            //int enterCode4 = r.Next(10000, 40000);
            //int enterCode5 = r.Next(10000, 40000);
            //int enterCode6 = r.Next(10000, 40000);
            //int enterCode7 = r.Next(10000, 40000);
            //int enterCode8 = r.Next(10000, 40000);
            //int enterCode9 = r.Next(10000, 40000);
            //int enterCode10 = r.Next(10000, 40000);
            //int enterCode11 = r.Next(10000, 40000);
            //int enterCode12 = r.Next(10000, 40000);
            //int enterCode13 = r.Next(10000, 40000);
            //Console.WriteLine(enterCode1);
            //Console.WriteLine(enterCode2);
            //Console.WriteLine(enterCode3);
            //Console.WriteLine(enterCode4);
            //Console.WriteLine(enterCode5);
            //Console.WriteLine(enterCode6);
            //Console.WriteLine(enterCode7);
            //Console.WriteLine(enterCode8);
            //Console.WriteLine(enterCode9);
            //Console.WriteLine(enterCode10);
            //Console.WriteLine(enterCode11);
            //Console.WriteLine(enterCode12);
            //Console.WriteLine(enterCode13); 
            #endregion

            int count = 0;
            for (int index = 0; index < 101; index++)
            {
                if (count == 5)
                {
                    Console.WriteLine();
                    count = 0;
                }
                else
                {
                    count++;
                    int tmpEnterCode = r.Next(100000, 999999);
                    Console.Write(tmpEnterCode + " ");
                }

            }
            Console.ReadKey();
        }
    }
}
