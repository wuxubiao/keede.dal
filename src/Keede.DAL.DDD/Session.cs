using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keede.DAL.DDD
{
    public class Session
    {
        [ThreadStatic]
        private static string _id;

        public static string ID
        {
            get => _id;
            set => _id = value;
        }
    }
}
