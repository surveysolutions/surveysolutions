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
using WB.Core.SharedKernel.Utils.Serialization;

namespace WB.Core.GenericSubdomain.Rest.Android
{
    internal class AndroidRestServiceWrapperFactory : IRestServiceWrapperFactory
    {
        private readonly IJsonUtils jsonUtils;

        public AndroidRestServiceWrapperFactory(IJsonUtils jsonUtils)
        {
            this.jsonUtils = jsonUtils;
        }

        public IRestServiceWrapper CreateRestServiceWrapper(string baseAddress)
        {
            return new AndroidRestServiceWrapper(baseAddress, jsonUtils);
        }
    }
}