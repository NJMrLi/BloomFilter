using System;
using System.Collections.Generic;
using System.Text;

namespace BloomFilterDemo.ComputeHash
{
    public interface IComputeHash
    {
        int ComputeHash(string value, int seed);
    }
}
