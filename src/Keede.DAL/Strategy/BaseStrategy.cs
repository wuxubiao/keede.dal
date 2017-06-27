using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Keede.DAL
{
    internal abstract class BaseStrategy
    {

        internal abstract string GetConnctionStr(string[] readConnctions);
    }
}
