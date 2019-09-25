using BloomFilterDemo.ComputeHash;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BloomFilterDemo
{
    public class BloomFilterHash
    {
        public readonly int _bitLength = 1 << 4;
        public readonly List<int> _seeds = new List<int> { 3, 5, 7, 11 };
        public readonly HashKind _hashKind;

        /// <summary>
        ///构造函数
        /// </summary>
        /// <param name="bitLength">Bit位数</param>
        /// <param name="hashFuncCount">Hash函数数量</param>
        public BloomFilterHash(int bitLength, int hashFuncCount, HashKind hashKind = HashKind.SimpleHash)
        {
            _bitLength = bitLength;
            _seeds = GetPrimes(hashFuncCount);
            _hashKind = hashKind;
        }

        /// <summary>
        /// 计算每一个Hash方法后，Value的值
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public HashSet<int> Hash(string value)
        {
            var hashSets = new HashSet<int>();
            IComputeHash hash;

            switch (_hashKind)
            {
                case HashKind.MSHash:
                     hash = new MSHash(_seeds.Count,_bitLength,true);
                    break;
                case HashKind.SimpleHash:
                default:
                     hash = new SimpleHash(_bitLength);         
                    break;
            }

            foreach (var seed in _seeds)
            {
                var bit = hash.ComputeHash(value, seed);
                hashSets.Add(bit);
            }

            return hashSets;
        }

        /// <summary>
        /// 获取质数
        /// </summary>
        /// <param name="nums"></param>
        /// <returns></returns>
        private List<int> GetPrimes(int nums)
        {
            var result = new List<int>();

            for (int j = 1; j < int.MaxValue; j++)
            {
                var isPrime = IsPrime(j);
                if (isPrime)
                {
                    result.Add(j);

                    if (result.Count == nums)
                    {
                        break;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 高效判断是否为质数
        /// </summary>
        private bool IsPrime(int num)
        {
            if (num < 2)
                return false;
            if (num == 2 || num == 3)
            {
                return true;
            }
            if (num % 6 != 1 && num % 6 != 5)
            {
                return false;
            }
            int sqr = (int)Math.Sqrt(num);
            for (int i = 5; i <= sqr; i += 6)
            {
                if (num % i == 0 || num % (i + 2) == 0)
                {
                    return false;
                }
            }
            return true;
        }

    }
}
