using System;
using System.Collections.Generic;
using UnityEngine;

namespace QFrameworkRemasteredRPGGame
{
    public class PlayerStateMachine : StateMachine
    {
        [SerializeField] PlayerState[] m_States;
        Animator m_Animator;
        PlayerCtrl m_PlayerCtrl;

        void Awake()
        {
            m_Animator = GetComponent<Animator>();
            m_PlayerCtrl = GetComponent<PlayerCtrl>();
            m_StateTableDict = new Dictionary<Type, IState>(m_States.Length);
            foreach (var state in m_States)
            {
                state.InitComponent(m_Animator, this, m_PlayerCtrl);
                m_StateTableDict.Add(state.GetType(), state);
            }
        }

        void Start()
        {
            SwitchOn(m_StateTableDict[typeof(PlayerState_Idle)]);
        }
    }
}