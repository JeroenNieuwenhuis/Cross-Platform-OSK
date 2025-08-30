using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Avalonia.Threading;
using Newtonsoft.Json;

namespace Typo;

[JsonObject(MemberSerialization.OptIn)]
public class Key : Polygon
{
 
    
    [JsonProperty]
    public IAction? clickAction { get; set; }
    [JsonProperty]
    public IAction? longClickAction { get; set; }
    [JsonProperty]
    public IAction? rightClickAction { get; set; }
    [JsonProperty]
    public IAction? hoverClickAction { get; set; }
    [JsonProperty] 
    public bool? actionOnClick { get; set; } = true;
    
    [JsonProperty]
    public int? hoverDuration { get; set; }
    
    [JsonProperty]
    public List<Point>? vertices { get; set; }
    
    [JsonProperty]
    public int? centerOffsetX { get; set; }
    [JsonProperty]
    public int? centerOffsetY { get; set; }
    
    [JsonProperty]
    public string? text { get; set; }
    [JsonProperty]
    public string? textColor { get; set; }
    [JsonProperty]
    public string? font { get; set; }
    [JsonProperty]
    public int? fontSize { get; set; }
    [JsonProperty]
    public string? imagePath { get; set; }
    [JsonProperty]
    public string? argbBorderColor { get; set; }
    [JsonProperty]
    public string? argbBackgroundColor { get; set; }
    [JsonProperty]
    public string? argbHoverColor { get; set; }
    [JsonProperty]
    public string? argbHoverClickColor { get; set; }

    private Canvas? _canvas;
    private IBrush? _previousColor;
    private bool _isHovered = false;
    private bool _isPressed = false;
    
    public void Initialize(Canvas canvas)
    {
        Avalonia.Threading.Dispatcher.UIThread.Invoke(() =>
        {
            _canvas = canvas;
            _canvas.
            IsHitTestVisible = true;
            
            // Ensure Points are synced with Vertices after deserialization
            if (vertices.Count > 2)
            {
                Points = new Points(vertices);
            }

            // Initialize colors, fonts, etc.
            if (!string.IsNullOrEmpty(argbBorderColor))
            {
                Stroke = new SolidColorBrush(Color.Parse(argbBorderColor));
                StrokeThickness = 1;
            }

            if (!string.IsNullOrEmpty(argbBackgroundColor))
            {
                Fill = new SolidColorBrush(Color.Parse(argbBackgroundColor));
            }

            if (!string.IsNullOrEmpty(text))
            {
                var textBlock = new TextBlock
                {
                    Text = text,
                    FontSize = fontSize ?? 12,
                    FontFamily = new FontFamily(font ?? "Arial"),
                    Foreground = new SolidColorBrush(Color.Parse(textColor ?? "#ffffffff")),
                };
                textBlock.Measure(Size.Infinity);
                textBlock.Arrange(new Rect(textBlock.DesiredSize));
                
                Point topLeft = GetTopLeft();
                Point bottomRight = GetBottomRight();
                int offsetX = centerOffsetX ?? (int)((bottomRight.X - topLeft.X) / 2);
                int offsetY = centerOffsetY ?? (int)((bottomRight.Y - topLeft.Y) / 2);;

                Canvas.SetLeft(textBlock, (topLeft.X + offsetX - (textBlock.DesiredSize.Width / 2)));
                Canvas.SetTop(textBlock, (topLeft.Y + offsetY - (textBlock.DesiredSize.Height / 2)));
                textBlock.ZIndex = 1;
                _canvas?.Children.Add(textBlock);

            }
        });
    }

    public void UpdateColor()
    {
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            if (_isPressed)
                Fill = new SolidColorBrush(Color.Parse(argbHoverClickColor ?? "#ff4545ff"));
            else if (_isHovered)
                Fill = new SolidColorBrush(Color.Parse(argbHoverColor ?? "#ff4545ff"));
            else
                Fill = new SolidColorBrush(Color.Parse(argbBackgroundColor ?? "#ff4545ff"));
        });
    }
    public Point GetTopLeft()
    {
        if (vertices == null || vertices.Count == 0)
            throw new InvalidOperationException("No vertices found.");

        double minX = vertices.Min(p => p.X);
        double minY = vertices.Min(p => p.Y);
        return new Point(minX, minY); 
    }
    
    public Point GetBottomRight()
    {
        if (vertices == null || vertices.Count == 0)
            throw new InvalidOperationException("No vertices found.");

        double maxX = vertices.Max(p => p.X);
        double maxY = vertices.Max(p => p.Y);
        return new Point(maxX, maxY); 
    }
    
    public virtual void KeyEntered()
    {
        _isHovered = true;
        UpdateColor();
        
    }

    public virtual void KeyExited()
    {
        _isHovered = false;
        UpdateColor();
    }

    public virtual void KeyPressed()
    {
        _isPressed = true;
        UpdateColor();
        
        if (actionOnClick == true)
        {
            clickAction?.Start();
        }
    }

    public virtual void KeyReleased()
    {
        _isPressed = false;
        UpdateColor();
        if (actionOnClick == true)
        {
            clickAction?.Stop();
        }
        else
        {
            clickAction?.Start();
            DispatcherTimer.RunOnce(() =>
            {
                clickAction?.Stop();
            }, TimeSpan.FromMilliseconds(25));
        }
    }
}