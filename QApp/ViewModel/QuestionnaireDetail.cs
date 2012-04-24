using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using DevExpress.RealtorWorld.Xpf.Helpers;
using DevExpress.RealtorWorld.Xpf.ViewModel;
using RavenQuestionnaire.Core;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Vertical;
using RavenQuestionnaire.Core.Views.Question;

namespace QApp.ViewModel {
    public class QuestionnaireDetailData : ModuleData
    {
        private string _questionnaireId;
        private Guid? group = null;

        public QuestionnaireDetailData()
        {
        }

        public QuestionnaireDetailData(string questionnaireId, Guid? group)
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
    #region research

    public class ModelBase : DependencyObject
    {
        public static readonly DependencyProperty NameProperty;

        static ModelBase()
        {
            NameProperty = DependencyProperty.Register("Name", typeof(string), typeof(ModelBase), new PropertyMetadata(""));
        }
        public string Name
        {
            get { return (string)GetValue(NameProperty); }
            set { SetValue(NameProperty, value); }
        }
    }

    public class BarModel : ModelBase
    {
        public static readonly DependencyProperty CommandsProperty;

        static BarModel()
        {
            CommandsProperty = DependencyProperty.Register("Commands", typeof(ObservableCollection<MyCommand>), typeof(BarModel), new PropertyMetadata(null));
        }
        public BarModel()
        {
            Commands = new ObservableCollection<MyCommand>();
        }
        public ObservableCollection<MyCommand> Commands
        {
            get { return ((ObservableCollection<MyCommand>)GetValue(CommandsProperty)); }
            set { SetValue(CommandsProperty, value); }
        }
    }

    public class MyCommand : DependencyObject, ICommand
    {
        Action action;
        public static readonly DependencyProperty CaptionProperty;
        public static readonly DependencyProperty LargeGlyphProperty;
        public static readonly DependencyProperty SmallGlyphProperty;

        static MyCommand()
        {
            CaptionProperty = DependencyProperty.Register("Caption", typeof(string), typeof(MyCommand), new PropertyMetadata(""));
            
        }
        public MyCommand()
        {

        }
        private void ShowMSGBX()
        {
            MessageBox.Show(String.Format("Command \"{0}\" executed", this.Caption));
        }
        public MyCommand(Action action)
        {
            this.action = action;
        }

        public string Caption
        {
            get { return (string)GetValue(CaptionProperty); }
            set { SetValue(CaptionProperty, value); }
        }
       

        #region ICommand
        bool b = false;
        public bool CanExecute(object parameter)
        {
            if (b == true) CanExecuteChanged.Invoke(this, new EventArgs());
            return true;
        }

        public event EventHandler CanExecuteChanged;

        public virtual void Execute(object parameter)
        {
            if (action != null)
                action();
            else
                ShowMSGBX();
        }
        #endregion
    }

    public class MyGroupCommand : MyCommand
    {
        public static readonly DependencyProperty CommandsProperty;

        public ObservableCollection<MyCommand> Commands
        {
            get { return (ObservableCollection<MyCommand>)GetValue(CommandsProperty); }
            set { SetValue(CommandsProperty, value); }
        }
        static MyGroupCommand()
        {
            CommandsProperty = DependencyProperty.Register("Commands", typeof(ObservableCollection<MyCommand>), typeof(MyGroupCommand), new PropertyMetadata(null));
        }
        public MyGroupCommand()
            : base(emptyFunc)
        {
            Commands = new ObservableCollection<MyCommand>();
        }
        public static void emptyFunc()
        {
        }
    }

    #endregion research

    public class QuestionnaireDetail : ModuleWithNavigator
    {
        private string _completedQuestionnaireId ;

        public QuestionnaireDetail()
        {
           Navigation = new ObservableCollection<NavigationItem>();
        }

        public override void InitData(object parameter) {
            base.InitData(parameter);
            string questionnaireId =  parameter as string;
            if (!String.IsNullOrEmpty(questionnaireId))
            {
                _completedQuestionnaireId = questionnaireId;

                //bad approach!!!
                //due to init manager doesn't support parameter passing
                //TODO: rewrite!!!

                Data = new QuestionnaireDetailData(_completedQuestionnaireId, null);
                (Data as QuestionnaireDetailData).Load();

            }

            CurrentGroup = CompletedQuestionnaireData.CompleteQuestionnaireItem.CurrentGroup;
        }

        ObservableCollection<NavigationItem> navigation;
        public ObservableCollection<NavigationItem> Navigation
        {
            get { return navigation; }
            set { SetValue<ObservableCollection<NavigationItem>>("Navigation", ref navigation, value); }
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


        public QuestionnaireDetailData CompletedQuestionnaireData { get { return (QuestionnaireDetailData)Data; } }

        private Question _detail;
        public Question Detail
        {
            get { return _detail; }
            private set { SetValue<Question>("Detail", ref _detail, value); }
        }


        #region Commands
        protected override void InitializeCommands() {
            base.InitializeCommands();

            SetCurrentGroupCommand = new SimpleActionCommand(DoSetCurrentGroup);
            SetCurrentSubGroupCommand = new SimpleActionCommand(DoSetCurrentSubGroup);
            ShowQuestionCommand = new SimpleActionCommand(DoShowQuestion);
            SelectMenuItemCommand = new SimpleActionCommand(DoSelectMenuItem);

        }

        private void DoSelectMenuItem(object obj)
        {
            var item = obj as NavigationItem;
            if (item != null)
            {
                item.Command.Execute(null);
            }
        }

        private void BuildMenu()
        {
            var root = new NavigationItem();
            root.Text = "root";
            root.Command = new SimpleActionCommand(DoSetCurrentGroup);

            Navigation.Clear();
            Navigation.Add(root);
            
        }




        void RaiseCurrentGroupChanged(CompleteGroupViewV oldValue, CompleteGroupViewV newValue)
        {
            if (newValue.Propagated == Propagate.None)
                GroupDetail = (CommonGroupDetail)ModulesManager.CreateModule(null, new CommonGroupDetailData(newValue), this, newValue);
            else
            {
                GroupDetail = (PropagatedGroupDetail)ModulesManager.CreateModule(null, new PropagatedGroupDetailData(newValue), this, newValue);
            }

            BuildMenu();
        }

        void DoSetCurrentGroup(object p)
        {
            //bad approach!!!
            //reload current data
            //TODO: load whole questionnaire!!!
            var group = p as CompleteGroupHeaders;
            if (group != null)
            {
                Data = new QuestionnaireDetailData(_completedQuestionnaireId, group.PublicKey);

                Stopwatch stopWatch = new Stopwatch();

                stopWatch.Start();

                DateTime start = DateTime.Now;
                (Data as QuestionnaireDetailData).Load();
                stopWatch.Start();
                var diff = start - DateTime.Now;

                //stopWatch.Elapsed;


            }
            
            
            DoSetCurrentSubGroup(CompletedQuestionnaireData.CompleteQuestionnaireItem.CurrentGroup);
        }

        void DoSetCurrentSubGroup(object p)
        {
            var group = p as CompleteGroupViewV;
            if (group != null)
            {
                CurrentGroup = group;
            }
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


        public ICommand SetCurrentSubGroupCommand { get; private set; }

        public ICommand ShowQuestionCommand { get; private set; }

        public ICommand SelectMenuItemCommand { get; private set; }

        #endregion
    }

    public class NavigationItem
    {
        public NavigationItem()
        {
        }

        public string Text { set; get; }

        public ICommand Command { set; get; }

    }
}
