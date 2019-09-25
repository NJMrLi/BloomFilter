using System;
using System.Collections.Generic;
using System.Text;

namespace BloomFilterDemo.ComputeHash
{
    public class SimpleHash: IComputeHash
    {
        public readonly int _bitLength;

        public SimpleHash(int bitLength)
        {
            _bitLength = bitLength;
        }

        public int ComputeHash(string value, int seed)
        {
            int a = 0;
            for (int i = 0; i < value.Length; i++)
            {
                var b = value[i];
                a = (a * seed + b) % _bitLength;
            }

            //产生单个信息指纹
            //var result = (_bitLength - 1) & a;
            return a;
        }
    }
}
