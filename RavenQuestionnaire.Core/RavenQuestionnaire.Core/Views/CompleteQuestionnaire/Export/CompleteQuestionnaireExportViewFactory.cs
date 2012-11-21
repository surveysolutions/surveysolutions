// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CompleteQuestionnaireExportViewFactory.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The complete questionnaire export view factory.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Collections;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Complete;
using Main.Core.View;

namespace RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Export
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Main.Core.Documents;
    using Main.Core.Entities;
    using Main.Core.Utility;
    using Main.Core.View.CompleteQuestionnaire;
    using Main.DenormalizerStorage;
    

    /// <summary>
    /// The complete questionnaire export view factory.
    /// </summary>
    public class CompleteQuestionnaireExportViewFactory : IViewFactory<CompleteQuestionnaireExportInputModel, CompleteQuestionnaireExportView>
    {
        #region Fields

        /// <summary>
        /// The document session.
        /// </summary>
        private readonly IDenormalizerStorage<CompleteQuestionnaireStoreDocument> documentSession;

        private readonly IDenormalizerStorage<CompleteQuestionnaireBrowseItem> documentShortView;
        private readonly IDenormalizerStorage<QuestionnaireDocument> templateStore;
        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CompleteQuestionnaireExportViewFactory"/> class.
        /// </summary>
        /// <param name="documentSession">
        /// The document session.
        /// </param>
        public CompleteQuestionnaireExportViewFactory(
            IDenormalizerStorage<CompleteQuestionnaireStoreDocument> documentSession, 
            IDenormalizerStorage<CompleteQuestionnaireBrowseItem> documentShortView, 
            IDenormalizerStorage<QuestionnaireDocument> templateStore)
        {
            this.templateStore = templateStore;
            this.documentSession = documentSession;
            this.documentShortView = documentShortView;
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
            IGroup template = this.templateStore.GetByGuid(input.QuestionnaryId);
            if(template==null)
            {
                return new CompleteQuestionnaireExportView();
            }
            if (input.PropagatableGroupPublicKey.HasValue)
                template = template.FirstOrDefault<IGroup>(c => c.PublicKey == input.PropagatableGroupPublicKey.Value);
            // Adjust the model appropriately
            var questionnairies = this.documentShortView.Query().Where(t => t.TemplateId == input.QuestionnaryId);
            if (!questionnairies.Any())
            {
                return new CompleteQuestionnaireExportView();
            }
            var documents = new List<CompleteQuestionnaireExportItem>(questionnairies.Count());
            var subObjects = new List<Guid>();
            var header = BuildHeader(template, subObjects);
            foreach (CompleteQuestionnaireBrowseItem completeQuestionnaireBrowseItem in questionnairies)
            {
                var document = this.documentSession.GetByGuid(completeQuestionnaireBrowseItem.CompleteQuestionnaireId);
                if (!input.PropagatableGroupPublicKey.HasValue)
                {
                    documents.Add(
                        new CompleteQuestionnaireExportItem(
                            document, header.Select(h => h.Key), null));
                }
                else
                {
                    var subGroups =
                        document.Find<ICompleteGroup>(
                            g =>
                            g.PropagationPublicKey.HasValue && g.PublicKey == input.PropagatableGroupPublicKey.Value);
                    foreach (ICompleteGroup completeGroup in subGroups)
                    {
                        documents.Add(
                            new CompleteQuestionnaireExportItem(
                                completeGroup, header.Select(h => h.Key), document.PublicKey));
                    }
                }
            }
            return new CompleteQuestionnaireExportView(documents, subObjects, header);
        }

        #endregion

        protected Dictionary<Guid, string> BuildHeader(IGroup template,  List<Guid> subObjects)
        {
            var result = new Dictionary<Guid, string>();
            result.Add(template.PublicKey, "PublicKey");
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
                    result.Add(question.PublicKey, question.QuestionText);
                    continue;
                }
                var group = item as IGroup;
                if (group != null)
                {
                    if (group.Propagated != Propagate.None )
                    {
                        subObjects.Add(group.PublicKey);
                        continue;
                    }
                    foreach (IComposite child in group.Children)
                    {
                        queue.Enqueue(child);
                    }
                }
            }
            result.Add(Guid.Empty, "ForeignKey");
            return result;
        }
    }
}