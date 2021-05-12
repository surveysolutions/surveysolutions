using System;
using System.Collections.Generic;
using WB.UI.Headquarters.Models.Api;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Models
{
    public class TypeaheadApiView : TypeaheadApiView<Guid>
    {
        public TypeaheadApiView(int page, int pageSize, int totalCount, IEnumerable<TypeaheadOptionalApiView<Guid>> options, string order) 
            : base(page, pageSize, totalCount, options, order)
        {
        }
    }
    public class TypeaheadApiView<T> : BaseApiView
    {
        public IEnumerable<TypeaheadOptionalApiView<T>> options { get; set; }

        public TypeaheadApiView(int page, int pageSize, int totalCount, IEnumerable<TypeaheadOptionalApiView<T>> options, string order)
        {
            this.Offset = page;
            this.TotalCount = totalCount;
            this.Limit = pageSize;
            this.options = options;
            this.Order = order;
        }

    }

    public class TypeaheadOptionalApiView : TypeaheadOptionalApiView<Guid>
    {
    }

    public class TypeaheadOptionalApiView<T>
    {
        public T key { get; set; }
        public string value { get; set; }
        public string iconClass { get; set; }
    }
}
