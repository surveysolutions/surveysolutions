namespace Main.Core.Entities.SubEntities.Question
{
    public class MultimediaQuestion : AbstractQuestion, IMultimediaQuestion
    {
        public override QuestionType QuestionType => QuestionType.Multimedia;

        public bool IsSignature { get; set; }
    }
}
