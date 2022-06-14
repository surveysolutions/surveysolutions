namespace WB.UI.Designer.Models
{
    public class AnonymousSharingEmailModel
    {
        public AnonymousSharingEmailModel(string userName, string sharingLink, string questionnaire)
        {
            UserName = userName;
            SharingLink = sharingLink;
            Questionnaire = questionnaire;
        }

        public string UserName { get; }
        public string Questionnaire { get; }
        public string SharingLink { get;  }

    }
}