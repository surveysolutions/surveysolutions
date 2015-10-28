using System.Threading.Tasks;
using Cirrious.MvvmCross.ViewModels;

namespace WB.Core.SharedKernels.Enumerator.Services
{
    public interface IViewModelNavigationService
    {
        Task NavigateToAsync<TViewModel>() where TViewModel : IMvxViewModel;
        Task NavigateToAsync<TViewModel>(object perameters) where TViewModel : IMvxViewModel;
        Task NavigateToDashboardAsync();
        Task NavigateToInterviewAsync(string interviewId);
        Task NavigateToPrefilledQuestionsAsync(string interviewId);
    }
}