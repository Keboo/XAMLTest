using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Windows;
using Expression = System.Linq.Expressions.Expression;

namespace XamlTest
{
    internal static class DependencyPropertyHelper
    {
        private static Func<string, Type, DependencyProperty> FromName { get; }

        static DependencyPropertyHelper()
        {
            MethodInfo fromNameMethod = typeof(DependencyProperty).GetMethod("FromName", BindingFlags.Static | BindingFlags.NonPublic)
                ?? throw new InvalidOperationException($"Failed to find FromName method");

            var nameParameter = Expression.Parameter(typeof(string));
            var ownerTypeParameter = Expression.Parameter(typeof(Type));
            var call = Expression.Call(fromNameMethod, nameParameter, ownerTypeParameter);

            var fromName = Expression.Lambda<Func<string, Type, DependencyProperty>>(call, nameParameter, ownerTypeParameter);
            FromName = fromName.Compile();
        }

        private static DependencyProperty? Find(string name, Type ownerType)
            => FromName(name, ownerType);

        public static bool TryGetDependencyProperty(string name, string? ownerType,
            [NotNullWhen(true)]out DependencyProperty? dependencyProperty)
        {
            Type? type = Type.GetType(ownerType);
            if (type is null)
            {
                dependencyProperty = null;
                return false;
            }

            dependencyProperty = Find(name, type);
            return dependencyProperty != null;
        }
    }
}
