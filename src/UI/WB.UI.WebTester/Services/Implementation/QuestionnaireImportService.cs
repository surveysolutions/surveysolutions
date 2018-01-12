using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using AppDomainToolkit;
using Autofac;
using Main.Core.Documents;
using Newtonsoft.Json;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.Modularity.Autofac;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.Infrastructure.Native.Logging;
using WB.UI.Shared.Enumerator.Services.Internals;
using WB.UI.WebTester.Controllers;
using WB.UI.WebTester.Infrastructure;

namespace WB.UI.WebTester.Services.Implementation
{
    public class QuestionnaireImportService : IQuestionnaireImportService
    {
        

        private readonly IQuestionnaireStorage questionnaireRepository;

        public QuestionnaireImportService(IQuestionnaireStorage questionnaireRepository)
        {
            this.questionnaireRepository = questionnaireRepository;
        }

        public void ImportQuestionnaire(QuestionnaireIdentity questionnaireIdentity,
            QuestionnaireDocument questionnaireDocument,
            string supportingAssembly,
            TranslationDto[] translations,
            List<QuestionnaireAttachment> attachments)
        {
            //translationManagementService.Delete(questionnaireIdentity);
            //translationManagementService.Store(translations.Select(x => new TranslationInstance
            //{
            //    QuestionnaireId = questionnaireIdentity,
            //    Value = x.Value,
            //    QuestionnaireEntityId = x.QuestionnaireEntityId,
            //    Type = x.Type,
            //    TranslationIndex = x.TranslationIndex,
            //    TranslationId = x.TranslationId
            //}));

            this.questionnaireRepository.StoreQuestionnaire(questionnaireIdentity.QuestionnaireId, questionnaireIdentity.Version, questionnaireDocument);
            //this.questionnaireAssemblyFileAccessor.StoreAssembly(questionnaireIdentity.QuestionnaireId, questionnaireIdentity.Version, supportingAssembly);

            //var attachmnetsToStore = attachments.Select(x => Tuple.Create(x, (object)x.Content.Id));
            //this.attachmentsStorage.Store(attachmnetsToStore);
        }
    }
}
