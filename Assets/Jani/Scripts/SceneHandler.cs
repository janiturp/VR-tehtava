using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Valve.VR.Extras;

public class SceneHandler : MonoBehaviour
{
    public SteamVR_LaserPointer laserPointer;

    private void Awake()
    {
        laserPointer.PointerIn += PointerInside;
        laserPointer.PointerOut += PointerOutside;
        laserPointer.PointerClick += PointerClick;
    }

    public void PointerClick(object sender, PointerEventArgs e)
    {
        if(e.target.CompareTag("Grab"))
        {
            Debug.Log(e.target + "clicked.");
        }
    }

    public void PointerInside(object sender, PointerEventArgs e)
    {
        if (e.target.CompareTag("Grab"))
        {
            Debug.Log(e.target + "entered.");
        }
    }

    public void PointerOutside(object sender, PointerEventArgs e)
    {
        if (e.target.CompareTag("Grab"))
        {
            Debug.Log(e.target + "exited");
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
