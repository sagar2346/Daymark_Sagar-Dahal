using DailyJournalApp.ViewModels;

namespace DailyJournalApp.Views
{
    public partial class LoginPage : ContentPage
    {
        public LoginPage(LoginViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }

        // Add a secondary way to toggle visibility if needed, but keeping it simple for now
        public bool IsShowingPassword { get; set; } = false;
    }
}
