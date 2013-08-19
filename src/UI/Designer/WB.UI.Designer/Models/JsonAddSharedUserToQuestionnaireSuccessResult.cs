namespace WB.UI.Designer.Models
{
    public class JsonAddSharedUserToQuestionnaireSuccessResult: JsonSuccessResult
    {
        public bool IsAlreadyShared { get; set; }
        public bool IsOwner { get; set; }
    }
}