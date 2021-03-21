﻿using UnityEngine;
using System;

namespace ViewSpace
{
    public class ViewFacade : MonoBehaviour, IViewFacade
    {
        [Header("References")]
        [SerializeField] Camera cam;

        [Header("Move variables")]
        [SerializeField] float orbitSpeed = 100f;
        [SerializeField] float rotSpeed = 1f;
        [SerializeField] float moveSpeed = 1f;
        private bool focused = false;

        private IViewMove viewMove;
        private OrbitMove orbitMove;
        private FlyMove flyMove;

        private Vector3 nearTopLeft;
        private Vector3 nearTopRight;
        private Vector3 nearBottomRight;
        private Vector3 nearBottomLeft;
        private Vector3 farTopLeft;
        private Vector3 farTopRight;
        private Vector3 farBottomRight;
        private Vector3 farBottomLeft;
        private bool changed;

        public Vector3 NearTopLeft { get => nearTopLeft; set { if (!nearTopLeft.Equals(value)) { nearTopLeft = value; changed = true; } } }
        public Vector3 NearTopRight { get => nearTopRight; set { if (!nearTopRight.Equals(value)) { nearTopRight = value; changed = true; } } }
        public Vector3 NearBottomRight { get => nearBottomRight; set { if (!nearBottomRight.Equals(value)) { nearBottomRight = value; changed = true; } } }
        public Vector3 NearBottomLeft { get => nearBottomLeft; set { if (!nearBottomLeft.Equals(value)) { nearBottomLeft = value; changed = true; } } }
        public Vector3 FarTopLeft { get => farTopLeft; set { if (!farTopLeft.Equals(value)) { farTopLeft = value; changed = true; } } }
        public Vector3 FarTopRight { get => farTopRight; set { if (!farTopRight.Equals(value)) { farTopRight = value; changed = true; } } }
        public Vector3 FarBottomRight { get => farBottomRight; set { if (!farBottomRight.Equals(value)) { farBottomRight = value; changed = true; } } }
        public Vector3 FarBottomLeft { get => farBottomLeft; set { if (!farBottomLeft.Equals(value)) { farBottomLeft = value; changed = true; } } }
        public event Action onChanged;

        public void UseOrbitMove()
        {
            viewMove = orbitMove;
        }
        public void UseFlyMove()
        {
            viewMove = flyMove;
        }

        void Start()
        {
            cam.transform.LookAt(Vector3.zero);
            orbitMove = new OrbitMove(orbitSpeed, cam.transform);
            flyMove = new FlyMove(rotSpeed, moveSpeed, cam.transform);
            viewMove = orbitMove;
        }

        private void LateUpdate()
        {
            if (focused)
                viewMove.TryMove();

            BeginModification();
            CalculatePlanes();
            EndModification();
        }

        private void BeginModification()
        {
            changed = false;
        }
        private void EndModification()
        {
            if (changed)
                onChanged?.Invoke();
            changed = false;
        }

        private void CalculatePlanes()
        {
            float near = cam.nearClipPlane;
            float far = cam.farClipPlane;
            Vector3 nearCenter = cam.transform.position + cam.transform.forward * near;
            if (cam.orthographic)
                CalculatePlanesOrtographic(near, far, nearCenter);
            else
                CalculatePlanesPerspective(near, far, nearCenter);
        }

        private void CalculatePlanesOrtographic(float near, float far, Vector3 nearCenter)
        {

            float ortoSize = cam.orthographicSize;
            NearTopLeft = nearCenter - ortoSize * cam.transform.right + cam.transform.up * ortoSize;
            NearTopRight = nearCenter + ortoSize * cam.transform.right + cam.transform.up * ortoSize;
            NearBottomRight = nearCenter + ortoSize * cam.transform.right - cam.transform.up * ortoSize;
            NearBottomLeft = nearCenter - ortoSize * cam.transform.right - cam.transform.up * ortoSize;
            FarTopLeft = NearTopLeft + cam.transform.forward * (far - near);
            FarTopRight = NearTopRight + cam.transform.forward * (far - near);
            FarBottomRight = NearBottomRight + cam.transform.forward * (far - near);
            FarBottomLeft = NearBottomLeft + cam.transform.forward * (far - near);
        }

        private void CalculatePlanesPerspective(float near, float far, Vector3 nearCenter)
        {
            float fov = cam.fieldOfView;
            Vector3 farCenter = cam.transform.position + cam.transform.forward * far;
            float fovRad = fov * Mathf.PI / 180f;
            float nearSize = Mathf.Tan(fovRad * 0.5f) * near;
            float farSize = Mathf.Tan(fovRad * 0.5f) * far;
            NearTopLeft = nearCenter - nearSize * cam.transform.right + cam.transform.up * nearSize;
            NearTopRight = nearCenter + nearSize * cam.transform.right + cam.transform.up * nearSize;
            NearBottomRight = nearCenter + nearSize * cam.transform.right - cam.transform.up * nearSize;
            NearBottomLeft = nearCenter - nearSize * cam.transform.right - cam.transform.up * nearSize;
            FarTopLeft = farCenter - farSize * cam.transform.right + cam.transform.up * farSize;
            FarTopRight = farCenter + farSize * cam.transform.right + cam.transform.up * farSize;
            FarBottomRight = farCenter + farSize * cam.transform.right - cam.transform.up * farSize;
            FarBottomLeft = farCenter - farSize * cam.transform.right - cam.transform.up * farSize;
        }
    }
}
