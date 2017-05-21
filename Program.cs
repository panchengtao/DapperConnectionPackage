using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using DapperDemo.Models;

namespace DapperDemo
{
    internal class Program
    {
        public static string ConnString = "Server=(localdb)\\MSSQLLocalDB;Database=DapperDB;";

        private static void Main(string[] args)
        {
            DeleteAfterUpdating();
        }

        public static List<Person> GetPersonList()
        {
            var people = new List<Person>();

            ExecuteWithoutTransaction(conn =>
            {
                people = conn.Query<Person>("select * from Person where id>@id", new {id = 2}).ToList();
            });

            return people;
        }

        public static void BuckInsert()
        {
            ExecuteWithoutTransaction(conn =>
            {
                var r = conn.Execute(
                    @"insert Person(username, password,age,registerDate,address) values (@a, @b,@c,@d,@e)",
                    new[]
                    {
                        new {a = 1, b = 1, c = 1, d = DateTime.Now, e = 1},
                        new {a = 2, b = 2, c = 2, d = DateTime.Now, e = 2},
                        new {a = 3, b = 3, c = 3, d = DateTime.Now, e = 3}
                    });
            });
        }

        public static bool Update()
        {
            var r = 0;

            ExecuteWithoutTransaction(conn =>
            {
                r = conn.Execute(@"update Person set password='www.lanhuseo.com' where username=@username",
                    new {username = 2});
            });

            return r > 0;
        }

        public static bool Delete()
        {
            var r = 0;

            ExecuteWithoutTransaction(conn => { r = conn.Execute(@"delete from Person where id=@id", new {id = 10}); });

            return r > 0;
        }

        public static bool DeleteAfterUpdating()
        {
            var r = 0;

            ExecuteWithTransaction((conn, trans) =>
            {
                r = conn.Execute(@"update Person set password='www.lanhuseo.com' where id=@id", new {id = 5}, trans,
                    null, null);
                r += conn.Execute("delete from Person where id=@id", new {id = 6}, trans, null, null);
            });

            return r > 0;
        }

        /// <summary>
        ///     Used for query
        /// </summary>
        /// <param name="action"></param>
        public static void ExecuteWithoutTransaction(Action<SqlConnection> action)
        {
            UseConnectObj(action);
        }

        /// <summary>
        ///     Used for curd
        /// </summary>
        /// <param name="action"></param>
        public static void ExecuteWithTransaction(Action<SqlConnection, IDbTransaction> action)
        {
            UseConnectObj(conn =>
            {
                IDbTransaction trans = conn.BeginTransaction();

                action(conn, trans);

                trans.Commit();
            });
        }

        /// <summary>
        ///     Use Action Connection
        /// </summary>
        /// <param name="action"></param>
        public static void UseConnectObj(Action<SqlConnection> action)
        {
            using (var conn = new SqlConnection(ConnString))
            {
                conn.Open();
                action(conn);
            }
        }
    }
}