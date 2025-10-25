using System.IO;
using System.Security.Cryptography;
using System.Text;

public static class FolderHasher
{
    public static Hash128 ComputeFolderHash(string folderPath)
    {
        var files = Directory.GetFiles(folderPath, "*", SearchOption.AllDirectories)
                           .OrderBy(f => f, StringComparer.Ordinal)
                           .ToArray();
        
        using var md5 = MD5.Create();
        var combinedHash = Hash128.Zero;
        
        foreach (var file in files)
        {
            var relativePath = Path.GetRelativePath(folderPath, file);
            var fileHash = ComputeFileHash(file, relativePath, md5);
            combinedHash ^= fileHash; // XOR combine
        }
        
        return combinedHash;
    }
    
    private static Hash128 ComputeFileHash(string filePath, string relativePath, MD5 md5)
    {
        // Include both path and content in hash
        var pathBytes = Encoding.UTF8.GetBytes(relativePath);
        var pathHash = new Hash128(md5.ComputeHash(pathBytes));
        
        using var fileStream = File.OpenRead(filePath);
        var contentHash = new Hash128(md5.ComputeHash(fileStream));
        
        return pathHash ^ contentHash;
    }
}
