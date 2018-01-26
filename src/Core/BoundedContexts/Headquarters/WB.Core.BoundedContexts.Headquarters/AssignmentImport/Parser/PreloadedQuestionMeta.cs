namespace WB.Core.BoundedContexts.Headquarters.AssignmentImport.Parser
{
    public class PreloadedQuestionMeta
    {
        public PreloadedQuestionMeta(string variableName, int[] indexes, string[] headers)
        {
            Variable = variableName;
            Indexes = indexes;
            Headers = headers;
        }

        public string Variable { get;  }
        public int[] Indexes { get;  }
        public string[] Headers { get; }
    }
}