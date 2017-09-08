using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dapper.Extension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dapper.Extension.Tests
{
    [TestClass()]
    public class TypeMapperTests
    {
        [TestMethod()]
        public void InitializeTest()
        {
            TypeMapper.Initialize("Keede.DAL.DDD");
        }

        [TestMethod()]
        public void assemblyTest()
        {

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies().Where(ent => !ent.GlobalAssemblyCache ))//&& ent.FullName.ToLower().Contains("keede")))
            {
                //foreach (var type in assembly.GetTypes().Where(type => realType.IsAssignableFrom(type)))//assembly.GetTypes().Where(ent => ent.IsClass).Where(ent => ent.BaseType == realType || (ent.BaseType != null && ent.BaseType.BaseType == realType)))
                //{
                //    constructor = type.GetConstructor(new Type[0]);
                //    _constructorDic.AddOrUpdate(realType.FullName, constructor, (key, existed) => constructor);
                //    return constructor;
                //}
            }
        }

    }
}