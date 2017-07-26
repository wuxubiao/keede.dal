using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using Keede.SQLHelper.Attributes;

namespace Keede.SQLHelper.Mapper
{
    internal class DataReader
    {
        #region -- Static
        private static ConcurrentDictionary<string, DataReader> _dataReaderTable;

        public static DataReader Create(Type type)
        {
            if (_dataReaderTable == null)
            {
                _dataReaderTable = new ConcurrentDictionary<string, DataReader>();
            }
            var key = type.FullName;
            if (!_dataReaderTable.ContainsKey(key))
            {
                DataReader reader = new DataReader(type);
                return _dataReaderTable.AddOrUpdate(key, reader,(k,v) => reader);
            }
            return _dataReaderTable[key];
        }

        #endregion -- Static

        #region -- init()

        internal DataReader(Type type)
        {
            foreach (PropertyInfo p in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                DataReadProperty rp = new DataReadProperty { Handler = new PropertyHandler(p) };
                IDAttribute[] ida = AttributeUtility.GetAttributes<IDAttribute>(p, false);
                rp.Name = p.Name;
                if (ida.Length > 0)
                {
                    if (!string.IsNullOrEmpty(ida[0].Name))
                    {
                        rp.Name = ida[0].Name;
                    }
                }
                else
                {
                    ColumnAttribute[] columns = AttributeUtility.GetAttributes<ColumnAttribute>(p, false);
                    if (columns.Length > 0)
                    {
                        if (!string.IsNullOrEmpty(columns[0].Name))
                        {
                            rp.Name = columns[0].Name;
                        }
                    }
                }
                _dataReadProperties.Add(rp);
            }
        }

        #endregion -- init()

        private readonly ConcurrentBag<DataReadProperty> _dataReadProperties = new ConcurrentBag<DataReadProperty>();

        public Type Type
        {
            get;
            set;
        }

        public void ReaderToObject(IDataReader reader, ref object obj)
        {
            lock (this)
            {
                ReadColumnIndex(reader);
                foreach (DataReadProperty p in _dataReadProperties)
                {
                    ReaderToProperty(reader, ref obj, p);
                }
            }
        }

        private void ReaderToProperty(IDataReader reader, ref object obj, DataReadProperty p)
        {
#if debug
            try
            {
                if (p.Index >= 0)
                {
                    object dbvalue = reader[p.Index];
                    if (dbvalue != DBNull.Value)
                    {
                        var val = ConvertTo(dbvalue, p.Handler.Property.PropertyType);
                        p.Handler.Set(obj, val);
                    }
                }
            }
            catch (Exception exp)
            {
                throw new ApplicationException(string.Format(ErrorMessage.READER_TO_PROPERTY_ERROR, dbvalue, p.Handler.Property.Name), exp);
            }
#else
            if (p.Index >= 0)
            {
                object dbvalue = reader[p.Index];
                if (dbvalue != DBNull.Value)
                {
                    var val = ConvertTo(dbvalue, p.Handler.Property.PropertyType);
                    p.Handler.Set(obj, val);
                }
            }
#endif
        }

        private object ConvertTo(object value, Type type)
        {
            if (type == typeof(int?) || type == typeof(int))
            {
                return Convert.ToInt32(value);
            }
            if (type == typeof(long?) || type == typeof(long))
            {
                return Convert.ToInt64(value);
            }
            if (type == typeof(decimal?) || type == typeof(decimal))
            {
                return Convert.ToDecimal(value);
            }
            if (type.IsEnum)
            {
                return Enum.Parse(type, value.ToString());
            }
            if (type == typeof(string))
            {
                return value.ToString();
            }
            if (type == typeof(double?) || type == typeof(double))
            {
                return Convert.ToDouble(value);
            }
            if (type == typeof(DateTime?) || type == typeof(DateTime))
            {
                return Convert.ToDateTime(value);
            }
            if (type == typeof(bool))
            {
                return Convert.ToBoolean(value);
            }
            if (type == typeof(Guid) || type == typeof(Guid?))
            {
                return value.ToString().ToGuid();
            }
            return Convert.ChangeType(value, type);
        }

        private void ReadColumnIndex(IDataReader reader)
        {
            var dict = GetReadField(reader);
            foreach (DataReadProperty pm in _dataReadProperties)
            {
                var fieldName = pm.Name.ToLower();
                if (dict.ContainsKey(fieldName))
                {
                    pm.Index = dict[fieldName];
                }
                else
                {
                    pm.Index = -1;
                }
            }
        }

        private IDictionary<string, int> GetReadField(IDataReader reader)
        {
            var dict = new Dictionary<string, int>();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                dict.Add(reader.GetName(i).ToLower(), i);
            }
            return dict;
        }
    }

    internal class DataReadProperty
    {
        public DataReadProperty()
        {
            Index = -1;
        }

        public string Name
        {
            get;
            set;
        }

        public PropertyHandler Handler
        {
            get;
            set;
        }

        public int Index
        {
            get;
            set;
        }
    }
}