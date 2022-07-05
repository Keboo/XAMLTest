using System;
using System.Linq.Expressions;
using XamlTest.Query;

namespace XamlTest;

public static class QueryMixins
{
    public static IQuery<T> ChildOfType<T>(this IQuery query)
        => AppendQuery<T>(query, ElementQuery.TypeQueryString<T>());

    public static IQuery<T> ChildWithName<T>(this IQuery query, string name)
        => AppendQuery<T>(query, ElementQuery.NameQuery(name));

    public static IQuery<T> Property<T>(this IQuery query, string propertyName)
        => AppendQuery<T>(query, ElementQuery.PropertyQuery(propertyName));

    public static IQuery<TProperty> Property<TElement, TProperty>(this IQuery query, Expression<Func<TElement, TProperty>> propertyExpression)
        => Property<TProperty>(query, ElementQuery.GetPropertyName(propertyExpression));

    public static IQuery<T> PropertyExpression<T>(this IQuery query, string propertyName, object value)
        => AppendQuery<T>(query, ElementQuery.PropertyExpressionQuery(propertyName, value));

    public static IQuery<T> PropertyExpression<T>(this IQuery query, Expression<Func<T, object>> propertyExpression, object value)
        => PropertyExpression<T>(query, ElementQuery.GetPropertyName(propertyExpression), value);

    public static IQuery<T> AtIndex<T>(this IQuery<T> query, int index)
        => AppendQuery<T>(query, ElementQuery.IndexQuery(index));


    private static IQuery<T> AppendQuery<T>(IQuery query, string newQueryPart)
    {
        if (query is not IQuery<T> typedQuery)
        {
            typedQuery = new StringBuilderQuery<T>(query.QueryString);
        }
        typedQuery.Add(newQueryPart);
        return typedQuery;
    }
}