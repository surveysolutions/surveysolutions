using System;
using System.Collections.Generic;
using System.IO;
using Main.Core.Documents;
using Microsoft.Practices.ServiceLocation;
using WB.Core.BoundedContexts.Designer.Implementation.Services.LookupTableService;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Services.CodeGeneration;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.ExpressionStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace PerformanceTest
{
    public static class Mocks
    {
        public class DynamicCompilerSettingsStub : IDynamicCompilerSettings
        {
            public string Name { get; set; }
            public string PortableAssembliesPath { get; set; }
            public IEnumerable<string> DefaultReferencedPortableAssemblies { get; set; }
        }

        public class CompilerSettingsStub : ICompilerSettings
        {
            public CompilerSettingsStub(IEnumerable<IDynamicCompilerSettings> settings)
            {
                this.SettingsCollection = settings;
            }

            public bool EnableDump { get; } = false;
            public string DumpFolder { get; } = Path.Combine(Directory.GetCurrentDirectory(), "_dump");
            public IEnumerable<IDynamicCompilerSettings> SettingsCollection { get; }
        }

        public class MacrosSubstitutionServiceStub : IMacrosSubstitutionService
        {
            public string InlineMacros(string expression, IEnumerable<Macro> macros) => expression;
        }

        public class InterviewExpressionStatePrototypeProviderStub : IInterviewExpressionStatePrototypeProvider
        {
            private readonly Func<ILatestInterviewExpressionState> getState;
            private readonly Func<IInterviewExpressionStorage> getStorage;

            public InterviewExpressionStatePrototypeProviderStub(Func<ILatestInterviewExpressionState> getState = null, Func<IInterviewExpressionStorage> getStorage = null)
            {
                this.getState = getState;
                this.getStorage = getStorage;
            }

            public ILatestInterviewExpressionState GetExpressionState(Guid questionnaireId, long questionnaireVersion)
            {
                return this.getState();
            }

            public IInterviewExpressionStorage GetExpressionStorage(QuestionnaireIdentity questionnaireIdentity)
            {
                return this.getStorage();
            }
        }

        public class ServiceLocatorStub : IServiceLocator
        {
            readonly Dictionary<Type, Type> _cache = new Dictionary<Type, Type>();

            public object GetService(Type serviceType)
            {
                
                throw new NotImplementedException();
            }

            public object GetInstance(Type serviceType)
            {
                throw new NotImplementedException();
            }

            public object GetInstance(Type serviceType, string key)
            {
                throw new NotImplementedException();
            }

            public IEnumerable<object> GetAllInstances(Type serviceType)
            {
                throw new NotImplementedException();
            }

            public TService GetInstance<TService>()
            {
                throw new NotImplementedException();
            }

            public TService GetInstance<TService>(string key)
            {
                throw new NotImplementedException();
            }

            public IEnumerable<TService> GetAllInstances<TService>()
            {
                throw new NotImplementedException();
            }
        }

        public class QuestionnaireStorageStub : IQuestionnaireStorage {
            private readonly Func<IQuestionnaire> getForAll;
            private readonly Func<QuestionnaireDocument> getDocForAll;

            public QuestionnaireStorageStub(Func<IQuestionnaire> getForAll = null, Func<QuestionnaireDocument> getDocForAll = null)
            {
                this.getForAll = getForAll;
                this.getDocForAll = getDocForAll;
            }

            public IQuestionnaire GetQuestionnaire(QuestionnaireIdentity identity, string language)
            {
                return this.getForAll?.Invoke();
            }

            public void StoreQuestionnaire(Guid id, long version, QuestionnaireDocument questionnaireDocument)
            {
                throw new NotImplementedException();
            }

            public QuestionnaireDocument GetQuestionnaireDocument(QuestionnaireIdentity identity)
            {
                return this.getDocForAll?.Invoke();
            }

            public QuestionnaireDocument GetQuestionnaireDocument(Guid id, long version)
            {
                return this.getDocForAll?.Invoke();
            }

            public void DeleteQuestionnaireDocument(Guid id, long version)
            {
                throw new NotImplementedException();
            }
        }

        public class LookupTableServiceStub : ILookupTableService
        {
            public void SaveLookupTableContent(Guid questionnaireId, Guid lookupTableId, string fileContent)
            {
                
            }

            public void DeleteAllByQuestionnaireId(Guid questionnaireId)
            {
                
            }

            public LookupTableContent GetLookupTableContent(Guid questionnaireId, Guid lookupTableId)
            {
                return null;
            }

            public LookupTableContentFile GetLookupTableContentFile(Guid questionnaireId, Guid lookupTableId)
            {
                return null;
            }

            public Dictionary<Guid, string> GetQuestionnairesLookupTables(Guid questionnaireId)
            {
                return null;
            }

            public void CloneLookupTable(Guid sourceQuestionnaireId, Guid sourceTableId, string sourceLookupTableName,
                Guid newQuestionnaireId, Guid newLookupTableId)
            {
                
            }

            public bool IsLookupTableEmpty(Guid questionnaireId, Guid tableId, string lookupTableName)
            {
                return true;
            }
        }
    }
}