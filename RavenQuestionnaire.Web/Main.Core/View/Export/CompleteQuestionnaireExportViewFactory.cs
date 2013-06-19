// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CompleteQuestionnaireExportViewFactory.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The complete questionnaire export view factory.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Complete;
using Main.Core.Entities.SubEntities.Question;
using Main.DenormalizerStorage;

using WB.Core.Infrastructure;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace Main.Core.View.Export
{
    /// <summary>
    /// The complete questionnaire export view factory.
    /// </summary>
    public class CompleteQuestionnaireExportViewFactory : IViewFactory<CompleteQuestionnaireExportInputModel, CompleteQuestionnaireExportView>
    {
        #region Fields

        /// <summary>
        /// The document session.
        /// </summary>
        private readonly IReadSideRepositoryReader<CompleteQuestionnaireStoreDocument> documentSession;

        private readonly IReadSideRepositoryReader<QuestionnaireDocument> templateSession;
        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CompleteQuestionnaireExportViewFactory"/> class.
        /// </summary>
        /// <param name="documentSession">
        /// The document session.
        /// </param>
        public CompleteQuestionnaireExportViewFactory(
            IReadSideRepositoryReader<CompleteQuestionnaireStoreDocument> documentSession, IReadSideRepositoryReader<QuestionnaireDocument> templateSession)
        {
            this.documentSession = documentSession;
            this.templateSession = templateSession;
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The load.
        /// </summary>
        /// <param name="input">
        /// The input.
        /// </param>
        /// <returns>
        /// The RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Export.CompleteQuestionnaireExportView.
        /// </returns>
        public CompleteQuestionnaireExportView Load(CompleteQuestionnaireExportInputModel input)
        {
            var template = this.templateSession.GetById(input.TemplateId);
            if (template == null)
            {
                return null;
            }
            if (input.PropagatableGroupPublicKey.HasValue)
            {
                var groupTemplate =
                    template.FirstOrDefault<IGroup>(g => g.PublicKey == input.PropagatableGroupPublicKey.Value);

                if (groupTemplate == null)
                {
                    return null;
                }
                return CreateVew(groupTemplate, input.TemplateId, input.PropagatableGroupPublicKey,
                                 input.QuestionnairiesForImport);
            }
            else if (input.AutoPropagatebleQuestionPublicKey.HasValue)
                return HandleAutoPropagatedQuestion(template, input.AutoPropagatebleQuestionPublicKey.Value,
                                                    input.QuestionnairiesForImport);
            else
                return CreateVew(template, input.TemplateId, input.PropagatableGroupPublicKey,
                                 input.QuestionnairiesForImport);
        }

        #endregion
        protected CompleteQuestionnaireExportView HandleAutoPropagatedQuestion(QuestionnaireDocument template, Guid questionKey, IEnumerable<Guid> questionnairies)
        {
            var question = template.FirstOrDefault<IQuestion>(q => q.PublicKey == questionKey);
            var autoQuestion = question as IAutoPropagate;
            CompleteQuestionnaireExportView result = null;
            if (autoQuestion == null)
                return null;

            foreach (Guid trigger in autoQuestion.Triggers)
            {
                var groupTemplate =
                    template.FirstOrDefault<IGroup>(g => g.PublicKey == trigger);
                if (result != null)
                    result = result.Merge(CreateVew(groupTemplate, template.PublicKey, trigger, questionnairies));
                else
                    result = CreateVew(groupTemplate, template.PublicKey, trigger, questionnairies);
            }
            
            return result;
        }

        protected CompleteQuestionnaireExportView CreateVew(IGroup template,Guid questionnaieKey,Guid? propagatableGroupPublicKey,  IEnumerable<Guid> questionnairies )
        {
            var documents = new List<CompleteQuestionnaireExportItem>(questionnairies.Count());
            var subObjects = new List<Guid>();
            var autoQuestions = new List<AutoQuestionWithTriggers>();
            var header = BuildHeader(template, subObjects, autoQuestions);
         //   var headerKey = header.Select(h => h.Key);
            
            foreach (var key in questionnairies)
            {
                var document = this.documentSession.GetById(key);
                if (document.TemplateId != questionnaieKey)
                    throw new ArgumentException("questionnaire has different template");
                if (!propagatableGroupPublicKey.HasValue)
                {
                    documents.Add(
                        new CompleteQuestionnaireExportItem(
                            document, header.Keys, null));
                }
                else
                {
                    var subGroups =
                        document.Find<ICompleteGroup>(
                            g =>
                            g.PropagationPublicKey.HasValue && g.PublicKey == propagatableGroupPublicKey.Value);
                    foreach (ICompleteGroup completeGroup in subGroups)
                    {
                        documents.Add(
                            new CompleteQuestionnaireExportItem(
                                completeGroup, header.Keys, document.PublicKey));
                    }
                }
            }
            return new CompleteQuestionnaireExportView(template.PublicKey,/*propagatableGroupPublicKey,*/ template.Title, documents, subObjects,
                                                       autoQuestions.Select(q => q.PublicKey), header);
        }

        protected HeaderCollection BuildHeader(IGroup template, List<Guid> subObjects, List<AutoQuestionWithTriggers> autoQuestions)
        {
            var result = new HeaderCollection();
            Queue<IComposite> queue=new Queue<IComposite>();
            foreach (IComposite composite in template.Children)
            {
                queue.Enqueue(composite);
            }
            while (queue.Count > 0)
            {
                var item = queue.Dequeue();
                var question = item as IQuestion;
                if (question != null)
                {
                    result.Add(question);
                    var autoQuestion = question as AutoPropagateQuestion;
                    if (autoQuestion != null)
                    {
                        autoQuestions.Add(new AutoQuestionWithTriggers(autoQuestion));
                        foreach (Guid guid in autoQuestion.Triggers)
                        {
                            subObjects.Remove(guid);
                        }
                    }
                    continue;
                }
                var group = item as IGroup;
                if (group != null)
                {
                    if (group.Propagated != Propagate.None)
                    {
                        if (!autoQuestions.SelectMany(q=>q.Triggers).Contains(group.PublicKey))
                            subObjects.Add(group.PublicKey);
                        continue;
                    }
                    foreach (IComposite child in group.Children)
                    {
                        queue.Enqueue(child);
                    }
                }
            }
            return result;
        }
        public class AutoQuestionWithTriggers
        {
            public AutoQuestionWithTriggers(AutoPropagateQuestion question)
            {
                PublicKey = question.PublicKey;
                Triggers = question.Triggers;
            }

            public Guid PublicKey { get; private set; }
            public IEnumerable<Guid> Triggers { get; private set; }
        }
    }
}