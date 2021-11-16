using System;

namespace DevExtreme.Dapper.Data.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class DapperOperations : Attribute
    {
        public readonly string[] Allowed;

        public DapperOperations(params string[] allow)
        {
            Allowed = allow;
        }
    }
}
