using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

namespace Typo;

public partial class App : Application
{
    private Settings settings;
    
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            settings = Settings.LoadFromFile(Settings.GetSettingsPath());
            desktop.MainWindow = settings.layouts[0].keyboards[0];
            
            desktop.Exit += OnAppExit;
        }
        
        base.OnFrameworkInitializationCompleted();
    }
    
    

    private void OnAppExit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
    {
        //settings.SaveToFile(Settings.GetSettingsPath());
        // Perform cleanup or save state here
    }
}