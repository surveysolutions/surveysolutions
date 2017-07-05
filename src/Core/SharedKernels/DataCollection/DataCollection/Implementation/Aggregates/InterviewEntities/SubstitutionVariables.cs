using System;
using System.Collections.Generic;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities
{
    public class SubstitutionVariables
    {
        public SubstitutionVariables()
        {
            this.ByRosters = new List<SubstitutionVariable>();
            this.ByVariables = new List<SubstitutionVariable>();
            this.ByQuestions = new List<SubstitutionVariable>();
        }

        public List<SubstitutionVariable> ByQuestions { get; set; }
        public List<SubstitutionVariable> ByVariables { get; set; }
        public List<SubstitutionVariable> ByRosters { get; set; }
    }

    public class SubstitutionVariable
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }
}