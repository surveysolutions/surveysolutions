using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Cirrious.MvvmCross.Plugins.Sqlite;

namespace AndroidNcqrs.Eventing.Storage.SQLite.Extensions
{
  /*  public static class SQLiteConnectionFactoryExtensions
    {
        public static T WrapConnection<T>(this ISQLiteConnectionFactory factory ,Func<ISQLiteConnection, T> action)
        {
            using (var connection = factory.Create("EventStore"))
            {
                return action(connection);
            }
        }
    }*/
}