using MoveTo.Core.FileMover;

namespace MoveTo.Shell;

internal sealed class ShellFileSystem : FileSystemPort
{
    public bool Exists(string path) => File.Exists(path) || Directory.Exists(path);

    public bool HasPermission(string path)
    {
        try
        {
            // Simple check: can enumerate or create temp handle
            if (Directory.Exists(path))
            {
                Directory.GetFiles(path); // may throw
                return true;
            }
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir) && Directory.Exists(dir))
            {
                return true;
            }
        }
        catch
        {
            return false;
        }
        return false;
    }

    public bool FileExistsInDestination(string fileName, string folder)
    {
        var target = Path.Combine(folder, fileName);
        return File.Exists(target) || Directory.Exists(target);
    }

    public void Move(string source, string destination, bool overwrite)
    {
        if (File.Exists(source))
        {
            if (overwrite && File.Exists(destination))
            {
                File.Delete(destination);
            }
            File.Move(source, destination);
            return;
        }

        if (Directory.Exists(source))
        {
            if (overwrite && Directory.Exists(destination))
            {
                Directory.Delete(destination, recursive: true);
            }
            Directory.Move(source, destination);
            return;
        }

        throw new FileNotFoundException(source);
    }
}
