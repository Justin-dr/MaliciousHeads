using System.Collections.Generic;
using UnityEngine;

namespace MaliciousHeads.Util
{
    internal static class REPOLibUtils
    {
        internal static List<EnemyParent>? SpawnEnemyNowInVanillaManner(EnemySetup enemySetup)
        {
            List<EnemyParent>? parents = REPOLib.Modules.Enemies.SpawnEnemy(enemySetup, Vector3.zero, Quaternion.identity, true);
            if (parents != null)
            {
                foreach (var parent in parents)
                {
                    parent.DespawnedTimer = 1;
                }
            }

            return parents;
        } 
    }
}
