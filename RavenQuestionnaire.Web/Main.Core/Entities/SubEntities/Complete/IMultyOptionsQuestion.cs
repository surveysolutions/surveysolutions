namespace Main.Core.Entities.SubEntities.Complete
{
    /// <summary>
    /// The MultyOptionsQuestion interface.
    /// </summary>
    public interface IMultyOptionsQuestion : IQuestion
    {
        #region Public Properties

        bool IsAnswersOrdered { get; set; }
        int? MaxAllowedAnswers { get; set; }

        #endregion
    }
}