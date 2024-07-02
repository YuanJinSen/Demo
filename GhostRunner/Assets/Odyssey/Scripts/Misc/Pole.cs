using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Odyssey
{
    public class Pole : MonoBehaviour
    {
        public new CapsuleCollider collider { get; private set; }
        public float radius => collider.radius;
        public Vector3 center => collider.center;

        #region Unity

        private void Awake()
        {
            tag = GameTag.Pole;
            collider = GetComponent<CapsuleCollider>();
        }

        #endregion

        #region Public

        public Vector3 GetDirectionToPole(Transform other, out float dis)
        {
            Vector3 target = center - other.position;
            target.y = 0;
            dis = target.magnitude;
            return target / dis;
        }

        public Vector3 ClampPointToPoleHeight(Vector3 point, float offset)
        {
            float minHeight = collider.bounds.min.y + offset;
            float maxHeight = collider.bounds.max.y - offset;
            float clampedHeight = Mathf.Clamp(point.y, minHeight, maxHeight);
            return new Vector3(point.x, clampedHeight, point.z);
        }

        #endregion
    }
}