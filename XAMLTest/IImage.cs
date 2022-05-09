using System.IO;
using System.Threading.Tasks;

namespace XamlTest;

public interface IImage
{
    Task Save(Stream stream);
}
