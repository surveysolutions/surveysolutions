namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration
{
    internal class CodeGenerationSettings
    {
        public CodeGenerationSettings(
            string abstractConditionalLevelClassName,
            string[] additionInterfaces, 
            string[] namespaces, 
            bool areRosterServiceVariablesPresent, string rosterType)
        {
            AbstractConditionalLevelClassName = abstractConditionalLevelClassName;
            AdditionInterfaces = additionInterfaces;
            Namespaces = namespaces;
            AreRosterServiceVariablesPresent = areRosterServiceVariablesPresent;
            RosterType = rosterType;
        }

        public string AbstractConditionalLevelClassName { get; private set; }

        public string[] AdditionInterfaces { get; private set; }

        public string[] Namespaces { get; private set; }

        public bool AreRosterServiceVariablesPresent { get; private set; }

        public string RosterType { get; private set; }
    }
}
