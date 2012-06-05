using System;

namespace RavenQuestionnaire.Core.Entities.SubEntities.Complete
{
    public interface ICompleteQuestion : IQuestion
    {
        Guid? PropogationPublicKey { get; set; }
        bool Enabled { get; set; }
        bool Valid { get; set; }
        DateTime? AnswerDate { get; set; }
        object Answer { get; set; }
        void SetAnswer(object answer);
        string GetAnswerString();
        object GetAnswerObject();
    }

    public interface INumericQuestion
    {
        string AddNumericAttr { get; set; }
        int IntAttr { get; set; }
    }

    public interface ISingleQuestion
    {
        string AddSingleAttr { get; set; }
    }

    public interface IDateTimeQuestion
    {
        string AddDateTimeAttr { get; set; }
        DateTime DateTimeAttr { get; set; }
    }

    public interface IMultyOptionsQuestion
    {
        string AddMultyAttr { get; set; }
    }

    public interface IGpsCoordinatesQuestion
    {
        string AddGpsCoordinateAttr { get; set; }
        char IntAttr { get; set; }
    }

    public interface IPercentageQuestion
    {
        double AddPercentageAttr { get; set; }
    }

    public interface ITextCompleteQuestion
    {
        string AddTextAttr { get; set; }
    }

    public interface IYesNoQuestion
    {
        string AddYesNoAttr { get; set; }
    }
    public interface IAutoPropagate
    {
        Guid TargetGroupKey { get; set; }
    }
}

               
