using System;
using MvvmCross.ViewModels;

namespace WB.Core.SharedKernels.Enumerator.Views
{
    public class MapItem : MvxNotifyPropertyChanged
    {
        private string mapName;

        public string MapName
        {
            get => mapName;
            set => SetProperty(ref mapName, value);
        }

        private long size;
        public long Size {
            get => size;
            set => SetProperty(ref size, value);
        }

        private DateTime creationDate;
        public DateTime CreationDate
        {
            get => creationDate;
            set => SetProperty(ref creationDate, value);
        }
    }
}
