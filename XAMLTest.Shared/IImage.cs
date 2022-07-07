using System.IO;

namespace XamlTest;

public interface IImage
{
    Task Save(Stream stream);
}
