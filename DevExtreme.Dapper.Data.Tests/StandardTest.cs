using DevExtreme.AspNet.Data;
using DevExtreme.Dapper.Data.Tests.DTO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Xunit;

namespace DevExtreme.Dapper.Data.Tests
{
    public class StandardTest
    {
        private const string ConnStr = "SERVER=.;DATABASE=TestDB;Integrated Security=True";
        private static IDbConnection Conn => new SqlConnection(ConnStr);

        [Fact]
        public void IsCountQuery_Only()
        {
            var opt = new DataSourceLoadOptions()
            {
                IsCountQuery = true
            };

            var result = DataSourceDapperLoader.Load<Users>(Conn, opt);
        }

        [Fact]
        public void IsSummaryQuery_Only()
        {
            var opt = new DataSourceLoadOptions()
            {
                IsSummaryQuery = true,
                TotalSummary = new[]
                {
                    new SummaryInfo()
                    {
                        Selector = "Id",
                        SummaryType = "count"
                    },
                    new SummaryInfo()
                    {
                        Selector = "SignInCount",
                        SummaryType = "sum"
                    }
                }
            };

            var result = DataSourceDapperLoader.Load<Users>(Conn, opt);
        }

        [Fact]
        public void Where_And_Count()
        {
            var whereItems = new List<object>();

            whereItems.Add(new[] { "Id", "=", "1" });
            whereItems.Add("and");
            whereItems.Add(new[] { "FirstName", "=", "გიორგი" });

            whereItems.Add("or");

            whereItems.Add(new object[]
            {
                new object[] { "LastName", "=", "მაისურაძე" },
                "or",
                new object[] { new object[]
                    {
                        new[] { "Id", "=", "1" },
                        "or",
                        new[] { "Id", "=", "1" }
                    }
                }
            });


            var opt = new DataSourceLoadOptions
            {
                Skip = 0,
                Take = 10,
                RequireTotalCount = true,
                Sort = new[]
                {
                    new SortingInfo
                    {
                        Selector = "Id",
                        Desc = true
                    },
                    new SortingInfo
                    {
                        Selector = "CreateDate"
                    }
                },
                Filter = whereItems
            };

            using var conn = new SqlConnection(ConnStr);
            var result = DataSourceDapperLoader.Load<Users>(conn, opt);

        }

        [Fact]
        public void Where_And_TotalSummary()
        {
            var whereItems = new List<object>();

            whereItems.Add(new[] { "Id", ">", "1" });


            var opt = new DataSourceLoadOptions
            {
                Skip = 0,
                Take = 10,
                RequireTotalCount = true,
                Sort = new[]
                {
                    new SortingInfo()
                    {
                        Selector = "Id",
                        Desc = true
                    }
                },
                Filter = whereItems,
                TotalSummary = new[]
                {
                    new SummaryInfo
                    {
                        Selector = "SignInCount",
                        SummaryType = "sum"
                    }
                }
            };

            using var conn = new SqlConnection(ConnStr);
            var result = DataSourceDapperLoader.Load<Users>(conn, opt);

        }
    }
}
