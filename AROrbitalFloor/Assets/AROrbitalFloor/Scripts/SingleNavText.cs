using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace AROF
{
    public class SingleNavText : MonoBehaviour
    {
        Text txt;
        // Start is called before the first frame update
        void Start()
        {
            txt = gameObject.GetComponent<Text>();
            txt.text = "Say \"Digitize!\" to collect a point on the implant.";
        }

        // Update is called once per frame
        void Update()
        {
            if (DataHandler.d.isDigedFeature && (!DataHandler.d.startNavigate))
            {
                txt.text = "Digitized.\nSay \"Navigate!\" to start the navigation.";
                
            }
            if (DataHandler.d.startNavigate)
            {
                txt.text = "Distances to target along axes: (mm)\nx " 
                    + (1000.0f * DataHandler.d.finalPos.x).ToString("F2") + "\ny "
                    + (1000.0f * DataHandler.d.finalPos.y).ToString("F2") + "\nz "
                    + (1000.0f * DataHandler.d.finalPos.z).ToString("F2") + "\n" +
                    "Distance to target: (mm) " + (1000.0f * Mathf.Sqrt(
                    DataHandler.d.finalPos.x * DataHandler.d.finalPos.x +
                    DataHandler.d.finalPos.y * DataHandler.d.finalPos.y +
                    DataHandler.d.finalPos.z * DataHandler.d.finalPos.z
                    )).ToString("F2");
            }

        }
    }
}