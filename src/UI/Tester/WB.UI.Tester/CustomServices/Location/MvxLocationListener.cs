// MvxLocationListener.cs
// (c) Copyright Cirrious Ltd. http://www.cirrious.com
// MvvmCross is licensed using Microsoft Public License (Ms-PL)
// Contributions and inspirations noted in readme.md and license.txt
// 
// Project Lead - Stuart Lodge, @slodge, me@slodge.com


using ILocationListener = Android.Gms.Location.ILocationListener;

namespace WB.UI.QuestionnaireTester.CustomServices.Location
{
    public class MvxLocationListener
        : Java.Lang.Object
        , ILocationListener
    {
        private readonly IMvxLocationReceiver _owner;

        public MvxLocationListener(IMvxLocationReceiver owner)
        {
            this._owner = owner;
        }

        public void OnLocationChanged(Android.Locations.Location p0)
        {
            this._owner.OnLocationChanged(p0);
        }
    }
}