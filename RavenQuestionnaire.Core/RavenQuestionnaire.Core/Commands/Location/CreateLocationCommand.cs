using System;
using Ncqrs.Commanding;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using RavenQuestionnaire.Core.Domain;

namespace RavenQuestionnaire.Core.Commands.Location
{
    [Serializable]
    [MapsToAggregateRootConstructor(typeof(LocationAR))]
    public class CreateLocationCommand : CommandBase
    {
        public Guid LocationId
        {
            get;
            set;
        }

        public String Text
        {
            get;
            set;
        }

        public CreateLocationCommand(Guid locationId, string title)
        {
            LocationId = locationId;
            Text = title;

        }
    }
}
