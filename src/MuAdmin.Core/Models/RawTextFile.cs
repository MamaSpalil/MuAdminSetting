namespace MuAdmin.Core.Models
{
    /// <summary>
    /// Fallback container for files that have no structured editor support
    /// (binary <c>.dat</c> blobs, unknown formats). The UI shows them in a
    /// simple text/hex editor.
    /// </summary>
    public sealed class RawTextFile
    {
        public string Path { get; set; }
        public string Text { get; set; }
        public EncodingHelper.DetectionResult Encoding { get; set; }
        public bool IsBinary { get; set; }
        public byte[] Bytes { get; set; }
    }
}
