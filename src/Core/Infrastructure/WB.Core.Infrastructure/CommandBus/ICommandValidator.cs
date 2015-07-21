using WB.Core.Infrastructure.Aggregates;

namespace WB.Core.Infrastructure.CommandBus
{
    public interface ICommandValidator<in TAggregateRoot, in TCommand> 
        where TAggregateRoot : IAggregateRoot
        where TCommand : ICommand
    {
        void Validate(TAggregateRoot aggregate, TCommand command);
    }
}