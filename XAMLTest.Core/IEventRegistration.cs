using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace XamlTest;

public interface IEventRegistration : IAsyncDisposable
{
    Task<IList<IEventInvocation>> GetInvocations();
}
