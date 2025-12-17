using System.Text;

namespace MoveTo.Core.ConflictResolution;

public sealed class FileName : IEquatable<FileName>
{
    public FileName(string baseName, string extension)
    {
        BaseName = baseName;
        Extension = extension ?? string.Empty;
    }

    public string BaseName { get; }

    public string Extension { get; }

    public string GetFullName()
    {
        if (string.IsNullOrEmpty(Extension))
        {
            return BaseName;
        }
        return new StringBuilder(BaseName.Length + Extension.Length + 1)
            .Append(BaseName)
            .Append('.')
            .Append(Extension)
            .ToString();
    }

    public bool HasExtension() => !string.IsNullOrEmpty(Extension);

    public static FileName Parse(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            throw new ArgumentException("File name must not be empty", nameof(fullName));
        }

        var ext = Path.GetExtension(fullName);
        if (string.IsNullOrEmpty(ext))
        {
            return new FileName(Path.GetFileName(fullName), string.Empty);
        }

        var baseName = Path.GetFileNameWithoutExtension(fullName);
        var trimmedExt = ext.TrimStart('.');
        return new FileName(baseName, trimmedExt);
    }

    public override bool Equals(object? obj) => obj is FileName other && Equals(other);

    public bool Equals(FileName? other)
    {
        if (other is null) return false;
        return string.Equals(BaseName, other.BaseName, StringComparison.OrdinalIgnoreCase)
               && string.Equals(Extension, other.Extension, StringComparison.OrdinalIgnoreCase);
    }

    public override int GetHashCode()
    {
        var bn = (BaseName ?? string.Empty).ToLowerInvariant();
        var ext = (Extension ?? string.Empty).ToLowerInvariant();
        unchecked
        {
            return (bn.GetHashCode() * 397) ^ ext.GetHashCode();
        }
    }
}
