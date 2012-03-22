using System;
using System.ComponentModel;
using System.Web;

namespace RavenQuestionnaire.Web.Models
{
    public class ImageNewViewModel
    {
        public Guid PublicKey { get; set; }

        public string QuestionnaireId { get; set; }

        [DisplayName("Title")]
        public string Title { get; set; }

        [DisplayName("Description")]
        public string Desc { get; set; }

        [DisplayName("File")]
        public HttpPostedFileBase FileDummy { get; set; }
    }
}