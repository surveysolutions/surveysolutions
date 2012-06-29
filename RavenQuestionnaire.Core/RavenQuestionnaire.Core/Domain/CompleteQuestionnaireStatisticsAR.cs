using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ncqrs.Domain;

namespace RavenQuestionnaire.Core.Domain
{
    /// <summary>
    /// CompleteQuestionnaireStatistics root
    /// </summary>
    public class CompleteQuestionnaireStatisticsAR : AggregateRootMappedByConvention
    {
        public CompleteQuestionnaireStatisticsAR ()
        {
        }


        public void PreLoad()
        {
            //loads into the cache
            //no logic
        }

    }
}
