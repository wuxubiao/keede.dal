using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dapper;
using Keede.DAL.Conntion;

namespace Keede.DAL.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            UInt16 idx = UInt16.MaxValue;

            Console.WriteLine(idx++);
            for (int i = 0; i < 10; i++)
            {
                Console.WriteLine(idx++);
            }



            using (var connection = new Databases().GetDbConnection())
            {
                
                using (var tran = connection.BeginTransaction())
                {

                    tran.Commit();
                }
            }
        }

        private void Init()
        {
            ConnectionContainer.ClearConnections();
            string[] readConnctions = { "Data Source=tcp:192.168.152.52,1433;Initial Catalog=DB01;User ID=sa;Password=!QAZ2wsx;", "Data Source=tcp:192.168.152.53,1433;Initial Catalog=DB01;User ID=sa;Password=!QAZ2wsx;" };
            ConnectionContainer.AddDbConnections("DB01", "Data Source=tcp:192.168.152.52,1433;Initial Catalog=DB01;User ID=sa;Password=!QAZ2wsx;", readConnctions, EnumStrategyType.Random);
        }
    }
}
