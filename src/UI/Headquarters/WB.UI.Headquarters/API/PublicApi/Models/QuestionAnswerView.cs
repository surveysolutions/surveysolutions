namespace WB.UI.Headquarters.API.PublicApi.Models
{
    public class QuestionAnswerView
    {
        public int Answer { get; set;}
        public string Text { get; set; }

        /// <summary>
        /// Column name in API
        /// </summary>
        public string Data { get; set; }
    }
}
