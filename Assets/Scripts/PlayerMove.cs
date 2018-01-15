using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour {

    public float walkSpeed = 2.0f;
    public float jumpPower = 75.0f;

    public bool enableJump = false;

    /// <summary>
    /// FIRST PERSON VIEW SETTING VARIABLES
    /// </summary>
    public enum RotationAxes { MouseXAndY = 0, MouseX = 1, MouseY = 2 }
    public RotationAxes axes = RotationAxes.MouseXAndY;

    public float sensitivityX = 3f;
    public float sensitivityY = 3f;

    public float minimumX = -360F;
    public float maximumX = 360F;

    public float minimumY = -60F;
    public float maximumY = 60F;

    float rotationX = 0F;
    float rotationY = 0F;

    private List<float> rotArrayX;
    float rotAverageX = 0F;

    private List<float> rotArrayY;
    float rotAverageY = 0F;

    public float frameCounter = 5;

    private Quaternion originalRotation;
    // END

    private Rigidbody rb;
    private Animator animator;

    private Vector3 move;
    private bool isMoving = false;

    // GameManager
    private BlockManager bm;

    public CameraMovement head;

    private void Start() {

        rotArrayX = new List<float>();
        rotArrayY = new List<float>();

        rb = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();
        originalRotation = transform.localRotation;

        bm = GameObject.FindGameObjectWithTag("GameController").GetComponent<BlockManager>();

    }

    private void Update() {

        bm.UpdatePlayerPosition(transform.position);

    }

    private void FixedUpdate () {
        
        // Handle Moving with W, A, S, D
        move = (transform.forward * Input.GetAxis("Vertical")
                     + transform.right * Input.GetAxis("Horizontal")) * walkSpeed;
        transform.position += move * Time.deltaTime;
        
        isMoving = (move.magnitude > 0.1f);
        animator.SetBool("Moving", isMoving);

        // Jump Check
        enableJump = Physics.Raycast(transform.position + Vector3.up, -Vector3.up, 0.7f);

        // Handle Jump with Space
        if (enableJump) {
            var jumpForce = transform.up * Input.GetAxis("Jump") * jumpPower;
            rb.AddForce(jumpForce);
        }

        // Rotation & Camera View with Mouse Input
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

            rotAverageY = ClampAngle(rotAverageY, minimumY, maximumY);
            rotAverageX = ClampAngle(rotAverageX, minimumX, maximumX);

            Quaternion yQuaternion = Quaternion.AngleAxis(rotAverageY, Vector3.left);
            Quaternion xQuaternion = Quaternion.AngleAxis(rotAverageX, Vector3.up);

            transform.localRotation = originalRotation * xQuaternion * yQuaternion;
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

            rotAverageX = ClampAngle(rotAverageX, minimumX, maximumX);

            Quaternion xQuaternion = Quaternion.AngleAxis(rotAverageX, Vector3.up);
            
            transform.localRotation = originalRotation * xQuaternion;
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

            rotAverageY = ClampAngle(rotAverageY, minimumY, maximumY);

            Quaternion yQuaternion = Quaternion.AngleAxis(rotAverageY, Vector3.left);
            transform.localRotation = originalRotation * yQuaternion;
        }

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
