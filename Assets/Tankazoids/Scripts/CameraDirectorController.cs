using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraDirectorController : MonoBehaviour
{
    public GameObject followingObject;
    public float mousePlayerInterpolation;
    public float dampening;
    public Vector2 maxViewPort;

    public Camera myCamera;

    private Vector3 _velocity = Vector3.zero;

    void Update()
    {
        if (followingObject == null)
        {
            Destroy(gameObject);
            return;
        }
        Vector3 viewPortCoords = myCamera.ScreenToViewportPoint(Input.mousePosition);

        // we gotta clamp the view port coords or else multi montitor setups would be op!!!
        Vector3 worldPointCoords = myCamera.ViewportToWorldPoint(new Vector3(Mathf.Clamp(viewPortCoords.x, 0, maxViewPort.x), Mathf.Clamp(viewPortCoords.y, 0, maxViewPort.y), -myCamera.transform.position.z));

        Vector3 mouseWorldCoords = new(worldPointCoords.x, worldPointCoords.y, 0);
        Vector3 targetLookAtPosition = mouseWorldCoords * (mousePlayerInterpolation) + (followingObject.transform.position * (1 - mousePlayerInterpolation));

        transform.position = Vector3.SmoothDamp(transform.position, targetLookAtPosition, ref _velocity, dampening);
    }
}
