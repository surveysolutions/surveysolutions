using WB.Core.Infrastructure.Aggregates;

namespace WB.Core.Infrastructure.CommandBus
{
    public interface ICommandValidator<in TAggregateRoot> where TAggregateRoot : IAggregateRoot
    {
        void Validate(TAggregateRoot aggregate);
    }
}