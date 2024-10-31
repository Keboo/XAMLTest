using System.Windows.Controls;
using XamlTest;
using XamlTest.Tests;

[assembly: GenerateHelpers(typeof(IntButton))]
[assembly: GenerateHelpers(typeof(DecimalButton))]

namespace XamlTest.Tests;

public class GenericBase<T> : Button
{
    public T SomeValue
    {
        get => (T)GetValue(SomeValueProperty);
        set => SetValue(SomeValueProperty, value);
    }

    // Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty SomeValueProperty =
        DependencyProperty.Register("SomeValue", typeof(T), typeof(GenericBase<>), new PropertyMetadata(default(T)));
}

public class IntButton : GenericBase<int>;

public class DecimalButton : GenericBase<decimal>;

[TestClass]
public class GeneratorTests
{
    [TestMethod]
    [Ignore("This test is used to verify that the generator is working correctly; so we only need to compile")]
    public void CanAccessGeneratedGenericBaseClassExtensions()
    {
        IVisualElement<IntButton> intButton = default!;
        IVisualElement<IntButton> decimalButton = default!;

        _ = intButton.GetSomeValue();
        _ = decimalButton.GetSomeValue();
    }
}
