using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dapper;

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
    }
}
