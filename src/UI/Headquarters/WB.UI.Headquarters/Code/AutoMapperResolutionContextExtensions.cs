using AutoMapper;

namespace WB.UI.Headquarters.Code
{
    public static class AutoMapperResolutionContextExtensions
    {
        /// <summary>
        /// Simple storage accessor for resolution context items
        /// </summary>
        public static T Get<T>(this ResolutionContext ctx) where T : class
        {
            return ctx.Items[typeof(T).Name] as T;
        }

        /// <summary>
        /// Simple storage setter for resolution context items
        /// </summary>
        public static void Set<T>(this ResolutionContext ctx, T item)
        {
            ctx.Items[typeof(T).Name] = item;
        }

        /// <summary>
        /// Resolve service from underlying DI
        /// </summary>
        public static T Resolve<T>(this ResolutionContext ctx) where T : class
        {
            return ctx.Mapper.ServiceCtor(typeof(T)) as T;
        }
    }
}
