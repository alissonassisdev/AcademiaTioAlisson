using Foundation;

namespace AcademiaTioAlisson.Presentation.AppMaui
{
    [Register("AppDelegate")]
    public class AppDelegate : MauiUIApplicationDelegate
    {
        protected override MauiApp CreateMauiApp() => TemaPreferencesUpdatedMessage.CreateMauiApp();
    }
}
