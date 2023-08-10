using UnityEngine;

namespace QFrameworkRemasteredRPGGame
{
    [CreateAssetMenu(fileName = "PlayerState_Run", menuName = "Data/StateMachine/PlayerState/Run")]
    public class PlayerState_Run : PlayerState
    {
        public override void OnLogicUpdate()
        {
            if (!m_PlayerCtrl.IsMove)
            {
                m_PlayerStateMachine.SwitchState(typeof(PlayerState_Idle));
            }

            if (m_PlayerCtrl.IsAttack)
            {
                m_PlayerStateMachine.SwitchState(typeof(PlayerState_Attack));
            }
        }
    }
}