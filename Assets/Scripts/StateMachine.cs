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

        void OnEnable()
    {
        CurrentState = State.PlayerTurnStart;
        PlayerTurnStart();
    }

    void PlayerTurnStart()
    {
        CurrentState = State.PlayerTurn;
    }

}
