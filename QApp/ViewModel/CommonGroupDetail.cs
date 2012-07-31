using System.Windows;
using DevExpress.Xpf.Core;
using System.Windows.Input;
using QApp.Heritage;
using RavenQuestionnaire.Core.Views.Question;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Mobile;

namespace QApp.ViewModel
{
    public class CommonGroupDetailData : ModuleData
    {
        private CompleteGroupMobileView group;
        
        public CommonGroupDetailData(CompleteGroupMobileView group)
        {
            Group = group;
        }

        public CompleteGroupMobileView Group
        {
            get { return group; }
            private set { SetValue<CompleteGroupMobileView>("Group", ref group, value); }
        }
    }

    public class CommonGroupDetail : Module
    {
        public override void InitData(object parameter)
        {
            base.InitData(parameter);
           /* var item = parameter as CompleteGroupViewV;
            if(item != null)
                Data = new CommonGroupDetailData(item);*/
        }

       /* public CompleteGroupViewV ParentItem
        {
            set;get;
        }*/


        public CommonGroupDetailData GroupDetailData { get { return (CommonGroupDetailData)Data; } }



       /* protected override void InitializeCommands()
        {
           base.InitializeCommands();

            ShowModalQuestionCommand = new ExtendedActionCommand(DoShowModuleModal<QuestionData>, this, "CurrentModuleDataType", AllowSwitchToTheModule, typeof(QuestionData));
        }
*/
        

        #region Commands

        protected override void InitializeCommands()
        {
            base.InitializeCommands();
            //ShowModalQuestionCommand = new SimpleActionCommand(DoShowModalQuestionCommand);
            ShowQuestionCommand = new SimpleActionCommand(DoShowQuestion);
        }


        
        //move out of here
         void DoShowQuestion(object p)
        {
            var question = p as CompleteQuestionView;
            if (question != null)
            {
                var detail = (Question)ModulesManager.CreateModule(null, new QuestionData(question), this, question);

                DXWindow window = new DXWindow();
                window.Content = detail.View;
                window.WindowState = WindowState.Maximized;
                window.MinHeight = 350;
                window.ShowDialog();
            }
        }
        
        public ICommand ShowQuestionCommand { get; private set; }
        
        #endregion

    }
}
