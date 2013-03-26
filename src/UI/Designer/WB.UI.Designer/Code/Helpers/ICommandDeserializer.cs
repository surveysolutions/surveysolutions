namespace WB.UI.Designer.Code.Helpers
{
    using Ncqrs.Commanding;

    public interface ICommandDeserializer
    {
        ICommand Deserialize(string commandType, string serializedCommand);
    }
}