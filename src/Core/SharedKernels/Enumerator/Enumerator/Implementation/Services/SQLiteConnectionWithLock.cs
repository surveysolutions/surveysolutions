using System;
using System.Threading;
using SQLite;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Services
{
    public class SQLiteConnectionWithLock : SQLiteConnection
    {
        private readonly object _lockPoint = new object();

        public SQLiteConnectionWithLock(string databasePath, bool storeDateTimeAsTicks = true)
            : base(databasePath, storeDateTimeAsTicks) { }

        public SQLiteConnectionWithLock(string databasePath, SQLiteOpenFlags openFlags, bool storeDateTimeAsTicks = true)
            : base( databasePath, openFlags, storeDateTimeAsTicks) { }

        
        public IDisposable Lock()
        {
            return new LockWrapper(_lockPoint);
        }

        private class LockWrapper : IDisposable
        {
            private readonly object _lockPoint;

            public LockWrapper(object lockPoint)
            {
                _lockPoint = lockPoint;
                Monitor.Enter(_lockPoint);
            }

            public void Dispose()
            {
                Monitor.Exit(_lockPoint);
            }
        }
    }
}
