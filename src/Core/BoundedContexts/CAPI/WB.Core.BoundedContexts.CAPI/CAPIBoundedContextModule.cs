using Ninject.Modules;
using WB.Core.BoundedContexts.Capi.Aggregates;
using WB.Core.BoundedContexts.Capi.Commands;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.ReadSide;

namespace WB.Core.BoundedContexts.Capi
{
    public class CapiBoundedContextModule : NinjectModule
    {
        public override void Load()
        {
            this.Bind<IViewFactory<QuestionnaireScreenInput, InterviewViewModel>>()
               .To<QuestionnaireScreenViewFactory>().InSingletonScope();

            CommandRegistry
               .Setup<FileAR>()
               .InitializesWith<UploadFileCommand>(command => command.PublicKey, (command, aggregate) => aggregate.UploadFile(command.Description, command.OriginalFile, command.PublicKey, command.Title));
        }
    }
}
