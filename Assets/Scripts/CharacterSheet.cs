using UnityEngine;

public class CharacterSheet : MonoBehaviour
{
    [System.Serializable]
    public class Stats
    {
        public int maxHealth;
        public int maxAP;
        public int APRegen;
        public int Weight;
        public int MovementMax;
    }
    public Stats PlayerStats;

}




