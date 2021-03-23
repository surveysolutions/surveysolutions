using System.Collections.Generic;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Models
{
    public class ChangePasswordResult
    {
        public ChangePasswordResult()
        {
            this.Errors = new List<string>();
        }
        
        /// <summary>
        /// Created user id
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// List of errors when user is not created. Will be empty on success
        /// </summary>
        public List<string> Errors { get; set; }
    }
}