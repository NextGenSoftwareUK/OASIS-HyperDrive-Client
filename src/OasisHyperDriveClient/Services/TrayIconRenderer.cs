using OasisHyperDriveClient.Core.Models;
using SkiaSharp;

namespace OasisHyperDriveClient.Services;

public static class TrayIconRenderer
{
    private static readonly Dictionary<TrayState, SKColor> StateColours = new()
    {
        [TrayState.Healthy]    = new SKColor(0,   255, 238),   // #00FFEE cyan
        [TrayState.Degraded]   = new SKColor(255, 215, 0),    // #FFD700 yellow
        [TrayState.Error]      = new SKColor(255, 51,  51),   // #FF3333 red
        [TrayState.Syncing]    = new SKColor(204, 68,  255),  // #CC44FF purple
        [TrayState.Busy]       = new SKColor(255, 136, 0),    // #FF8800 orange
        [TrayState.Connecting] = new SKColor(68,  136, 255),  // #4488FF blue
        [TrayState.Disabled]   = new SKColor(128, 128, 128),  // #808080 grey
    };

    public static Stream Render(TrayState state, int size = 32)
    {
        var colour = StateColours.GetValueOrDefault(state, SKColors.Gray);
        var info = new SKImageInfo(size, size, SKColorType.Rgba8888, SKAlphaType.Premul);

        using var surface = SKSurface.Create(info);
        var canvas = surface.Canvas;
        canvas.Clear(SKColors.Transparent);

        float cx = size / 2f;
        float cy = size / 2f;
        float r  = size * 0.38f;
        float stroke = size * 0.12f;

        // Outer glow (blur ring)
        if (state != TrayState.Disabled)
        {
            using var glowPaint = new SKPaint
            {
                Color = colour.WithAlpha(90),
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = stroke * 2.2f,
                MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, size * 0.14f)
            };
            canvas.DrawCircle(cx, cy, r, glowPaint);
        }

        // Main ring — the O
        using var ringPaint = new SKPaint
        {
            Color = colour,
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = stroke
        };
        canvas.DrawCircle(cx, cy, r, ringPaint);

        // Inner dot for Connecting / Syncing / Busy states
        if (state is TrayState.Connecting or TrayState.Syncing or TrayState.Busy)
        {
            using var dotPaint = new SKPaint
            {
                Color = colour.WithAlpha(200),
                IsAntialias = true,
                Style = SKPaintStyle.Fill
            };
            canvas.DrawCircle(cx, cy, size * 0.1f, dotPaint);
        }

        using var image = surface.Snapshot();
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        var ms = new MemoryStream();
        data.SaveTo(ms);
        ms.Position = 0;
        return ms;
    }
}
