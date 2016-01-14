namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration
{
    internal class CodeGenerationSettings
    {
        public CodeGenerationSettings(
            string abstractConditionalLevelClassName,
            string[] additionInterfaces, 
            string[] namespaces, 
            bool areRosterServiceVariablesPresent, 
            string rosterType,
            bool isLookupTablesFeatureSupported)
        {
            AbstractConditionalLevelClassName = abstractConditionalLevelClassName;
            AdditionInterfaces = additionInterfaces;
            Namespaces = namespaces;
            AreRosterServiceVariablesPresent = areRosterServiceVariablesPresent;
            RosterType = rosterType;
            IsLookupTablesFeatureSupported = isLookupTablesFeatureSupported;
        }

        public string AbstractConditionalLevelClassName { get; private set; }

        public string[] AdditionInterfaces { get; private set; }

        public string[] Namespaces { get; private set; }

        public bool AreRosterServiceVariablesPresent { get; private set; }

        public string RosterType { get; private set; }

        public bool IsLookupTablesFeatureSupported { get; private set; }
    }
}
