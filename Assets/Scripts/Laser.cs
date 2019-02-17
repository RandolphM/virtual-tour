using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//[RequireComponent(typeof(LineRenderer))]
public class Laser : SteamVR_TrackedController
{
    LineRenderer line;
    GameObject currFocus = null;

    public GameObject ray;
    public GameObject reticle = null;
    public AudioSource menuClick = null;
    public bool isColliding;
    public float angle = 45;
    public int lengthOfRay = 10;
    public Vector3 reticleLength = new Vector3(0, 0, 2);

    private void Awake()
    {
        this.TriggerClicked += OnClicked;
        this.TriggerUnclicked += OnUnClicked;
        reticle.SetActive(true);
        line = ray.GetComponent<LineRenderer>();
    }

    void OnClicked(object sender, ClickedEventArgs e)
    {
        if (isColliding == true)
             menuClick.Play();
    }

    void OnUnClicked(object sender, ClickedEventArgs e)
    {
        //line.enabled = false;

        Vector3 cVel = SteamVR_Controller.Input((int)this.controllerIndex).velocity;
        Vector3 aVel = SteamVR_Controller.Input((int)this.controllerIndex).angularVelocity;

        if (currFocus != null)
        {
            Interactive l = currFocus.GetComponent<Interactive>();
            if (l != null)
            {
                if (l.type == Interactive.InteractiveTypes.Grab)
                {
                    l.Release(cVel);
                }

                //if (l.type == Interactive.InteractiveTypes.Marker)
                //{
                //    AnotationManager canvs = currFocus.GetComponent<AnotationManager>();
                //    canvs.single.SetActive(false);
                //}

                if (l.type == Interactive.InteractiveTypes.Button)
                {
                    Button b = l.gameObject.GetComponent<Button>();
                    if (b != null)
                    {
                        b.onClick.Invoke();
                    }
                }
            }
        }
    }


    // Update is called once per frame
    protected override void Update()
    {
        base.Update();



        //line.SetPosition(0, this.transform.position);
        //Quaternion q = Quaternion.AngleAxis(angle, transform.right);
        //Vector3 v = q * (this.transform.forward * lengthOfRay);
        //Vector3 targetPos = this.transform.position + v;
        //line.SetPosition(1, targetPos);

        // hold info for what ever the ray hits
        RaycastHit hit;
        if (Physics.Raycast(this.transform.position, ray.transform.forward, out hit, lengthOfRay))
        {
            line.SetPosition(1, ray.transform.InverseTransformPoint(hit.point));

            currFocus = hit.collider.gameObject;

            if (currFocus.gameObject.tag != "No Interaction")
            {
                isColliding = true;
                SteamVR_Controller.Input((int)this.controllerIndex).TriggerHapticPulse(1000);
                reticle.transform.position = hit.point;// + this.transform.position;
                ray.SetActive(true);
            }
            else
            {
                reticle.transform.position = ray.transform.forward + this.transform.position;
                isColliding = false;
                ray.SetActive(false);
            }


            //print(hit.collider.gameObject.name);
        }
        else
        {
            currFocus = null;
            reticle.transform.position = ray.transform.forward + this.transform.position;
            ray.SetActive(false);
            isColliding = false;
        }
    }

    public override void OnTriggerClicked(ClickedEventArgs e)
    {
        base.OnTriggerClicked(e);

        if (currFocus != null)
        {
            Interactive l = currFocus.GetComponent<Interactive>();

            if (l != null)
            {
                if (l.type == Interactive.InteractiveTypes.Grab)
                    l.Grab(this.transform);

                if (l.type == Interactive.InteractiveTypes.Marker)
                {
                    AnotationManager canvs = currFocus.GetComponent<AnotationManager>();
                }

                if (l.type == Interactive.InteractiveTypes.ObjButton)
                    l.ObjectClick();

                if (l.type == Interactive.InteractiveTypes.Canvas)
                    l.ShowCanvas();

            }


        }
    }
}
