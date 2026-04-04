using System.IO;

namespace GameConfig
{
    public static class ConfigManager
    {
        public static TbTestTable Tests { get; private set; }

        public static void LoadAll(string configPath)
        {
            Tests = TbTestTable.LoadFromFile(Path.Combine(configPath, "test.sfc"));
        }

        public static void UnloadAll()
        {
            Tests = null;
        }

        public static TbTestTable GetTests()
        {
            return Tests;
        }
    }
}
