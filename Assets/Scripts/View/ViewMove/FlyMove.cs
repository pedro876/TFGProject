using System.Collections;
using UnityEngine;

namespace ViewSpace
{
    public class FlyMove : IViewMove
    {
        private float x, y;
        private float forwardMovement;
        private float horizontalMovement;
        Transform camTransform;

        public float RotSpeed { get; set; }
        public float MoveSpeed { get; set; }

        public FlyMove(float rotSpeed, float moveSpeed, Transform camTransform)
        {
            RotSpeed = rotSpeed;
            MoveSpeed = moveSpeed;
            this.camTransform = camTransform;
        }

        public bool TryMove()
        {
            bool moved = GetInput();
            if (moved)
            {
                Move();
            }
            return moved;
        }

        private bool GetInput()
        {
            x = Input.GetAxis("Mouse X");
            y = Input.GetAxis("Mouse Y");
            forwardMovement = Input.GetAxis("Vertical");
            horizontalMovement = Input.GetAxis("Horizontal");
            bool changed = false;
            if (forwardMovement != 0f || horizontalMovement != 0f) changed = true;
            if (Mathf.Abs(x) > 0.001f || Mathf.Abs(y) > 0.001f) changed = true;
            return changed;
        }

        private void Move()
        {
            Vector3 upVec = Vector3.up;
            float degreesXToRot = x * RotSpeed * Time.fixedDeltaTime;
            camTransform.RotateAround(camTransform.position, upVec, degreesXToRot);
            float degreesYToRot = -y * RotSpeed * Time.fixedDeltaTime;
            degreesYToRot = LimitDegreesY(degreesYToRot);
            camTransform.RotateAround(camTransform.position, camTransform.right, degreesYToRot);
            camTransform.position += camTransform.forward * forwardMovement * MoveSpeed * Time.fixedDeltaTime;
            camTransform.position += camTransform.right * horizontalMovement * MoveSpeed * Time.fixedDeltaTime;
        }

        private float LimitDegreesY(float degreesToRot)
        {
            const float limit = 89.5f;
            float currentDegrees = Vector3.SignedAngle(camTransform.position, Vector3.ProjectOnPlane(camTransform.position, Vector3.up), -camTransform.right);
            if (currentDegrees + degreesToRot >= limit && degreesToRot > 0f)
            {
                degreesToRot = limit - currentDegrees;
            }
            else if (currentDegrees + degreesToRot <= -limit && degreesToRot < 0f)
            {
                degreesToRot = -limit - currentDegrees;
            }
            return degreesToRot;
        }
    }
}