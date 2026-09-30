using Unity.Mathematics;
using UnityEngine;

public class CharacterSheet : MonoBehaviour
{
    [System.Serializable]
    public class Stats
    {
        public int maxHealth;
        public int maxAP;
        public int currentHealth;
        public int currentAP;
        public int APRegen;
        public int Weight;
        public float MovementperAP;
        public float MovementMax;
        public float currentMovement;
        public int IntermidiateAP;
    }
    public Stats PlayerStats;
 
    void Awake()
    {
        PlayerStats.MovementMax = (PlayerStats.maxAP * PlayerStats.MovementperAP)+PlayerStats.MovementperAP;
        PlayerStats.IntermidiateAP = PlayerStats.maxAP;
    }
}




