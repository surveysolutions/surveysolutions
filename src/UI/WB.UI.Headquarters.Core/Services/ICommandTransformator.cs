using System;
using WB.Core.Infrastructure.CommandBus;

namespace WB.UI.Headquarters.Code.CommandTransformation
{
    public interface ICommandTransformator
    {
        ICommand TransformCommnadIfNeeded(ICommand command, Guid? responsibleId = null);
    }
}
