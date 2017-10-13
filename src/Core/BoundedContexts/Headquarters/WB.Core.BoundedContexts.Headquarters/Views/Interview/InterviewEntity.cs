using System;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Views.Interview;
using WB.Core.SharedKernels.Questionnaire.Documents;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.BoundedContexts.Headquarters.Views.Interview
{
    public class InterviewEntity : IView
    {
        public virtual Guid InterviewId { get; set; }
        public virtual Identity Identity { get; set; }
        public virtual EntityType EntityType { get; set; }

        public virtual bool HasFlag { get; set; }
        public virtual AnswerType? AnswerType { get; set; }
        public virtual bool IsEnabled { get; set; }
        public virtual int[] InvalidValidations { get; set; }
        public virtual bool IsReadonly { get; set; }
        public virtual int? AsInt { get; set; }
        public virtual double? AsDouble { get; set; }
        public virtual long? AsLong { get; set; }
        public virtual string AsString { get; set; }
        public virtual DateTime? AsDateTime { get; set; }
        public virtual bool? AsBool { get; set; }
        public virtual int[] AsIntArray { get; set; }
        public virtual InterviewTextListAnswer[] AsList { get; set; }
        public virtual AnsweredYesNoOption[] AsYesNo { get; set; }
        public virtual int[][] AsIntMatrix { get; set; }
        public virtual GeoPosition AsGps { get; set; }
        public virtual AudioAnswer AsAudio { get; set; }
        public virtual Area AsArea { get; set; }

        public static AnswerType? GetAnswerType(object answer)
        {
            switch (answer)
            {
                case decimal asdecimal:
                case double asdouble:
                    return Interview.AnswerType.Double;
                case long aslong:
                    return Interview.AnswerType.Long;
                case int asint:
                    return Interview.AnswerType.Int;
                case string asstring:
                    return Interview.AnswerType.String;
                case decimal[] asdecimalarray:
                case double[] asdoublearray:
                case int[] asintarray:
                    return Interview.AnswerType.IntArray;
                case double[][] asdoublematrix:
                case decimal[][] asdecimalmatrix:
                case int[][] asintmatrix:
                    return Interview.AnswerType.IntMatrix;
                case DateTime asdatetime:
                    return Interview.AnswerType.Datetime;
                case GeoPosition asgps:
                    return Interview.AnswerType.Gps;
                case InterviewTextListAnswers astextlist:
                case Tuple<decimal, string>[] astuple:
                    return Interview.AnswerType.TextList;
                case AnsweredYesNoOption[] asyesnolist:
                    return Interview.AnswerType.YesNoList;
                case AudioAnswer asaudio:
                    return Interview.AnswerType.Audio;
                case Area asarea:
                    return Interview.AnswerType.Area;
                case bool asbool:
                    return Interview.AnswerType.Bool;
                case null:
                    return null;
                default:
                    throw new NotSupportedException("Unknown type of answer");
            }
        }

        public override bool Equals(object obj)
        {
            var target = obj as InterviewEntity;
            if (target == null) return false;

            return this.InterviewId == target.InterviewId && this.Identity == target.Identity;
        }

        public override int GetHashCode() => this.InterviewId.GetHashCode() ^ this.Identity.GetHashCode();
    }
}