namespace XamlTest;

public interface IEventRegistration : IAsyncDisposable
{
    Task<IList<IEventInvocation>> GetInvocations();
}
