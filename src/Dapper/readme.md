Dapper.Extension�����õ���ԭ��dapper��internal���͵���չ���������Ժ�ԭ��Dapper������ͬһ����Ŀ���������Dapper�汾ע��

======================================
2017/12/25 �����
SqlMapper�����ķ�������������catch��䣬���ڸ����״��sql���
catch (Exception e)
{
	throw new SqlStatementException(command.CommandText, command.Parameters, e);
}
�漰�ķ�����ExecuteImpl��QueryMultipleImpl��QueryImpl��QueryRowImpl��MultiMapImpl(����ͬ��)��ExecuteCommand��ExecuteScalarImpl��ExecuteReaderImpl��һ��9������
�Զ����쳣SqlStatementException����Newtonsoft.Json
======================================
2018/07/24 �����
�ڹٷ��汾1.50.2����������֧��.net standard2.0