namespace WB.UI.QuestionnaireTester.CustomControls.MvxBindableAutoCompleteTextViewControl
{
    public interface IMvxBindableListItemView
    {
        int TemplateId { get; }
        void BindTo(object source);
    }
}