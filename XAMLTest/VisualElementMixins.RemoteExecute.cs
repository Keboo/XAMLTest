namespace XamlTest;

public static partial class VisualElementMixins
{
    public static Task<TResult?> RemoteExecute<T, TResult>(this IVisualElement<T> element,
        Func<T, TResult> action)
    {
        return element.RemoteExecute<TResult>(action, Array.Empty<object?>());
    }

    public static Task<TResult?> RemoteExecute<T, T1, TResult>(this IVisualElement<T> element,
        Func<T, T1, TResult> action, T1 param1)
    {
        return element.RemoteExecute<TResult>(action, new object?[] { param1 });
    }

    public static Task<TResult?> RemoteExecute<T, T1, T2, TResult>(this IVisualElement<T> element,
        Func<T, T1, T2, TResult> action, T1 param1, T2 param2)
    {
        return element.RemoteExecute<TResult>(action, new object?[] { param1, param2 });
    }

    public static Task<TResult?> RemoteExecute<T, T1, T2, T3, TResult>(this IVisualElement<T> element,
        Func<T, T1, T2, T3, TResult> action, T1 param1, T2 param2, T3 param3)
    {
        return element.RemoteExecute<TResult>(action, new object?[] { param1, param2, param3 });
    }

    public static Task<TResult?> RemoteExecute<T, T1, T2, T3, T4, TResult>(this IVisualElement<T> element,
        Func<T, T1, T2, T3, T4, TResult> action, T1 param1, T2 param2, T3 param3, T4 param4)
    {
        return element.RemoteExecute<TResult>(action, new object?[] { param1, param2, param3, param4 });
    }

    public static Task<TResult?> RemoteExecute<T, T1, T2, T3, T4, T5, TResult>(this IVisualElement<T> element,
        Func<T, T1, T2, T3, T4, T5, TResult> action, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5)
    {
        return element.RemoteExecute<TResult>(action, new object?[] { param1, param2, param3, param4, param5 });
    }

    public static Task<TResult?> RemoteExecute<T, T1, T2, T3, T4, T5, T6, TResult>(this IVisualElement<T> element,
        Func<T, T1, T2, T3, T4, T5, T6, TResult> action, 
        T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6)
    {
        return element.RemoteExecute<TResult>(action, new object?[] { param1, param2, param3, param4, param5, param6 });
    }

    public static Task<TResult?> RemoteExecute<T, T1, T2, T3, T4, T5, T6, T7, TResult>(this IVisualElement<T> element,
        Func<T, T1, T2, T3, T4, T5, T6, T7, TResult> action,
        T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7)
    {
        return element.RemoteExecute<TResult>(action, new object?[] { param1, param2, param3, param4, param5, param6, param7 });
    }

    public static Task<TResult?> RemoteExecute<T, T1, T2, T3, T4, T5, T6, T7, T8, TResult>(this IVisualElement<T> element,
        Func<T, T1, T2, T3, T4, T5, T6, T7, T8, TResult> action,
        T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7, T8 param8)
    {
        return element.RemoteExecute<TResult>(action, new object?[] { param1, param2, param3, param4, param5, param6, param7, param8 });
    }

    public static Task RemoteExecute<T>(this IVisualElement<T> element, Action<T> action)
    {
        return element.RemoteExecute<object?>(action, Array.Empty<object?>());
    }

    public static Task RemoteExecute<T, T1>(this IVisualElement<T> element,
        Action<T, T1> action, T1 param1)
    {
        return element.RemoteExecute<object?>(action, new object?[] { param1 });
    }

    public static Task RemoteExecute<T, T1, T2>(this IVisualElement<T> element, 
        Action<T, T1, T2> action, T1 param1, T2 param2)
    {
        return element.RemoteExecute<object?>(action, new object?[] { param1, param2 });
    }

    public static Task RemoteExecute<T, T1, T2, T3>(this IVisualElement<T> element,
        Action<T, T1, T2, T3> action, T1 param1, T2 param2, T3 param3)
    {
        return element.RemoteExecute<object?>(action, new object?[] { param1, param2, param3 });
    }

    public static Task RemoteExecute<T, T1, T2, T3, T4>(this IVisualElement<T> element,
        Action<T, T1, T2, T3, T4> action, T1 param1, T2 param2, T3 param3, T4 param4)
    {
        return element.RemoteExecute<object?>(action, new object?[] { param1, param2, param3, param4 });
    }

    public static Task RemoteExecute<T, T1, T2, T3, T4, T5>(this IVisualElement<T> element,
        Action<T, T1, T2, T3, T4, T5> action, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5)
    {
        return element.RemoteExecute<object?>(action, new object?[] { param1, param2, param3, param4, param5 });
    }

    public static Task RemoteExecute<T, T1, T2, T3, T4, T5, T6>(this IVisualElement<T> element,
        Action<T, T1, T2, T3, T4, T5, T6> action, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6)
    {
        return element.RemoteExecute<object?>(action, new object?[] { param1, param2, param3, param4, param5, param6 });
    }

    public static Task RemoteExecute<T, T1, T2, T3, T4, T5, T6, T7>(this IVisualElement<T> element,
        Action<T, T1, T2, T3, T4, T5, T6, T7> action, 
        T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7)
    {
        return element.RemoteExecute<object?>(action, new object?[] { param1, param2, param3, param4, param5, param6, param7 });
    }

    public static Task RemoteExecute<T, T1, T2, T3, T4, T5, T6, T7, T8>(this IVisualElement<T> element,
        Action<T, T1, T2, T3, T4, T5, T6, T7, T8> action,
        T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7, T8 param8)
    {
        return element.RemoteExecute<object?>(action, new object?[] { param1, param2, param3, param4, param5, param6, param7, param8 });
    }
}
