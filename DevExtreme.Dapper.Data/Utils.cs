using DevExtreme.AspNet.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DevExtreme.Dapper.Data
{
    static class Utils
    {

        public static string[] GetPrimaryKey(Type type)
        {
            return new MemberInfo[0]
                .Concat(type.GetRuntimeProperties())
                .Concat(type.GetRuntimeFields())
                .Where(m => m.GetCustomAttributes(true).Any(i => i.GetType().Name == "KeyAttribute"))
                .Select(m => m.Name)
                .OrderBy(i => i)
                .ToArray();
        }

        public static IEnumerable<SortingInfo> AddRequiredSort(IEnumerable<SortingInfo> sort, IEnumerable<string> requiredSelectors)
        {
            sort = sort ?? new SortingInfo[0];
            requiredSelectors = requiredSelectors.Except(sort.Select(i => i.Selector), StringComparer.OrdinalIgnoreCase);

            var desc = sort.LastOrDefault()?.Desc;

            return sort.Concat(requiredSelectors.Select(i => new SortingInfo
            {
                Selector = i,
                Desc = desc != null && desc.Value
            }));
        }
    }
}
