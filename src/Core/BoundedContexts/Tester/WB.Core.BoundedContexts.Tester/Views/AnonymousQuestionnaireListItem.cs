using System;
using SQLite;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Core.BoundedContexts.Tester.Views;

public class AnonymousQuestionnaireListItem : IPlainStorageEntity
{
    [PrimaryKey]
    public string Id { get; set; }
    public string Title { get; set; }
    public DateTime LastEntryDate { get; set; }
}
