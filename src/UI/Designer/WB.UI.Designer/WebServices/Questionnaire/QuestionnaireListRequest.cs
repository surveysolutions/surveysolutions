using System.ServiceModel;

namespace WB.UI.Designer.WebServices.Questionnaire
{
    [MessageContract]
    public class QuestionnaireListRequest
    {
        [MessageHeader]
        public string Filter;

        [MessageHeader]
        public int PageIndex;

        [MessageHeader]
        public int PageSize;

        [MessageHeader]
        public string SortOrder;
    }
}