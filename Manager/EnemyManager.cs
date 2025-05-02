using System.Collections.Generic;

namespace MaliciousHeads.Manager
{
    internal class EnemyManager
    {
        private static readonly ISet<string> _supportedEnemyNames = new HashSet<string>() { "Enemy - Malicious Head" };
        private static ISet<EnemySetup> _enemySetups = new HashSet<EnemySetup>();

        internal static EnemyManager Instance { get; private set; } = new EnemyManager();

        public byte SpawnedHeads { get; set; } = 0;

        public bool IsSupported(EnemySetup enemySetup) 
        {
            return _supportedEnemyNames.Contains(enemySetup.name);
        }

        public void RememberEnemy(EnemySetup enemySetup)
        {
            _enemySetups.Add(enemySetup);
            MaliciousHeads.Logger.LogDebug($"Currently remembering: {string.Join(", ", _enemySetups)}");
        }

        public ISet<EnemySetup> ReleaseRememberedEnemies()
        {
            ISet<EnemySetup> toRelease = _enemySetups;
            _enemySetups = new HashSet<EnemySetup>();

            return toRelease;
        }

        public bool HasEnemiesToRelease()
        {
            return _enemySetups.Count > 0;
        }
    }
}
