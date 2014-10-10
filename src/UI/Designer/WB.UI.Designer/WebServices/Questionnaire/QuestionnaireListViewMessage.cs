using System.Linq;
using System.ServiceModel;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;

namespace WB.UI.Designer.WebServices.Questionnaire
{
    [MessageContract]
    public class QuestionnaireListViewMessage
    {
        public QuestionnaireListViewMessage(QuestionnaireListView data)
        {
            this.Order = data.Order;
            this.Page = data.Page;
            this.PageSize = data.PageSize;
            this.TotalCount = data.TotalCount;
            this.Items = data.Items.Select(item => new QuestionnaireListViewItemMessage(item)).ToArray();
        }

        [MessageHeader]
        public string Order;

        [MessageHeader]
        public int Page;

        [MessageHeader]
        public int PageSize;

        [MessageHeader]
        public int TotalCount;

        [MessageBodyMember]
        public QuestionnaireListViewItemMessage[] Items;
    }
}