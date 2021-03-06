﻿using System;
using System.Collections;
using System.Collections.Generic;
using Core.Camera;

using UnityEngine;


public class RSTCamera : MonoBehaviour {


    public float HorSpeed = 10f;//移动速度

    public float VerSpeed = 10f;//垂直移速

    private Transform m_transform;

    private CameraRig cameraRig;

    //Use this for initializationvoid 
    void Start()
    {
        m_transform = Camera.main.transform;
        cameraRig = GetComponent<CameraRig>();
    }


    //Update is called once per framevoid 
    void Update(){
        Vector3 v1 = Camera.main.ScreenToViewportPoint(Input.mousePosition);
        Vector3 dir_forward = m_transform.forward;
        Vector3 dir_right = m_transform.right;

        //消除高度变化
        dir_right.y = 0;
        dir_forward.y = 0;
        dir_forward = dir_forward.normalized;
        dir_right = dir_right.normalized;


         MouseKeyboardMove(v1, dir_forward, dir_right);

    }


    private void DragMove(Vector3 dir_forward, Vector3 dir_right,float deltaX,float deltaY)
    {

        if (deltaY != 0)
        {
            cameraRig.PanCamera(dir_forward * VerSpeed * deltaY * Time.deltaTime);

        }
        if (deltaX != 0)
        {
            cameraRig.PanCamera(dir_right * HorSpeed * deltaX * Time.deltaTime);
        }
    }


    //pc端移动视角
    private void MouseKeyboardMove(Vector3 v1, Vector3 dir_forward, Vector3 dir_right)
    {
        //鼠标移动屏幕边界自动移动
        /*if (v1.x < 0.05f)
        {
            m_transform.Translate(-dir_right * HorSpeed * Time.deltaTime, Space.World);
        }
        if (v1.x > 1 - 0.05f)
        {
            m_transform.Translate(dir_right * HorSpeed * Time.deltaTime, Space.World);
        }

        if (v1.y < 0.05f)
        {
            m_transform.Translate(-dir_forward * VerSpeed * Time.deltaTime, Space.World);
        }
        if (v1.y > 1 - 0.05f)
        {
            m_transform.Translate(dir_forward * VerSpeed * Time.deltaTime, Space.World);
        }*/
        //键盘输入
        DragMove(dir_forward, dir_right, Input.GetAxis("Horizontal"),Input.GetAxis("Vertical"));

    }



}
