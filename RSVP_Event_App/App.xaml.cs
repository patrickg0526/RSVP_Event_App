using RSVP_Event_App.Data;

namespace RSVP_Event_App;

public partial class App : Application
{
	// Single shared instance of the data access layer used by every page
	public static RsvpDatabase Database { get; } = new();

	public App()
	{
		InitializeComponent();
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		return new Window(new AppShell());
	}
}
