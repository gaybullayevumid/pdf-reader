namespace PdfReader.Database;

public static class AppPaths
{
    /// <summary>Overrides the data folder (used by automated tests so they never touch real user data).</summary>
    public const string DataDirEnvVar = "PDFREADER_DATA_DIR";

    public static string DataDirectory { get; } = Ensure(string.Empty);
    public static string LogDirectory { get; } = Ensure("logs");

    private static string Ensure(string relative)
    {
        var root = Environment.GetEnvironmentVariable(DataDirEnvVar) is { Length: > 0 } custom
            ? custom
            : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PdfReader");
        var path = Path.Combine(root, relative);
        Directory.CreateDirectory(path);
        return path;
    }
}
