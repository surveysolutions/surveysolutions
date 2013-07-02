using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Environment = Android.OS.Environment;

namespace CAPI.Android.Core.Model.Backup
{
    public class SqliteBackup : IBackup
    {
        private readonly string eventStoreName;
        private readonly string projectionStoreName;
        private const string Capi = "CAPI";
        private const string BackupFolder = "Backup";
        private readonly string backupPath;

        public SqliteBackup(string eventStoreName, string projectionStoreName)
        {
            this.eventStoreName = eventStoreName;
            this.projectionStoreName = projectionStoreName;

            backupPath = Environment.ExternalStorageDirectory.AbsolutePath;
            if (Directory.Exists(backupPath))
            {
                backupPath = System.IO.Path.Combine(backupPath, Capi);
                if (!Directory.Exists(backupPath))
                {
                    Directory.CreateDirectory(backupPath);
                }
                backupPath = System.IO.Path.Combine(backupPath, BackupFolder);
                if (!Directory.Exists(backupPath))
                    Directory.CreateDirectory(backupPath);

            }
            else
            {
                backupPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            }
        }

        public void Backup()
        {
            CopyDb(eventStoreName, "events");
            CopyDb(projectionStoreName, "projections");
        }

        private void CopyDb(string sourceName, string sourceType)
        {
            File.Copy(
                System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal),
                                       sourceName),
                Path.Combine(backupPath, string.Format("backup-{0}{1}", sourceType, DateTime.Now.Ticks)), true);
        }

        public void Restore()
        {
            throw new NotImplementedException();
        }
    }
}