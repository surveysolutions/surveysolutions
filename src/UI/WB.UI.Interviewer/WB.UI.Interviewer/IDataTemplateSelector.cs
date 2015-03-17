using Xamarin.Forms;

namespace WB.UI.Interviewer
{
    public interface IDataTemplateSelector
    {
        DataTemplate SelectTemplate(object view, object dataItem);
    }
}