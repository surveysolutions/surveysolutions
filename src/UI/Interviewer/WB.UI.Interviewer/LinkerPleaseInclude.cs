using System.Collections.Specialized;
using Android.Views;
using Android.Widget;
using Flurl;
using Flurl.Http;

namespace WB.UI.Interviewer
{
    public class LinkerPleaseInclude
    {
        public void Include(Button button)
        {
            button.Click += (s, e) => button.Text = button.Text + "";
        }

        public void Include(CheckBox checkBox)
        {
            checkBox.CheckedChange += (sender, args) => checkBox.Checked = !checkBox.Checked;
        }

        public void Include(View view)
        {
            view.Click += (s, e) => view.ContentDescription = view.ContentDescription + "";
        }

        public void Include(TextView text)
        {
            text.TextChanged += (sender, args) => text.Text = "" + text.Text;
            text.Hint = "" + text.Hint;
        }

        public void Include(CompoundButton cb)
        {
            cb.CheckedChange += (sender, args) => cb.Checked = !cb.Checked;
        }

        public void Include(SeekBar sb)
        {
            sb.ProgressChanged += (sender, args) => sb.Progress = sb.Progress + 1;
        }

        public void Include(INotifyCollectionChanged changed)
        {
            changed.CollectionChanged += (s, e) => { var test = string.Format("{0}{1}{2}{3}{4}", e.Action, e.NewItems, e.NewStartingIndex, e.OldItems, e.OldStartingIndex); };
        }

        public void Include(System.Windows.Input.ICommand command)
        {
            command.CanExecuteChanged += (s, e) => { if (command.CanExecute(null)) command.Execute(null); };
        }

        public void Include(TableRow row)
        {
            row.Visibility = ViewStates.Gone;
        }

        public void Include(LinearLayout linearLayout)
        {
            linearLayout.Clickable = !linearLayout.Clickable;
        }

        public void Include()
        {
            //fix for Thai calendar (KP-6403)
            var thai = new System.Globalization.ThaiBuddhistCalendar();
        }

        public void Include(IFlurlClient flurlClient)
        {
            flurlClient.WithHeader("name", "value");
        }
    }
}