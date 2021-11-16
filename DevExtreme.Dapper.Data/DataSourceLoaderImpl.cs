using Dapper;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Data.ResponseModel;
using DevExtreme.Dapper.Data.RemoteGrouping;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace DevExtreme.Dapper.Data
{
    internal class DataSourceLoaderImpl
    {
        private readonly IDbConnection Conn;
        private readonly DataSourceDapperLoadContext Context;
        private readonly SqlServerQueryBuilder SqlServer;

        public DataSourceLoaderImpl(IDbConnection conn, Type type, DataSourceLoadOptions options, string name = null)
        {
            this.Conn = conn;
            this.Context = new DataSourceDapperLoadContext(options, type, name);
            this.SqlServer = new SqlServerQueryBuilder(Context);
        }

        internal LoadResult Load()
        {
            if (Context.IsCountQuery)
                return new LoadResult { totalCount = ExecTotalCount() };

            if (Context.IsSummaryQuery)
                return LoadAggregatesOnly();


            var result = new LoadResult();



            if (Context.UseRemoteGrouping && Context.ShouldEmptyGroups)
            {
                throw new NotImplementedException(nameof(Context.UseRemoteGrouping));
                //var remotePaging = Context.HasPaging && Context.Group.Count == 1;
                //var groupingResult = await ExecRemoteGroupingAsync(remotePaging, false, remotePaging);

                //EmptyGroups(groupingResult.Groups, Context.Group.Count);

                //result.data = groupingResult.Groups;
                //if (!remotePaging)
                //    result.data = Paginate(result.data, Context.Skip, Context.Take);

                //if (remotePaging)
                //{
                //    if (Context.HasTotalSummary)
                //    {
                //        var totalsResult = await ExecRemoteTotalsAsync();
                //        result.summary = totalsResult.Totals;
                //        result.totalCount = totalsResult.TotalCount;
                //    }
                //    else if (Context.RequireTotalCount)
                //    {
                //        result.totalCount = await ExecTotalCountAsync();
                //    }
                //}
                //else
                //{
                //    result.summary = groupingResult.Totals;
                //    result.totalCount = groupingResult.TotalCount;
                //}

                //if (Context.RequireGroupCount)
                //{
                //    throw new NotImplementedException(nameof(Context.RequireGroupCount));
                //    //result.groupCount = remotePaging
                //    //    ? await ExecCountAsync(CreateBuilder().BuildGroupCountExpr())
                //    //    : groupingResult.Groups.Count();
                //}
            }
            else
            {
                var deferPaging = Context.HasGroups || !Context.UseRemoteGrouping && !Context.SummaryIsTotalCountOnly && Context.HasSummary;

                //Expression loadExpr;

                if (!deferPaging && Context.PaginateViaPrimaryKey && Context.Take > 0)
                {
                    if (!Context.HasPrimaryKey)
                    {
                        throw new InvalidOperationException(nameof(DataSourceLoadOptionsBase.PaginateViaPrimaryKey)
                            + " requires a primary key."
                            + " Specify it via the " + nameof(DataSourceLoadOptionsBase.PrimaryKey) + " property.");
                    }

                    //var loadKeysExpr = CreateBuilder().BuildLoadExpr(true, selectOverride: Context.PrimaryKey);
                    //var keyTuples = await ExecExprAnonAsync(loadKeysExpr);

                    //loadExpr = CreateBuilder().BuildLoadExpr(false, filterOverride: FilterFromKeys(keyTuples));
                }
                else
                {
                    //loadExpr = CreateBuilder().BuildLoadExpr(!deferPaging);
                }

                if (Context.HasAnySelect)
                {
                    //ContinueWithGrouping(
                    //    await ExecWithSelect(),
                    //    result
                    //);
                }
                else
                {
                    //ContinueWithGroupingAsync(
                    //    await ExecExprAsync<S>(loadExpr),
                    //    result
                    //);
                }

                if (deferPaging)
                    result.data = Paginate(result.data, Context.Skip, Context.Take);

                //if (Context.ShouldEmptyGroups)
                //    EmptyGroups(result.data, Context.Group.Count);
            }

            return result;
        }

        void ContinueWithAggregation(/*IEnumerable data, IAccessor<R> accessor,*/ LoadResult result/*, bool includeData*/)
        {
            if (Context.IsRemoteTotalSummary)
            {
                RemoteGroupingResult totalsResult = ExecRemoteTotals();
                result.totalCount = totalsResult.TotalCount;
                result.summary = totalsResult.Totals;
            }
            else
            {
                var totalCount = -1;

                if (Context.RequireTotalCount || Context.SummaryIsTotalCountOnly)
                    totalCount = ExecTotalCount();

                if (Context.RequireTotalCount)
                    result.totalCount = totalCount;

                if (Context.SummaryIsTotalCountOnly)
                {
                    result.summary = Enumerable.Repeat((object)totalCount, Context.TotalSummary.Count).ToArray();
                }
                else if (Context.HasSummary)
                {
                    //if (includeData)
                    //    data = Buffer<R>(data);

                    //result.summary = new AggregateCalculator<R>(data, accessor, Context.TotalSummary, Context.GroupSummary).Run();
                }
            }

            //if (includeData)
            //    result.data = data;
        }

        private RemoteGroupingResult ExecRemoteTotals()
        {
            var sql = SqlServer.BuildAggregateQuery();

            var row = Conn.QuerySingle(sql, Context.Parameters);

            var list = new List<object>();
            foreach (var item in row)
            {
                list.Add(item.Value);
            }

            return new RemoteGroupingResult
            {
                TotalCount = -1,
                Totals = list.ToArray()
            };
        }

        private LoadResult LoadAggregatesOnly()
        {
            var result = new LoadResult();

            if (!Context.HasTotalSummary || Context.IsRemoteTotalSummary)
            {
                ContinueWithAggregation(result);
            }
            else
            {
                ContinueWithAggregation(result);
                //var data = await ExecExprAsync<T>(CreateBuilder().BuildLoadExpr(false));
                //await ContinueWithAggregationAsync(data, new DefaultAccessor<S>(), result, false);
            }

            return result;
        }

        private int ExecTotalCount()
        {
            var sql = SqlServer.BuildCountQuery();
            return Conn.ExecuteScalar<int>(sql);
        }

        private IEnumerable Paginate(IEnumerable data, int skip, int take)
        {
            throw new NotImplementedException();
        }

        private static IEnumerable Buffer<B>(IEnumerable data)
        {
            return data is ICollection ? data : ((IEnumerable<B>)data).ToArray();
        }

    }
}
