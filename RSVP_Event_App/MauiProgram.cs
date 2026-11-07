using Microsoft.Extensions.Logging;
using RSVP_Event_App.Data;
using RSVP_Event_App.ViewModels;
using RSVP_Event_App.Views;

namespace RSVP_Event_App;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		// Data access layer (web service client)
		builder.Services.AddSingleton<RsvpDatabase>();

		// View Models and Views, wired together through dependency injection
		builder.Services.AddTransient<LoginViewModel>();
		builder.Services.AddTransient<CreateAccountViewModel>();
		builder.Services.AddTransient<EventsViewModel>();
		builder.Services.AddTransient<EventDetailsViewModel>();
		builder.Services.AddTransient<AddEventViewModel>();
		builder.Services.AddTransient<RsvpViewModel>();

		builder.Services.AddTransient<LoginPage>();
		builder.Services.AddTransient<CreateAccountPage>();
		builder.Services.AddTransient<EventsPage>();
		builder.Services.AddTransient<EventDetailsPage>();
		builder.Services.AddTransient<AddEventPage>();
		builder.Services.AddTransient<RsvpPage>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
