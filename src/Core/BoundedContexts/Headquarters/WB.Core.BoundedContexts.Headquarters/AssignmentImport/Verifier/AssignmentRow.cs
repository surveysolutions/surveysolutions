using System;
using WB.Core.BoundedContexts.Headquarters.Views.User;

namespace WB.Core.BoundedContexts.Headquarters.AssignmentImport.Verifier
{
    public class AssignmentRow
    {
        public AssignmentValue[] Answers { get; set; }
    }

    public abstract class AssignmentValue
    {
        public string FileName { get; set; }
        public int Row { get; set; }
        public string Column { get; set; }
        public string Value { get; set; }
        public string InterviewId { get; set; }
    }

    public class AssignmentTextAnswer : AssignmentAnswer
    {
        public string Mask { get; set; }
    }
    public class AssignmentGpsAnswer : AssignmentAnswers { }

    public class AssignmentIntegerAnswer : AssignmentAnswer
    {
        public bool IsRosterSizeForLongRoster { get; set; }
        public bool IsRosterSize { get; set; }
        public int? Answer { get; set; }
    }
    public class AssignmentDateTimeAnswer : AssignmentAnswer
    {
        public DateTime? Answer { get; set; }
    }

    public class AssignmentDoubleAnswer : AssignmentAnswer
    {
        public double? Answer { get; set; }
    }

    public class AssignmentCategoricalSingleAnswer : AssignmentAnswer { }

    public class AssignmentCategoricalMultiAnswer : AssignmentAnswers
    {
        public int? MaxAnswersCount { get; set; }
    }

    public class AssignmentAnswer : AssignmentValue
    {
        public string VariableName { get; set; }
    }

    public class AssignmentRosterInstanceCode : AssignmentAnswer
    {
        public int? Code { get; set; }
    }

    public class AssignmentAnswers: AssignmentValue
    {
        public string VariableName { get; set; }
        public AssignmentAnswer[] Values { get; set; }
    }

    public class AssignmentResponsible : AssignmentValue
    {
        public UserToVerify Responsible { get; set; }
    }

    public class AssignmentQuantity : AssignmentValue
    {
        public int? Quantity { get; set; }
    }
}