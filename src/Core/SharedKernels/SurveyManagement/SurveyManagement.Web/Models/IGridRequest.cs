using System.Collections.Generic;
using Main.Core.Entities;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Models
{
    public interface IGridRequest<out T>
    {
        #region Public Properties

        PagerData Pager { get; }

        T Request { get; }

        IEnumerable<OrderRequestItem> SortOrder { get; }

        #endregion
    }
}