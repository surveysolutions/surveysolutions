using System.Collections.Generic;
using System.Linq;

// It was done on purpose in oder not to include new namespaces where this extensions are used
namespace System
{
	public static class CollectionExtensions
	{
		public static IEnumerable<TBase> ToBaseCollection<TDerived, TBase>(this IEnumerable<TDerived> collection)
			where TDerived : TBase where TBase : class
		{
			return collection.Select(item => item as TBase)
				.ToList();
		}
	}
}