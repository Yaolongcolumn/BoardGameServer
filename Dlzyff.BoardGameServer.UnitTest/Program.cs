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
            #region 测试
            //int index = 2;
            //List<int> ints = new List<int>() { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            //int remainIndex = (ints.Count) - index;
            //int[] intArray = new int[remainIndex];
            //for (int i = remainIndex - 1; i >= 0; i--)
            //{
            //    intArray[i] = ints[i];
            //}
            //List<int> resList = new List<int>();
            //for (int i = index + 1; i < ints.Count; i++)
            //{
            //    Console.WriteLine(ints[i]);
            //}

            //int[] intArray1 = new int[index + 1];

            //for (int i = 0; i < intArray1.Length; i++)
            //{
            //    Console.WriteLine(intArray1[i]);
            //}

            #endregion

            #region 第一次测试[整数测试]
            //int int1 = 10;
            //int int2 = 10;
            //int[] ints = new int[5] { 10, 9, 10, 5, 6 };
            //bool isSuccess = false;
            //for (int i = 0; i < ints.Length; i++)
            //{
            //    int tmpNumber = ints[i];
            //    int nextIndex = i + 1;
            //    if (nextIndex > ints.Length - 1)
            //        nextIndex = ints.Length - 1;
            //    int tmpNextNumber = ints[nextIndex];
            //    if (tmpNumber == int1 && tmpNextNumber == int2)
            //    {
            //        isSuccess = true;
            //        break;
            //    }
            //}
            // Console.WriteLine("是否找到了两个相同的值? " + isSuccess);
            #endregion

            #region 第二次测试[字符串测试]
            string str1 = "HeartTwo";
            string str2 = "HeartTwo";
            string[] strs = new string[5] { "SpadeTwo", "SpadeTwo", "HeartTwo", "HeartTwo", "大王" };
            bool isSuccess = false;
            if (strs.Length >= 2)
            {
                for (int strIndex = 0; strIndex < strs.Length; strIndex++)
                {
                    string tmpStr = strs[strIndex];
                    int nextIndex = strIndex + 1;
                    if (nextIndex > strs.Length - 1)
                        nextIndex = strs.Length - 1;
                    string tmpNextStr = strs[nextIndex];
                    if (tmpStr == str1 && tmpNextStr == str2)
                    {
                        isSuccess = true;
                        break;
                    }
                }
                Console.WriteLine("是否找到了两个相同的值? " + isSuccess);
            }
            else
                Console.WriteLine("值不足,不能进行判断~");
            #endregion

            Console.ReadKey();
        }
    }
}
