// -----------------------------------------------------------------------
// <copyright file="MainCoreRegistry.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using System.Linq;
using System.Reflection;
using Core.HQ.Synchronization;
using Main.Core;
using Main.Core.Events;
using Questionnaire.Core.Web.Export;
using Questionnaire.Core.Web.Security;
using RavenQuestionnaire.Core.Export;
using RavenQuestionnaire.Core.Export.csv;
using RavenQuestionnaire.Core.Views.Questionnaire;
using RavenQuestionnaire.Web.Export;

namespace RavenQuestionnaire.Web.Injections
{
    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class MainCoreRegistry : CoreRegistry
    {
        public MainCoreRegistry(string repositoryPath, bool isEmbeded) : base(repositoryPath, isEmbeded)
        {
        }
        public override System.Collections.Generic.IEnumerable<System.Reflection.Assembly> GetAssweblysForRegister()
        {
            return
                base.GetAssweblysForRegister().Concat(new Assembly[] { typeof(HQEventSync).Assembly, typeof(QuestionnaireView).Assembly, typeof(QuestionnaireMembershipProvider).Assembly });
        }
        public override void Load()
        {
            base.Load();   
            this.Unbind<IEventSync>();
            this.Bind<IEventSync>().To<HQEventSync>();
            this.Unbind<IExportImport>();
            this.Bind<IExportImport>().To<TemplateExporter>();
            this.Bind<ITemplateExporter>().To<TemplateExporter>();
            this.Bind<IExportProvider>().To<CSVExporter>();
        }
    }
}
