using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.Questionnaire.Documents;

namespace WB.Enumerator.Native.WebInterview.Models
{
    public static class WebInterviewMapper
    {
        // InterviewTreeQuestion → Validity
        public static Validity ToValidity(this InterviewTreeQuestion question) => new Validity
        {
            IsValid = question.IsValid
        };

        // InterviewTreeStaticText → Validity
        public static Validity ToValidity(this InterviewTreeStaticText staticText) => new Validity
        {
            IsValid = staticText.IsValid
        };

        // Apply base InterviewEntity properties from InterviewTreeQuestion
        private static void ApplyBaseEntity(GenericQuestion target, InterviewTreeQuestion question)
        {
            target.Id = question.Identity.ToString();
            target.Title = question.Title.BrowserReadyText;
            target.Validity = question.ToValidity();
            target.IsAnswered = question.IsAnswered();
            target.IsDisabled = question.IsDisabled();
            target.Instructions = question.Instructions.BrowserReadyText;
        }

        public static StubEntity ToStubEntity(this InterviewTreeQuestion question)
        {
            var result = new StubEntity();
            ApplyBaseEntity(result, question);
            return result;
        }

        public static InterviewTextQuestion ToTextQuestion(this InterviewTreeQuestion question)
        {
            var result = new InterviewTextQuestion();
            ApplyBaseEntity(result, question);
            result.Answer = question.GetAsInterviewTreeTextQuestion().GetAnswer()?.Value;
            return result;
        }

        public static InterviewBarcodeQuestion ToBarcodeQuestion(this InterviewTreeQuestion question)
        {
            var result = new InterviewBarcodeQuestion();
            ApplyBaseEntity(result, question);
            result.Answer = question.GetAsInterviewTreeQRBarcodeQuestion().GetAnswer()?.DecodedText;
            return result;
        }

        public static InterviewAreaQuestion ToAreaQuestion(this InterviewTreeQuestion question)
        {
            var result = new InterviewAreaQuestion();
            ApplyBaseEntity(result, question);
            if (question.IsAnswered())
            {
                result.Answer = ToGeometryAnswer(question.GetAsInterviewTreeAreaQuestion().GetAnswer().Value);
            }
            return result;
        }

        public static InterviewAudioQuestion ToAudioQuestion(this InterviewTreeQuestion question)
        {
            var result = new InterviewAudioQuestion();
            ApplyBaseEntity(result, question);
            var answer = question.GetAsInterviewTreeAudioQuestion().GetAnswer();
            if (answer != null)
            {
                result.Filename = answer.FileName;
                result.Answer = (long)answer.Length.TotalMilliseconds;
            }
            return result;
        }

        public static InterviewSingleOptionQuestion ToSingleOptionQuestion(this InterviewTreeQuestion question)
        {
            var result = new InterviewSingleOptionQuestion();
            ApplyBaseEntity(result, question);
            if (question.IsAnswered())
            {
                result.Answer = question.IsLinkedToListQuestion
                    ? (int?)question.GetAsInterviewTreeSingleOptionLinkedToListQuestion().GetAnswer().SelectedValue
                    : (int?)question.GetAsInterviewTreeSingleOptionQuestion().GetAnswer().SelectedValue;
            }
            return result;
        }

        public static InterviewFilteredQuestion ToFilteredQuestion(this InterviewTreeQuestion question)
        {
            var result = new InterviewFilteredQuestion();
            ApplyBaseEntity(result, question);
            if (question.IsAnswered())
            {
                result.Answer = GetSingleFixedOptionAnswerAsDropdownItem(question);
            }
            return result;
        }

        public static InterviewLinkedSingleQuestion ToLinkedSingleQuestion(this InterviewTreeQuestion question)
        {
            var result = new InterviewLinkedSingleQuestion();
            ApplyBaseEntity(result, question);
            if (question.IsAnswered())
            {
                result.Answer = question.GetAsInterviewTreeSingleLinkedToRosterQuestion().GetAnswer().SelectedValue;
            }
            return result;
        }

        public static InterviewLinkedMultiQuestion ToLinkedMultiQuestion(this InterviewTreeQuestion question)
        {
            var result = new InterviewLinkedMultiQuestion();
            ApplyBaseEntity(result, question);
            result.Answer = question.GetAsInterviewTreeMultiLinkedToRosterQuestion().GetAnswer()?.CheckedValues.ToArray()
                ?? Array.Empty<RosterVector>();
            return result;
        }

        public static InterviewMutliOptionQuestion ToMultiOptionQuestion(this InterviewTreeQuestion question)
        {
            var result = new InterviewMutliOptionQuestion();
            ApplyBaseEntity(result, question);
            result.Answer = question.IsLinkedToListQuestion
                ? question.GetAsInterviewTreeMultiOptionLinkedToListQuestion().GetAnswer()?.CheckedValues.ToArray()
                    ?? Array.Empty<int>()
                : question.GetAsInterviewTreeMultiOptionQuestion().GetAnswer()?.CheckedValues.ToArray()
                    ?? Array.Empty<int>();
            return result;
        }

        public static InterviewYesNoQuestion ToYesNoQuestion(this InterviewTreeQuestion question)
        {
            var result = new InterviewYesNoQuestion();
            ApplyBaseEntity(result, question);
            result.Answer = question.GetAsInterviewTreeYesNoQuestion().GetAnswer()?.CheckedOptions
                ?.Select(o => new InterviewYesNoAnswer { Value = o.Value, Yes = o.Yes }).ToArray()
                ?? Array.Empty<InterviewYesNoAnswer>();
            return result;
        }

        public static InterviewIntegerQuestion ToIntegerQuestion(this InterviewTreeQuestion question)
        {
            var result = new InterviewIntegerQuestion();
            ApplyBaseEntity(result, question);
            if (question.IsAnswered())
            {
                result.Answer = question.GetAsInterviewTreeIntegerQuestion().GetAnswer().Value;
            }
            return result;
        }

        public static InterviewDoubleQuestion ToDoubleQuestion(this InterviewTreeQuestion question)
        {
            var result = new InterviewDoubleQuestion();
            ApplyBaseEntity(result, question);
            if (question.IsAnswered())
            {
                result.Answer = question.GetAsInterviewTreeDoubleQuestion().GetAnswer().Value;
            }
            return result;
        }

        public static InterviewDateQuestion ToDateQuestion(this InterviewTreeQuestion question)
        {
            var result = new InterviewDateQuestion();
            ApplyBaseEntity(result, question);
            result.IsTimestamp = question.GetAsInterviewTreeDateTimeQuestion().IsTimestamp;
            if (question.IsAnswered())
            {
                result.Answer = question.GetAsInterviewTreeDateTimeQuestion().GetAnswer().Value;
            }
            return result;
        }

        public static InterviewTextListQuestion ToTextListQuestion(this InterviewTreeQuestion question)
        {
            var result = new InterviewTextListQuestion();
            ApplyBaseEntity(result, question);
            result.Rows = question.GetAsInterviewTreeTextListQuestion().GetAnswer()?.Rows
                ?.Select(r => new TextListAnswerRowDto { Value = r.Value, Text = r.Text }).ToList()
                ?? new List<TextListAnswerRowDto>();
            return result;
        }

        public static InterviewGpsQuestion ToGpsQuestion(this InterviewTreeQuestion question)
        {
            var result = new InterviewGpsQuestion();
            ApplyBaseEntity(result, question);
            if (question.IsAnswered())
            {
                result.Answer = ToGpsAnswer(question.GetAsInterviewTreeGpsQuestion().GetAnswer().Value);
            }
            return result;
        }

        public static InterviewMultimediaQuestion ToMultimediaQuestion(this InterviewTreeQuestion question)
        {
            var result = new InterviewMultimediaQuestion();
            ApplyBaseEntity(result, question);
            var answer = question.GetAsInterviewTreeMultimediaQuestion().GetAnswer();
            result.AnswerTimeUtc = answer.AnswerTimeUtc;
            result.Answer = $@"?interviewId={question.Tree.InterviewId}&questionId={question.Identity}&filename={answer.FileName}";
            return result;
        }

        // InterviewTreeStaticText → InterviewStaticText
        public static InterviewStaticText ToStaticText(this InterviewTreeStaticText staticText) => new InterviewStaticText
        {
            Id = staticText.Identity.ToString(),
            Title = staticText.Title.BrowserReadyText,
            IsDisabled = staticText.IsDisabled(),
            Validity = staticText.ToValidity()
        };

        // InterviewTreeVariable → InterviewVariable
        public static InterviewVariable ToVariable(this InterviewTreeVariable variable) => new InterviewVariable
        {
            Id = variable.Identity.ToString(),
            Value = variable.GetValueAsStringBrowserReady()
        };

        // InterviewTreeGroup → InterviewGroupOrRosterInstance
        public static InterviewGroupOrRosterInstance ToGroupOrRosterInstance(this InterviewTreeGroup group) =>
            new InterviewGroupOrRosterInstance
            {
                Id = group.Identity.ToString(),
                Title = group.Title.BrowserReadyText,
                IsDisabled = group.IsDisabled(),
                IsRoster = group is InterviewTreeRoster
            };

        // InterviewTreeRoster → InterviewGroupOrRosterInstance
        public static InterviewGroupOrRosterInstance ToGroupOrRosterInstance(this InterviewTreeRoster roster) =>
            new InterviewGroupOrRosterInstance
            {
                Id = roster.Identity.ToString(),
                Title = roster.Title.BrowserReadyText,
                IsDisabled = roster.IsDisabled(),
                RosterTitle = roster.RosterTitle,
                IsRoster = true
            };

        // InterviewTreeGroup → SidebarPanel
        public static SidebarPanel ToSidebarPanel(this InterviewTreeGroup group) => new SidebarPanel
        {
            Id = group.Identity.ToString(),
            ParentId = group.Parent == null ? null : group.Parent.Identity.ToString(),
            HasChildren = group.Children.OfType<InterviewTreeGroup>().Any(c => !c.IsDisabled()),
            Title = group.Title.BrowserReadyText,
            RosterTitle = group is InterviewTreeRoster roster ? HttpUtility.HtmlEncode(roster.RosterTitle) : null,
            IsRoster = group is InterviewTreeRoster
        };

        // GeoPosition → GpsAnswer
        public static GpsAnswer ToGpsAnswer(GeoPosition position) => new GpsAnswer
        {
            Latitude = position.Latitude,
            Longitude = position.Longitude,
            Accuracy = position.Accuracy,
            Altitude = position.Altitude,
            Timestamp = position.Timestamp.ToUnixTimeMilliseconds()
        };

        // Area → InterviewGeometryAnswer
        private static InterviewGeometryAnswer ToGeometryAnswer(Area area) => new InterviewGeometryAnswer
        {
            SelectedPoints = area.Coordinates.Split(';').Select(x =>
                new GeoLocation(double.Parse(x.Split(',')[1]), double.Parse(x.Split(',')[0]), 0, 0)).ToArray(),
            Length = area.Length,
            Area = area.AreaSize,
            RequestedAccuracy = area.RequestedAccuracy,
            RequestedFrequency = area.RequestedFrequency
        };

        private static DropdownItem GetSingleFixedOptionAnswerAsDropdownItem(InterviewTreeQuestion question)
        {
            var singleOptionAnswer = question.GetAsInterviewTreeSingleOptionQuestion().GetAnswer();
            var selectedValue = singleOptionAnswer.SelectedValue;
            int? parentAnswer = null;

            if (question.IsCascading && question.InterviewQuestion is InterviewTreeCascadingQuestion cascading)
                parentAnswer = cascading.GetCascadingParentQuestion()?.GetAnswer()?.SelectedValue;

            var attachmentName = question.Tree.GetAttachmentNameForQuestionOptionByOptionValue(question.Identity.Id, selectedValue, parentAnswer);
            return new DropdownItem(selectedValue, question.GetAnswerAsString(), attachmentName);
        }
    }
}
