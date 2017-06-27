using System;

namespace Keede.DAL
{
    internal class LoopStrategy : BaseStrategy
    {
        ushort i;
        internal override string GetConnctionStr(string[] readConnctions)
        {
            int index = i++ % readConnctions.Length;
            return readConnctions[index];
        }
    }
}
