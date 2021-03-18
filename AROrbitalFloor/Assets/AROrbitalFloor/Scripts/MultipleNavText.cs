﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AROF
{
    public class MultipleNavText : MonoBehaviour
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
            txt.text = "" + DataHandler.d.numOfFeatCollected + "/3 features collected.";
            if (DataHandler.d.isDigedFeatures)
            {
                txt.text = "Digitized.\nSay \"Navigate!\" to start the navigation.";
            }
            if (DataHandler.d.startNavigate)
            {
                Vector3 diffPoseEuler = DataHandler.d.diffPoseR.eulerAngles;
                txt.text = "Distances to target along axes: (mm)\nTransverse "
                    + (1000.0f * DataHandler.d.finalPos.x).ToString("F2") + ", Sagittal "
                    + (1000.0f * DataHandler.d.finalPos.y).ToString("F2") + ", Coronal "
                    + (1000.0f * DataHandler.d.finalPos.z).ToString("F2") + "\nOrientation to target: (degrees in Euler angles)\n" +
                    "Transverse: " + diffPoseEuler.x.ToString("F2") + "\n" +
                    "Sagittal: " + diffPoseEuler.y.ToString("F2") + "\n" +
                    "Coronal: " + diffPoseEuler.z.ToString("F2") + "\n" +
                    "Distance to target: (mm) " + (1000.0f * Mathf.Sqrt( 
                    DataHandler.d.finalPos.x * DataHandler.d.finalPos.x +
                    DataHandler.d.finalPos.y * DataHandler.d.finalPos.y +
                    DataHandler.d.finalPos.z * DataHandler.d.finalPos.z
                    )).ToString("F2");
                
            }
        }
    }

}
