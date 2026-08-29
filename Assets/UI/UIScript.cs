using UnityEngine;
using System.Collections;
using UnityEngine.UIElements;

public class UIScript : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;

    private ProgressBar healthBar;
    private int maxHealth = 10;
    private int currentHealth = 10;

    private void OnEnable()
    {
        var root = uiDocument.rootVisualElement;
        healthBar = root.Q<ProgressBar>("Health");

        healthBar.highValue = maxHealth;
        healthBar.value = currentHealth;
        healthBar.title = $"{currentHealth} / {maxHealth}"; 
    }

    public void SetHealth(int newHealth)
    {
        currentHealth = Mathf.Clamp(newHealth, 0, maxHealth);
        StartCoroutine(AnimateHealth(currentHealth));
        healthBar.value = currentHealth; 
        healthBar.title = $"{currentHealth} / {maxHealth} "; 
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
}