using HarmonyLib;
using MaliciousHeads.Manager;
using UnityEngine;
using System.Collections.Generic;
using System.Text;
using MaliciousHeads.Util;

namespace MaliciousHeads.Patch
{
    [HarmonyPatch(typeof(PlayerHealth))]
    internal class PlayerHealthPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("Death")]
        static void PlayerHealthDeathPostfix()
        {
            MaliciousHeads.Logger.LogDebug("Player died");
            if (!SemiFunc.IsMultiplayer() || !SemiFunc.IsMasterClient() || !SemiFunc.RunIsLevel()) return;

            EnemyManager enemyManager = EnemyManager.Instance;

            if (enemyManager.HasEnemiesToRelease())
            {
                foreach (EnemySetup enemySetup in enemyManager.ReleaseRememberedEnemies())
                {
                    EnemyParent.Difficulty? enemyDifficulty = GetDifficulty(GetEnemyParent(enemySetup));
                    if (enemyDifficulty != null)
                    {
                        MaliciousHeads.Logger.LogDebug($"Added {enemySetup.name} to {enemyDifficulty}");
                        AssignEnemyToDifficulty(enemySetup, enemyDifficulty.Value);
                    }
                }
            }

            bool canSpawn = enemyManager.SpawnedHeads < Settings.MaxHeadSpawnsPerLevel.Value && Random.Range(0, 100) < Settings.SpawnChanceOnPlayeDeath.Value;
            if (canSpawn && REPOLib.Modules.Enemies.TryGetEnemyByName("Malicious Head", out EnemySetup? setup))
            {
                REPOLibUtils.SpawnEnemyNowInVanillaManner(setup);
                enemyManager.SpawnedHeads++;
            }
        }

        private static EnemyParent? GetEnemyParent(EnemySetup enemySetup)
        {
            foreach (var item in enemySetup.spawnObjects)
            {
                if (item.TryGetComponent(out EnemyParent enemyParent))
                {
                    return enemyParent;
                }
            }
            return null;
        }

        private static EnemyParent.Difficulty? GetDifficulty(EnemyParent? enemyParent)
        {
            if (enemyParent == null)
            {
                return null;
            }

            return enemyParent.difficulty;
        }

        private static void AssignEnemyToDifficulty(EnemySetup enemySetup, EnemyParent.Difficulty difficulty)
        {
            switch (difficulty)
            {
                case EnemyParent.Difficulty.Difficulty1:
                    EnemyDirector.instance.enemiesDifficulty1.Add(enemySetup);
                    break;
                case EnemyParent.Difficulty.Difficulty2:
                    EnemyDirector.instance.enemiesDifficulty2.Add(enemySetup);
                    break;
                case EnemyParent.Difficulty.Difficulty3:
                    EnemyDirector.instance.enemiesDifficulty3.Add(enemySetup);
                    break;
            }
        }
    }
}
