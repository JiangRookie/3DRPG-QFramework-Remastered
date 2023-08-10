using UnityEngine;

namespace QFrameworkRemasteredRPGGame
{
    [CreateAssetMenu(fileName = "PlayerState_Idle", menuName = "Data/StateMachine/PlayerState/Idle")]
    public class PlayerState_Idle : PlayerState
    {
        public override void OnLogicUpdate()
        {
            if (m_PlayerCtrl.IsMove)
            {
                m_PlayerStateMachine.SwitchState(typeof(PlayerState_Run));
            }

            if (m_PlayerCtrl.IsAttack)
            {
                m_PlayerStateMachine.SwitchState(typeof(PlayerState_Attack));
            }
        }
    }
}