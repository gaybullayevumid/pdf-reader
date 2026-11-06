namespace PdfReader.Database;

public static class AppPaths
{
    public static string DataDirectory { get; } = Ensure("PdfReader");
    public static string LogDirectory { get; } = Ensure(Path.Combine("PdfReader", "logs"));

    private static string Ensure(string relative)
    {
        var path = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), relative);
        Directory.CreateDirectory(path);
        return path;
    }
}
