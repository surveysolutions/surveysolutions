using System.Windows;
using System.Windows.Input;
using DevExpress.RealtorWorld.Xpf.Helpers;
using DevExpress.RealtorWorld.Xpf.ViewModel;
using DevExpress.Xpf.Core;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Vertical;
using RavenQuestionnaire.Core.Views.Question;

namespace QApp.ViewModel
{
    public class PropagatedGroupDetailData : ModuleData
    {
        private CompleteGroupViewV _group;


        public PropagatedGroupDetailData(CompleteGroupViewV group)
        {
            Group = group;
        }

         public CompleteGroupViewV  Group {
            get { return _group; }
            private set { SetValue<CompleteGroupViewV >("Group", ref _group, value); }
        }

    }

    public class PropagatedGroupDetail : Module
    {
        public override void InitData(object parameter)
        {
            base.InitData(parameter);
        }

        public PropagatedGroupDetailData GroupDetailData { get { return (PropagatedGroupDetailData)Data; } }


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
                window.ShowDialog();
            }
        }


        public ICommand ShowQuestionCommand { get; private set; }


        
        #endregion

    }
}
