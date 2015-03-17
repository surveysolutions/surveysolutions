using WB.Core.BoundedContexts.Capi.ViewModel;
using Xamarin.Forms;

namespace WB.UI.Interviewer
{
    public class InterviewEntityTemplateSelector : IDataTemplateSelector
    {
        public DataTemplate GroupTemplate { get; set; }
        public DataTemplate RosterTemplate { get; set; }
        public DataTemplate StaticTextTemplate { get; set; }
        public DataTemplate TextQuestionTemplate { get; set; }
        public DataTemplate DateQuestionTemplate { get; set; }
        public DataTemplate DecimalQuestionTemplate { get; set; }
        public DataTemplate ImageQuestionTemplate { get; set; }
        public DataTemplate IntegerQuestionTemplate { get; set; }
        public DataTemplate MultiChoiceQuestionTemplate { get; set; }
        public DataTemplate SingleChoiceQuestionTemplate { get; set; }
        public DataTemplate AutocompleteSingleChoiceQuestionTemplate { get; set; }
        public DataTemplate CascadingSingleChoiceQuestionTemplate { get; set; }
        public DataTemplate GeolocationQuestionTemplate { get; set; }
        public DataTemplate LinkedMultiChoiceQuestionWithStringOptionsTemplate { get; set; }
        public DataTemplate LinkedSingleChoiceQuestionWithStringOptionsTemplate { get; set; }
        public DataTemplate LinkedMultiChoiceQuestionWithIntegerOptionsTemplate { get; set; }
        public DataTemplate LinkedSingleChoiceQuestionWithIntegerOptionsTemplate { get; set; }
        public DataTemplate LinkedMultiChoiceQuestionWithDecimalOptionsTemplate { get; set; }
        public DataTemplate LinkedSingleChoiceQuestionWithDecimalOptionsTemplate { get; set; }
        public DataTemplate LinkedMultiChoiceQuestionWithGeoOptionsTemplate { get; set; }
        public DataTemplate LinkedSingleChoiceQuestionWithGeoOptionsTemplate { get; set; }
        public DataTemplate ListQuestionTemplate { get; set; }
        public DataTemplate QRBarcodeQuestionTemplate { get; set; }

        public DataTemplate SelectTemplate(object view, object dataItem)
        {
            if (dataItem is InterviewRoster) return RosterTemplate;
            else if (dataItem is InterviewGroup) return GroupTemplate;
            else if (dataItem is InterviewStaticText) return StaticTextTemplate;
            else if (dataItem is InterviewTextQuestion) return TextQuestionTemplate;
            else if (dataItem is InterviewDateQuestion) return DateQuestionTemplate;
            else if (dataItem is InterviewDecimalQuestion) return DecimalQuestionTemplate;
            else if (dataItem is InterviewImageQuestion) return ImageQuestionTemplate;
            else if (dataItem is InterviewIntegerQuestion) return IntegerQuestionTemplate;
            else if (dataItem is InterviewMultiChoiceQuestion) return MultiChoiceQuestionTemplate;
            else if (dataItem is InterviewAutocompleteSingleChoiceQuestion) return AutocompleteSingleChoiceQuestionTemplate;
            else if (dataItem is InterviewCascadingSingleChoiceQuestion) return CascadingSingleChoiceQuestionTemplate;
            else if (dataItem is InterviewSingleChoiceQuestion) return SingleChoiceQuestionTemplate;
            else if (dataItem is InterviewGeolocationQuestion) return GeolocationQuestionTemplate;
            else if (dataItem is InterviewLinkedMultiChoiceQuestion<string>) return LinkedMultiChoiceQuestionWithStringOptionsTemplate;
            else if (dataItem is InterviewLinkedSingleChoiceQuestion<string>) return LinkedSingleChoiceQuestionWithStringOptionsTemplate;
            else if (dataItem is InterviewLinkedMultiChoiceQuestion<int>) return LinkedMultiChoiceQuestionWithIntegerOptionsTemplate;
            else if (dataItem is InterviewLinkedSingleChoiceQuestion<int>) return LinkedSingleChoiceQuestionWithIntegerOptionsTemplate;
            else if (dataItem is InterviewLinkedMultiChoiceQuestion<decimal>) return LinkedMultiChoiceQuestionWithDecimalOptionsTemplate;
            else if (dataItem is InterviewLinkedSingleChoiceQuestion<decimal>) return LinkedSingleChoiceQuestionWithDecimalOptionsTemplate;
            else if (dataItem is InterviewLinkedMultiChoiceQuestion<InterviewGeoLocation>) return LinkedMultiChoiceQuestionWithGeoOptionsTemplate;
            else if (dataItem is InterviewLinkedSingleChoiceQuestion<InterviewGeoLocation>) return LinkedSingleChoiceQuestionWithGeoOptionsTemplate;
            else if (dataItem is InterviewListQuestion) return ListQuestionTemplate;
            else if (dataItem is InterviewQrBarcodeQuestion) return QRBarcodeQuestionTemplate;

            return null;
        }
    }
}