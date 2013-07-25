using System.Configuration;
using Core.Supervisor.Denormalizer;
using Core.Supervisor.Views;
using Main.Core;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Main.Core.Documents;
using Ninject;
using Ninject.Activation;
using WB.Core.Infrastructure.Raven.Implementation.ReadSide.RepositoryAccessors;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Commands.Questionnaire;

namespace CapiDataGenerator
{
    public class CapiDataGeneratorRegistry : CoreRegistry
    {
        public CapiDataGeneratorRegistry(string repositoryPath, bool isEmbeded)
            : base(repositoryPath, isEmbeded)
        {
        }
        /// <summary>
        /// The get assweblys for register.
        /// </summary>
        /// <returns>
        /// </returns>
        public override IEnumerable<Assembly> GetAssweblysForRegister()
        {
            return
                Enumerable.Concat(base.GetAssweblysForRegister(), new[]
                {
                    GetType().Assembly,
                    typeof (CompleteQuestionnaireDenormalizer).Assembly,
                    typeof(ImportQuestionnaireCommand).Assembly
                });
        }
    }
}
