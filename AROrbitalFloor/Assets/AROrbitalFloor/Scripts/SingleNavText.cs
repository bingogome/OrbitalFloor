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
                txt.text = "Distances to target: (mm)\nx " 
                    + (1000.0f * DataHandler.d.finalPos.x).ToString("F2") + "\ny "
                    + (1000.0f * DataHandler.d.finalPos.y).ToString("F2") + "\nz "
                    + (1000.0f * DataHandler.d.finalPos.z).ToString("F2");
            }

        }
    }
}