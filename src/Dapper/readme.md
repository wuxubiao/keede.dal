Dapper.Extension由于用到了原生dapper中internal类型的扩展方法，所以和原生Dapper放在了同一个项目里，后续升级Dapper版本注意

======================================
2017/12/25 吴旭标
SqlMapper这个类的方法加入了以下catch语句，用于跟踪抛错的sql语句
catch (Exception e)
{
	throw new SqlStatementException(command.CommandText, command.Parameters, e);
}
涉及的方法有ExecuteImpl、QueryMultipleImpl、QueryImpl、QueryRowImpl、MultiMapImpl(两个同名)、ExecuteCommand、ExecuteScalarImpl、ExecuteReaderImpl，一共9个方法
自定义异常SqlStatementException依赖Newtonsoft.Json
======================================
2018/07/24 吴旭标
在官方版本1.50.2进行升级，支持.net standard2.0