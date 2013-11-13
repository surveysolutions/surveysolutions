using WB.UI.QuestionnaireTester.Adapters;
using Android.App;
using Android.OS;

namespace WB.UI.QuestionnaireTester
{
    public class QuestionnaireListActivity : ListActivity 
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            this.ListAdapter = new QuestionnaireListAdapter(this);
        }
    }
}