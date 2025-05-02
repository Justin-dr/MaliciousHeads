using HarmonyLib;
using MaliciousHeads.Manager;
using System.Collections.Generic;
using System.Linq;

namespace MaliciousHeads.Patch
{
    [HarmonyPatch(typeof(EnemyDirector))]
    internal class EnemyDirectorPatch
    {
        
        [HarmonyPostfix]
        [HarmonyPatch("Start")]
        static void EnemyDirectorStartPostfix()
        {
            if (SemiFunc.IsNotMasterClient() || !SemiFunc.RunIsLevel()) return;

            EnemyDirector enemmyDirector = EnemyDirector.instance;
            EnemyManager enemyManager = EnemyManager.Instance;

            MaliciousHeads.Logger.LogDebug("Level changed, setting SpawnedHeads count back to 0");
            enemyManager.SpawnedHeads = 0;

            IList<EnemySetup> setupsToRemove = enemmyDirector.enemiesDifficulty1
                .Concat(enemmyDirector.enemiesDifficulty2)
                .Concat(enemmyDirector.enemiesDifficulty3)
                .Where(enemyManager.IsSupported)
                .ToList();

            foreach (EnemySetup setup in setupsToRemove)
            {
                MaliciousHeads.Logger.LogDebug($"Removing setup: {setup.name}");
                enemyManager.RememberEnemy(setup);
                RemoveFromDirector(setup);
            }
        }

        private static void RemoveFromDirector(EnemySetup enemy)
        {
            EnemyDirector.instance.enemiesDifficulty1.Remove(enemy);
            EnemyDirector.instance.enemiesDifficulty2.Remove(enemy);
            EnemyDirector.instance.enemiesDifficulty3.Remove(enemy);
        }
    }
}
