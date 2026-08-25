namespace WB.UI.Designer.Code.ImportExport
{
    public interface IQuestionnaireSchemaProvider
    {
        string GetSchema();

        /// <summary>
        /// Stable identifier of the currently deployed schema. Changes whenever the schema text changes.
        /// </summary>
        string GetSchemaVersion();
    }
}
