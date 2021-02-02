using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AROF
{
    public class RegistrationText : MonoBehaviour
    {
        Text txt;
        private int numOfFidCollected = 0;
        // Start is called before the first frame update
        void Start()
        {
            txt = gameObject.GetComponent<Text>();
            txt.text = "" + numOfFidCollected + "/7 fiducials collected.";
        }

        // Update is called once per frame
        void Update()
        {
            numOfFidCollected = DataHandler.d.numOfFidCollected;
            txt.text = "" + numOfFidCollected + "/7 fiducials collected.";
            if (numOfFidCollected == 7)
            {
                txt.text = "" + numOfFidCollected + "/7 fiducials collected.\nSay \"Register!\"";
            }
            if(DataHandler.d.isRegistered==true)
            {
                txt.text = "" + numOfFidCollected + "/7 fiducials collected.\nRegistered.\nSay \"Save!\"";
            }
            if(DataHandler.d.isSavedReg == true)
            {
                txt.text = "" + numOfFidCollected + "/7 fiducials collected.\nRegistered.\nSaved.\nSay \"Single!\" or \"Multiple!\"";
            }
        }
    }
}
