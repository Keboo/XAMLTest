using System.Linq.Expressions;
using XamlTest.Query;

namespace XamlTest;

public static class ElementQuery
{
    public static IQuery<T> OfType<T>()
        => new StringBuilderQuery<T>(TypeQueryString<T>());

    public static IQuery<T> WithName<T>(string name)
        => new StringBuilderQuery<T>(NameQuery(name));

    public static IQuery<T> Property<T>(string propertyName)
        => new StringBuilderQuery<T>(PropertyQuery(propertyName));

    public static IQuery<TProperty> Property<TElement, TProperty>(Expression<Func<TElement, TProperty>> propertyExpression)
        => Property<TProperty>(GetPropertyName(propertyExpression));

    public static IQuery<T> PropertyExpression<T>(string propertyName, object value)
        => new StringBuilderQuery<T>(PropertyExpressionQuery(propertyName, value));

    public static IQuery<T> PropertyExpression<T>(Expression<Func<T, object>> propertyExpression, object value)
        => PropertyExpression<T>(GetPropertyName(propertyExpression), value);


    internal static string TypeQueryString<T>() => $"/{typeof(T).Name}";
    internal static string NameQuery(string name) => $"~{name}";
    internal static string PropertyQuery(string propertyName) => $".{propertyName}";
    internal static string PropertyExpressionQuery(string propertyName, object value) => $"[{propertyName}={value}]";
    internal static string IndexQuery(int index) => $"[{index}]";

    internal static string GetPropertyName<TSource, TProperty>(
        Expression<Func<TSource, TProperty>> propertyExpression)
    {
        MemberExpression? member = propertyExpression.Body as MemberExpression;
        if (member is null)
            throw new ArgumentException($"Expression '{propertyExpression}' refers to a method, not a property.", nameof(propertyExpression));

        PropertyInfo? propInfo = member.Member as PropertyInfo;
        if (propInfo is null)
            throw new ArgumentException($"Expression '{propertyExpression}' does not refer to a property.", nameof(propertyExpression));

        return propInfo.Name;
    }
}
