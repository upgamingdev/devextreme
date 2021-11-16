using DevExtreme.AspNet.Data.ResponseModel;
using System;
using System.Data;

namespace DevExtreme.Dapper.Data
{
    public class DataSourceDapperLoader
    {
        public static LoadResult Load<T>(IDbConnection conn, DataSourceLoadOptions options)
        {
            return new DataSourceLoaderImpl(conn, typeof(T), options).Load();
        }

        public static LoadResult Load(IDbConnection conn, Type type, DataSourceLoadOptions options, string name = null)
        {
            return new DataSourceLoaderImpl(conn, type, options, name).Load();
        }
    }
}
