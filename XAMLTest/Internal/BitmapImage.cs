using Google.Protobuf;

namespace XamlTest.Internal;

internal class BitmapImage : IImage
{
    private ByteString Data { get; }

    public BitmapImage(ByteString data) => Data = data ?? throw new ArgumentNullException(nameof(data));

    public Task Save(Stream stream)
    {
        Data.WriteTo(stream);
        return Task.CompletedTask;
    }
}
