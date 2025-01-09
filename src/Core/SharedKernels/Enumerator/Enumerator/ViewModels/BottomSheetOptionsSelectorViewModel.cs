using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MvvmCross.Commands;
using MvvmCross.ViewModels;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Core.SharedKernels.Enumerator.ViewModels;

public class BottomSheetOptionsSelectorViewModel: MvxViewModel<BottomSheetOptionsSelectorViewModelArgs>, IDisposable
{
    private BottomSheetOption[] options;
    private string title;

    public Func<BottomSheetOption, Task> Callback { get; set; }

    public string Title
    {
        get => title;
        set => SetProperty(ref title, value);
    }

    public BottomSheetOption[] Options
    {
        get => options;
        set => SetProperty(ref options, value);
    }

    public IMvxAsyncCommand<BottomSheetOption> SelectOptionCommand => 
        new MvxAsyncCommand<BottomSheetOption>(OnOptionSelected);
    
    private async Task OnOptionSelected(BottomSheetOption option)
    {
        if (option.IsSelected)
        {
            if (!SelectionRequired)
            {
                option.IsSelected = false;

                if (Callback != null)
                    await Callback.Invoke(null);
            }
        }
        else
        {
            Options.ForEach(o => o.IsSelected = false);
            option.IsSelected = true;

            if (Callback != null)
                await Callback.Invoke(option);
        }
    }

    public override void Prepare(BottomSheetOptionsSelectorViewModelArgs parameter)
    {
        this.Options = parameter.Options;
        this.Title = parameter.Title;
        this.Callback = parameter.Callback;
        this.SelectionRequired = parameter.SelectionRequired;
    }

    private bool SelectionRequired { get; set; }

    public void Dispose()
    {
        Callback = null;
    }
}

public class BottomSheetOptionsSelectorViewModelArgs
{
    public string Title { get; set; }
    public BottomSheetOption[] Options { get; set; }
    public Func<BottomSheetOption, Task> Callback { get; set; }
    public bool SelectionRequired { get; set; } = true;
}

public class BottomSheetOption : MvxViewModel
{
    public string Name { get; set; }
    public string Value { get; set; }
    

    private bool isSelected;
    public bool IsSelected
    {
        get => isSelected;
        set
        {
            if (value == isSelected) return;
            isSelected = value;
            RaisePropertyChanged(() => IsSelected);
        }
    }
}
