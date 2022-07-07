using System.Collections.Generic;

namespace XamlTest;

public interface IEventInvocation
{
    IReadOnlyList<object> Parameters { get; }
}
