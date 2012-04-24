using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using DevExpress.RealtorWorld.Xpf.Helpers;
using DevExpress.RealtorWorld.Xpf.ViewModel;
using DevExpress.Xpf.Core;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Vertical;
using RavenQuestionnaire.Core.Views.Question;

namespace QApp.ViewModel
{
    public class CommonGroupDetailData : ModuleData
    {
        private CompleteGroupViewV group;


        public CommonGroupDetailData(CompleteGroupViewV group)
        {
            Group = group;
        }

         public CompleteGroupViewV  Group {
            get { return group; }
            private set { SetValue<CompleteGroupViewV >("Group", ref group, value); }
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
