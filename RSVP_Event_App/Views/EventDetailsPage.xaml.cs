using RSVP_Event_App.ViewModels;

namespace RSVP_Event_App.Views;

public partial class EventDetailsPage : ContentPage
{
    private readonly EventDetailsViewModel _viewModel;

    public EventDetailsPage(EventDetailsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadAsync();
    }
}
