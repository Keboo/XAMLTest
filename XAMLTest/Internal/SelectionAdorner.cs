namespace XamlTest.Internal;

#if WPF
internal class SelectionAdorner : Adorner, IDisposable
{
    static SelectionAdorner()
    {
        IsHitTestVisibleProperty.OverrideMetadata(typeof(SelectionAdorner), new UIPropertyMetadata(false));
        UseLayoutRoundingProperty.OverrideMetadata(typeof(SelectionAdorner), new FrameworkPropertyMetadata(true));
    }

    public SelectionAdorner(UIElement adornedElement)
        : base(adornedElement)
    { }

    public Brush? BorderBrush { get; set; }
    public double? BorderThickness { get; set; }
    public Brush? OverlayBrush { get; set; }

    public AdornerLayer? AdornerLayer { get; set; }

    protected override void OnRender(DrawingContext drawingContext)
    {
        if (ActualWidth <= 0 || ActualHeight <= 0)
        {
            return;
        }
        Pen? pen = null;
        if (BorderBrush is { } borderBrush && 
            BorderThickness is { } borderThickness)
        {
            pen = new Pen(borderBrush, borderThickness);
        }

        drawingContext.DrawRectangle(OverlayBrush, pen, new Rect(0, 0, ActualWidth, ActualHeight));
    }

    public void Dispose()
    {
        AdornerLayer?.Remove(this);
    }
}
#endif
