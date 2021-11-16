using Dapper;
using DevExtreme.AspNet.Data;
using DevExtreme.Dapper.Data.Aggregation;
using DevExtreme.Dapper.Data.Attributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DevExtreme.Dapper.Data
{
    internal partial class DataSourceDapperLoadContext
    {
        private int _paramIndex;
        private readonly FieldInfo[] Fields;
        private readonly DataSourceLoadOptions _options;
        private readonly DynamicParameters _params = new DynamicParameters();



        public Type _type { get; }
        public string SourceName { get; }

        public DynamicParameters Parameters => _params;
        public readonly string[] AllowedWhereFields;
        public readonly string[] AllowedSortFields;


        public DataSourceDapperLoadContext(DataSourceLoadOptions options, Type type, string name = null)
        {
            _options = options;
            _type = type;
            Fields = type.GetFields();


            if (name != null)
            {
                SourceName = name;
            }
            else
            {
                var meta = (TableAttribute)type.GetCustomAttributes()
                    .Single(x => x.GetType() == typeof(TableAttribute));

                SourceName = $"[{meta.Schema}].[{meta.Name}]";
            }

            _fullSelect = Fields
                .Where(x => x.CustomAttributes.All(xx => xx.AttributeType != typeof(DapperNotSelect)))
                .Select(x => $"[{x.Name}]").ToArray();

            AllowedWhereFields = Fields
                .Where(x => x.CustomAttributes.All(xx => xx.AttributeType != typeof(DapperNotWhere)))
                .Select(x => $"[{x.Name}]").ToArray();


            AllowedSortFields = AllowedWhereFields;


            if (_options.Take > 0)
            {
                HasPaging = true;
            }

            HasSort = !IsEmpty(_options.Sort);
        }

        public string AddIndexParam(object value)
        {
            var key = $"@p{++_paramIndex}";
            _params.Add(key, value);
            return key;
        }

        static bool IsEmpty<T>(IReadOnlyCollection<T> collection)
        {
            return collection == null || collection.Count < 1;
        }

        static bool IsEmptyList(IList list)
        {
            return list == null || list.Count < 1;
        }
    }

    // Total count
    internal partial class DataSourceDapperLoadContext
    {
        public bool RequireTotalCount => _options.RequireTotalCount;
        public bool IsCountQuery => _options.IsCountQuery;
    }

    // Filter
    internal partial class DataSourceDapperLoadContext
    {
        public IList Filter => _options.Filter;
        public bool HasFilter => _options.Filter != null && _options.Filter.Count > 1;
    }

    // Paging
    internal partial class DataSourceDapperLoadContext
    {
        public int Skip => _options.Skip;
        public int Take => _options.Take;
        public bool HasPaging { get; }
        public bool PaginateViaPrimaryKey => _options.PaginateViaPrimaryKey.GetValueOrDefault(false);
    }

    // Select
    internal partial class DataSourceDapperLoadContext
    {
        string[] _fullSelect;
        bool? _useRemoteSelect;

        public bool HasAnySelect => FullSelect.Count > 0;

        //public bool UseRemoteSelect
        //{
        //    get
        //    {
        //        if (!_useRemoteSelect.HasValue)
        //            _useRemoteSelect = _options.RemoteSelect ?? (!_providerInfo.IsLinqToObjects && FullSelect.Count <= AnonType.MAX_SIZE);

        //        return _useRemoteSelect.Value;
        //    }
        //}

        public IReadOnlyList<string> FullSelect
        {
            get
            {
                //string[] Init()
                //{
                //    var hasSelect = !IsEmpty(_options.Select);
                //    var hasPreSelect = !IsEmpty(_options.PreSelect);

                //    if (hasPreSelect && hasSelect)
                //        return Enumerable.Intersect(_options.PreSelect, _options.Select, StringComparer.OrdinalIgnoreCase).ToArray();

                //    if (hasPreSelect)
                //        return _options.PreSelect;

                //    if (hasSelect)
                //        return _options.Select;

                //    return new string[0];
                //}

                //if (_fullSelect == null)
                //    _fullSelect = Init();

                return _fullSelect;
            }
        }
    }

    //Sort & Primary Key
    internal partial class DataSourceDapperLoadContext
    {
        bool _primaryKeyAndDefaultSortEnsured;
        string[] _primaryKey;
        string _defaultSort;

        public bool HasAnySort => /*HasGroups ||*/ HasSort || ShouldSortByPrimaryKey || HasDefaultSort;

        public bool HasSort { get; }

        public IReadOnlyList<string> PrimaryKey
        {
            get
            {
                EnsurePrimaryKeyAndDefaultSort();
                return _primaryKey;
            }
        }

        string DefaultSort
        {
            get
            {
                EnsurePrimaryKeyAndDefaultSort();
                return _defaultSort;
            }
        }

        public bool HasPrimaryKey => !IsEmpty(PrimaryKey);

        bool HasDefaultSort => !String.IsNullOrEmpty(DefaultSort);

        bool ShouldSortByPrimaryKey => HasPrimaryKey && _options.SortByPrimaryKey.GetValueOrDefault(true);

        public IEnumerable<SortingInfo> GetFullSort()
        {
            var memo = new HashSet<string>();
            var result = new List<SortingInfo>();

            //if (HasGroups)
            //{
            //    foreach (var g in Group)
            //    {
            //        if (memo.Contains(g.Selector))
            //            continue;

            //        memo.Add(g.Selector);
            //        result.Add(g);
            //    }
            //}

            if (HasSort)
            {
                foreach (var s in _options.Sort)
                {
                    if (memo.Contains(s.Selector))
                        continue;

                    memo.Add(s.Selector);
                    result.Add(s);
                }
            }

            IEnumerable<string> requiredSort = new string[0];

            if (HasDefaultSort)
                requiredSort = requiredSort.Concat(new[] { DefaultSort });

            if (ShouldSortByPrimaryKey)
                requiredSort = requiredSort.Concat(PrimaryKey);

            return Utils.AddRequiredSort(result, requiredSort);
        }

        private void EnsurePrimaryKeyAndDefaultSort()
        {
            if (_primaryKeyAndDefaultSortEnsured)
                return;

            var primaryKey = _options.PrimaryKey;
            var defaultSort = _options.DefaultSort;

            if (IsEmpty(primaryKey))
                primaryKey = Utils.GetPrimaryKey(_type);

            if (HasPaging && string.IsNullOrEmpty(defaultSort) && IsEmpty(primaryKey))
                defaultSort = "Id";

            _primaryKey = primaryKey;
            _defaultSort = defaultSort;
            _primaryKeyAndDefaultSortEnsured = true;
        }
    }

    // Summary
    internal partial class DataSourceDapperLoadContext
    {
        bool? _summaryIsTotalCountOnly;

        public IReadOnlyList<SummaryInfo> TotalSummary => _options.TotalSummary;

        public IReadOnlyList<SummaryInfo> GroupSummary => _options.GroupSummary;

        public bool HasSummary => HasTotalSummary || HasGroupSummary;

        public bool HasTotalSummary => !IsEmpty(TotalSummary);

        public bool HasGroupSummary => HasGroups && !IsEmpty(GroupSummary);

        public bool SummaryIsTotalCountOnly
        {
            get
            {
                if (!_summaryIsTotalCountOnly.HasValue)
                    _summaryIsTotalCountOnly = !HasGroupSummary && HasTotalSummary && TotalSummary.All(i => i.SummaryType == AggregateName.COUNT);
                return _summaryIsTotalCountOnly.Value;
            }
        }

        public bool IsSummaryQuery => _options.IsSummaryQuery;

        public bool IsRemoteTotalSummary => UseRemoteGrouping && !SummaryIsTotalCountOnly && HasSummary && !HasGroups;

        public IReadOnlyList<SummaryInfo> GetValidTotalSummary() => TotalSummary;
    }

    // Grouping
    internal partial class DataSourceDapperLoadContext
    {
        bool?
            _shouldEmptyGroups,
            _useRemoteGrouping;

        public bool RequireGroupCount => _options.RequireGroupCount;

        public IReadOnlyList<GroupingInfo> Group => _options.Group;

        public bool HasGroups => !IsSummaryQuery && !IsEmpty(Group);

        public bool ShouldEmptyGroups
        {
            get
            {
                if (!_shouldEmptyGroups.HasValue)
                    _shouldEmptyGroups = HasGroups && !Group.Last().GetIsExpanded();
                return _shouldEmptyGroups.Value;
            }
        }

        public bool UseRemoteGrouping
        {
            get
            {

                bool HasAvg(IEnumerable<SummaryInfo> summary)
                {
                    return summary != null && summary.Any(i => i.SummaryType == "avg");
                }

                bool ShouldUseRemoteGrouping()
                {
                    return true;
                    //if (_providerInfo.IsLinqToObjects)
                    //    return false;

                    //if (_providerInfo.IsEFCore)
                    //{
                    //    var version = _providerInfo.Version;

                    //    // https://github.com/aspnet/EntityFrameworkCore/issues/2341
                    //    // https://github.com/aspnet/EntityFrameworkCore/issues/11993
                    //    // https://github.com/aspnet/EntityFrameworkCore/issues/11999
                    //    if (version < new Version(2, 2, 0))
                    //        return false;

                    //    if (version.Major < 5)
                    //    {
                    //        // https://github.com/aspnet/EntityFrameworkCore/issues/11711
                    //        if (HasAvg(TotalSummary) || HasAvg(GroupSummary))
                    //            return false;
                    //    }
                    //}

                    //return true;
                }

                if (!_useRemoteGrouping.HasValue)
                    _useRemoteGrouping = _options.RemoteGrouping ?? ShouldUseRemoteGrouping();

                return _useRemoteGrouping.Value;
            }
        }
    }
}
