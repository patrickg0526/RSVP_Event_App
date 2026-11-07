using RSVP_Event_App.ViewModels;

namespace RSVP_Event_App.Views;

public partial class AddEventPage : ContentPage
{
    public AddEventPage(AddEventViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
