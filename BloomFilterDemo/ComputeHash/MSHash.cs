using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace BloomFilterDemo.ComputeHash
{
    /// <summary>
    /// 参考微软roslyn中的布隆过滤器
    /// http://source.roslyn.io/#Microsoft.CodeAnalysis.Workspaces/Shared/Utilities/BloomFilter.cs
    /// 
    /// </summary>
    public class MSHash: IComputeHash
    {
        /// <summary>
        ///来自murturhash：
        ///‘m’和‘r’是离线产生的混合常数。m和r的值是通过实验证明的。
        ///MurmurHash 是一种非加密型哈希函数，适用于一般的哈希检索操作。
        /// </summary>
        private const uint Compute_Hash_m = 0x5bd1e995;
        private const int Compute_Hash_r = 24;

        private readonly int _hashFunctionCount;
        private readonly int _bitArrayLength;
        private readonly bool _isCaseSensitive;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="k">hash函数个数</param>
        /// <param name="m">Bit数组的位数</param>
        /// <param name="isCaseSensitive">是否区分大小写</param>
        public MSHash(int k, int m ,bool isCaseSensitive)
        {
            _hashFunctionCount = k;
            _isCaseSensitive = isCaseSensitive;
            _bitArrayLength = m;
        }


        /// <summary>
        /// Modification of the murmurhash2 algorithm.  Code is simpler because it operates over
        /// strings instead of byte arrays.  Because each string character is two bytes, it is known
        /// that the input will be an even number of bytes (though not necessarily a multiple of 4).
        /// 
        /// This is needed over the normal 'string.GetHashCode()' because we need to be able to generate
        /// 'k' different well distributed hashes for any given string s.  Also, we want to be able to
        /// generate these hashes without allocating any memory.  My ideal solution would be to use an
        /// MD5 hash.  However, there appears to be no way to do MD5 in .NET where you can:
        /// 
        /// a) feed it individual values instead of a byte[]
        /// 
        /// b) have the hash computed into a byte[] you provide instead of a newly allocated one
        /// 
        /// Generating 'k' pieces of garbage on each insert and lookup seems very wasteful.  So,
        /// instead, we use murmur hash since it provides well distributed values, allows for a
        /// seed, and allocates no memory.
        /// 
        /// Murmur hash is public domain.  Actual code is included below as reference.
        /// </summary>
        public int ComputeHash(string key, int seed)
        {
            unchecked
            {
                // Initialize the hash to a 'random' value

                var numberOfCharsLeft = key.Length;
                var h = (uint)(seed ^ numberOfCharsLeft);

                // Mix 4 bytes at a time into the hash.  NOTE: 4 bytes is two chars, so we iterate
                // through the string two chars at a time.
                var index = 0;
                while (numberOfCharsLeft >= 2)
                {
                    var c1 = GetCharacter(key, index);
                    var c2 = GetCharacter(key, index + 1);

                    h = CombineTwoCharacters(h, c1, c2);

                    index += 2;
                    numberOfCharsLeft -= 2;
                }

                // Handle the last char (or 2 bytes) if they exist.  This happens if the original string had
                // odd length.
                if (numberOfCharsLeft == 1)
                {
                    var c = GetCharacter(key, index);
                    h = CombineLastCharacter(h, c);
                }

                // Do a few final mixes of the hash to ensure the last few bytes are well-incorporated.

                h =  FinalMix(h);

                var inth = (int)h;
                var hash = inth % _bitArrayLength;
                return Math.Abs(hash);
            }


        }

        private char GetCharacter(string key, int index)
        {
            var c = key[index];
            return _isCaseSensitive ? c : char.ToLowerInvariant(c);
        }

        private static uint CombineTwoCharacters(uint h, uint c1, uint c2)
        {
            unchecked
            {
                var k = c1 | (c2 << 16);

                k *= Compute_Hash_m;
                k ^= k >> Compute_Hash_r;
                k *= Compute_Hash_m;

                h *= Compute_Hash_m;
                h ^= k;

                return h;
            }
        }

        private static uint CombineLastCharacter(uint h, uint c)
        {
            unchecked
            {
                h ^= c;
                h *= Compute_Hash_m;
                return h;
            }
        }

        private static uint FinalMix(uint h)
        {
            unchecked
            {
                h ^= h >> 13;
                h *= Compute_Hash_m;
                h ^= h >> 15;
                return h;
            }
        }
    }
}
