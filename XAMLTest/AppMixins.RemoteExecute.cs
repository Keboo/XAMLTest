namespace XamlTest;

public static partial class AppMixins
{
    /// <summary>
    /// Executes a function remotely on the app and returns a result.
    /// </summary>
    /// <typeparam name="T">The type of the target object.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="app">The app instance.</param>
    /// <param name="action">The function to execute.</param>
    /// <returns>A task representing the asynchronous operation, with the result.</returns>
    public static Task<TResult?> RemoteExecute<T, TResult>(this IApp app,
        Func<T, TResult> action)
    {
        return app.RemoteExecute<TResult>(action, []);
    }

    /// <summary>
    /// Executes a function remotely on the app with one parameter and returns a result.
    /// </summary>
    /// <typeparam name="T">The type of the target object.</typeparam>
    /// <typeparam name="T1">The type of the first parameter.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="app">The app instance.</param>
    /// <param name="action">The function to execute.</param>
    /// <param name="param1">The first parameter.</param>
    /// <returns>A task representing the asynchronous operation, with the result.</returns>
    public static Task<TResult?> RemoteExecute<T, T1, TResult>(this IApp app,
        Func<T, T1, TResult> action, T1 param1)
    {
        return app.RemoteExecute<TResult>(action, [param1]);
    }

    /// <summary>
    /// Executes a function remotely on the app with two parameters and returns a result.
    /// </summary>
    /// <typeparam name="T">The type of the target object.</typeparam>
    /// <typeparam name="T1">The type of the first parameter.</typeparam>
    /// <typeparam name="T2">The type of the second parameter.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="app">The app instance.</param>
    /// <param name="action">The function to execute.</param>
    /// <param name="param1">The first parameter.</param>
    /// <param name="param2">The second parameter.</param>
    /// <returns>A task representing the asynchronous operation, with the result.</returns>
    public static Task<TResult?> RemoteExecute<T, T1, T2, TResult>(this IApp app,
        Func<T, T1, T2, TResult> action, T1 param1, T2 param2)
    {
        return app.RemoteExecute<TResult>(action, [param1, param2]);
    }

    /// <summary>
    /// Executes a function remotely on the app with three parameters and returns a result.
    /// </summary>
    /// <typeparam name="T">The type of the target object.</typeparam>
    /// <typeparam name="T1">The type of the first parameter.</typeparam>
    /// <typeparam name="T2">The type of the second parameter.</typeparam>
    /// <typeparam name="T3">The type of the third parameter.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="app">The app instance.</param>
    /// <param name="action">The function to execute.</param>
    /// <param name="param1">The first parameter.</param>
    /// <param name="param2">The second parameter.</param>
    /// <param name="param3">The third parameter.</param>
    /// <returns>A task representing the asynchronous operation, with the result.</returns>
    public static Task<TResult?> RemoteExecute<T, T1, T2, T3, TResult>(this IApp app,
        Func<T, T1, T2, T3, TResult> action, T1 param1, T2 param2, T3 param3)
    {
        return app.RemoteExecute<TResult>(action, [param1, param2, param3]);
    }

    /// <summary>
    /// Executes a function remotely on the app with four parameters and returns a result.
    /// </summary>
    /// <typeparam name="T">The type of the target object.</typeparam>
    /// <typeparam name="T1">The type of the first parameter.</typeparam>
    /// <typeparam name="T2">The type of the second parameter.</typeparam>
    /// <typeparam name="T3">The type of the third parameter.</typeparam>
    /// <typeparam name="T4">The type of the fourth parameter.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="app">The app instance.</param>
    /// <param name="action">The function to execute.</param>
    /// <param name="param1">The first parameter.</param>
    /// <param name="param2">The second parameter.</param>
    /// <param name="param3">The third parameter.</param>
    /// <param name="param4">The fourth parameter.</param>
    /// <returns>A task representing the asynchronous operation, with the result.</returns>
    public static Task<TResult?> RemoteExecute<T, T1, T2, T3, T4, TResult>(this IApp app,
        Func<T, T1, T2, T3, T4, TResult> action, T1 param1, T2 param2, T3 param3, T4 param4)
    {
        return app.RemoteExecute<TResult>(action, [param1, param2, param3, param4]);
    }

    /// <summary>
    /// Executes a function remotely on the app with five parameters and returns a result.
    /// </summary>
    /// <typeparam name="T">The type of the target object.</typeparam>
    /// <typeparam name="T1">The type of the first parameter.</typeparam>
    /// <typeparam name="T2">The type of the second parameter.</typeparam>
    /// <typeparam name="T3">The type of the third parameter.</typeparam>
    /// <typeparam name="T4">The type of the fourth parameter.</typeparam>
    /// <typeparam name="T5">The type of the fifth parameter.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="app">The app instance.</param>
    /// <param name="action">The function to execute.</param>
    /// <param name="param1">The first parameter.</param>
    /// <param name="param2">The second parameter.</param>
    /// <param name="param3">The third parameter.</param>
    /// <param name="param4">The fourth parameter.</param>
    /// <param name="param5">The fifth parameter.</param>
    /// <returns>A task representing the asynchronous operation, with the result.</returns>
    public static Task<TResult?> RemoteExecute<T, T1, T2, T3, T4, T5, TResult>(this IApp app,
        Func<T, T1, T2, T3, T4, T5, TResult> action, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5)
    {
        return app.RemoteExecute<TResult>(action, [param1, param2, param3, param4, param5]);
    }

    /// <summary>
    /// Executes a function remotely on the app with six parameters and returns a result.
    /// </summary>
    /// <typeparam name="T">The type of the target object.</typeparam>
    /// <typeparam name="T1">The type of the first parameter.</typeparam>
    /// <typeparam name="T2">The type of the second parameter.</typeparam>
    /// <typeparam name="T3">The type of the third parameter.</typeparam>
    /// <typeparam name="T4">The type of the fourth parameter.</typeparam>
    /// <typeparam name="T5">The type of the fifth parameter.</typeparam>
    /// <typeparam name="T6">The type of the sixth parameter.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="app">The app instance.</param>
    /// <param name="action">The function to execute.</param>
    /// <param name="param1">The first parameter.</param>
    /// <param name="param2">The second parameter.</param>
    /// <param name="param3">The third parameter.</param>
    /// <param name="param4">The fourth parameter.</param>
    /// <param name="param5">The fifth parameter.</param>
    /// <param name="param6">The sixth parameter.</param>
    /// <returns>A task representing the asynchronous operation, with the result.</returns>
    public static Task<TResult?> RemoteExecute<T, T1, T2, T3, T4, T5, T6, TResult>(this IApp app,
        Func<T, T1, T2, T3, T4, T5, T6, TResult> action, 
        T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6)
    {
        return app.RemoteExecute<TResult>(action, [param1, param2, param3, param4, param5, param6]);
    }

    /// <summary>
    /// Executes a function remotely on the app with seven parameters and returns a result.
    /// </summary>
    /// <typeparam name="T">The type of the target object.</typeparam>
    /// <typeparam name="T1">The type of the first parameter.</typeparam>
    /// <typeparam name="T2">The type of the second parameter.</typeparam>
    /// <typeparam name="T3">The type of the third parameter.</typeparam>
    /// <typeparam name="T4">The type of the fourth parameter.</typeparam>
    /// <typeparam name="T5">The type of the fifth parameter.</typeparam>
    /// <typeparam name="T6">The type of the sixth parameter.</typeparam>
    /// <typeparam name="T7">The type of the seventh parameter.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="app">The app instance.</param>
    /// <param name="action">The function to execute.</param>
    /// <param name="param1">The first parameter.</param>
    /// <param name="param2">The second parameter.</param>
    /// <param name="param3">The third parameter.</param>
    /// <param name="param4">The fourth parameter.</param>
    /// <param name="param5">The fifth parameter.</param>
    /// <param name="param6">The sixth parameter.</param>
    /// <param name="param7">The seventh parameter.</param>
    /// <returns>A task representing the asynchronous operation, with the result.</returns>
    public static Task<TResult?> RemoteExecute<T, T1, T2, T3, T4, T5, T6, T7, TResult>(this IApp app,
        Func<T, T1, T2, T3, T4, T5, T6, T7, TResult> action,
        T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7)
    {
        return app.RemoteExecute<TResult>(action, [param1, param2, param3, param4, param5, param6, param7]);
    }

    /// <summary>
    /// Executes a function remotely on the app with eight parameters and returns a result.
    /// </summary>
    /// <typeparam name="T">The type of the target object.</typeparam>
    /// <typeparam name="T1">The type of the first parameter.</typeparam>
    /// <typeparam name="T2">The type of the second parameter.</typeparam>
    /// <typeparam name="T3">The type of the third parameter.</typeparam>
    /// <typeparam name="T4">The type of the fourth parameter.</typeparam>
    /// <typeparam name="T5">The type of the fifth parameter.</typeparam>
    /// <typeparam name="T6">The type of the sixth parameter.</typeparam>
    /// <typeparam name="T7">The type of the seventh parameter.</typeparam>
    /// <typeparam name="T8">The type of the eighth parameter.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="app">The app instance.</param>
    /// <param name="action">The function to execute.</param>
    /// <param name="param1">The first parameter.</param>
    /// <param name="param2">The second parameter.</param>
    /// <param name="param3">The third parameter.</param>
    /// <param name="param4">The fourth parameter.</param>
    /// <param name="param5">The fifth parameter.</param>
    /// <param name="param6">The sixth parameter.</param>
    /// <param name="param7">The seventh parameter.</param>
    /// <param name="param8">The eighth parameter.</param>
    /// <returns>A task representing the asynchronous operation, with the result.</returns>
    public static Task<TResult?> RemoteExecute<T, T1, T2, T3, T4, T5, T6, T7, T8, TResult>(this IApp app,
        Func<T, T1, T2, T3, T4, T5, T6, T7, T8, TResult> action,
        T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7, T8 param8)
    {
        return app.RemoteExecute<TResult>(action, [param1, param2, param3, param4, param5, param6, param7, param8]);
    }

    /// <summary>
    /// Executes an action remotely on the app.
    /// </summary>
    /// <typeparam name="T">The type of the target object.</typeparam>
    /// <param name="app">The app instance.</param>
    /// <param name="action">The action to execute.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static Task RemoteExecute<T>(this IApp app, Action<T> action)
    {
        return app.RemoteExecute<object?>(action, []);
    }

    /// <summary>
    /// Executes an action remotely on the app with one parameter.
    /// </summary>
    /// <typeparam name="T">The type of the target object.</typeparam>
    /// <typeparam name="T1">The type of the first parameter.</typeparam>
    /// <param name="app">The app instance.</param>
    /// <param name="action">The action to execute.</param>
    /// <param name="param1">The first parameter.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static Task RemoteExecute<T, T1>(this IApp app,
        Action<T, T1> action, T1 param1)
    {
        return app.RemoteExecute<object?>(action, [param1]);
    }

    /// <summary>
    /// Executes an action remotely on the app with two parameters.
    /// </summary>
    /// <typeparam name="T">The type of the target object.</typeparam>
    /// <typeparam name="T1">The type of the first parameter.</typeparam>
    /// <typeparam name="T2">The type of the second parameter.</typeparam>
    /// <param name="app">The app instance.</param>
    /// <param name="action">The action to execute.</param>
    /// <param name="param1">The first parameter.</param>
    /// <param name="param2">The second parameter.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static Task RemoteExecute<T, T1, T2>(this IApp app, 
        Action<T, T1, T2> action, T1 param1, T2 param2)
    {
        return app.RemoteExecute<object?>(action, [param1, param2]);
    }

    /// <summary>
    /// Executes an action remotely on the app with three parameters.
    /// </summary>
    /// <typeparam name="T">The type of the target object.</typeparam>
    /// <typeparam name="T1">The type of the first parameter.</typeparam>
    /// <typeparam name="T2">The type of the second parameter.</typeparam>
    /// <typeparam name="T3">The type of the third parameter.</typeparam>
    /// <param name="app">The app instance.</param>
    /// <param name="action">The action to execute.</param>
    /// <param name="param1">The first parameter.</param>
    /// <param name="param2">The second parameter.</param>
    /// <param name="param3">The third parameter.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static Task RemoteExecute<T, T1, T2, T3>(this IApp app,
        Action<T, T1, T2, T3> action, T1 param1, T2 param2, T3 param3)
    {
        return app.RemoteExecute<object?>(action, [param1, param2, param3]);
    }

    /// <summary>
    /// Executes an action remotely on the app with four parameters.
    /// </summary>
    /// <typeparam name="T">The type of the target object.</typeparam>
    /// <typeparam name="T1">The type of the first parameter.</typeparam>
    /// <typeparam name="T2">The type of the second parameter.</typeparam>
    /// <typeparam name="T3">The type of the third parameter.</typeparam>
    /// <typeparam name="T4">The type of the fourth parameter.</typeparam>
    /// <param name="app">The app instance.</param>
    /// <param name="action">The action to execute.</param>
    /// <param name="param1">The first parameter.</param>
    /// <param name="param2">The second parameter.</param>
    /// <param name="param3">The third parameter.</param>
    /// <param name="param4">The fourth parameter.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static Task RemoteExecute<T, T1, T2, T3, T4>(this IApp app,
        Action<T, T1, T2, T3, T4> action, T1 param1, T2 param2, T3 param3, T4 param4)
    {
        return app.RemoteExecute<object?>(action, [param1, param2, param3, param4]);
    }

    /// <summary>
    /// Executes an action remotely on the app with five parameters.
    /// </summary>
    /// <typeparam name="T">The type of the target object.</typeparam>
    /// <typeparam name="T1">The type of the first parameter.</typeparam>
    /// <typeparam name="T2">The type of the second parameter.</typeparam>
    /// <typeparam name="T3">The type of the third parameter.</typeparam>
    /// <typeparam name="T4">The type of the fourth parameter.</typeparam>
    /// <typeparam name="T5">The type of the fifth parameter.</typeparam>
    /// <param name="app">The app instance.</param>
    /// <param name="action">The action to execute.</param>
    /// <param name="param1">The first parameter.</param>
    /// <param name="param2">The second parameter.</param>
    /// <param name="param3">The third parameter.</param>
    /// <param name="param4">The fourth parameter.</param>
    /// <param name="param5">The fifth parameter.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static Task RemoteExecute<T, T1, T2, T3, T4, T5>(this IApp app,
        Action<T, T1, T2, T3, T4, T5> action, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5)
    {
        return app.RemoteExecute<object?>(action, [param1, param2, param3, param4, param5]);
    }

    /// <summary>
    /// Executes an action remotely on the app with six parameters.
    /// </summary>
    /// <typeparam name="T">The type of the target object.</typeparam>
    /// <typeparam name="T1">The type of the first parameter.</typeparam>
    /// <typeparam name="T2">The type of the second parameter.</typeparam>
    /// <typeparam name="T3">The type of the third parameter.</typeparam>
    /// <typeparam name="T4">The type of the fourth parameter.</typeparam>
    /// <typeparam name="T5">The type of the fifth parameter.</typeparam>
    /// <typeparam name="T6">The type of the sixth parameter.</typeparam>
    /// <param name="app">The app instance.</param>
    /// <param name="action">The action to execute.</param>
    /// <param name="param1">The first parameter.</param>
    /// <param name="param2">The second parameter.</param>
    /// <param name="param3">The third parameter.</param>
    /// <param name="param4">The fourth parameter.</param>
    /// <param name="param5">The fifth parameter.</param>
    /// <param name="param6">The sixth parameter.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static Task RemoteExecute<T, T1, T2, T3, T4, T5, T6>(this IApp app,
        Action<T, T1, T2, T3, T4, T5, T6> action, T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6)
    {
        return app.RemoteExecute<object?>(action, [param1, param2, param3, param4, param5, param6]);
    }

    /// <summary>
    /// Executes an action remotely on the app with seven parameters.
    /// </summary>
    /// <typeparam name="T">The type of the target object.</typeparam>
    /// <typeparam name="T1">The type of the first parameter.</typeparam>
    /// <typeparam name="T2">The type of the second parameter.</typeparam>
    /// <typeparam name="T3">The type of the third parameter.</typeparam>
    /// <typeparam name="T4">The type of the fourth parameter.</typeparam>
    /// <typeparam name="T5">The type of the fifth parameter.</typeparam>
    /// <typeparam name="T6">The type of the sixth parameter.</typeparam>
    /// <typeparam name="T7">The type of the seventh parameter.</typeparam>
    /// <param name="app">The app instance.</param>
    /// <param name="action">The action to execute.</param>
    /// <param name="param1">The first parameter.</param>
    /// <param name="param2">The second parameter.</param>
    /// <param name="param3">The third parameter.</param>
    /// <param name="param4">The fourth parameter.</param>
    /// <param name="param5">The fifth parameter.</param>
    /// <param name="param6">The sixth parameter.</param>
    /// <param name="param7">The seventh parameter.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static Task RemoteExecute<T, T1, T2, T3, T4, T5, T6, T7>(this IApp app,
        Action<T, T1, T2, T3, T4, T5, T6, T7> action, 
        T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7)
    {
        return app.RemoteExecute<object?>(action, [param1, param2, param3, param4, param5, param6, param7]);
    }

    /// <summary>
    /// Executes an action remotely on the app with eight parameters.
    /// </summary>
    /// <typeparam name="T">The type of the target object.</typeparam>
    /// <typeparam name="T1">The type of the first parameter.</typeparam>
    /// <typeparam name="T2">The type of the second parameter.</typeparam>
    /// <typeparam name="T3">The type of the third parameter.</typeparam>
    /// <typeparam name="T4">The type of the fourth parameter.</typeparam>
    /// <typeparam name="T5">The type of the fifth parameter.</typeparam>
    /// <typeparam name="T6">The type of the sixth parameter.</typeparam>
    /// <typeparam name="T7">The type of the seventh parameter.</typeparam>
    /// <typeparam name="T8">The type of the eighth parameter.</typeparam>
    /// <param name="app">The app instance.</param>
    /// <param name="action">The action to execute.</param>
    /// <param name="param1">The first parameter.</param>
    /// <param name="param2">The second parameter.</param>
    /// <param name="param3">The third parameter.</param>
    /// <param name="param4">The fourth parameter.</param>
    /// <param name="param5">The fifth parameter.</param>
    /// <param name="param6">The sixth parameter.</param>
    /// <param name="param7">The seventh parameter.</param>
    /// <param name="param8">The eighth parameter.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static Task RemoteExecute<T, T1, T2, T3, T4, T5, T6, T7, T8>(this IApp app,
        Action<T, T1, T2, T3, T4, T5, T6, T7, T8> action,
        T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6, T7 param7, T8 param8)
    {
        return app.RemoteExecute<object?>(action, [param1, param2, param3, param4, param5, param6, param7, param8]);
    }
}
