using System;
using System.Linq;
using Newtonsoft.Json;

namespace CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails
{
    public struct ItemPublicKey
    {
        [JsonConstructor]
        public ItemPublicKey(Guid publicKey, Guid? propagationKey)
        {
            PublicKey = publicKey;
            PropagationKey = propagationKey;
        }
        public readonly Guid PublicKey;
        public readonly Guid? PropagationKey;


        public override bool Equals(object obj)
        {
            return obj is ItemPublicKey && this == (ItemPublicKey)obj;
        }
        public override int GetHashCode()
        {
            return PublicKey.GetHashCode() ^ PropagationKey.GetHashCode();
        }
        public static bool operator ==(ItemPublicKey x, ItemPublicKey y)
        {
            return x.PublicKey == y.PublicKey && (
                                                     (!x.PropagationKey.HasValue && !y.PropagationKey.HasValue) ||
                                                     (x.PropagationKey == y.PropagationKey)
                                                 );
        }

        public static bool operator !=(ItemPublicKey x, ItemPublicKey y)
        {
            return !(x == y);
        }
        public override string ToString()
        {
            if (PropagationKey.HasValue)
                return string.Format("{0},{1}", PublicKey, PropagationKey);
            return PublicKey.ToString();
        }
        public static explicit operator ItemPublicKey(string b)  // explicit byte to digit conversion operator
        {
            return Parse(b);
        }
        public static ItemPublicKey Parse(string value)
        {
            if (value.Contains(','))
            {
                var items = value.Split(',');
                return new ItemPublicKey(Guid.Parse(items[0]), Guid.Parse(items[1]));
            }
            return new ItemPublicKey(Guid.Parse(value), null);
        }
    }

}