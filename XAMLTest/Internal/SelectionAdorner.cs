using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace XamlTest.Internal;

internal class SelectionAdorner : Adorner, IDisposable
{
    static SelectionAdorner()
    {
        IsHitTestVisibleProperty.OverrideMetadata(typeof(SelectionAdorner), new UIPropertyMetadata(false));
        UseLayoutRoundingProperty.OverrideMetadata(typeof(SelectionAdorner), new FrameworkPropertyMetadata(true));
    }

    public SelectionAdorner(UIElement adornedElement)
        : base(adornedElement)
    {
        
    }

    public Brush Brush { get; set; } = new SolidColorBrush(Colors.Red);

    public AdornerLayer? AdornerLayer { get; set; }

    protected override void OnRender(DrawingContext drawingContext)
    {
        if (ActualWidth <= 0 || ActualHeight <= 0)
        {
            return;
        }

        var pen = new Pen(Brush, 5);

        drawingContext.DrawRectangle(null, pen, new Rect(0, 0, ActualWidth, ActualHeight));
    }

    public void Dispose()
    {
        AdornerLayer?.Remove(this);
    }
}
