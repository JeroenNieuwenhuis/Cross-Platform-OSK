using System;
using System.Collections.Generic;
using Avalonia.Threading;
using Newtonsoft.Json;
using Typo.Windows;

namespace Typo;

[JsonObject(MemberSerialization.OptIn)]
public class Layout
{
    [JsonProperty]
    public string name { get; set; } = "Default";
    [JsonProperty]
    public bool uiAccesDllInjection { get; set; } = false;
    [JsonProperty]
    public List<Keyboard> keyboards { get; set; } = new List<Keyboard>();
    

    public Layout()
    { 
    }
    
    public void Initialize()
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            foreach (var keyboard in keyboards)
            {
                keyboard.Initialize();
            } 
            #if _WINDOWS
            DispatcherTimer.RunOnce(() =>
            {
                List<string> keyboardNames = new List<string>();
                foreach (var keyboard in keyboards)
                {
                    if (keyboard.Title != null) keyboardNames.Add(keyboard.Title);
                }
                //Injector.InjectDllInto(Settings.GetDllInjectionPath(), "explorer.exe", keyboardNames);
            }, TimeSpan.FromMilliseconds(250));
            #endif
        });
        
        
        
    }
}