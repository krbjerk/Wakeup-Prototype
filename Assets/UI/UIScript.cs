using UnityEngine;
using System.Collections;
using UnityEngine.UIElements;

public class UIScript : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;

    private ProgressBar healthBar;
    private ProgressBar APBar;

    private ProgressBar MovementBar;

    private int currentHealth;
    private int currentAP;
    private int currentMovement;

    public CharacterSheet PlayerSheet;
    void OnEnable()
    {
        currentHealth = PlayerSheet.PlayerStats.maxHealth;
        currentAP = PlayerSheet.PlayerStats.maxAP;
        currentMovement = PlayerSheet.PlayerStats.MovementMax;
        var root = uiDocument.rootVisualElement;
        healthBar = root.Q<ProgressBar>("Health");
        APBar = root.Q<ProgressBar>("AP");
        MovementBar = root.Q<ProgressBar>("Movement");

        healthBar.highValue = PlayerSheet.PlayerStats.maxHealth;
        healthBar.value = currentHealth;
        healthBar.title = $"{currentHealth} / {PlayerSheet.PlayerStats.maxHealth}"; 

        APBar.highValue = PlayerSheet.PlayerStats.maxAP;
        APBar.value = currentAP;
        APBar.title = $"{currentAP} / {PlayerSheet.PlayerStats.maxAP}"; 

        MovementBar.highValue = PlayerSheet.PlayerStats.MovementMax;
        MovementBar.value = currentMovement;
        MovementBar.title = $"{currentMovement} / {PlayerSheet.PlayerStats.MovementMax}";

        var fillElement = healthBar.Q(className: "unity-progress-bar__progress");
        fillElement.style.backgroundColor = new StyleColor(Color.red);
        
        var fillElement1 = APBar.Q(className: "unity-progress-bar__progress");
        fillElement1.style.backgroundColor = new StyleColor(Color.green);

        var fillElement2 = MovementBar.Q(className: "unity-progress-bar__progress");
        fillElement2.style.backgroundColor = new StyleColor(Color.blue);
        
    }

    public void SetHealth(int newHealth)
    {
        currentHealth = Mathf.Clamp(newHealth, 0, PlayerSheet.PlayerStats.maxHealth);
        StartCoroutine(AnimateHealth(currentHealth, healthBar));
        healthBar.value = currentHealth; 
        healthBar.title = $"{currentHealth} / {PlayerSheet.PlayerStats.maxHealth} "; 
    }

    public void UseAP(int UsedAP)
    {
        currentAP = Mathf.Clamp(currentAP-UsedAP, 0, PlayerSheet.PlayerStats.maxAP);
        StartCoroutine(AnimateHealth(currentAP, APBar));
        APBar.value = currentAP;
        APBar.title = $"{currentAP} / {PlayerSheet.PlayerStats.maxAP} ";
    }


    private IEnumerator AnimateHealth(int target, ProgressBar bar, float duration = 0.3f)
    {
        float start = bar.value;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            bar.value = Mathf.Lerp(start, target, t / duration);
            yield return null;
        }
        bar.value = target;
    }

    public void TakeDamage(int amount) => SetHealth(currentHealth - amount);
    public void Heal(int amount) => SetHealth(currentHealth + amount);

    public void ShowObject(GameObject ShowObj)
    {
        if (ShowObj.TryGetComponent<Entities>(out Entities Entity))
            {
                //Debug.Log(Entity.Instance.Type + " : " + Entity.Instance.Health + " HP");
            }
    }
}