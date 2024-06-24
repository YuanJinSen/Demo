using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Car
{
    public class Car : MonoBehaviour
    {
        public Transform[] wheelTransforms;
        public WheelCollider[] wheelColliders;
        public int maxAngle = 30;
        public int rotateSpeed = 10;
        public float motorTorque = 10;

        private int dir = 0;

        private void Start()
        {
            //rigidbody = GetComponent<Rigidbody>();
            if (wheelTransforms.Length < 4 || wheelColliders.Length < 4)
                throw new Exception("轮子或轮子碰撞体数量小于4");
        }

        private void LateUpdate()
        {
            Power();
            Rotate();

            View();

        }

        private void View()
        {
            for (int i = 0; i < 4; i++)
            {
                Quaternion qua;
                Vector3 pos;
                wheelColliders[i].GetWorldPose(out pos, out qua);
                wheelTransforms[i].position = pos;
                wheelTransforms[i].rotation = qua;
            }
        }

        private void Rotate()
        {
            dir = 0;
            if (Input.GetKey(KeyCode.A)) dir -= 1;
            if (Input.GetKey(KeyCode.D)) dir += 1;
            float angle = wheelColliders[0].steerAngle + dir * rotateSpeed * Time.deltaTime;
            angle = Mathf.Clamp(angle, -maxAngle, maxAngle);
            wheelColliders[0].steerAngle = angle;
            wheelColliders[1].steerAngle = angle;
            //Quaternion target = Quaternion.Euler(0, targetAngle, 0);
            //transform.rotation = Quaternion.RotateTowards(transform.rotation, target, Time.deltaTime * rotateSpeed);
        }

        private void Power()
        {
            float temp = 0;
            if (Input.GetKey(KeyCode.W)) temp = motorTorque;
            wheelColliders[2].motorTorque = temp;
            wheelColliders[3].motorTorque = temp;
        }
    }
}