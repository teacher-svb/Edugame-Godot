using Godot;
using System;
using System.Collections.Generic;
using System.IO;

namespace TnT.Extensions
{
using Godot;
using System;
using System.Collections.Generic;
using System.IO;

public static class ResourceFinder
{
    /// <summary>
    /// Finds all resources of type T in the given folder (recursively), loading only as needed.
    /// </summary>
    public static List<T> FindObjectsOfTypeAll<T>(string rootPath = "res://") where T : Resource
    {
        var results = new List<T>();
        ScanDirectory(rootPath, results);
        return results;
    }

    private static void ScanDirectory<T>(string path, List<T> results) where T : Resource
    {
        var dir = DirAccess.Open(path);
        if (dir == null)
            return;

        dir.ListDirBegin();
        string fileName;
        while ((fileName = dir.GetNext()) != "")
        {
            if (dir.CurrentIsDir())
            {
                if (fileName != "." && fileName != "..")
                    ScanDirectory(path + "/" + fileName, results);
            }
            else
            {
                var fullPath = path + "/" + fileName;
                var ext = Path.GetExtension(fileName).ToLowerInvariant();

                // Fast path: known unambiguous file extensions
                if (typeof(T) == typeof(Texture2D) && (ext == ".png" || ext == ".jpg" || ext == ".jpeg" || ext == ".webp"))
                {
                    var tex = ResourceLoader.Load<Texture2D>(fullPath, null, ResourceLoader.CacheMode.Ignore);
                    if (tex != null)
                        results.Add(tex as T);
                }
                else if (typeof(T) == typeof(AudioStreamWav) && ext == ".wav")
                {
                    var audio = ResourceLoader.Load<AudioStreamWav>(fullPath, null, ResourceLoader.CacheMode.Ignore);
                    if (audio != null)
                        results.Add(audio as T);
                }
                else if (typeof(T) == typeof(AudioStreamOggVorbis) && ext == ".ogg")
                {
                    var audio = ResourceLoader.Load<AudioStreamOggVorbis>(fullPath, null, ResourceLoader.CacheMode.Ignore);
                    if (audio != null)
                        results.Add(audio as T);
                }
                // Slow path: ambiguous extensions (.tres, .res, .tscn)
                else if (ext == ".tres" || ext == ".res" || ext == ".tscn" || ext == ".scn")
                {
                    var res = ResourceLoader.Load(fullPath, null, ResourceLoader.CacheMode.Ignore);
                    if (res is T typed)
                        results.Add(typed);
                    else
                        res?.Dispose(); // Free if not used
                }
            }
        }
        dir.ListDirEnd();
    }
}

}
