using RSVP_Event_App.Views;

namespace RSVP_Event_App;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();

		// Pages reached by navigation from the two top-level routes
		Routing.RegisterRoute(nameof(CreateAccountPage), typeof(CreateAccountPage));
		Routing.RegisterRoute(nameof(EventDetailsPage), typeof(EventDetailsPage));
		Routing.RegisterRoute(nameof(AddEventPage), typeof(AddEventPage));
		Routing.RegisterRoute(nameof(RsvpPage), typeof(RsvpPage));
	}
}
