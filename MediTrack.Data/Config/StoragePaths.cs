namespace MediTrack.Data.Config;

public class StoragePaths
{
    public StoragePaths(string baseDirectory)
    {
        BaseDirectory = baseDirectory;
        Directory.CreateDirectory(BaseDirectory);
    }

    public string BaseDirectory { get; }

    public string GetFilePath(string fileName) => Path.Combine(BaseDirectory, fileName);
}
