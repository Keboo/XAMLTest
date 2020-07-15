using System.IO;
using System.Threading.Tasks;

namespace XAMLTest
{
    public interface IImage
    {
        Task Save(Stream stream);
    }
}
