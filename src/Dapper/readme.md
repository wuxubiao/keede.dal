Dapper.Extension由于用到了原生dapper中internal类型的扩展方法，后续升级Dapper版本注意

SqlMapper这个类的方法加入了，用于跟踪抛错的sql语句
catch (Exception e)
{
	throw new SqlException(command.CommandText, e);
}