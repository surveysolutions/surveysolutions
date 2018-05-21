using System;
using System.Collections.Generic;
using WB.Core.BoundedContexts.Headquarters.Views.User;

namespace WB.Core.BoundedContexts.Headquarters.AssignmentImport.Verifier
{
    public class PreloadingAssignmentRow
    {
        public BaseAssignmentValue[] Answers { get; set; }
        public AssignmentRosterInstanceCode[] RosterInstanceCodes { get; set; }
        public int Row { get; set; }
        public AssignmentValue InterviewIdValue { get; set; }
        public AssignmentQuantity Quantity { get; set; }
        public AssignmentResponsible Responsible { get; set; }
        public string FileName { get; set; }
        public string QuestionnaireOrRosterName { get; set; }
    }

    public abstract class BaseAssignmentValue
    {
        public string VariableName { get; set; }
    }

    public abstract class AssignmentValue : BaseAssignmentValue
    {
        public string Column { get; set; }
        public string Value { get; set; }
    }

    public class AssignmentTextAnswer : AssignmentAnswer { }
    public class AssignmentGpsAnswer : AssignmentAnswers { }

    public class AssignmentIntegerAnswer : AssignmentAnswer
    {
        public int? Answer { get; set; }
    }

    public class AssignmentCategoricalSingleAnswer : AssignmentAnswer
    {
        public int? OptionCode { get; set; }
    }
    public class AssignmentDateTimeAnswer : AssignmentAnswer
    {
        public DateTime? Answer { get; set; }
    }

    public class AssignmentDoubleAnswer : AssignmentAnswer
    {
        public double? Answer { get; set; }
    }

    public class AssignmentMultiAnswer : AssignmentAnswers
    {
    }

    public interface IAssignmentAnswer
    {
        string VariableName { get; }
    }

    public class AssignmentAnswer : AssignmentValue, IAssignmentAnswer { }

    public class AssignmentRosterInstanceCode : AssignmentAnswer
    {
        public int? Code { get; set; }
    }

    public class AssignmentInterviewId : AssignmentValue { }

    public class AssignmentAnswers : BaseAssignmentValue, IAssignmentAnswer
    {
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
