using System;
using System.Collections.Generic;
using System.Windows.Input;
using DevExpress.RealtorWorld.Xpf.Helpers;
using DevExpress.RealtorWorld.Xpf.ViewModel;
using RavenQuestionnaire.Core;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Vertical;
using RavenQuestionnaire.Core.Views.Question;

namespace QApp.ViewModel {
    public class CompletedQuestionnaireData : ModuleData
    {
        private string _questionnaireId ;
        private Guid? group = null;

        public CompletedQuestionnaireData()
        {
        }

        public CompletedQuestionnaireData(string questionnaireId, Guid? group)
        {
            QuestionnaireId = questionnaireId;
            GroupId = group;

        }

        public string QuestionnaireId
        {
            get { return _questionnaireId; }
            private set { SetValue<string>("QuestionnaireId", ref _questionnaireId, value); }
        }

        public Guid? GroupId
        {
            get { return group; }
            private set { SetValue<Guid?>("GroupId", ref group, value); }
        }

        public override void Load() {
            base.Load();


            if (string.IsNullOrEmpty(QuestionnaireId)) return;
            
            //replace with injections
            ViewRepository viewRepository = new ViewRepository(Initializer.Kernel);


            CompleteQuestionnaireItem =
                 viewRepository.Load<CompleteQuestionnaireViewInputModel, CompleteQuestionnaireViewV>(
                     new CompleteQuestionnaireViewInputModel(QuestionnaireId) { CurrentGroupPublicKey = GroupId });
            
        }

        private CompleteQuestionnaireViewV completeQuestionnaireItem;
        public CompleteQuestionnaireViewV CompleteQuestionnaireItem
        {
            get { return completeQuestionnaireItem; }
            private set { SetValue<CompleteQuestionnaireViewV>("CompleteQuestionnaireItem", ref completeQuestionnaireItem, value); }
        }

      

    }
    public class CompletedQuestionnaire : ModuleWithNavigator
    {
        private string completedQuestionnaireId /*= "171009"*/;

        public CompletedQuestionnaire(){}

        public override void InitData(object parameter) {
            base.InitData(parameter);
            string questionnaireId =  parameter as string;
            if (!String.IsNullOrEmpty(questionnaireId))
            {
                completedQuestionnaireId = questionnaireId;

                //bad approach!!!
                //due to init manager doesn't support parameter passing
                //TODO: rewrite!!!

                Data = new CompletedQuestionnaireData(completedQuestionnaireId, null);
                (Data as CompletedQuestionnaireData).Load();

            }

            CurrentGroup = CompletedQuestionnaireData.CompleteQuestionnaireItem.CurrentGroup;
        }
        public override void SaveData() {
            base.SaveData();
        }


        public override List<Module> GetSubmodules()
        {
            List<Module> submodules = base.GetSubmodules();
            submodules.Add(GroupDetail);
            return submodules;
        }

        private CompleteGroupViewV currentGroup;
        public CompleteGroupViewV CurrentGroup
        {
            get { return currentGroup; }
            set { SetValue<CompleteGroupViewV>("CurrentGroup", ref currentGroup, value, RaiseCurrentGroupChanged); }
        }

        private Module groupDetail;
        public Module GroupDetail
        {
            get { return groupDetail; }
            private set { SetValue<Module>("GroupDetail", ref groupDetail, value); }
        }


        public CompletedQuestionnaireData CompletedQuestionnaireData { get { return (CompletedQuestionnaireData)Data; } }

        Question detail;

        public Question Detail
        {
            get { return detail; }
            private set { SetValue<Question>("Detail", ref detail, value); }
        }


        #region Commands
        protected override void InitializeCommands() {
            base.InitializeCommands();
            SetCurrentGroupCommand = new SimpleActionCommand(DoSetCurrentGroup);
            ShowQuestionCommand = new SimpleActionCommand(DoShowQuestion);
        }


        void RaiseCurrentGroupChanged(CompleteGroupViewV oldValue, CompleteGroupViewV newValue)
        {

            if (newValue.Propagated == Propagate.None)
                GroupDetail = (CommonGroupDetail)ModulesManager.CreateModule(GroupDetail, new CommonGroupDetailData(newValue), this, newValue);
            else
            {
                GroupDetail = (PropagatedGroupDetail)ModulesManager.CreateModule(GroupDetail, new PropagatedGroupDetailData(newValue), this, newValue);
            }
        }

        void DoSetCurrentGroup(object p)
        {
            //bad approach!!!
            //reload current data
            //TODO: !!!
            var currentGroup = p as CompleteGroupHeaders;
            if (currentGroup != null)
            {
                Data = new CompletedQuestionnaireData(completedQuestionnaireId, currentGroup.PublicKey);
                (Data as CompletedQuestionnaireData).Load();
            }
            
            CurrentGroup = CompletedQuestionnaireData.CompleteQuestionnaireItem.CurrentGroup;
        }

        void DoShowQuestion(object p)
        {

            var question = p as CompleteQuestionView;
            if (question != null)
                Detail = (Question)ModulesManager.CreateModule(Detail, new QuestionData(question), this);

            /*Window win = new Window();
            //win.Owner = this;
            win.ShowDialog();*/


        }

        public ICommand SetCurrentGroupCommand { get; private set; }


        public ICommand ShowQuestionCommand { get; private set; }

        #endregion
    }
}
