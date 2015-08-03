using System.Runtime.CompilerServices;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;

namespace WB.Core.BoundedContexts.Tester
{
    internal static class Extensions
    {
        public static Identity ToIdentityForEvents(this SharedKernels.DataCollection.Identity identity)
        {
            return new Identity(identity.Id, identity.RosterVector);
        }

        public static TReturn RaiseAndSetIfChanged<T, TReturn>(this T source, ref TReturn backingField, TReturn newValue, [CallerMemberName] string propertyName = "")
            where T : IMvxNotifyPropertyChanged
        {
            return MvxNotifyPropertyChangedExtensions.RaiseAndSetIfChanged(source, ref backingField, newValue, propertyName);
        }
    }
}