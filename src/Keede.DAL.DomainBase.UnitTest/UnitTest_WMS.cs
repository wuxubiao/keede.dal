using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Dapper;
using Dapper.Extension;
using Keede.DAL.DDD.Repositories;
using Keede.DAL.DDD.UnitTest;
using Keede.DAL.DDD.UnitTest.Models;
using Keede.DAL.RWSplitting;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace Keede.RepositoriesTests
{
    [TestClass]
    public class UnitTest_WMS
    {
        public UnitTest_WMS()
        {
            string[] readConnctions = { "server=192.168.117.126;database=Group.WMS;user id=test;password=t#@!$%;" };
            string writeConnction = "server=192.168.117.126;database=Group.WMS;user id=test;password=t#@!$%;";
            ConnectionContainer.AddDbConnections("DB01", writeConnction, readConnctions, EnumStrategyType.Loop);
            TypeMapper.Initialize("Keede.DAL.DDD.UnitTest.Models");
        }

        [TestMethod]
        public void TestAdd()
        {
            using (var repository = new NewsRepository())
            {
                var sql = @"select OB.[No], OB.WarehouseId, OB.StorageType, OB.HostingFilialeId, OB.SaleFilialeId, OB.CreateTime, 
OB.SourceNo, OB.SourceType, OB.[State], OB.FinishedTime, OB.ExpressId, OB.ExpressNo, OB.ApplyCancelTime, OB.IsExistCancelPool from OutGoodsBill OB WITH(NOLOCK) where  
WarehouseId = 'b5bcdf6e-95d5-4aee-9b19-6ee218255c05' AND IsExistCancelPool = 1 AND[State] IN('7', '8') ORDER by OB.CreateTime desc";
                repository.GetPagedList<News>(sql, null, 1, 10);

            }
        }

        //public ConstructorInfo FindConstructor(string[] names, Type[] types)
        //{
        //    var constructors = _type.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        //    foreach (ConstructorInfo ctor in constructors.OrderBy(c => c.IsPublic ? 0 : (c.IsPrivate ? 2 : 1)).ThenBy(c => c.GetParameters().Length))
        //    {
        //        ParameterInfo[] ctorParameters = ctor.GetParameters();
        //        if (ctorParameters.Length == 0)
        //            return ctor;

        //        if (ctorParameters.Length != types.Length)
        //            continue;

        //        int i = 0;
        //        for (; i < ctorParameters.Length; i++)
        //        {
        //            if (!String.Equals(ctorParameters[i].Name, names[i], StringComparison.OrdinalIgnoreCase))
        //                break;
        //            if (types[i] == typeof(byte[]) && ctorParameters[i].ParameterType.FullName == SqlMapper.LinqBinary)
        //                continue;
        //            var unboxedType = Nullable.GetUnderlyingType(ctorParameters[i].ParameterType) ?? ctorParameters[i].ParameterType;
        //            if ((unboxedType != types[i] && !SqlMapper.HasTypeHandler(unboxedType))
        //                && !(unboxedType.IsEnum() && Enum.GetUnderlyingType(unboxedType) == types[i])
        //                && !(unboxedType == typeof(char) && types[i] == typeof(string))
        //                && !(unboxedType.IsEnum() && types[i] == typeof(string)))
        //            {
        //                break;
        //            }
        //        }

        //        if (i == ctorParameters.Length)
        //            return ctor;
        //    }

        //    return null;
        //}

    }
}
