using System;

namespace BloomFilterDemo
{
    class Program
    {
        public static void Main(string[] args)
        {

            var bf = new BloomFilter(200000, 0.0001, HashKind.SimpleHash);

            //var bf = new BloomFilter(200000, 0.0001, HashKind.MSHash);

            bf.AddEntriesByFile("English.txt");

            //随机字符串
            Random rd = new Random();
            for (int i = 0; i < 1000; i++)
            {
                var length = rd.Next(5, 15);

                var str = GenerateRandomNumber(length);

                var exist = bf.IsExists(str);

                Console.WriteLine($" search: {str}  exist: {exist}");
            }

            //自己输入
            while (true)
            {
                var value = Console.ReadLine();

                if (value == "OK")
                {
                    break;
                }

                var exist = bf.IsExists(value);

                Console.WriteLine($" search: {value}  exist: {exist}");
            }

            Console.ReadKey();
        }



        static readonly char[] constant =
        {
            '0','1','2','3','4','5','6','7','8','9',
            'a','b','c','d','e','f','g','h','i','j','k','l','m','n','o','p','q','r','s','t','u','v','w','x','y','z',
            'A','B','C','D','E','F','G','H','I','J','K','L','M','N','O','P','Q','R','S','T','U','V','W','X','Y','Z'
        };
        public static string GenerateRandomNumber(int Length)
        {
            System.Text.StringBuilder newRandom = new System.Text.StringBuilder(62);
            Random rd = new Random();
            for (int i = 0; i < Length; i++)
            {
                newRandom.Append(constant[rd.Next(62)]);
            }
            return newRandom.ToString();
        }
    }
}
