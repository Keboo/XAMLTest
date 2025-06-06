using System.Reflection.Emit;

namespace XamlTest.Event;

internal static class EventRegistrar
{
    private class EventDetails
    {
        public List<object[]> Invocations { get; } = [];
        public Delegate Delegate { get; }
        public EventInfo Event { get; }
        public object? Source { get; }

        public EventDetails(EventInfo eventInfo, Delegate @delegate, object? source)
        {
            Event = eventInfo;
            Delegate = @delegate;
            Source = source;
        }
    }

    private static object SyncObject { get; } = new();

    private static Dictionary<string, EventDetails> RegisteredEvents { get; } = [];

    private static Dictionary<string, List<object[]>> EventInvocations { get; } = [];
    private static Dictionary<string, Delegate> EventDelegates { get; } = [];

    public static void AddInvocation(string eventId, object[] parameters)
    {
        lock(SyncObject)
        {
            if (RegisteredEvents.TryGetValue(eventId, out EventDetails? eventDetails))
            {
                eventDetails.Invocations.Add(parameters);
            }
        }
    }

    public static bool Unregister(string eventId)
    {
        lock (SyncObject)
        {
            if (RegisteredEvents.TryGetValue(eventId, out EventDetails? eventDetails))
            {
                MethodInfo? removeMethod = eventDetails.Event.GetRemoveMethod();
                removeMethod?.Invoke(eventDetails.Source, [eventDetails.Delegate]);
                return removeMethod != null;
            }
        }
        return false;
    }

    internal static IReadOnlyList<object[]>? GetInvocations(string eventId)
    {
        lock (SyncObject)
        {
            if (RegisteredEvents.TryGetValue(eventId, out EventDetails? eventDetails))
            {
                return eventDetails.Invocations;
            }
        }
        return null;
    }

    public static void Regsiter(string eventId, EventInfo eventInfo, object? source)
    {
        if (eventId is null)
        {
            throw new ArgumentNullException(nameof(eventId));
        }

        if (eventInfo is null)
        {
            throw new ArgumentNullException(nameof(eventInfo));
        }

        Type delegateType = eventInfo.EventHandlerType ??
            throw new InvalidOperationException($"Could not determine Event Handler Type for event '{eventInfo.Name}'");
        
        Type returnType = GetDelegateReturnType(delegateType);
        if (returnType != typeof(void))
            throw new XamlTestException("Event delegate must return void.");

        var delegateParameterTypes = GetDelegateParameterTypes(delegateType);

        DynamicMethod handler = new ("", null, delegateParameterTypes, typeof(EventRegistrar));

        ILGenerator ilgen = handler.GetILGenerator();
        MethodInfo addInvocationMethod = typeof(EventRegistrar)
            .GetMethod(nameof(AddInvocation))
            ?? throw new InvalidOperationException("Failed to find method");
        int foo = 0;
        string bar = "";
        object[] array = [foo, bar];

        ilgen.Emit(OpCodes.Ldstr, eventId);
        ilgen.Emit(OpCodes.Ldc_I4, delegateParameterTypes.Length);
        ilgen.Emit(OpCodes.Newarr, typeof(object));
        
        for(int i = 0; i < delegateParameterTypes.Length; i++)
        {
            ilgen.Emit(OpCodes.Dup);
            ilgen.Emit(OpCodes.Ldc_I4, i);
            ilgen.Emit(OpCodes.Ldarg, i);
            if (!delegateParameterTypes[i].IsClass)
            {
                ilgen.Emit(OpCodes.Box, delegateParameterTypes[i]);
            }
            ilgen.Emit(OpCodes.Stelem_Ref);
        }
        ilgen.Emit(OpCodes.Call, addInvocationMethod);
        ilgen.Emit(OpCodes.Ret);

        MethodInfo addHandler = eventInfo.GetAddMethod() ?? 
            throw new InvalidOperationException($"Could not find add method for event '{eventInfo.Name}'");
        Delegate dEmitted = handler.CreateDelegate(delegateType);
        addHandler.Invoke(source, [dEmitted]);

        lock(RegisteredEvents)
        {
            RegisteredEvents.Add(eventId, new EventDetails(eventInfo, dEmitted, source));
        }
    }

    private static Type[] GetDelegateParameterTypes(Type delegateType)
    {
        if (delegateType.BaseType != typeof(MulticastDelegate))
        {
            throw new XamlTestException($"'{delegateType.FullName}' is not a delegate type.");
        }

        MethodInfo invoke = delegateType.GetMethod(nameof(Action.Invoke))
            ?? throw new MissingMethodException($"Could not find {nameof(Action.Invoke)} method on delegate {delegateType.FullName}");

        ParameterInfo[] parameters = invoke.GetParameters();
        Type[] typeParameters = new Type[parameters.Length];
        for (int i = 0; i < parameters.Length; i++)
        {
            typeParameters[i] = parameters[i].ParameterType;
        }
        return typeParameters;
    }

    private static Type GetDelegateReturnType(Type delegateType)
    {
        if (delegateType.BaseType != typeof(MulticastDelegate))
        {
            throw new XamlTestException($"'{delegateType.FullName}' is not a delegate type.");
        }

        MethodInfo? invoke = delegateType.GetMethod(nameof(Action.Invoke));
        return invoke is null
            ? throw new MissingMethodException($"Could not find {nameof(Action.Invoke)} method on delegate {delegateType.FullName}")
            : invoke.ReturnType;
    }

}
