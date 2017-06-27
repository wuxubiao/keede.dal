using System;

namespace Keede.DAL
{
    internal class RondomStrategy : BaseStrategy
    {
        static readonly Random _random = new Random();

        internal override string GetConnctionStr(string[] readConnctions)
        {
            int rdNum = _random.Next(1, 1000);
            int index = rdNum % readConnctions.Length;
            return readConnctions[index];
        }
    }
}