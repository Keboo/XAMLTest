using Grpc.Core;

namespace XamlTest.Host;

public abstract partial class InternalTestService : Protocol.ProtocolBase
{
    public sealed override Task<GetWindowsResult> GetWindows(GetWindowsQuery request, ServerCallContext context) 
        => GetWindows(request);
    protected abstract Task<GetWindowsResult> GetWindows(GetWindowsQuery request);

    public sealed override Task<GetWindowsResult> GetMainWindow(GetWindowsQuery request, ServerCallContext context)
        => GetMainWindow(request);
    protected abstract Task<GetWindowsResult> GetMainWindow(GetWindowsQuery request);


    public sealed override Task<PropertyResult> GetProperty(PropertyQuery request, ServerCallContext context)
        => GetProperty(request);
    protected abstract Task<PropertyResult> GetProperty(PropertyQuery request);

    public sealed override Task<EffectiveBackgroundResult> GetEffectiveBackground(EffectiveBackgroundQuery request, ServerCallContext context)
        => GetEffectiveBackground(request);
    protected abstract Task<EffectiveBackgroundResult> GetEffectiveBackground(EffectiveBackgroundQuery request);

    public sealed override Task<PropertyResult> SetProperty(SetPropertyRequest request, ServerCallContext context)
        => SetProperty(request);
    protected abstract Task<PropertyResult> SetProperty(SetPropertyRequest request);

    public sealed override Task<ElementResult> SetXamlProperty(SetXamlPropertyRequest request, ServerCallContext context)
        => SetXamlProperty(request);
    protected abstract Task<ElementResult> SetXamlProperty(SetXamlPropertyRequest request);

    public sealed override Task<ResourceResult> GetResource(ResourceQuery request, ServerCallContext context)
        => GetResource(request);
    protected abstract Task<ResourceResult> GetResource(ResourceQuery request);

    public sealed override Task<CoordinatesResult> GetCoordinates(CoordinatesQuery request, ServerCallContext context)
        => GetCoordinates(request);
    protected abstract Task<CoordinatesResult> GetCoordinates(CoordinatesQuery request);

    public sealed override Task<ApplicationResult> InitializeApplication(ApplicationConfiguration request, ServerCallContext context)
        => InitializeApplication(request);
    protected abstract Task<ApplicationResult> InitializeApplication(ApplicationConfiguration request);

    public sealed override Task<WindowResult> CreateWindow(WindowConfiguration request, ServerCallContext context)
        => CreateWindow(request);
    protected abstract Task<WindowResult> CreateWindow(WindowConfiguration request);

    public sealed override Task<ImageResult> GetScreenshot(ImageQuery request, ServerCallContext context)
        => GetScreenshot(request);
    protected abstract Task<ImageResult> GetScreenshot(ImageQuery request);

    public sealed override Task<ShutdownResult> Shutdown(ShutdownRequest request, ServerCallContext context)
        => Shutdown(request);
    protected abstract Task<ShutdownResult> Shutdown(ShutdownRequest request);
    

    public sealed override Task<SerializerResult> RegisterSerializer(SerializerRequest request, ServerCallContext context)
    {
        SerializerResult reply = new();
        try
        {
            if (string.IsNullOrWhiteSpace(request.SerializerType))
            {
                reply.ErrorMessages.Add("Serializer type must be specified");
                return Task.FromResult(reply);
            }
            if (Type.GetType(request.SerializerType) is { } serializerType &&
                Activator.CreateInstance(serializerType) is ISerializer serializer)
            {
                AddSerializer(serializer, request.InsertIndex);
            }
            else
            {
                reply.ErrorMessages.Add($"Failed to resolve serializer type '{request.SerializerType}'");
            }
        }
        catch (Exception e)
        {
            reply.ErrorMessages.Add(e.ToString());
        }
        return Task.FromResult(reply);
    }

    public sealed override Task<VersionResult> GetVersion(VersionRequest request, ServerCallContext context)
        => GetVersion(request);
    protected abstract Task<VersionResult> GetVersion(VersionRequest request);

    public override Task<ElementResult> GetElement(ElementQuery request, ServerCallContext context)
        => GetElement(request);
    protected abstract Task<ElementResult> GetElement(ElementQuery request);

}
