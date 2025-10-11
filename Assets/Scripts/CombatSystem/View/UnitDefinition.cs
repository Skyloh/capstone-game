using UnityEngine;

namespace CombatSystem.View
{
    [CreateAssetMenu(fileName = "New Unit Definition", menuName = "Combat/Unit Definition")]
    public class UnitDefinition : ScriptableObject
    {
        public Sprite portrait;
        public string characterMame;
        public string characterDescription;
        /// <summary>
        /// prefab should have an animator 
        /// </summary>
        public GameObject prefab;
    }
}