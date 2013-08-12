using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ncqrs.Eventing.ServiceModel.Bus.ViewConstructorEventBus
{
    public interface ISmartEventBus : IEventBus
    {
        void DisableEventHandler(Type handlerType);
        void EnableAllHandlers();
        IEnumerable<IEventHandler> GetAllRegistredEventHandlers();
        void AddHandler(IEventHandler handler);
        void RemoveHandler(IEventHandler handler);
       
    }
}
