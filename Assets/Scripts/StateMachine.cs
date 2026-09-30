using UnityEngine;

public class StateMachine : MonoBehaviour
{
    public enum State
    {
        PlayerTurnStart,
        PlayerTurn,
        PlayerTurnEnd, 
        EnemyTurnStart,
        EnemyTurn,
        EnemyTurnEnd      
    }
    public State CurrentState;
    public CharacterControls PlayerController;
    public CharacterSheet PlayerSheet;
    UIScript uiScript;

    public void Start()
    {
        PlayerController = FindFirstObjectByType<CharacterControls>();
        CurrentState = State.PlayerTurnStart;
        PlayerTurnStart();
    }


    public void PlayerTurnStart()
    {
        PlayerSheet.PlayerStats.MovementMax = PlayerSheet.PlayerStats.maxAP * PlayerSheet.PlayerStats.MovementperAP + PlayerSheet.PlayerStats.MovementperAP;
        PlayerSheet.PlayerStats.IntermidiateAP = PlayerSheet.PlayerStats.maxAP;
        uiScript = FindFirstObjectByType<UIScript>();
        uiScript.UseAP(-PlayerSheet.PlayerStats.APRegen);
        PlayerController.enabled = true;
        ParticleSystem Ring = PlayerController.Movement.MovementRing.GetComponent<ParticleSystem>();
        Ring.Play();
        PlayerController.UpdateMovementRing();
        CurrentState = State.PlayerTurn;
    }

    public void PlayerTurnEnd()
    {
        PlayerController.Movement.MovementRing.GetComponent<ParticleSystem>().Stop();
        PlayerController.animator.SetBool("WalkingAnimation", false);
        PlayerController.Movement.Speed = 0;
        PlayerController.enabled = false;
        CurrentState = State.EnemyTurnStart;
        EnemyTurnStart();


    }

    public void EnemyTurnStart()
    {
        CurrentState = State.EnemyTurn;
        StartCoroutine(EnemyTurn());
    }

    System.Collections.IEnumerator EnemyTurn()
    {
        yield return new WaitForSeconds(2f);
        CurrentState = State.EnemyTurnEnd;
        PlayerTurnStart();
    }

    
}
