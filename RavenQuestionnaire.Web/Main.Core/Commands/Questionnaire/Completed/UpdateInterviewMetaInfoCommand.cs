using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Documents;
using Main.Core.Domain;
using Main.Core.Entities.SubEntities;
using Ncqrs.Commanding;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using WB.Core.SharedKernel.Structures.Synchronization;

namespace Main.Core.Commands.Questionnaire.Completed
{
    [Serializable]
    [MapsToAggregateRootMethodOrConstructor(typeof(CompleteQuestionnaireAR), "UpdateInterviewMetaInfo")]
    public class UpdateInterviewMetaInfoCommand : CommandBase
    {
        public UpdateInterviewMetaInfoCommand(Guid publicKey, Guid templateId, string title, Guid? responsibleId,
                                              Guid statusId, List<FeaturedQuestionMeta> featuredQuestionsMeta)
            : base(publicKey)
        {
            Id = PublicKey = publicKey;
            TemplateId = templateId;
            Title = title;
            ResponsibleId = responsibleId;
            StatusId = statusId;
            FeaturedQuestionsMeta = featuredQuestionsMeta;
        }

        [AggregateRootId]
        public Guid PublicKey { get; set; }

        public Guid Id { get; set; }

        public Guid TemplateId { get; set; }

        public string Title { get; set; }

        public Guid? ResponsibleId { get; set; }

        public Guid StatusId { get; set; }

        public List<FeaturedQuestionMeta> FeaturedQuestionsMeta { get; set; }
    }
}
