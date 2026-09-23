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
        public float MovementMax;
        public float currentMovement;
    }
    public Stats PlayerStats;
 
 
}




