// -----------------------------------------------------------------------
// <copyright file="IDataExport.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Main.Core.Entities.SubEntities;
using Main.Core.Events;
using Main.Core.Export;
using Main.Core.View;
using Main.Core.View.Export;
using Main.Core.View.StatusReport;
using Ninject;
using Ninject.Parameters;

namespace Questionnaire.Core.Web.Export
{
    public class DataExport : ZipExportImport,IDataExport
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateExporter"/> class.
        /// </summary>
        /// <param name="synchronizer">
        /// The synchronizer.
        /// </param>
        public DataExport(IKernel kernel)
            : base(kernel.Get<IEventSync>())
        {
            this.kernel = kernel;
            this.viewRepository = kernel.Get<IViewRepository>();
        }

        #endregion
        #region Fields

        private readonly IViewRepository viewRepository;
        private readonly IKernel kernel;
        #endregion
        public byte[] ExportData(Guid templateGuid, string type)
        {
            if (type == "csv" || type == "tab")
            {
                string fileName = string.Format("exported{0}.zip", DateTime.Now.ToLongTimeString());
                var provider = kernel.Get<IExportProvider<CompleteQuestionnaireExportView>>(new ConstructorArgument("delimeter", type == "csv" ? ',' : '\t'));// new CSVExporter(type == "csv" ? ',' : '\t');
                var manager = new ExportManager<CompleteQuestionnaireExportView>(provider);
                var allLevels = new Dictionary<string, byte[]>();
                var questionnairies =
                    this.viewRepository.Load<CQStatusReportViewInputModel, CQStatusReportView>(
                        new CQStatusReportViewInputModel(templateGuid, SurveyStatus.Approve.PublicId));
                StringBuilder doContent=new StringBuilder();
                System.Text.ASCIIEncoding encoding = new ASCIIEncoding();
                
                CollectLevels(new CompleteQuestionnaireExportInputModel(questionnairies.Items.Select(q => q.PublicKey), templateGuid, null), allLevels, manager, doContent, string.Empty);
                doContent.AppendLine("list");
                allLevels.Add("data.do", encoding.GetBytes(doContent.ToString()));
                return this.ExportInternal(allLevels,
                                           fileName);
            }
            return null;
        }


        protected void CollectLevels(/*Guid templateId, */CompleteQuestionnaireExportInputModel input, Dictionary<string, byte[]> container, ExportManager<CompleteQuestionnaireExportView> manager, StringBuilder doContent, string parentName)
        {
            CompleteQuestionnaireExportView records =
                this.viewRepository.Load<CompleteQuestionnaireExportInputModel, CompleteQuestionnaireExportView>
                    (input);
            var fileName = GetName(records.GroupName, container, 0);
            var currentName = "PublicKey";
            container.Add(fileName, manager.ExportToStream(records));
            doContent.AppendLine("clear");
            doContent.AppendLine(string.Format("insheet using \"{0}\", comma", fileName));
           
            if(!input.AutoPropagatebleQuestionPublicKey.HasValue && !input.PropagatableGroupPublicKey.HasValue)
            {
                doContent.AppendLine("sort " + currentName);
                doContent.AppendLine("tempfile ind");
                doContent.AppendLine("save \"`ind'\"");
            }
            else
            {
                currentName += (input.AutoPropagatebleQuestionPublicKey??input.PropagatableGroupPublicKey).ToString();
                doContent.AppendLine(string.Format("gen {0}=string(PublicKey)", currentName));
                doContent.AppendLine("drop PublicKey");

                doContent.AppendLine(string.Format("gen {0}=string(ForeignKey)", parentName));
                doContent.AppendLine("drop ForeignKey");
                doContent.AppendLine("sort " + parentName);
                doContent.AppendLine(string.Format("merge m:1 {0} using \"`ind'\"", parentName));
                doContent.AppendLine("drop _merge");
                
            }
            foreach (Guid autoPropagatebleQuestionPublicKey in records.AutoPropagatebleQuestionsPublicKeys)
            {

                CollectLevels(new CompleteQuestionnaireExportInputModel(input.QuestionnairiesForImport, input.TemplateId) { AutoPropagatebleQuestionPublicKey = autoPropagatebleQuestionPublicKey }, container,
                                  manager, doContent, currentName);
            }
            foreach (Guid subPropagatebleGroup in records.SubPropagatebleGroups)
            {
                CollectLevels(new CompleteQuestionnaireExportInputModel(input.QuestionnairiesForImport, input.TemplateId, subPropagatebleGroup), container, manager, doContent, currentName);
            }
        
        }
        protected string GetName(string name, Dictionary<string, byte[]> container, int i)
        {
            if (i == 0)
            {
                if (!container.ContainsKey(name + ".csv"))
                    return name + ".csv";
                else
                    return GetName(name, container, i + 1);
            }
            if (!container.ContainsKey(name + i + ".csv"))
                return name + i + ".csv";
            else
                return GetName(name, container, i + 1);
        }
    }
}
