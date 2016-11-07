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

        public IEnumerable<SubstitutionVariable> ByQuestions { get; set; }
        public IEnumerable<SubstitutionVariable> ByVariables { get; set; }
        public IEnumerable<SubstitutionVariable> ByRosters { get; set; }
    }

    public class SubstitutionVariable
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }
}