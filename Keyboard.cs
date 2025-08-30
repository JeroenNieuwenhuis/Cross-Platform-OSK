using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.VisualTree;
using Newtonsoft.Json;
using Typo;

namespace Typo;

[JsonObject(MemberSerialization.OptIn)]
public class Keyboard : Window
{
    [JsonProperty]
    public string name { get; set; } = "Default";
    [JsonProperty]
    public int transparency { get; set; } = 0;
    [JsonProperty]
    public double _width { get; set; } = 523;
    [JsonProperty]
    public double _height { get; set; } = 145;
    [JsonProperty]
    public List<Key> keys { get; set; } = new List<Key>();
    [JsonProperty]
    public string? backgroundColor { get; set; } 
    
    // These variables set the Key variables if they're not set
    [JsonProperty] public string? keyTextColor { get; set; }
    [JsonProperty] public string? keyFont { get; set; }
    [JsonProperty] public int? keyFontSize { get; set; }
    [JsonProperty] public string? keyArgbBorderColor { get; set; }
    [JsonProperty] public string? keyArgbBackgroundColor { get; set; }
    [JsonProperty] public string? keyArgbHoverColor { get; set; }
    [JsonProperty] public string? keyArgbHoverClickColor { get; set; }
    
    private Canvas _canvas;
    private Key? _currentlyHoveredKey = null;
    private Key? _currentlyLeftPressedKey = null;
    private Key? _currentlyRightPressedKey = null;
    private bool _leftPressed = false;
    private bool _rightPressed = false;
    
    public Keyboard()
    {
    }
    
    public void Initialize()
    {
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            // Hide window bar
            this.ExtendClientAreaToDecorationsHint = true;
            this.ExtendClientAreaChromeHints = Avalonia.Platform.ExtendClientAreaChromeHints.NoChrome;
            
            CanResize = false;
            _canvas = new Canvas();
            Content = _canvas;
            _canvas.Background = Brushes.Transparent;
            this.Width = _width;
            this.Height = _height;
            this.Title = "Typo: " + name;
            this.Focusable = false;

            IWindowManagerInterface windowManager = IWindowManagerInterface.GetInstance(this);
            //windowManager.SetAlwaysOnTop();
            windowManager.SetUnfocusable();
            
            this.TransparencyLevelHint = new List<WindowTransparencyLevel>
            {
                WindowTransparencyLevel.Transparent
            };
            this.Background = new SolidColorBrush(Color.Parse(backgroundColor ?? "#a0ffffff"));
            foreach (var key in keys)
            {
                if (keyTextColor != null && key.textColor == null) key.textColor = keyTextColor;
                if (keyFont != null && key.font == null) key.font = keyFont;
                if (keyFontSize != null && key.fontSize == null) key.fontSize = keyFontSize;
                if (keyArgbBorderColor != null && key.argbBorderColor == null) key.argbBorderColor = keyArgbBorderColor;
                if (keyArgbBackgroundColor != null && key.argbBackgroundColor == null) key.argbBackgroundColor = keyArgbBackgroundColor;
                if (keyArgbHoverColor != null && key.argbHoverColor == null) key.argbHoverColor = keyArgbHoverColor;
                if (keyArgbHoverClickColor != null && key.argbHoverClickColor == null) key.argbHoverClickColor = keyArgbHoverClickColor;
                
                key.Initialize(_canvas);
                _canvas.Children.Add(key);
            }
            
            
            _canvas.PointerMoved += Canvas_PointerMoved;
            _canvas.PointerPressed += Canvas_PointerPressed;
            _canvas.PointerReleased += Canvas_PointerReleased;
            _canvas.PointerExited += Canvas_PointerExited;
        });
    }

    private void PointerUpdate(Key? hitKey, bool leftPressed, bool rightPressed)
    {
        if (_leftPressed != leftPressed)
        {
            if (leftPressed)
            {
                hitKey?.KeyPressed();
                _currentlyLeftPressedKey = hitKey;
            }
            else
            {
                hitKey?.KeyReleased();
                _currentlyLeftPressedKey = null;
            }
            _leftPressed = leftPressed;
        }
        
        if (_currentlyLeftPressedKey != hitKey)
        {
            _currentlyLeftPressedKey?.KeyReleased();
            _currentlyLeftPressedKey = hitKey;
            if (leftPressed)
            {
                
                if (hitKey is { actionOnClick: false })
                {
                    hitKey?.KeyPressed();
                }
            }
        }
        
        if (_rightPressed != rightPressed)
        {
            if (rightPressed)
            {
                //hitKey?.KeyPressed();
                _currentlyRightPressedKey = hitKey;
            }
            else
            {
                //hitKey?.KeyReleased();
                _currentlyRightPressedKey = null;
            }
            _rightPressed = rightPressed;
        }

        if (_currentlyRightPressedKey != hitKey)
        {
            if (rightPressed)
            {
                //_currentlyRightPressedKey?.KeyReleased();
                if (_currentlyRightPressedKey is { actionOnClick: false })
                {
                    //_currentlyRightPressedKey.KeyPressed();
                }
            }
        }
        
        if (_currentlyHoveredKey != hitKey)
        {
            _currentlyHoveredKey?.KeyExited();
            
            hitKey?.KeyEntered();

            _currentlyHoveredKey = hitKey;
        }
    }

    private Key? GetKeyUnderPointer(PointerEventArgs e)
    {
        Point pointerPosition = e.GetPosition(_canvas);

        // InputHitTest directly gives you the IInputElement.
        // It considers IsHitTestVisible and Z-order.
        var hitElement = _canvas.GetVisualsAt(pointerPosition)
            .OfType<Key>()
            .FirstOrDefault();

        return hitElement;
    }

    private void Canvas_PointerMoved(object? sender, PointerEventArgs e)
    {
        bool leftPressed = e.GetCurrentPoint(_canvas).Properties.IsLeftButtonPressed;
        bool rightPressed = e.GetCurrentPoint(_canvas).Properties.IsLeftButtonPressed;
        
        Key? hitKey = GetKeyUnderPointer(e);

        PointerUpdate(hitKey, leftPressed, rightPressed);
    }

    private void Canvas_PointerExited(object? sender, PointerEventArgs e)
    {
        if (_currentlyHoveredKey != null)
        {
            _currentlyHoveredKey.KeyExited();
            _currentlyHoveredKey = null;
        }
    }
    
    private void Canvas_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        bool leftPressed = e.GetCurrentPoint(_canvas).Properties.IsLeftButtonPressed;
        bool rightPressed = e.GetCurrentPoint(_canvas).Properties.IsLeftButtonPressed;
        
        Key? hitKey = GetKeyUnderPointer(e);

        PointerUpdate(hitKey, leftPressed, rightPressed);
    }

    private void Canvas_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        bool leftPressed = e.GetCurrentPoint(_canvas).Properties.IsLeftButtonPressed;
        bool rightPressed = e.GetCurrentPoint(_canvas).Properties.IsLeftButtonPressed;
        
        Key? hitKey = GetKeyUnderPointer(e);

        PointerUpdate(hitKey, leftPressed, rightPressed);
    }
}