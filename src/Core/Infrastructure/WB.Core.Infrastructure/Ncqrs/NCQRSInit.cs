using WB.Core.Infrastructure.CommandBus;
using CommandService = Ncqrs.Commanding.ServiceModel.CommandService;

namespace Ncqrs
{
    public static class NcqrsInit
    {
        public static void InitializeCommandService(ICommandListSupplier commandSupplier, CommandService service)
        {
            //var mapper = new AttributeBasedCommandMapper();
            //foreach (Type type in commandSupplier.GetCommandList())
            //{
            //    service.RegisterExecutor(type, new UoWMappedCommandExecutor(mapper));
            //}
            NcqrsEnvironment.SetDefault<ICommandService>(service);
        }
    }
}