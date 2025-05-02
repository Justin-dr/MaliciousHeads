using BepInEx.Configuration;

namespace MaliciousHeads
{
    internal class Settings
    {
        public static ConfigEntry<byte> MaxHeadSpawnsPerLevel { get; private set; }
        public static ConfigEntry<byte> SpawnChanceOnPlayeDeath { get; private set; }
        public static ConfigEntry<float> FuseTime { get; private set; }
        public static void Initialize(ConfigFile config)
        {
            MaxHeadSpawnsPerLevel = config.Bind<byte>(
                "Heads",
                "Maximum Head Spawns",
                2,
                new ConfigDescription(
                "Maximum amount of heads that can spawn per level",
                new AcceptableValueRange<byte>(0, 255)
                ));

            SpawnChanceOnPlayeDeath = config.Bind<byte>(
                "Heads",
                "Spawn Chance",
                50,
                new ConfigDescription(
                "Percentage chance of spawning a head when a player dies",
                new AcceptableValueRange<byte>(0, 100)
                ));
        }
    }
}
