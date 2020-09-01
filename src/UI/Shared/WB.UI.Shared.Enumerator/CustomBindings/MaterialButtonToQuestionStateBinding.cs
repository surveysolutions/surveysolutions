using Android.Content.Res;
using Android.Graphics;
using AndroidX.Core.Content;
using Google.Android.Material.Button;
using Polly;
using WB.UI.Shared.Enumerator.Converters;

namespace WB.UI.Shared.Enumerator.CustomBindings
{
    public class MaterialButtonToQuestionStateBinding : BaseBinding<MaterialButton, QuestionStateStyle>
    {
        public MaterialButtonToQuestionStateBinding(MaterialButton androidControl) : base(androidControl)
        {
        }

        protected override void SetValueToView(MaterialButton control, QuestionStateStyle value)
        {
            switch (value)
            {
                case QuestionStateStyle.InvalidEnabled:
                case QuestionStateStyle.AnsweredEnabled:
                    var borderColor = new Color(ContextCompat.GetColor(control.Context, Resource.Color.editorAnsweredBorder));
                    control.StrokeColor = ColorStateList.ValueOf(borderColor);
                    break;
                default:
                    control.StrokeColor = ColorStateList.ValueOf(new Color(Android.Resource.Color.Transparent));
                    break;
            }
        }
    }
}
