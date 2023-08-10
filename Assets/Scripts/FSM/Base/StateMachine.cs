using System;
using System.Collections.Generic;
using UnityEngine;

namespace QFrameworkRemasteredRPGGame
{
    public class StateMachine : MonoBehaviour
    {
        protected Dictionary<Type, IState> m_StateTableDict;
        IState m_CurrentState;

        void Update()
        {
            m_CurrentState.OnLogicUpdate();
        }

        protected void SwitchOn(IState newState)
        {
            m_CurrentState = newState;
            m_CurrentState.OnEnter();
        }

        public void SwitchState(Type newStateType)
        {
            SwitchState(m_StateTableDict[newStateType]);
        }

        void SwitchState(IState newState)
        {
            m_CurrentState.OnExit();
            SwitchOn(newState);
        }
    }
}