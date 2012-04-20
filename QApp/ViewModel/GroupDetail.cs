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
    public class GroupDetailData : ModuleData
    {
        private CompleteGroupViewV group;


        public GroupDetailData(CompleteGroupViewV group)
        {
            Group = group;
        }

         public CompleteGroupViewV  Group {
            get { return group; }
            private set { SetValue<CompleteGroupViewV >("Group", ref group, value); }
        }

    }

    public class GroupDetail : Module
    {
        public override void InitData(object parameter)
        {
            base.InitData(parameter);
            var item = parameter as CompleteGroupViewV;
            ObservableCollection<GroupDetail> groupDetailsLoc = new ObservableCollection<GroupDetail>();

            if (item != null)
            {

                foreach (var completeGroupViewV in item.Groups)
                {
                    groupDetailsLoc.Add((GroupDetail)ModulesManager.CreateModule(null, new GroupDetailData(completeGroupViewV), this, completeGroupViewV));
                }
            }

            GroupDetails = groupDetailsLoc;


            //ShowModalQuestionCommand = new ExtendedActionCommand(DoShowModuleModal<QuestionData>, this, "CurrentModuleDataType", AllowSwitchToTheModule, typeof(QuestionData));

        }

        public override List<Module> GetSubmodules()
        {
            List<Module> submodules = base.GetSubmodules();
            foreach (var module in GroupDetails)
            {
                submodules.Add(module);
            }
            return submodules;
        }



        private ObservableCollection<GroupDetail> groupDetail;
        public ObservableCollection<GroupDetail> GroupDetails
        {
            get { return groupDetail; }
            private set { SetValue<ObservableCollection<GroupDetail>>("GroupDetails", ref groupDetail, value); }
        }


        public GroupDetailData GroupDetailData { get { return (GroupDetailData)Data; } }



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
               var Detail = (Question)ModulesManager.CreateModule(null, new QuestionData(question), this, question);
                
                DXWindow _window = new DXWindow();
                _window.Content = Detail.View;
                _window.WindowState = WindowState.Maximized;
                _window.ShowDialog();
            }
             

        }


        public ICommand ShowQuestionCommand { get; private set; }


        /*void DoShowModalQuestionCommand(object p)
        {
            CompleteQuestionView data = p as CompleteQuestionView;
            if (data != null)
            {
                var module = ModulesManager.CreateModule(null, data, this, p);

                DXWindow _window = new DXWindow();
                _window.Content = module.View;
                _window.ShowDialog();
            }
        }
        public ICommand ShowModalQuestionCommand { get; private set; }*/
        #endregion

    }
}
