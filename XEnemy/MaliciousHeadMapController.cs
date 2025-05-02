using UnityEngine;

namespace MaliciousHeads.XEnemy
{
    internal class MaliciousHeadMapController : MonoBehaviour
    {
        public MapCustom mapCustom;
        public GameObject enableObject;
        public EnemyMaliciousHead enemyMaliciousHead;

        public void Update()
        {
            if (!enemyMaliciousHead.serverSeen || !enableObject.activeSelf)
            {
                mapCustom.Hide();
            }
        }
    }
}
