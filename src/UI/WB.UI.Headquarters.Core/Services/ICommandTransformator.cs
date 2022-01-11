using System;
using WB.Core.Infrastructure.CommandBus;

namespace WB.UI.Headquarters.Code.CommandTransformation
{
    public interface ICommandTransformator
    {
        ICommand TransformCommandIfNeeded(ICommand command, Guid? responsibleId = null);
    }
}
