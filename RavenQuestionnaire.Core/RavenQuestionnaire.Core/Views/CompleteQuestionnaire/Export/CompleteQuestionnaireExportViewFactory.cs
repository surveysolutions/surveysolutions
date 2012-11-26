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

        private readonly IDenormalizerStorage<QuestionnaireDocument> templateSession;
        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CompleteQuestionnaireExportViewFactory"/> class.
        /// </summary>
        /// <param name="documentSession">
        /// The document session.
        /// </param>
        public CompleteQuestionnaireExportViewFactory(
            IDenormalizerStorage<CompleteQuestionnaireStoreDocument> documentSession, IDenormalizerStorage<QuestionnaireDocument> templateSession)
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
            if (!input.QuestionnairiesForImport.Any())
            {
                return new CompleteQuestionnaireExportView();
            }
            IGroup template = this.templateSession.GetByGuid(input.TemplateId);
            if (input.PropagatableGroupPublicKey.HasValue)
                template = template.FirstOrDefault<IGroup>(g => g.PublicKey == input.PropagatableGroupPublicKey.Value);

            if (template==null)
            {
                return new CompleteQuestionnaireExportView();
            }
            var documents = new List<CompleteQuestionnaireExportItem>(input.QuestionnairiesForImport.Count());
            var subObjects = new List<Guid>();
            var header = BuildHeader(template, subObjects);
            var headerKey = header.Select(h => h.Key);
            foreach (var key in input.QuestionnairiesForImport)
            {
                var document = this.documentSession.GetByGuid(key);
                if(document.TemplateId!=input.TemplateId)
                    throw new ArgumentException("questionnaire has different template");
                if (!input.PropagatableGroupPublicKey.HasValue)
                {
                    documents.Add(
                        new CompleteQuestionnaireExportItem(
                            document, headerKey, null));
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
                                completeGroup, headerKey, document.PublicKey));
                    }
                }
            }
            return new CompleteQuestionnaireExportView(template.Title, documents, subObjects, header);
        }

        #endregion

        protected Dictionary<Guid, HeaderItem> BuildHeader(IGroup template, List<Guid> subObjects)
        {
            var result = new Dictionary<Guid, HeaderItem>();
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
                    result.Add(question.PublicKey, new HeaderItem(question));
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
            return result;
        }
    }
}