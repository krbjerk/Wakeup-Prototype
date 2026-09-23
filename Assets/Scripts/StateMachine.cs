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
    UIScript uiScript;

    public void Start()
    {
        CurrentState = State.PlayerTurnStart;
        PlayerTurnStart();
    }


    public void PlayerTurnStart()
    {
        uiScript = FindFirstObjectByType<UIScript>();
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
