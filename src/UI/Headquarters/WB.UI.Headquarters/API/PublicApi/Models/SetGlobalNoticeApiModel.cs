using System.ComponentModel.DataAnnotations;

namespace WB.UI.Headquarters.API.PublicApi.Models
{
    public class SetGlobalNoticeApiModel
    {
        [Required]
        public string Message { get; set; }
    }
}
