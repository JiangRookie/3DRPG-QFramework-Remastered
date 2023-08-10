using UnityEngine;

namespace QFrameworkRemasteredRPGGame
{
    [CreateAssetMenu(fileName = "PlayerState_Attack", menuName = "Data/StateMachine/PlayerState/Attack")]
    public class PlayerState_Attack : PlayerState
    {
        public override void OnLogicUpdate()
        {
            if (IsAnimationFinished)
            {
                m_PlayerCtrl.IsAttack = false;

                if (!m_PlayerCtrl.IsMove)
                {
                    m_PlayerStateMachine.SwitchState(typeof(PlayerState_Idle));
                }
                else
                {
                    m_PlayerStateMachine.SwitchState(typeof(PlayerState_Run));
                }
            }
        }
    }
}