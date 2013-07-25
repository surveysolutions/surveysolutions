using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ncqrs.Domain;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates
{
    internal class Interview : AggregateRootMappedByConvention
    {
        public Interview(Guid questionnaireId)
        {
        }
    }
}
