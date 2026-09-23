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

    public CharacterSheet PlayerSheet;

    public void OnEnable()
    {
        CurrentState = State.PlayerTurnStart;
        PlayerTurnStart();
    }

    public void PlayerTurnStart()
    {
        PlayerSheet.PlayerStats.currentAP = Mathf.Clamp(PlayerSheet.PlayerStats.currentAP + PlayerSheet.PlayerStats.APRegen, 0, PlayerSheet.PlayerStats.maxAP);
        UIScript uiScript = FindObjectOfType<UIScript>();
        uiScript.UseAP(-PlayerSheet.PlayerStats.APRegen);
        CurrentState = State.PlayerTurn;
    }

    public void PlayerTurnEnd()
    {
        Debug.Log("Player Turn Ended");
        CurrentState = State.EnemyTurnStart;
        EnemyTurnStart();
    }

    public void EnemyTurnStart()
    {
        CurrentState = State.EnemyTurn;
        PlayerTurnStart();
    }

}
