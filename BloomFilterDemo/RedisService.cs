using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace BloomFilterDemo
{
    public class RedisService
    {
        private static readonly ConnectionMultiplexer _conn;

        private readonly IDatabase _db;

        static RedisService()
        {
            _conn = ConnectionMultiplexer.Connect("192.168.7.238:6379");
        }

        public RedisService()
        {
            _db = _conn.GetDatabase();
        }

        public void MultiSetBit(string name, bool value, params long[] offsets)
        {
            foreach (var offset in offsets)
            {
                 _db.StringSetBit(name, offset, value);
            }
        }

        public void MultiSetBit(string name, BitArray bitArray)
        {

            for (int i = 0; i < bitArray.Count; i++)
            {
                _db.StringSetBit(name, i, bitArray[i]);
            }
        }


        public async Task MultiSetBitAsync(string name, BitArray bitArray)
        {

            for (int i = 0; i < bitArray.Count; i++)
            {
                _db.StringSetBitAsync(name, i, bitArray[i]);
            }
        }



        public async Task MultiSetBitAsync(string name, bool value, params long[] offsets)
        {
            foreach (var offset in offsets)
            {
                await _db.StringSetBitAsync(name, offset, value);
            }
        }


        public List<bool> MultiGetBit(string name, params long[] offsets)
        {
            var result = new List<bool>();

            foreach (var offset in offsets)
            {
                var bit =  _db.StringGetBit(name, offset);
                result.Add(bit);
            }

            return result;
        }
    }
}
