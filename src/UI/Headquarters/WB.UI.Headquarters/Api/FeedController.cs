using System.Collections.Generic;
using System.Web.Http;
using WB.UI.Headquarters.Api.Authentication;

namespace WB.UI.Headquarters.Api
{
   
    public class FeedController : ApiController
    {
        [HttpGet]
        [Route("api/feed/v0")]
        [BasicAuthentication]
        public IEnumerable<object> Index()
        {
            return new List<object>()
            {
                new
                {
                    Test = "test",
                    Test1 = "test1"
                },
                new
                {
                    Test = "test2",
                    Test1 = "test3"
                }
            };
        }
    }
}