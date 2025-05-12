using Firebase.Auth;
using Firebase.Auth.Providers;
using Firebase.Auth.Repository;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.LifecycleEvents;
using PeriodCounter.Classes;
using Plugin.Firebase.Core.Platforms.Android;
using Plugin.Firebase.CloudMessaging;

namespace PeriodCounter
{
    public static class MauiProgram
    {
        private static MauiAppBuilder RegisterFirebaseServices(this MauiAppBuilder builder)
        {
            builder.ConfigureLifecycleEvents(events =>
            {
                events.AddAndroid(android => android.OnCreate((activity, _) =>
                CrossFirebase.Initialize(activity)));
            });

#if Debug
            AppDomain.CurrentDomain.FirstChanceException += (s, e) =>
            {
                System.Diagnostics.Debug.WriteLine("********** OMG! FirstChanceException **********");
                System.Diagnostics.Debug.WriteLine(e.Exception);
            };
#endif
            return builder;
        }

        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .RegisterFirebaseServices()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddMauiBlazorWebView();
            builder.Services.AddTransient<PeriodAPI>();
            builder.Services.AddSingleton(new FirebaseAuthClient(new FirebaseAuthConfig()
            {
                ApiKey = "AIzaSyDXOBTE0Jlx-8ysuzAh7Gkx9gSZwAIc2No",
                AuthDomain = "p-tracker-f9e4d.firebaseapp.com",
                Providers =
                [
                    new EmailProvider()
                ],
                UserRepository = new FileUserRepository("LoginState")
            }));
#if DEBUG
    		builder.Services.AddBlazorWebViewDeveloperTools();
    		builder.Logging.AddDebug();
#endif
            return builder.Build();
        }
    }
}
