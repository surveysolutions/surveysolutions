using System;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model
{
    public class LinkedQuestionVerifierModel
    {
        public LinkedQuestionVerifierModel(Guid id, string rosterScopeName, string memberName, string typeName, string stateName)
        {
            Id = id;
            RosterScopeName = rosterScopeName;
            MemberName = memberName;
            TypeName = typeName;
            StateName = stateName;
        }

        public Guid Id { set; get; }
        public string RosterScopeName { set; get; }
        public string MemberName { set; get; }
        public string TypeName { set; get; }
        public string StateName { get; set; }
    }
}
