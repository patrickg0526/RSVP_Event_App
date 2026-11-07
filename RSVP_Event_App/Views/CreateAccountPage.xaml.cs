using RSVP_Event_App.ViewModels;

namespace RSVP_Event_App.Views;

public partial class CreateAccountPage : ContentPage
{
    public CreateAccountPage(CreateAccountViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
