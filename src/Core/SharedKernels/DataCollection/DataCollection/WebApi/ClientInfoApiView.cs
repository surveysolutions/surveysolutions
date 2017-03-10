namespace WB.Core.SharedKernels.DataCollection.WebApi
{
    public class ClientInfoApiView
    {
        /// <summary>
        /// It is json string of questionnaire document, because json deserializer could not deserialize IComposite 
        /// </summary>
        public string QuestionnaireDocument { get; set; }
        public bool AllowCensus { get; set; }
    }
}
