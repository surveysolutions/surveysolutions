using System.Net.Http;
using System.Web.Http;
using WB.UI.Headquarters.Resources;

namespace WB.UI.Headquarters.API.Resources
{
    [System.Web.Http.AllowAnonymous]
    public class DataTableTranslationsController : ApiController
    {
        [HttpGet]
        public HttpResponseMessage Get()
        {
            var result = new
            {
                sEmptyTable = DataTables.EmptyTable,
                sInfo = DataTables.Info,
                sInfoEmpty = DataTables.InfoEmpty,
                sInfoFiltered = DataTables.InfoFiltered,
                sInfoPostFix = DataTables.InfoPostFix,
                sInfoThousands = DataTables.InfoThousands,
                sLengthMenu = DataTables.LengthMenu,
                sLoadingRecords = "<div>" + DataTables.LoadingRecords + "</div>",
                sProcessing = "<div>" + DataTables.Processing + "</div>",
                sSearch = DataTables.Search,
                searchPlaceholder = DataTables.SearchPlaceholder,
                sZeroRecords = DataTables.ZeroRecords,
                oPaginate = new
                {
                    sFirst = DataTables.Paginate_First,
                    sLast = DataTables.Paginate_Last,
                    sNext = DataTables.Paginate_Next,
                    sPrevious = DataTables.Paginate_Previous
                },
                oAria = new
                {
                    sSortAscending = DataTables.Aria_SortAscending,
                    sSortDescending = DataTables.Aria_SortDescending
                }
            };

            return Request.CreateResponse(result);
        }
    }
}
