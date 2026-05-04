using System;
using System.IO;

namespace MuAdmin.Core
{
    /// <summary>
    /// Creates timestamped backups of a configuration file under
    /// <c>&lt;ServerRoot&gt;/_MuAdmin_Backups/&lt;timestamp&gt;/</c> before any
    /// destructive save. Mirrors the relative path of the original file inside
    /// the backup folder so multiple files saved in the same session are
    /// grouped together.
    /// </summary>
    public static class BackupManager
    {
        public const string BackupFolderName = "_MuAdmin_Backups";

        public static string CreateBackup(string serverRoot, string filePath)
        {
            if (string.IsNullOrEmpty(serverRoot) || string.IsNullOrEmpty(filePath))
                return null;
            if (!File.Exists(filePath)) return null;

            string ts = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string root = Path.Combine(serverRoot, BackupFolderName, ts);
            string rel;
            try { rel = MakeRelative(serverRoot, filePath); }
            catch { rel = Path.GetFileName(filePath); }

            string dest = Path.Combine(root, rel);
            Directory.CreateDirectory(Path.GetDirectoryName(dest));
            File.Copy(filePath, dest, overwrite: true);
            return dest;
        }

        private static string MakeRelative(string root, string file)
        {
            var r = new Uri(AppendSeparator(Path.GetFullPath(root)));
            var f = new Uri(Path.GetFullPath(file));
            string rel = Uri.UnescapeDataString(r.MakeRelativeUri(f).ToString());
            return rel.Replace('/', Path.DirectorySeparatorChar);
        }

        private static string AppendSeparator(string p)
        {
            if (p.EndsWith(Path.DirectorySeparatorChar.ToString())) return p;
            return p + Path.DirectorySeparatorChar;
        }
    }
}
