using System;
using System.Collections.Generic;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Entities
{
    public interface IQuestionnaireItemPlaceholder
    {
        Guid Id { get; set; }
        string Title { get; set; }
    }

    public class GroupModelPlaceholder : IQuestionnaireItemPlaceholder
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
    }

    public class RosterModelPlaceholder : GroupModelPlaceholder
    {
    }

    public class QuestionModelPlaceholder : IQuestionnaireItemPlaceholder
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
    }

    public class StaticTextModel : IQuestionnaireItemPlaceholder
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
    }

    public class GroupModel
    {
        public GroupModel()
        {
            Placeholders = new List<IQuestionnaireItemPlaceholder>();
        }

        public Guid Id { get; set; }

        public string Title { get; set; }

        public List<IQuestionnaireItemPlaceholder> Placeholders { get; set; }
    }

    public class RosterModel : GroupModel
    {
    }

    public abstract class BaseQuestionModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string VariableName { get; set; }
        public bool IsPrefilled { get; set; }
        public string Instructions { get; set; }
        public bool IsMandatory { get; set; }
        public abstract QuestionModelType Type { get; }
    }

    public class OptionModel
    {
        public decimal Id { get; set; }
        public string Title { get; set; }
    }

    public class SingleOptionQuestionModel : BaseQuestionModel
    {
        public Guid? CascadeFromQuestionId { get; set; }

        public bool? IsFilteredCombobox { get; set; }

        public List<OptionModel> Options { get; set; }

        public override QuestionModelType Type { get { return QuestionModelType.SingleOption; } }
    }

    public class MultiOptionQuestionModel : BaseQuestionModel
    {
        public bool AreAnswersOrdered { get; set; }

        public int? MaxAllowedAnswers { get; set; }

        public List<OptionModel> Options { get; set; }

        public override QuestionModelType Type { get { return QuestionModelType.MultiOption; } }
    }

    public class LinkedSingleOptionQuestionModel : BaseQuestionModel
    {
        public Guid LinkedToQuestionId { get; set; }

        public override QuestionModelType Type { get { return QuestionModelType.LinkedSingleOption; } }
    }

    public class LinkedMultiOptionQuestionModel : BaseQuestionModel
    {
        public Guid LinkedToQuestionId { get; set; }

        public override QuestionModelType Type { get { return QuestionModelType.LinkedMultiOption; } }
    }

    public class IntegerNumericQuestionModel : BaseQuestionModel
    {
        public int? MaxValue { get; set; }

        public override QuestionModelType Type { get { return QuestionModelType.IntegerNumeric; } }
    }

    public class RealNumericQuestionModel : BaseQuestionModel
    {
        public int? CountOfDecimalPlaces { get; set; }

        public override QuestionModelType Type { get { return QuestionModelType.RealNumeric; } }
    }

    public class MaskedTextQuestionModel : BaseQuestionModel
    {
        public string Mask { get; set; }

        public override QuestionModelType Type { get { return QuestionModelType.MaskedText; } }
    }

    public class TextListQuestionModel : BaseQuestionModel
    {
        public int? MaxAnswerCount { get; set; }

        public override QuestionModelType Type { get { return QuestionModelType.TextList; } }
    }

    public class QrBarcodeQuestionModel : BaseQuestionModel
    {
        public override QuestionModelType Type { get { return QuestionModelType.QrBarcode; } }
    }

    public class MultimediaQuestionModel : BaseQuestionModel
    {
        public override QuestionModelType Type { get { return QuestionModelType.Multimedia; } }
    }

    public class DateTimeQuestionModel : BaseQuestionModel
    {
        public override QuestionModelType Type { get { return QuestionModelType.DateTime; } }
    }

    public class GpsCoordinatesQuestionModel : BaseQuestionModel
    {
        public override QuestionModelType Type { get { return QuestionModelType.GpsCoordinates; } }
    }

    public interface IQuestionnaireModel
    {

    }

    public class QuestionnaireModel : IQuestionnaireModel
    {
        public Guid Id { get; set; }

        public string Title { get; set; }

        public List<Guid> ListOfRostersId { get; set; }

        public List<Guid> PrefilledQuestionsIds { get; set; }

        public Dictionary<Guid, BaseQuestionModel> Questions { get; set; }

        public Dictionary<Guid, GroupModel> GroupsWithoutNestedChildren { get; set; }

        public Dictionary<Guid, List<GroupModelPlaceholder>> GroupParents { get; set; }
    }
}
