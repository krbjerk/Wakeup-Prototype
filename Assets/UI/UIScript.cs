using UnityEngine;
using System.Collections;
using UnityEngine.UIElements;

public class UIScript : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;

    private ProgressBar healthBar;
    private ProgressBar APBar;
    private Button EndTurnButton;

    public StateMachine stateMachine;

    public CharacterSheet PlayerSheet;
    public CharacterControls PlayerController;

    void OnEnable()
    {
        var root = uiDocument.rootVisualElement;
        healthBar = root.Q<ProgressBar>("Health");
        APBar = root.Q<ProgressBar>("AP");

        healthBar.highValue = PlayerSheet.PlayerStats.maxHealth;
        healthBar.value = PlayerSheet.PlayerStats.currentHealth;
        healthBar.title = $"{PlayerSheet.PlayerStats.currentHealth} / {PlayerSheet.PlayerStats.maxHealth}"; 

        APBar.highValue = PlayerSheet.PlayerStats.maxAP;
        APBar.value = PlayerSheet.PlayerStats.currentAP;
        APBar.title = $"{PlayerSheet.PlayerStats.currentAP} / {PlayerSheet.PlayerStats.maxAP}"; 

        EndTurnButton = root.Q<Button>("EndTurn");
        EndTurnButton.clicked += () => stateMachine.PlayerTurnEnd();

        var fillElement = healthBar.Q(className: "unity-progress-bar__progress");
        fillElement.style.backgroundColor = new StyleColor(new Color32(220, 17, 17, 255));
        
        var fillElement1 = APBar.Q(className: "unity-progress-bar__progress");
        fillElement1.style.backgroundColor = new StyleColor(new Color32(39, 200, 245, 255));


        SetHealth(PlayerSheet.PlayerStats.currentHealth);

        
    }

    public void SetHealth(int newHealth)
    {
        PlayerSheet.PlayerStats.currentHealth = Mathf.Clamp(newHealth, 0, PlayerSheet.PlayerStats.maxHealth);
        StartCoroutine(AnimateHealth(PlayerSheet.PlayerStats.currentHealth, healthBar));
        healthBar.value = PlayerSheet.PlayerStats.currentHealth; 
        healthBar.title = $"{PlayerSheet.PlayerStats.currentHealth} / {PlayerSheet.PlayerStats.maxHealth} "; 
    }

    public void UseAP(int UsedAP)
    {
        PlayerSheet.PlayerStats.currentAP = Mathf.Clamp(PlayerSheet.PlayerStats.currentAP - UsedAP, 0, PlayerSheet.PlayerStats.maxAP);
        StartCoroutine(AnimateHealth(PlayerSheet.PlayerStats.currentAP, APBar));
        APBar.title = $"{PlayerSheet.PlayerStats.currentAP} / {PlayerSheet.PlayerStats.maxAP} ";
        PlayerSheet.PlayerStats.IntermidiateAP = PlayerSheet.PlayerStats.currentAP;
        PlayerController.UpdateMovementRing();
    }

    private Coroutine apAnimCoroutine;

    public void setAP(int newAP)
    {
        PlayerSheet.PlayerStats.currentAP = Mathf.Clamp(newAP, 0, PlayerSheet.PlayerStats.maxAP);

        if (apAnimCoroutine != null) StopCoroutine(apAnimCoroutine);
        apAnimCoroutine = StartCoroutine(AnimateHealth(PlayerSheet.PlayerStats.currentAP, APBar, 0.3f));

        APBar.title = $"{PlayerSheet.PlayerStats.currentAP} / {PlayerSheet.PlayerStats.maxAP} ";
    }

    private IEnumerator AnimateHealth(float target, ProgressBar bar, float duration = 0.3f)
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

    public void TakeDamage(int amount) => SetHealth(PlayerSheet.PlayerStats.currentHealth - amount);

    public void ShowObject(GameObject ShowObj)
    {
        if (ShowObj.TryGetComponent<Entities>(out Entities Entity))
            {
                //Debug.Log(Entity.Instance.Type + " : " + Entity.Instance.Health + " HP");
            }
    }
}