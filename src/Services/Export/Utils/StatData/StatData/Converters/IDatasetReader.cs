namespace StatData.Converters
{
    public interface IDatasetReader
    {
        string[,] GetData(string srcFile);
        string GetVarNamesStr(string filename);
    }
}
