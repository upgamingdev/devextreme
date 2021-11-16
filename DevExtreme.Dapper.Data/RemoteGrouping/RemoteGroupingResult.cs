using DevExtreme.AspNet.Data.ResponseModel;
using System.Collections.Generic;

namespace DevExtreme.Dapper.Data.RemoteGrouping
{
    internal class RemoteGroupingResult
    {
        public List<Group> Groups;
        public object[] Totals;
        public int TotalCount;
    }
}
