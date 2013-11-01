using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ninject.Modules;
using WB.Core.SharedKernels.QuestionnaireVerification.Implementation.Services;
using WB.Core.SharedKernels.QuestionnaireVerification.Services;

namespace WB.Core.SharedKernels.QuestionnaireVerification
{
    public class QuestionnaireVerificationModule : NinjectModule
    {
        public override void Load()
        {
            this.Bind<IQuestionnaireVerifier>().To<QuestionnaireVerifier>().InSingletonScope();
        }
    }
}
