using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour {

    public enum RotationAxes { MouseXAndY = 0, MouseX = 1, MouseY = 2 }
    public RotationAxes axes = RotationAxes.MouseXAndY;
    public float sensitivityX = 3f;
    public float sensitivityY = 3f;

    public float minimumX = -360F;
    public float maximumX = 360F;

    public float minimumY = -60F;
    public float maximumY = 60F;

    public bool reverseX = false;
    public bool reverseY = false;
    public bool swapXY = false;

    float rotationX = 0F;
    float rotationY = 0F;

    private List<float> rotArrayX = new List<float>();
    float rotAverageX = 0F;

    private List<float> rotArrayY = new List<float>();
    float rotAverageY = 0F;

    public float frameCounter = 5;

    Quaternion originalRotation;

    private void FixedUpdate() {
        if (axes == RotationAxes.MouseXAndY) {
            rotAverageY = 0f;
            rotAverageX = 0f;

            rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
            rotationX += Input.GetAxis("Mouse X") * sensitivityX;

            rotArrayY.Add(rotationY);
            rotArrayX.Add(rotationX);

            if (rotArrayY.Count >= frameCounter) {
                rotArrayY.RemoveAt(0);
            }
            if (rotArrayX.Count >= frameCounter) {
                rotArrayX.RemoveAt(0);
            }

            for (int j = 0; j < rotArrayY.Count; j++) {
                rotAverageY += rotArrayY[j];
            }
            for (int i = 0; i < rotArrayX.Count; i++) {
                rotAverageX += rotArrayX[i];
            }

            rotAverageY /= rotArrayY.Count;
            rotAverageX /= rotArrayX.Count;

            if (reverseX) rotAverageX *= -1.0f;
            if (reverseY) rotAverageY *= -1.0f;

            rotAverageY = ClampAngle(rotAverageY, minimumY, maximumY);
            rotAverageX = ClampAngle(rotAverageX, minimumX, maximumX);

            if (swapXY) {
                Quaternion yQuaternion = Quaternion.AngleAxis(rotAverageY, Vector3.up);
                Quaternion xQuaternion = Quaternion.AngleAxis(rotAverageX, Vector3.left);
                transform.localRotation = originalRotation * xQuaternion * yQuaternion;
            } else {
                Quaternion yQuaternion = Quaternion.AngleAxis(rotAverageY, Vector3.left);
                Quaternion xQuaternion = Quaternion.AngleAxis(rotAverageX, Vector3.up);
                transform.localRotation = originalRotation * xQuaternion * yQuaternion;
            }

        } else if (axes == RotationAxes.MouseX) {
            rotAverageX = 0f;

            rotationX += Input.GetAxis("Mouse X") * sensitivityX;

            rotArrayX.Add(rotationX);

            if (rotArrayX.Count >= frameCounter) {
                rotArrayX.RemoveAt(0);
            }
            for (int i = 0; i < rotArrayX.Count; i++) {
                rotAverageX += rotArrayX[i];
            }
            rotAverageX /= rotArrayX.Count;

            if (reverseX) rotAverageX *= -1.0f;

            rotAverageX = ClampAngle(rotAverageX, minimumX, maximumX);

            if (swapXY) {
                Quaternion xQuaternion = Quaternion.AngleAxis(rotAverageX, Vector3.left);
                transform.localRotation = originalRotation * xQuaternion;
            } else {
                Quaternion xQuaternion = Quaternion.AngleAxis(rotAverageX, Vector3.up);
                transform.localRotation = originalRotation * xQuaternion;
            }
        } else {
            rotAverageY = 0f;

            rotationY += Input.GetAxis("Mouse Y") * sensitivityY;

            rotArrayY.Add(rotationY);

            if (rotArrayY.Count >= frameCounter) {
                rotArrayY.RemoveAt(0);
            }
            for (int j = 0; j < rotArrayY.Count; j++) {
                rotAverageY += rotArrayY[j];
            }
            rotAverageY /= rotArrayY.Count;

            if (reverseY) rotAverageY *= -1.0f;

            rotAverageY = ClampAngle(rotAverageY, minimumY, maximumY);

            if (swapXY) {
                Quaternion yQuaternion = Quaternion.AngleAxis(rotAverageY, Vector3.up);
                transform.localRotation = originalRotation * yQuaternion;
            } else {
                Quaternion yQuaternion = Quaternion.AngleAxis(rotAverageY, Vector3.left);
                transform.localRotation = originalRotation * yQuaternion;
            }
        }
    }

    void Start() {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb)
            rb.freezeRotation = true;
        originalRotation = transform.localRotation;
    }

    public static float ClampAngle(float angle, float min, float max) {
        angle = angle % 360;
        if ((angle >= -360F) && (angle <= 360F)) {
            if (angle < -360F) {
                angle += 360F;
            }
            if (angle > 360F) {
                angle -= 360F;
            }
        }
        return Mathf.Clamp(angle, min, max);
    }

}
