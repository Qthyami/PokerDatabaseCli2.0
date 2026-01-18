
namespace PokerDatabaseCli2._0.Common;

public static class Files {
    public static bool
    IsDirectoryExists(this string directory) =>
        Directory.Exists(directory);

    public static string
    VerifyDirectoryExists(this string directory) {
        if (!directory.IsDirectoryExists())
            throw new DirectoryNotFoundException($"Directory not found: {directory.Quoted()}");
        return directory;
    }
    public static IEnumerable<string>
    GetAllDirectoryFiles(this string directory) =>
        Directory.GetFiles(directory, "*", SearchOption.AllDirectories);

    public static IEnumerable<string>
    WithFileExtension(this IEnumerable<string> files, string extension) =>
        files.Where(file => file.HasFileExtension(extension));

    public static bool
    HasFileExtension(this string file, string extension) =>
        file.EndsWith(extension, StringComparison.OrdinalIgnoreCase);

    public static string
    GetAllTextFromFile(this string filePath) =>
        File.ReadAllText(filePath);


}
