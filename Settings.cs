using BepInEx.Configuration;

namespace MaliciousHeads
{
    internal class Settings
    {
        public static ConfigEntry<byte> MaxHeadSpawnsPerLevel { get; private set; }
        public static ConfigEntry<byte> SpawnChanceOnPlayeDeath { get; private set; }
        public static ConfigEntry<float> HoldThreshold { get; private set; }
        public static ConfigEntry<bool> FlashEyesOnPickUp { get; private set; }
        public static ConfigEntry<bool> FlashEyesOnDetonate { get; private set; }
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
            
            HoldThreshold = config.Bind<float>(
                "Self-Destruct",
                "Hold Timer",
                2f,
                new ConfigDescription(
                "Maximum time to hold until self-destruct",
                new AcceptableValueRange<float>(0f, 60f)
                ));

            FlashEyesOnPickUp = config.Bind<bool>(
                "Heads",
                "Pickup - Flash Eyes",
                true,
                new ConfigDescription(
                "If true, the eyes of the Malicious Head will briefly flash red when picking it up"
                ));

            FlashEyesOnDetonate = config.Bind<bool>(
                "Heads",
                "Detonate - Flash Eyes",
                true,
                new ConfigDescription(
                "If true, the eyes of the Malicious Head will briefly flash red when killing a player"
                ));
        }
    }
}
