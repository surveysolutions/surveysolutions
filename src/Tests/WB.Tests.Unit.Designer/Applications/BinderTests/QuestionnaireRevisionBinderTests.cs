using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Designer.DataAccess;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.GenericSubdomains.Portable;
using WB.Tests.Abc;
using WB.UI.Designer.Models;

namespace WB.Tests.Unit.Designer.Applications.BinderTests
{
    public class QuestionnaireRevisionBinderTests
    {
        private DesignerDbContext db;
        private QuestionnaireRevisionBinder binder;
        private Guid questionnaireId;

        private ModelBindingContext PrepareContext(string idValue)
        {
            var bindingContext = new DefaultModelBindingContext
            {
                ModelName = "id",
                BindingSource = new BindingSource("", "", false, false),
                ModelState = new ModelStateDictionary()
            };

            var routeValueDictionary = new RouteValueDictionary
            {
                {"id", idValue}
            };

            bindingContext.ValueProvider = new RouteValueProvider(bindingContext.BindingSource, routeValueDictionary);

            return bindingContext;
        }

        [SetUp]
        public void Setup()
        {
            this.db = Create.InMemoryDbContext();
            this.binder = new QuestionnaireRevisionBinder(db);
            this.questionnaireId = Guid.NewGuid();
        }

        [Test]
        public async Task should_bind_uuid_to_questionnaire_revision()
        {            
            var context = PrepareContext(questionnaireId.ToString());
            
            // act
            await binder.BindModelAsync(context);

            // assert
            Assert.That(context.Result.IsModelSet, Is.True);

            var revision = context.Result.Model as QuestionnaireRevision;

            Assert.NotNull(revision);
            Assert.That(revision.QuestionnaireId, Is.EqualTo(questionnaireId));
        }

        [Test]
        public async Task should_bind_model_if_history_item_sequence_exists()
        {
            var context = PrepareContext($"{questionnaireId:N}$1");
            var revisionid = Id.g2;

            db.QuestionnaireChangeRecords.Add(new QuestionnaireChangeRecord
            {
                QuestionnaireId = questionnaireId.FormatGuid(),
                Sequence = 1,
                QuestionnaireChangeRecordId = revisionid.FormatGuid()
            });

            db.SaveChanges();

            // act
            await binder.BindModelAsync(context);

            // assert
            var revision = context.Result.Model as QuestionnaireRevision;
            Assert.NotNull(revision);

            Assert.That(revision.QuestionnaireId, Is.EqualTo(questionnaireId));
            Assert.That(revision.Revision, Is.EqualTo(revisionid));
        }
        
        [Test]
        public async Task should_not_bind_model_if_history_item_sequence_not_exists()
        {
            var context = PrepareContext($"{questionnaireId:N}$1");
            var revisionid = Id.g2;

            // act
            await binder.BindModelAsync(context);

            // assert
            var revision = context.Result.Model as QuestionnaireRevision;
            Assert.Null(revision);
            Assert.False(context.Result.IsModelSet);
        }
    }
}
