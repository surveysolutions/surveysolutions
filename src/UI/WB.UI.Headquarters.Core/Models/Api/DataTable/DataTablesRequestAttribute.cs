using Microsoft.AspNetCore.Mvc;

namespace WB.UI.Headquarters.Models.Api.DataTable
{
    public class DataTablesRequestAttribute : ModelBinderAttribute
    {
        /// <summary>
        /// Initialize a new instance of <see cref="DataTablesRequestAttribute"/>
        /// </summary>
        public DataTablesRequestAttribute() : base(typeof(DataTablesRequestModelBinder))
        {

        }
    }
}
