namespace XamlTest;

public static class ImageExtensions
{
    public static async Task Save(this IImage image, string filePath)
    {
        await using var file = File.OpenWrite(filePath);
        await image.Save(file);
    }
}
