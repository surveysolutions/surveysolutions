using System;
using System.Collections.Generic;
using System.Web;

namespace WB.UI.Headquarters.Models.Api
{
    public class DataTableResponse<T>
    {
        public int Draw { get; set; }
        public int RecordsTotal { get; set; }
        public int RecordsFiltered { get; set; }
        public IEnumerable<T> Data { get; set; }
    }

    public class QuestionnaireToBeImported
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string LastModified { get; set; }
        public string CreatedBy { get; set; }
    }
}