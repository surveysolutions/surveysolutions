using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Cirrious.MvvmCross.Platform;
using Cirrious.MvvmCross.ViewModels;

namespace WB.Core.BoundedContexts.Capi.ViewModel
{
    public class InterviewViewModel : MvxViewModel
    {
        private IList<InterviewEntity> prefilledQuestions;
        public IList<InterviewEntity> PrefilledQuestions
        {
            get { return prefilledQuestions; }
            set
            {
                prefilledQuestions = value;
                RaisePropertyChanged(() => PrefilledQuestions);
            }
        }

        private ObservableCollection<InterviewGroup> chapters;
        public ObservableCollection<InterviewGroup> Chapters
        {
            get { return chapters; }
            set
            {
                chapters = value;
                RaisePropertyChanged(() => Chapters);
            }
        }

        private ObservableCollection<InterviewEntity> groupsAndQuestions;
        public ObservableCollection<InterviewEntity> GroupsAndQuestions
        {
            get { return groupsAndQuestions; }
            set
            {
                groupsAndQuestions = value;
                RaisePropertyChanged(() => GroupsAndQuestions);
            }
        }

        private static string DefaultValueForStaticText = "Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.";

        private void ChangeStaticTextQuestions(string text)
        {
            foreach (var interviewEntity in GroupsAndQuestions)
            {
                var interviewStaticText = interviewEntity as InterviewStaticText;
                if (interviewStaticText != null)
                    interviewStaticText.Text = DefaultValueForStaticText + text;
            }
        }

        public async void Init(Guid interviewId)
        {
            await Task.Run(() => LoadInterview());
        }

        private void LoadInterview()
        {
            var interviewEntities = new List<InterviewEntity>();

            var rnd = new Random();

            for (int i = 0; i < 50; i++)
            {
                var collection = new InterviewEntity[]
                {
                    new InterviewStaticText(){Text = DefaultValueForStaticText}, 
                    new InterviewGroup() {Title = string.Format("Group {0}", i)},
                    //new InterviewRoster() {Title = string.Format("Roster {0}", i)},
                    new InterviewDateQuestion(){ Answer = new DateTime(rnd.Next()) },
                    new InterviewDecimalQuestion(){ Answer = 12.2323m},
                    new InterviewImageQuestion() { },
                    new InterviewIntegerQuestion(){ Answer = rnd.Next() },
                    //new InterviewListQuestion() {MaxAnswers = 5, Answers = new ObservableCollection<string>(new []{ "list 1", "list 2", "list 3", "list 4", "list 5" })},
                    //new InterviewQrBarcodeQuestion(){ Answer = "some QR Barcode answer" }, 
                    new InterviewTextQuestion(){ Answer = string.Format("answer on text") },
//                    new InterviewLinkedMultiChoiceQuestion<string>(){ Options = new ObservableCollection<InterviewDynamicOption<string>>(GenerateDynamicOptions<string>(rnd, false)) },
//                    new InterviewLinkedMultiChoiceQuestion<int>(){ Options = new ObservableCollection<InterviewDynamicOption<int>>(GenerateDynamicOptions<int>(rnd, false))},
//                    new InterviewLinkedMultiChoiceQuestion<decimal>(){Options = new ObservableCollection<InterviewDynamicOption<decimal>>(GenerateDynamicOptions<decimal>(rnd, false))},
//                    new InterviewLinkedMultiChoiceQuestion<InterviewGeoLocation>(){Options = new ObservableCollection<InterviewDynamicOption<InterviewGeoLocation>>(GenerateDynamicOptions<InterviewGeoLocation>(rnd, false))},
//                    new InterviewLinkedSingleChoiceQuestion<string>(){Options = new ObservableCollection<InterviewDynamicOption<string>>(GenerateDynamicOptions<string>(rnd, true))},
//                    new InterviewLinkedSingleChoiceQuestion<int>(){Options = new ObservableCollection<InterviewDynamicOption<int>>(GenerateDynamicOptions<int>(rnd, true))},
//                    new InterviewLinkedSingleChoiceQuestion<decimal>(){Options = new ObservableCollection<InterviewDynamicOption<decimal>>(GenerateDynamicOptions<decimal>(rnd, true))},
//                    new InterviewLinkedSingleChoiceQuestion<InterviewGeoLocation>(){Options = new ObservableCollection<InterviewDynamicOption<InterviewGeoLocation>>(GenerateDynamicOptions<InterviewGeoLocation>(rnd, true))},
//                    new InterviewAutocompleteSingleChoiceQuestion(){ Options = new ObservableCollection<InterviewStaticOption>(GenerateStaticOptions(rnd, true, 200)) },
//                    new InterviewCascadingSingleChoiceQuestion(){ Options = new ObservableCollection<InterviewStaticOption>(GenerateStaticOptions(rnd, true, 200)) },
                    new InterviewMultiChoiceQuestion(){ Options = new ObservableCollection<InterviewStaticOption>(GenerateStaticOptions(rnd, false, 5)) },
                    new InterviewSingleChoiceQuestion(){ Options = new ObservableCollection<InterviewStaticOption>(GenerateStaticOptions(rnd, true, 5)) }, 
//                    new InterviewGeolocationQuestion(){ Answer = new InterviewGeoLocation()
//                    {
//                        Accuracy = rnd.NextDouble(),
//                        Altitude = rnd.NextDouble(),
//                        Latitude = rnd.NextDouble(),
//                        Longitude = rnd.NextDouble(),
//                        Timestamp = new DateTime(rnd.Next())
//                    }}
                };

                collection.OfType<InterviewQuestion>().ForEach(question =>
                {
                    question.Id = Guid.NewGuid().ToString();
                    question.ParentId = Guid.NewGuid().ToString();
                    question.RosterVector = new decimal[] { };
                    question.Title = string.Format("{1} {0}", rnd.Next(), question.GetType());
                    question.Enabled = RandBool(rnd);
                    question.IsMandatory = RandBool(rnd);
                    question.IsValid = RandBool(rnd);
                    question.Instructions = "some instructions about";
                    question.PropertyChanged += (sender, args) =>
                    {
                        try
                        {
                            if (args.PropertyName == "Answer")
                            {
                                var runtimeProperty = sender.GetType().GetRuntimeProperty("Answer");
                                var text = runtimeProperty.GetValue(sender).ToString();
                                //var text = sender.GetType().GetPropertyValueAsString(runtimeProperty);
                                ChangeStaticTextQuestions(text);
                            }
                        }
                        catch { }
                    };
                });

                interviewEntities.AddRange(collection);
            }

            this.GroupsAndQuestions = new ObservableCollection<InterviewEntity>(interviewEntities);
        }


        private IEnumerable<InterviewDynamicOption<T>> GenerateDynamicOptions<T>(Random rnd, bool oneAnswerSelectedOnly)
        {
            var options = new List<InterviewDynamicOption<T>>();
            for (int i = 0; i < 20; i++)
            {
                options.Add(new InterviewDynamicOption<T>()
                {
                    Label = string.Format("option label {0}", rnd.Next()),
                    IsSelected = (!oneAnswerSelectedOnly || !options.Any(opt => opt.IsSelected)) && RandBool(rnd)
                });
            }

            return options;
        }

        private IEnumerable<InterviewStaticOption> GenerateStaticOptions(Random rnd, bool oneAnswerSelectedOnly, int numberOfItems = 20)
        {
            var options = new List<InterviewStaticOption>();
            for (int i = 0; i < numberOfItems; i++)
            {
                options.Add(new InterviewStaticOption()
                { 
                    Label = oneAnswerSelectedOnly
                     ? string.Format("radio option label {0}-{1}", i, rnd.Next())
                     : string.Format("check option label {0}-{1}", i, rnd.Next()),
                    Value = rnd.Next().ToString(),
                    IsSelected = (!oneAnswerSelectedOnly || !options.Any(opt=>opt.IsSelected)) && RandBool(rnd)
                });
            }
            return options;
        }

        private static bool RandBool(Random rnd)
        {
            return rnd.Next(0, 1) == 0;
        }
    }
}
