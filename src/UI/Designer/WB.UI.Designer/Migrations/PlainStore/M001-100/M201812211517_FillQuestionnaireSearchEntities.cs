using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using FluentMigrator;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Search;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.UI.Designer.Migrations.PlainStore
{
    [Migration(201812211517)]
    public class M201812211517_FillQuestionnaireSearchEntities : Migration
    {
        private readonly IServiceLocator serviceLocator;

        public M201812211517_FillQuestionnaireSearchEntities(IServiceLocator serviceLocator)
        {
            this.serviceLocator = serviceLocator;
        }

        public override void Up()
        {
            Thread.Sleep(5000);

            var questionnaireListViewItemStorage = serviceLocator.GetInstance<IPlainStorageAccessor<QuestionnaireListViewItem>>();
            var questionnaireDocumentStorage = serviceLocator.GetInstance<IPlainKeyValueStorage<QuestionnaireDocument>>();
            var questionnaireSearchStorage = serviceLocator.GetInstance<IQuestionnaireSearchStorage>();

            var questionnaireListViewItems = questionnaireListViewItemStorage.Query(_ => _
                .Where(item => !item.IsDeleted && item.IsPublic)
            );

            foreach (var item in questionnaireListViewItems)
            {
                var questionnaireDocument = questionnaireDocumentStorage.GetById(item.QuestionnaireId);
                var entities = questionnaireDocument.Children.TreeToEnumerable(e => e.Children);
                foreach (var entity in entities)
                {
                    if (entity is IQuestion
                        || entity is IVariable
                        || entity is IGroup
                        || entity is IStaticText)
                    {
                        questionnaireSearchStorage.AddOrUpdateEntity(questionnaireDocument.PublicKey, entity);
                    }
                }
            }
        }

        public override void Down()
        {
        }
    }
}
