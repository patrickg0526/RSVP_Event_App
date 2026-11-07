using RSVP_Event_App.ViewModels;

namespace RSVP_Event_App.Views;

public partial class RsvpPage : ContentPage
{
    public RsvpPage(RsvpViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
