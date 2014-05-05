namespace Web.Supervisor.Models
{
    using System.Collections.Generic;

    using Main.Core.Entities;

    public interface IGridRequest<out T>
    {
        #region Public Properties

        PagerData Pager { get; }

        T Request { get; }

        IEnumerable<OrderRequestItem> SortOrder { get; }

        #endregion
    }
}