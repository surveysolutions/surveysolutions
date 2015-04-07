using System.Collections.Generic;

using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels
{
    public class ScreenViewModel
    {
        public Identity Id { get; set; }
        public Identity ParentId { get; set; }

        public string Title { get; set; }
        public string RosterTitle { get; set; }
        public bool IsRoster { get; set; }

        public List<string> Breadcrumbs { get; set; }

        public int CountOfQuestionsWithErrors { get; set; }
        public int CountOfGroupsWithErrors { get; set; }
        public int CountOfAnsweredQuestions { get; set; }
        public int CountOfUnansweredQuestions { get; set; }

        public List<IInterviewItemViewModel> Items { get; set; }
    }

    public interface IInterviewItemViewModel
    {
        bool IsDisabled { get; set; }
    }

    public class GroupReferenceViewModel : IInterviewItemViewModel
    {
        public Identity Id { get; set; }
        public string Title { get; set; }
        public bool IsComplete { get; set; }
        public int CountOfAnsweredQuestions { get; set; }
        public int CountOfCompletedGroups { get; set; }
        public bool IsDisabled { get; set; }
    }

    public class RosterReferenceViewModel : GroupReferenceViewModel
    {
        public string RosterTitle { get; set; }
    }

    public class RostersReferenceViewModel : IInterviewItemViewModel
    {
        public bool IsDisabled { get; set; }
        public List<RosterReferenceViewModel> RosterReferences { get; set; }
    }

    public class StaticTextViewModel : IInterviewItemViewModel
    {
        public bool IsDisabled { get; set; }
        public Identity Id { get; set; }
        public string Title { get; set; }
    }

    public abstract class AbstractQuestionViewModel : IInterviewItemViewModel
    {
        public bool IsDisabled { get; set; }
        public Identity Id { get; set; }
        public string Title { get; set; }
    }

    public class SingleOptionQuestionViewModel : AbstractQuestionViewModel { }
    public class LinkedSingleOptionQuestionViewModel : AbstractQuestionViewModel { }
    public class FilteredSingleOptionQuestionViewModel : AbstractQuestionViewModel { }
    public class CascadingSingleOptionQuestionViewModel : AbstractQuestionViewModel { }

    public class MultiOptionQuestionViewModel : AbstractQuestionViewModel { }
    public class OrderedMultiOptionQuestionViewModel : AbstractQuestionViewModel { }
    public class LinkedMultiOptionQuestionViewModel : AbstractQuestionViewModel { }

    public class IntegerNumericQuestionViewModel : AbstractQuestionViewModel { }
    public class RealNumericQuestionViewModel : AbstractQuestionViewModel { }
    
    public class MaskedTextQuestionViewModel : AbstractQuestionViewModel { }

    public class TextListQuestionViewModel : AbstractQuestionViewModel { }
    public class QrBarcodeQuestionViewModel : AbstractQuestionViewModel { }
    public class MultimediaQuestionViewModel : AbstractQuestionViewModel { }
    public class DateTimeQuestionViewModel : AbstractQuestionViewModel { }
    public class GpsCoordinatesQuestionViewModel : AbstractQuestionViewModel { }
}
