using Android.Text;
using Android.Widget;
using AndroidX.Preference;

namespace WB.UI.Shared.Enumerator.Settings
{
    public static class PreferenceExtensions
    {
        class OnBindEditTextListener : Java.Lang.Object, EditTextPreference.IOnBindEditTextListener
        {
            private readonly InputTypes inputType;

            public OnBindEditTextListener(InputTypes inputType)
            {
                this.inputType = inputType;
            }

            public void OnBindEditText(EditText editText)
            {
                editText.InputType = inputType;
            }
        }
                
        public static Preference SetEditTextNumericMode(this Preference preference)
        {
            var editTextPreference = (EditTextPreference) preference;
            editTextPreference.SetOnBindEditTextListener(new OnBindEditTextListener(InputTypes.ClassNumber));
            return preference;
        }

        public static Preference SetEditTextDecimalMode(this Preference preference)
        {
            var editTextPreference = (EditTextPreference) preference;
            editTextPreference.SetOnBindEditTextListener(new OnBindEditTextListener(InputTypes.ClassNumber | InputTypes.NumberFlagDecimal));
            return preference;
        }
    }
}