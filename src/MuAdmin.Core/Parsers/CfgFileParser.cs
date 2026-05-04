using MuAdmin.Core.Models;

namespace MuAdmin.Core.Parsers
{
    /// <summary>
    /// Convenience wrapper around <see cref="IniFileParser"/> for files with
    /// the <c>.cfg</c> extension (notably <c>CommonServer.cfg</c>). They share
    /// the INI grammar verbatim so this parser delegates to the INI parser.
    /// </summary>
    public static class CfgFileParser
    {
        public static CfgFile Parse(string path)
        {
            return new CfgFile(IniFileParser.Parse(path));
        }

        public static void Save(CfgFile file, string path = null)
        {
            IniFileParser.Save(file.Inner, path);
        }
    }
}
