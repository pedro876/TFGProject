using System.Collections;
using UnityEngine;

namespace ViewSpace
{
    public class OrbitMove : IViewMove
    {
        private float orbitDistance;
        private float x, y;
        private Transform camTransform;

        public float OrbitSpeed { get; set; }


        public OrbitMove(float orbitSpeed, Transform camTransform)
        {
            OrbitSpeed = orbitSpeed;
            this.camTransform = camTransform;
            orbitDistance = camTransform.position.magnitude;
        }

        public bool TryMove()
        {
            bool moved = GetInput();
            if (moved)
            {
                Move();
                LimitDistance();
            }
            return moved;
        }

        private bool GetInput()
        {
            x = Input.GetAxis("Mouse X");
            y = Input.GetAxis("Mouse Y");
            return (Mathf.Abs(x) > 0.001f || Mathf.Abs(y) > 0.001f);
        }

        private void Move()
        {
            Vector3 zeroVec = Vector3.zero;
            Vector3 upVec = Vector3.up;
            camTransform.LookAt(zeroVec);
            float degreesXToRot = x * OrbitSpeed * Time.fixedDeltaTime;
            camTransform.RotateAround(zeroVec, upVec, degreesXToRot);
            float degreesYToRot = -y * OrbitSpeed * Time.fixedDeltaTime;
            degreesYToRot = LimitDegreesY(degreesYToRot);
            camTransform.RotateAround(zeroVec, camTransform.right, degreesYToRot);
            camTransform.LookAt(Vector3.zero);
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

        private void LimitDistance()
        {
            camTransform.position = camTransform.position.normalized * orbitDistance;
        }
    }
}