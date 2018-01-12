using System;
using System.Collections.Generic;
using System.IO;
using AppDomainToolkit;
using Autofac;
using Main.Core.Documents;
using Newtonsoft.Json;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.Modularity.Autofac;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Infrastructure.Native.Logging;
using WB.UI.Shared.Enumerator.Services.Internals;
using WB.UI.WebTester.Infrastructure;

namespace WB.UI.WebTester.Services.Implementation
{
    public class AppdomainsPerInterviewManager : IAppdomainsPerInterviewManager
    {
        private readonly string binFolderPath;
        private const int QuestionnaireVersion = 1;

        private readonly Dictionary<Guid, AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver>> appDomains = new Dictionary<Guid, AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver>>();

        public AppdomainsPerInterviewManager(string binFolderPath)
        {
            this.binFolderPath = binFolderPath;
        }

        public void SetupForInterview(Guid interviewId, QuestionnaireDocument questionnaireDocument, 
            string supportingAssembly)
        {
            if (appDomains.ContainsKey(interviewId))
            {
                appDomains[interviewId].Dispose();
                appDomains.Remove(interviewId);
            }

            var tempFileName = Path.GetTempFileName();
            try
            {
                File.WriteAllBytes(tempFileName, Convert.FromBase64String(supportingAssembly));

                AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver> domainContext = AppDomainContext.Create();
                domainContext.RemoteResolver.PrivateBinPath = binFolderPath;
                domainContext.LoadAssembly(LoadMethod.LoadFrom, tempFileName);

                string documentString = JsonConvert.SerializeObject(questionnaireDocument, Formatting.None,
                    new JsonSerializerSettings
                    {
                        TypeNameHandling = TypeNameHandling.Objects,
                        NullValueHandling = NullValueHandling.Ignore,
                        FloatParseHandling = FloatParseHandling.Decimal,
                        Formatting = Formatting.None,
                    });
                RemoteAction.Invoke(domainContext.Domain,
                    documentString, supportingAssembly,
                    (questionnaire, assembly) =>
                    {
                        SetupAppDomainsSeviceLocator();

                        QuestionnaireDocument document1 = JsonConvert.DeserializeObject<QuestionnaireDocument>(
                            questionnaire, new JsonSerializerSettings
                            {
                                TypeNameHandling = TypeNameHandling.Objects,
                                NullValueHandling = NullValueHandling.Ignore,
                                FloatParseHandling = FloatParseHandling.Decimal,
                                Formatting = Formatting.None,
                            });
                        ServiceLocator.Current.GetInstance<IQuestionnaireAssemblyAccessor>().StoreAssembly(document1.PublicKey, QuestionnaireVersion, assembly);
                        ServiceLocator.Current.GetInstance<IQuestionnaireStorage>().StoreQuestionnaire(document1.PublicKey, QuestionnaireVersion, document1);
                    });

                appDomains[interviewId] = domainContext;

            }
            finally
            {
                File.Delete(tempFileName);
            }
        }

        private static void SetupAppDomainsSeviceLocator()
        {
            ContainerBuilder containerBuilder = new ContainerBuilder();
            containerBuilder.RegisterModule(new WebTesterAppDomainModule().AsAutofac());
            containerBuilder.RegisterModule(new NLogLoggingModule().AsAutofac());

            var container = containerBuilder.Build();
            ServiceLocator.SetLocatorProvider(() => new AutofacServiceLocatorAdapter(container));
        }

        public void Dispose(Guid interviewId)
        {
            throw new NotImplementedException();
        }
    }
}