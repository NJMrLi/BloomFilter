using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BloomFilterDemo
{
    public class BloomFilter
    {
        private readonly bool _isUserRedis;
        private readonly string _filterKeyName;
        private readonly RedisService _redisService;
        private readonly BloomFilterHash _bloomFilterHash;

        private readonly BitArray _bitArray;

        public BloomFilter(int elementNum, double errorRate, HashKind hashKind = HashKind.SimpleHash, bool isUserRedis = false, string filterKeyName = "")
        {
            _filterKeyName = filterKeyName;
            _redisService = new RedisService();
            _isUserRedis = isUserRedis;

            var m = GetBitLength(elementNum, errorRate);
            var k = GetHashFuncCount(m, elementNum);

            Console.WriteLine($"m={m} n={elementNum} p={errorRate} k={k} ");
            Console.WriteLine($"ErrorRate={GetFalsePositiveProbability(k, elementNum, m)}");

            //Hash计算类
            _bloomFilterHash = new BloomFilterHash(m, k, hashKind);

            //创建本地的布隆过滤器bit数组
            _bitArray = new BitArray(m);


        }

        /// <summary>
        /// 设置单个值
        /// </summary>
        /// <param name="value"></param>
        public void Add(string value)
        {
            var hashes = _bloomFilterHash.Hash(value);
            var offsets = hashes.ToArray();

            foreach (var offest in offsets)
            {
                _bitArray[offest] = true;
            }

            if (_isUserRedis)
            {
                //设置Bit位
                _redisService.MultiSetBit(_filterKeyName, true, hashes.Select(Convert.ToInt64).ToArray());
            }
        }

        /// <summary>
        /// 判断Value是否存在
        /// </summary>
        /// <param name="value">待判断的值</param>
        /// <returns></returns>
        public bool IsExists(string value)
        {
            //计算该值的hashs
            var hashes = _bloomFilterHash.Hash(value);

            var result = new List<bool>();

            if (!_isUserRedis)
            {
                foreach (var hash in hashes)
                {
                    result.Add(_bitArray[hash]);
                }
            }
            else
            {
                var offsets = hashes.Select(Convert.ToInt64).ToArray();
                result = _redisService.MultiGetBit(_filterKeyName, offsets);
            }

            //判断是否存在false
            return result.All(bit => bit);
        }

        /// <summary>
        /// 批量添加
        /// </summary>
        /// <param name="path"></param>
        /// <param name="useRedis"></param>
        public void AddEntries(List<string> valueList)
        {

            if (!_isUserRedis)
            {
                foreach (var value in valueList)
                {
                    var hashes = _bloomFilterHash.Hash(value);
                    foreach (var index in hashes)
                    {
                        _bitArray[index] = true;
                    }
                }
                return;
            }

            foreach (var value in valueList)
            {
                var hashes = _bloomFilterHash.Hash(value);
                var offsets = hashes.Select(Convert.ToInt64).ToArray();
                //设置Bit位
                _redisService.MultiSetBit(_filterKeyName, true, offsets);
            }
        }

        /// <summary>
        /// 通过文件添加
        /// </summary>
        /// <param name="path"></param>
        /// <param name="useRedis"></param>
        public void AddEntriesByFile(string path)
        {
            var valueList = new List<string>();
            using (var fileStream = new FileStream(path, FileMode.Open))
            using (var streamReader = new StreamReader(fileStream))
            {
                while (!streamReader.EndOfStream)
                {
                    var line = streamReader.ReadLine();
                    valueList.Add(line);
                }
            }

            AddEntries(valueList);
        }

        #region 公式
        /// <summary>
        /// 计算错误率
        /// </summary>
        /// <returns></returns>
        private double GetFalsePositiveProbability(int hashFuncCount, int elementNum, int bitLength)
        {
            var result = Math.Pow((1 - Math.Exp(-hashFuncCount * elementNum / (double)bitLength)), hashFuncCount);
            return result;
        }

        /// <summary>
        /// 计算bit位
        /// </summary>
        /// <param name="elementNum"></param>
        /// <param name="errorRate"></param>
        /// <returns></returns>
        private int GetBitLength(int elementNum, double errorRate)
        {
            var result = -(elementNum * Math.Log(errorRate) / Math.Pow(Math.Log(2), 2));
            return (int)Math.Ceiling(result);
        }

        /// <summary>
        /// 计算Hash方法数量
        /// </summary>
        /// <param name="bitLength"></param>
        /// <param name="elementNum"></param>
        /// <returns></returns>
        private int GetHashFuncCount(int bitLength, int elementNum)
        {
            return (int)Math.Round((bitLength / elementNum) * Math.Log(2.0));
        }
        #endregion

    }
}
