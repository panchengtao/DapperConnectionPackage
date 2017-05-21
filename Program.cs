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
            BuckInsert();
            BuckInsert();
            BuckInsert();
            DeleteAfterUpdating();
            GetPersonList().ForEach(c=>Console.WriteLine(c.UserName));
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

        public static bool BuckInsert()
        {
            return ExecuteWithTransaction((conn, trans) =>
            {
                var r = conn.Execute(
                    @"insert Person(username, password,age,registerDate,address) values (@a, @b,@c,@d,@e)",
                    new[]
                    {
                        new {a = 1, b = 1, c = 1, d = DateTime.Now, e = 1},
                        new {a = 2, b = 2, c = 2, d = DateTime.Now, e = 2},
                        new {a = 3, b = 3, c = 3, d = DateTime.Now, e = 3}
                    },trans);

                return r;
            });
        }

        public static bool Update()
        {
            return ExecuteWithTransaction((conn, trans) =>
            {
                var r = conn.Execute(@"update Person set password='www.lanhuseo.com' where username=@username",
                    new {username = 2}, trans);

                return r;
            });
        }

        public static bool Delete()
        {
            return ExecuteWithTransaction((conn, trans) =>
            {
                var r = conn.Execute(@"delete from Person where id=@id", new {id = 1009}, trans);

                return r;
            });
        }

        public static bool DeleteAfterUpdating()
        {
            return ExecuteWithTransaction((conn, trans) =>
            {
                var r = conn.Execute(@"update Person set password='www.lanhuseo.com' where id=@id", new {id = 1009}, trans,
                    null, null);
                r += conn.Execute("delete from Person where id=@id", new {id = 1010}, trans, null, null);

                return r;
            });
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
        ///     Used for cud
        /// </summary>
        /// <returns>Execute Result</returns>
        /// <param name="func"></param>
        public static bool ExecuteWithTransaction(Func<SqlConnection, IDbTransaction, int> func)
        {
            var r = 0;

            UseConnectObj(conn =>
            {
                IDbTransaction trans = conn.BeginTransaction();

                r = func(conn, trans);

                trans.Commit();
            });

            return r > 0;
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