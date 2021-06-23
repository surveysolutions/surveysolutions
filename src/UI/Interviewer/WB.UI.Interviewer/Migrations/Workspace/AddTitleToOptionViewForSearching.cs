using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.UI.Interviewer.Migrations.Workspace
{
    [Migration(201904081208)]
    public class AddTitleToOptionViewForSearching : IMigration
    {
        private readonly IPlainStorage<OptionView, int?> optionsStorage;

        public AddTitleToOptionViewForSearching(IPlainStorage<OptionView, int?> optionsStorage)
        {
            this.optionsStorage = optionsStorage;
        }

        public void Up()
        {
            var hasEmptySearchTitles = optionsStorage.Count(x => x.SearchTitle == null) > 0;
            if (!hasEmptySearchTitles) return;

            var allOptions = optionsStorage.LoadAll();

            foreach (var optionView in allOptions)
                optionView.SearchTitle = optionView.Title.ToLower();

            optionsStorage.Store(allOptions);
        }
    }
}
