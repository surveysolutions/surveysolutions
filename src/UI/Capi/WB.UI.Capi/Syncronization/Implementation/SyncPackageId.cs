using System;
using Cirrious.MvvmCross.Plugins.Sqlite;

namespace WB.UI.Capi.Syncronization.Implementation
{
    public class SyncPackageId
    {
        [PrimaryKey]
        public string Id { get; set; }

        [Indexed]
        public int SortIndex { get; set; }
    }
}