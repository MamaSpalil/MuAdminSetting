using System.Collections.Generic;

namespace MuAdmin.Core.Catalogs
{
    /// <summary>
    /// Built-in MU Online map id → display name dictionary.
    /// Used to render numeric map indexes (in <c>Gate.txt</c>,
    /// <c>MoveLevel.txt</c>, <c>MonsterSetBase*.txt</c>, ...) as
    /// human-readable names in the UI.
    /// </summary>
    public static class MapNames
    {
        private static readonly Dictionary<int, string> _names = new Dictionary<int, string>
        {
            { 0,  "Lorencia" },        { 1,  "Dungeon" },        { 2,  "Devias" },
            { 3,  "Noria" },           { 4,  "LostTower" },      { 6,  "Stadium" },
            { 7,  "Atlans" },          { 8,  "Tarkan" },         { 9,  "DevilSquare" },
            { 10, "Icarus" },          { 11, "BloodCastle1" },   { 12, "BloodCastle2" },
            { 13, "BloodCastle3" },    { 14, "BloodCastle4" },   { 15, "BloodCastle5" },
            { 16, "BloodCastle6" },    { 17, "BloodCastle7" },   { 18, "ChaosCastle1" },
            { 19, "ChaosCastle2" },    { 20, "ChaosCastle3" },   { 21, "ChaosCastle4" },
            { 22, "ChaosCastle5" },    { 23, "ChaosCastle6" },   { 24, "Kalima1" },
            { 25, "Kalima2" },         { 26, "Kalima3" },        { 27, "Kalima4" },
            { 28, "Kalima5" },         { 29, "Kalima6" },        { 30, "ValleyOfLoren" },
            { 31, "LandOfTrials" },    { 32, "DevilSquare2" },   { 33, "Aida" },
            { 34, "CryWolf1" },        { 35, "CryWolf2" },       { 36, "Kalima7" },
            { 37, "Kanturu1" },        { 38, "Kanturu2" },       { 39, "Kanturu3" },
            { 40, "Silent" },          { 41, "BarrackOfBalgass" },{ 42, "Balgass" },
            { 45, "IllusionTemple1" }, { 46, "IllusionTemple2" },{ 47, "IllusionTemple3" },
            { 48, "IllusionTemple4" }, { 49, "IllusionTemple5" },{ 50, "IllusionTemple6" },
            { 51, "ElbeLand" },        { 52, "BloodCastle8" },   { 53, "ChaosCastle7" },
            { 56, "Swamp" },           { 57, "Raklion" },        { 58, "RaklionBoss" },
            { 62, "SantaTown" },       { 63, "VulcanusPvP" },    { 64, "DoubleGoer" },
            { 65, "ImperialFortress1" },{ 66, "ImperialFortress2" },
            { 67, "ImperialFortress3" },{ 68, "ImperialFortress4" }
        };

        public static string Get(int id)
        {
            string name;
            return _names.TryGetValue(id, out name) ? name : "Map " + id;
        }

        /// <summary>
        /// Reverse lookup of <see cref="Get"/>: returns the map number whose
        /// canonical name matches <paramref name="name"/> (case-insensitive),
        /// or <c>-1</c> if unknown.
        /// </summary>
        public static int GetNumberByName(string name)
        {
            if (string.IsNullOrEmpty(name)) return -1;
            foreach (var kv in _names)
                if (string.Equals(kv.Value, name, System.StringComparison.OrdinalIgnoreCase))
                    return kv.Key;
            return -1;
        }

        public static IReadOnlyDictionary<int, string> All { get { return _names; } }
    }
}
