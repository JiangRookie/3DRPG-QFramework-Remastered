using UnityEngine;

namespace QFrameworkRemasteredRPGGame
{
    public class PlayerCtrl : MonoBehaviour
    {
        public float MoveSpeed = 5f;
        public float TurnSpeed = 1000f;

        public bool IsMove { get; private set; }
        public bool IsAttack { get; set; }

        void Update()
        {
            float horizontalInput = Input.GetAxis("Horizontal");
            float verticalInput = Input.GetAxis("Vertical");

            IsMove = horizontalInput != 0 || verticalInput != 0;

            Vector3 moveDirection = new Vector3(horizontalInput, 0.0f, verticalInput);
            moveDirection.Normalize();

            transform.Translate(moveDirection * (MoveSpeed * Time.deltaTime));

            float mouseX = Input.GetAxis("Mouse X");

            transform.Rotate(Vector3.up * (mouseX * TurnSpeed * Time.deltaTime));

            if (Input.GetMouseButtonDown(0) && !IsAttack)
            {
                IsAttack = true;
            }
        }
    }
}