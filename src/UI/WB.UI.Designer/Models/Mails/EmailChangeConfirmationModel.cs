namespace WB.UI.Designer.Models.Mails
{
    public class EmailChangeConfirmationModel
    {
        public EmailChangeConfirmationModel(string userName, string confirmationLink)
        {
            UserName = userName;
            ConfirmationLink = confirmationLink;
        }

        public string UserName { get; }
        public string ConfirmationLink { get;  }
    }
}
