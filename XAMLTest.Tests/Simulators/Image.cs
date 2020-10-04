using System.IO;
using System.Threading.Tasks;

namespace XamlTest.Tests.Simulators
{
    public class Image : IImage
    {
        public Task Save(Stream stream) => Task.CompletedTask;
    }
}
