using System;
using System.Diagnostics;
using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;

namespace WB.Core.BoundedContexts.Headquarters.AssignmentImport.Verifier
{
    [DebuggerDisplay("{ToString()}")]
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

        public override string ToString() =>
            $"{InterviewIdValue?.Value}[{string.Join("_", RosterInstanceCodes.Select(x => x.Value))}]";
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

    public class AssignmentGpsAnswer : AssignmentAnswers
    {
        public GpsAnswer ToInterviewAnswer()
        {
            var doubleAnswers = this.Values.OfType<AssignmentDoubleAnswer>();
            var longitude = doubleAnswers.FirstOrDefault(x => x.VariableName == nameof(GeoPosition.Longitude).ToLower())?.Answer;
            var latitude = doubleAnswers.FirstOrDefault(x => x.VariableName == nameof(GeoPosition.Latitude).ToLower())?.Answer;
            var altitude = doubleAnswers.FirstOrDefault(x => x.VariableName == nameof(GeoPosition.Altitude).ToLower())?.Answer;
            var accuracy = doubleAnswers.FirstOrDefault(x => x.VariableName == nameof(GeoPosition.Accuracy).ToLower())?.Answer;
            var timestamp = this.Values.OfType<AssignmentDateTimeAnswer>()
                .FirstOrDefault(x => x.VariableName == nameof(GeoPosition.Timestamp).ToLower())?.Answer;

            return GpsAnswer.FromGeoPosition(new GeoPosition(latitude ?? 0, longitude ?? 0,
                accuracy ?? 0, altitude ?? 0, timestamp ?? DateTimeOffset.MinValue));
        }
    }

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
        public TextListAnswer ToInterviewTextListAnswer()
        {
            var textListAnswers = this.Values
                .OfType<AssignmentTextAnswer>()
                .Where(x => !string.IsNullOrWhiteSpace(x.Value))
                .Select(x => new Tuple<int, string>(Convert.ToInt32(x.VariableName), x.Value))
                .OrderBy(x => x.Item1)
                .ToArray();

            return TextListAnswer.FromTupleArray(textListAnswers);

        }

        public YesNoAnswer ToInterviewYesNoAnswer()
        {
            var ynOrderedAnswers = this.Values
                .OfType<AssignmentIntegerAnswer>()
                .Where(x => x.Answer.HasValue)
                .Select(x => new { code = Convert.ToInt32(x.VariableName), answer = x.Answer })
                .Where(x => x.answer > -1)
                .OrderBy(x => x.answer)
                .Select(x => new AnsweredYesNoOption(x.code, x.answer != 0))
                .ToArray();

            return YesNoAnswer.FromAnsweredYesNoOptions(ynOrderedAnswers);
        }

        public CategoricalFixedMultiOptionAnswer ToInterviewCategoricalMultiAnswer()
        {
            var orderedAnswers = this.Values
                .OfType<AssignmentIntegerAnswer>()
                .Where(x => x.Answer.HasValue)
                .Select(x => new { code = Convert.ToInt32(x.VariableName), answer = x.Answer })
                .Where(x => x.answer > 0)
                .OrderBy(x => x.answer)
                .Select(x => x.code)
                .Distinct()
                .ToArray();

            return CategoricalFixedMultiOptionAnswer.FromIntArray(orderedAnswers);
        }
    }

    public interface IAssignmentAnswer
    {
        string VariableName { get; }
    }

    public class AssignmentAnswer : AssignmentValue, IAssignmentAnswer { }

    [DebuggerDisplay("{Code}")]
    public class AssignmentRosterInstanceCode : AssignmentAnswer
    {
        public int? Code { get; set; }
    }

    [DebuggerDisplay("{Value}")]
    public class AssignmentInterviewId : AssignmentValue { }

    public class AssignmentAnswers : BaseAssignmentValue, IAssignmentAnswer
    {
        public AssignmentAnswer[] Values { get; set; }
    }

    [DebuggerDisplay("{Responsible.InterviewerId?.ToString() ?? Responsible.SupervsorId?.ToString ?? \"No responsible\"}")]
    public class AssignmentResponsible : AssignmentValue
    {
        public UserToVerify Responsible { get; set; }
    }

    [DebuggerDisplay("{Quantity}")]
    public class AssignmentQuantity : AssignmentValue
    {
        public int? Quantity { get; set; }
    }
}
