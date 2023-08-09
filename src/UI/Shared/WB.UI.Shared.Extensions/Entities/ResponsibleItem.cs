namespace WB.UI.Shared.Extensions.Entities;

public class ResponsibleItem
{
    public ResponsibleItem(Guid? responsibleId, string title)
    {
        ResponsibleId = responsibleId;
        Title = title;
    }

    public string Title {private set; get; }
    public Guid? ResponsibleId { private set; get; }
}