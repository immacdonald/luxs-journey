﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

    public Transform positionTarget;
    public Transform rotationTarget;

    public Transform cameraTransform;
    public Transform idealTarget;

    //private Vector3 offsetPosition;
    public float moveSpeed = 5;
    public float turnSpeed = 10;
    public float smoothSpeed = 0.5f;

    Quaternion targetRotation;
    Vector3 targetPosition;

    public LayerMask layerMask;
    public float maximumHeightAbove = 5;

    public float sharpRotationAngle;
    public float trueRotationAngle;

    public CameraControllerSettings defaultCameraSettings;
    [HideInInspector]
    public CameraControllerSettings cameraSettings;

    bool fullControl;
    Vector3 chosenAngles;
    Vector3 idealPosition;

    private void Start() {
        cameraSettings = defaultCameraSettings;
        //offsetPosition = cameraSettings.offsetPosition;
        cameraTransform = Camera.main.transform;
        idealTarget = GameObject.Find("Ideal Position").transform;
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.G)) {
            sharpRotationAngle += 45;
        }

        if (Input.GetKeyDown(KeyCode.F)) {
            fullControl = !fullControl;
            Cursor.visible = !fullControl;
            if (fullControl) {
                Cursor.lockState = CursorLockMode.Locked;
            }
            else {
                Cursor.lockState = CursorLockMode.None;
            }
        }

        if(fullControl) {
            chosenAngles.x = Mathf.Clamp(chosenAngles.x + Input.GetAxis("Mouse Y") * 1, -20, 30);
            chosenAngles.y += Input.GetAxis("Mouse X") * 2;
            chosenAngles.y += Input.GetAxis("Horizontal");
        }
    }

    private void LateUpdate() {
        MoveWithTarget(fullControl);
        trueRotationAngle = cameraTransform.rotation.eulerAngles.y;
    }

    private void MoveWithTarget(bool freeform) {
        transform.position = Vector3.Lerp(transform.position, positionTarget.position, moveSpeed * Time.deltaTime);

        RaycastHit linecastHit;
        float maxPotentialDistance = Vector3.Distance(positionTarget.position, idealTarget.position);
        float distance = maxPotentialDistance;
        Vector3 localCamera = cameraSettings.offsetPosition;
        if (Physics.Linecast(positionTarget.position, idealTarget.position, out linecastHit, layerMask)) {
            //Debug.Log("blocked");
            localCamera = transform.InverseTransformPoint(linecastHit.point);
            distance = linecastHit.distance;
        }
        //Vector3 localCamera = cameraSettings.offsetPosition.normalized * (distance - 0.25f);
        localCamera.y = Mathf.Max(maximumHeightAbove, localCamera.y);
        cameraTransform.localPosition = Vector3.Lerp(cameraTransform.localPosition, localCamera, moveSpeed * Time.deltaTime); ;
        idealTarget.localPosition = cameraSettings.offsetPosition;
        cameraTransform.localRotation = Quaternion.Slerp(cameraTransform.localRotation, Quaternion.Euler(Mathf.Lerp(18, 60, Mathf.InverseLerp(maxPotentialDistance, 0, distance)), 0, 0), turnSpeed * Time.deltaTime);
        Quaternion intendedRotation = Quaternion.identity;
        if (freeform) {
            sharpRotationAngle = chosenAngles.y;
            intendedRotation = Quaternion.Euler(chosenAngles);
        } else {
            intendedRotation = Quaternion.Euler(0, sharpRotationAngle, 0);
        }
        transform.rotation = Quaternion.Slerp(transform.rotation, intendedRotation, turnSpeed * Time.deltaTime);
    }


    public Vector3 MatrixMagicRotate(Vector3 pivot, float distance, Vector3 angles) {
        Vector3 distance_to_target = new Vector3(0, 0, -distance); // distance the camera should be from target
        Matrix4x4 t = Matrix4x4.TRS(pivot, Quaternion.Euler(angles), Vector3.one);
        return t.MultiplyPoint(distance_to_target);
    }


    public void SwapCameraControllerSettings(CameraControllerSettings settings) {
        cameraSettings = (settings != null) ? settings : defaultCameraSettings;
        if(cameraSettings.angleOverride) {
            sharpRotationAngle = cameraSettings.sharpRotationAngle;
            chosenAngles.y = sharpRotationAngle;
            chosenAngles.x = 0;
        }
        fullControl = cameraSettings.allowControl;
    }
}

[System.Serializable]
public class CameraControllerSettings {
    public bool followPlayer = true;
    public Vector3 offsetPosition;
    public bool angleOverride;
    public float sharpRotationAngle;
    public bool allowControl;
}
