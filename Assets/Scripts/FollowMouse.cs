using UnityEngine;

public class FollowMouse : MonoBehaviour
{
    [SerializeField] private float sensitivity = 200.0f;
    [SerializeField] private float angleClamp = 80.0f;
    private Vector3 currenRotation;

    // Use this for initialization
    void Start()
    {
        currenRotation = transform.localRotation.eulerAngles;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        float xPos = Input.GetAxis("Mouse X");
        float yPos = -Input.GetAxis("Mouse Y");

        currenRotation.y += xPos * sensitivity * Time.deltaTime;
        currenRotation.x += yPos * sensitivity * Time.deltaTime;

        currenRotation.x = Mathf.Clamp(currenRotation.x, -angleClamp, angleClamp);

        Quaternion localRotation = Quaternion.Euler(currenRotation.x, currenRotation.y, 0.0f);
        transform.rotation = localRotation;
    }
}
