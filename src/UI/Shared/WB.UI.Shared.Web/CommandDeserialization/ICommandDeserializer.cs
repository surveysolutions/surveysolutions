using Ncqrs.Commanding;

namespace WB.UI.Shared.Web.CommandDeserialization
{
    public interface ICommandDeserializer
    {
        ICommand Deserialize(string commandType, string serializedCommand);
    }
}