using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DevExtreme.Dapper.Data
{
    internal class SqlServerQueryBuilder
    {
        readonly DataSourceDapperLoadContext Context;
        readonly StringBuilder stringBuilder;

        private static readonly string[] LogicalOperators =
        {
            "AND",
            "OR"
        };

        private static readonly string[] ConditionOperators =
        {
            "=", ">", "<", ">=", "<=", "<>"
        };

        private string filterCMD;
        private string sortCMD;
        private string pagingCMD;
        private string selectCMD;
        private string selectCountCMD;
        private string selectTotalSummaryCMD;

        private string fromCMD => $"FROM {Context.SourceName}";

        public SqlServerQueryBuilder(DataSourceDapperLoadContext context)
        {
            Context = context;
            stringBuilder = new StringBuilder();
        }

        public string BuildCountQuery()
        {
            AddFilter();
            AddCount();

            return $"{selectCountCMD}{filterCMD};";
        }

        public string BuildAggregateQuery()
        {
            AddFilter();
            AddTotalSummary();

            return $"{selectTotalSummaryCMD}{filterCMD};";
        }


        public string BuildQuery()
        {
            AddSelect();
            AddFilter();
            AddSort();
            AddPaging();

            PrintResult();
            return $"{selectCMD}{filterCMD}{sortCMD}{pagingCMD};";
        }






        private void AddTotalSummary()
        {
            if (selectTotalSummaryCMD != null || !Context.HasTotalSummary) return;

            var columns = Context
                .GetValidTotalSummary()
                .Select(total => $"{total.SummaryType}([{total.Selector}])").ToArray();

            if (columns.Any())
                selectTotalSummaryCMD = $"SELECT {string.Join(", ", columns)} {fromCMD}";
        }

        private void AddFilter()
        {
            if (filterCMD == null && Context.HasFilter)
                filterCMD = Where(Context.Filter, Context.Filter.GetType(), true);
        }

        private string Where(object obj, Type typ, bool isFirstCall = false)
        {
            if (typ.IsArray || typ.IsGenericType)
            {
                var firstElement = ((IEnumerable<object>)obj).First();

                var elTyp = firstElement.GetType();
                if (elTyp == typeof(string))
                {
                    return AddCondition((IEnumerable<object>)obj);
                }

                var sb = new StringBuilder();
                foreach (var item in (IEnumerable<object>)obj)
                {
                    sb.Append(Where(item, item.GetType()));
                }

                return isFirstCall ? $"{Environment.NewLine}WHERE {sb}" : $"({sb})";
            }


            if (typ == typeof(string))
                return AddWhereOperator(obj.ToString().ToUpper());

            throw new InvalidOperationException();
        }

        private string AddCondition(IEnumerable<object> condition)
        {
            var cond = condition.ToArray();
            if (cond.Length != 3)
                throw new InvalidOperationException();

            return $"{cond[0]} {ValidateCondition(cond[1])} {Context.AddIndexParam(cond[2])}";
        }

        private void AddPaging()
        {
            if (pagingCMD != null || !Context.HasPaging) return;

            pagingCMD = $"{Environment.NewLine}OFFSET {Context.Skip} ROWS";
            pagingCMD += $"{Environment.NewLine}FETCH NEXT {Context.Take} ROWS ONLY";
        }

        private void AddCount()
        {
            selectCountCMD = selectCountCMD ?? $"SELECT COUNT(1) {Environment.NewLine}{fromCMD}";
        }

        private void AddSelect()
        {
            if (selectCMD != null) return;

            var select = string.Join(", ", Context.FullSelect);
            selectCMD = $"SELECT {select} {Environment.NewLine}{fromCMD}";
        }

        private static string AddWhereOperator(string op)
        {
            return LogicalOperators.Contains(op)
                ? $" {op} "
                : throw new ArgumentOutOfRangeException($"'{op}' operation not allowed");
        }


        private static object ValidateCondition(object op)
        {
            return ConditionOperators.Contains(op)
                ? op
                : throw new ArgumentOutOfRangeException($"'{op}' condition operation not allowed");
        }


        private void AddSort()
        {
            if (sortCMD != null || !Context.HasAnySort) return;

            var sorts = Context
                .GetFullSort()
                .Select(sort => $"{sort.Selector} {(sort.Desc ? "DESC" : "ASC")}").ToList();

            if (sorts.Any())
                sortCMD = $"{Environment.NewLine}ORDER BY " + string.Join(", ", sorts);
        }

        public void PrintResult()
        {
            //Console.WriteLine(new string('-', 50) + " Sql Command " + new string('-', 50));
            //Console.WriteLine();
            //Console.WriteLine(SqlCommand);

            Console.WriteLine();
            Console.WriteLine("Args");
            Console.WriteLine(new string('-', 50) + " Sql Args " + new string('-', 50));
            foreach (var param in Context.Parameters.ParameterNames)
            {
                Console.WriteLine(param + " = " + Context.Parameters.Get<object>(param));
            }

            Console.WriteLine();
            Console.WriteLine(new string('-', 50));
        }
    }
}
