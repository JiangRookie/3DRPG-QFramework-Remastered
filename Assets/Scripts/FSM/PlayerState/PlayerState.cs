using UnityEngine;

namespace QFrameworkRemasteredRPGGame
{
    public class PlayerState : ScriptableObject, IState
    {
        protected PlayerStateMachine m_PlayerStateMachine;
        protected PlayerCtrl m_PlayerCtrl;
        Animator m_Animator;

        [SerializeField] string m_StateName;
        [SerializeField, Range(0f, 1f)] float m_TransitionDuration = 0.1f;
        int m_StateHash;
        float m_StateStartTime;
        protected float StateDuration => Time.time - m_StateStartTime;
        protected bool IsAnimationFinished => StateDuration >= m_Animator.GetCurrentAnimatorStateInfo(0).length;

        void OnEnable()
        {
            m_StateHash = Animator.StringToHash(m_StateName);
        }

        public void InitComponent(Animator animator, PlayerStateMachine playerStateMachine, PlayerCtrl playerCtrl)
        {
            m_Animator = animator;
            m_PlayerStateMachine = playerStateMachine;
            m_PlayerCtrl = playerCtrl;
        }

        public virtual void OnEnter()
        {
            // 使用标准化时间创建从当前状态到任何其他状态的淡入淡出效果。
            m_Animator.CrossFade(m_StateHash, m_TransitionDuration);

            // 记录状态开始的时间
            m_StateStartTime = Time.time;
        }

        public virtual void OnLogicUpdate() { }
        public void OnPhysicUpdate() { }
        public virtual void OnExit() { }
    }
}