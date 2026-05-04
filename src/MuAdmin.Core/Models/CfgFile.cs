namespace MuAdmin.Core.Models
{
    /// <summary>
    /// CFG files (e.g. <c>CommonServer.cfg</c>) share the same structure as
    /// INI files (sectioned key/value with line and trailing comments) so they
    /// reuse <see cref="IniFile"/> as the underlying storage. This wrapper
    /// merely tags the file as a CFG so the UI can pick the right editor.
    /// </summary>
    public sealed class CfgFile
    {
        public IniFile Inner { get; }
        public string Path { get { return Inner.Path; } }
        public EncodingHelper.DetectionResult Encoding { get { return Inner.Encoding; } }

        public CfgFile(IniFile inner) { Inner = inner; }
    }
}
