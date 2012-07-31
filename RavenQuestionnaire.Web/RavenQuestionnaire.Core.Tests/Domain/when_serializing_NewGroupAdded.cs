using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ncqrs.Spec;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Events.Questionnaire;

namespace RavenQuestionnaire.Core.Tests.Domain
{
    public class when_serializing_NewGroupAdded : JsonEventSerializationFixture<NewGroupAdded>
    {
        protected override NewGroupAdded GivenEvent()
        {
            return new  NewGroupAdded
            {
                PublicKey = Guid.NewGuid(),
                GroupText = "text",
                ParentGroupPublicKey = Guid.NewGuid(),
                Paropagateble = Propagate.None,
                ConditionExpression = ""
            };
        }
    }
}
