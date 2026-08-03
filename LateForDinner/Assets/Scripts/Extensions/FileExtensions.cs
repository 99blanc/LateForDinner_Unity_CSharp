using System.IO;
using UnityEngine;

public static class FileExtensions
{
    public static string GetDirectory(this string directory)
    {
        string dir = Path.Combine(Application.persistentDataPath, directory);

        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        return dir;
    }
}
