using UnityEngine;
using System.Collections;
using UnityEngine.UIElements;

public class UIScript : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;

    private ProgressBar healthBar;

    [System.Serializable]

    public class Stats
    {
        public int maxHealth;
        public int maxAP;
        public int APRegen;
        public int Weight;
    }
    public Stats PlayerStats;

    private int currentHealth;
    private int currentAP;


    private void OnEnable()
    {
        currentHealth = PlayerStats.maxHealth;
        currentAP = PlayerStats.maxAP;
        var root = uiDocument.rootVisualElement;
        healthBar = root.Q<ProgressBar>("Health");

        healthBar.highValue = PlayerStats.maxHealth;
        healthBar.value = currentHealth;
        healthBar.title = $"{currentHealth} / {PlayerStats.maxHealth}"; 
    }

    public void SetHealth(int newHealth)
    {
        currentHealth = Mathf.Clamp(newHealth, 0, PlayerStats.maxHealth);
        StartCoroutine(AnimateHealth(currentHealth));
        healthBar.value = currentHealth; 
        healthBar.title = $"{currentHealth} / {PlayerStats.maxHealth} "; 
    }

    private IEnumerator AnimateHealth(int target, float duration = 0.3f)
    {
        float start = healthBar.value;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            healthBar.value = Mathf.Lerp(start, target, t / duration);
            yield return null;
        }
        healthBar.value = target;
    }

    public void TakeDamage(int amount) => SetHealth(currentHealth - amount);
    public void Heal(int amount) => SetHealth(currentHealth + amount);

    public void ShowObject(GameObject ShowObj)
    {
        if (ShowObj.TryGetComponent<Entities>(out Entities Entity))
            {
                Debug.Log(Entity.Instance.Type + " : " + Entity.Instance.Health + " HP");
            }
    }
}