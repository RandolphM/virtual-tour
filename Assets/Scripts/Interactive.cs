/// <summary>
/// Synopsis: This isn't being used anymore.
/// CREATED BY BENJAMIN MILIAN. FOLLOW MY INSTAGRAM
/// OR BE BANISHED TO THE WONDERFULL WORLD OF 
/// HOGWARTS OR THE GALACTIC EMPIRE OR THE HOBBIT
/// YOU PICK I DON'T CARE... @BENMILIAN XD
/// </summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Interactive : MonoBehaviour
{
    public enum InteractiveTypes
    {
        Button = 0,
        Grab,
        Marker,
        ObjButton,
        Canvas,
        None = 5000
    };
   
    public InteractiveTypes type = InteractiveTypes.None;
    public Vector3 endPosition = Vector3.zero;
    public Button objButton = null;
    public GameObject presentation = null;

    
    public void InvokeButtonClick()
    {
        Button butt = this.GetComponent<Button>();

        if (butt != null)
        {
            butt.onClick.Invoke();
        }
    }

    public void ObjectClick()
    {
        if (objButton != null)
            objButton.onClick.Invoke();
    }

    public void ShowCanvas()
    {
        if (presentation != null)
            presentation.SetActive(true);
    }

    public void SetActive(GameObject _annotation)
    {
        _annotation.SetActive(true);
    }

    public void Deactivate(GameObject _annotation)
    {
        _annotation.SetActive(false);
    }

    // use generic names
    public virtual void Grab(Transform _controller)
    {

        this.transform.SetParent(_controller, true);
        Rigidbody rgb = this.GetComponent<Rigidbody>();
        if (rgb != null)
            rgb.isKinematic = true;
    }

    public virtual void Release(Vector3 _controller)
    {
        endPosition = this.transform.position;
        this.transform.SetParent(null, true);
        this.transform.position = endPosition;
        Rigidbody rgb = this.GetComponent<Rigidbody>();
        if (rgb != null)
        {
            rgb.isKinematic = false;
            rgb.AddForce(_controller * 10, ForceMode.VelocityChange);
        }

    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.Space))
            InvokeButtonClick();
    }

}
