using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
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
            if (vertices is { Count: > 2 })
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

            if (!string.IsNullOrEmpty(imagePath))
            {
                var image = CreateImageControl(imagePath);
                if (image != null)
                {
                    _canvas?.Children.Add(image);
                }
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

    private Control? CreateImageControl(string imagePathValue)
    {
        if (vertices is not { Count: > 2 })
            return null;

        string resolvedPath = ResolveImagePath(imagePathValue);
        if (!File.Exists(resolvedPath))
            return null;

        Bitmap bitmap;
        try
        {
            bitmap = new Bitmap(resolvedPath);
        }
        catch
        {
            return null;
        }

        Point topLeft = GetTopLeft();
        Point bottomRight = GetBottomRight();
        double boundsWidth = Math.Max(1, bottomRight.X - topLeft.X);
        double boundsHeight = Math.Max(1, bottomRight.Y - topLeft.Y);
        double horizontalPadding = Math.Min(4, boundsWidth * 0.15);
        double verticalPadding = Math.Min(4, boundsHeight * 0.15);
        double imageWidth = Math.Max(1, boundsWidth - (horizontalPadding * 2));
        double imageHeight = Math.Max(1, boundsHeight - (verticalPadding * 2));
        var image = new Image
        {
            Source = bitmap,
            Width = imageWidth,
            Height = imageHeight,
            Stretch = Stretch.Uniform,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            IsHitTestVisible = false
        };

        var container = new Grid
        {
            Width = boundsWidth,
            Height = boundsHeight,
            IsHitTestVisible = false,
            Clip = CreateClipGeometry(topLeft)
        };

        container.Children.Add(image);
        Canvas.SetLeft(container, topLeft.X);
        Canvas.SetTop(container, topLeft.Y);
        container.ZIndex = 1;

        return container;
    }

    private string ResolveImagePath(string imagePathValue)
    {
        if (System.IO.Path.IsPathRooted(imagePathValue))
            return imagePathValue;

        return System.IO.Path.GetFullPath(System.IO.Path.Combine(AppContext.BaseDirectory, imagePathValue));
    }

    private Geometry CreateClipGeometry(Point topLeft)
    {
        string pathData = string.Join(" ",
            vertices!.Select((point, index) =>
                $"{(index == 0 ? "M" : "L")} {point.X - topLeft.X},{point.Y - topLeft.Y}")) + " Z";

        return Geometry.Parse(pathData);
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
            ActionCoordinator.GetInstance().NotifyActionCompleted(clickAction);
        }
        else
        {
            clickAction?.Start();
            DispatcherTimer.RunOnce(() =>
            {
                clickAction?.Stop();
                ActionCoordinator.GetInstance().NotifyActionCompleted(clickAction);
            }, TimeSpan.FromMilliseconds(25));
        }
    }

    public virtual void KeyRightPressed()
    {
        _isPressed = true;
        UpdateColor();
        
        if (actionOnClick == true)
        {
            rightClickAction?.Start();
        }
    }

    public virtual void KeyRightReleased()
    {
        _isPressed = false;
        UpdateColor();
        if (actionOnClick == true)
        {
            rightClickAction?.Stop();
            ActionCoordinator.GetInstance().NotifyActionCompleted(rightClickAction);
        }
        else
        {
            rightClickAction?.Start();
            DispatcherTimer.RunOnce(() =>
            {
                rightClickAction?.Stop();
                ActionCoordinator.GetInstance().NotifyActionCompleted(rightClickAction);
            }, TimeSpan.FromMilliseconds(25));
        }
    }
}
