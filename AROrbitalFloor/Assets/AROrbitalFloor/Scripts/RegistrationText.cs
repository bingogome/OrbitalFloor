using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace AROF
{
    public class RegistrationText : MonoBehaviour
    {
        Text txt;
        private int numOfFidCollected = 0;
        // Start is called before the first frame update
        private int totalFid;
        void Start()
        {
            if (SceneManager.GetActiveScene().name == "SkullRegistrationTRE")
                totalFid = 6;
            else
                totalFid = 7;

            txt = gameObject.GetComponent<Text>();
            txt.text = "" + numOfFidCollected + "/" + totalFid + " fiducials collected.";
        }

        // Update is called once per frame
        void Update()
        {
            numOfFidCollected = DataHandler.d.numOfFidCollected;
            txt.text = "" + numOfFidCollected + "/" + totalFid + " fiducials collected.";
            if (numOfFidCollected == totalFid)
            {
                txt.text = "" + numOfFidCollected + "/" + totalFid + " fiducials collected.\nSay \"Register!\"";
            }
            if(DataHandler.d.isRegistered==true)
            {
                txt.text = "" + numOfFidCollected + "/" + totalFid + " fiducials collected.\nRegistered.\nSay \"Save!\"";
                if ((SceneManager.GetActiveScene().name == "SkullRegistrationTRE") ||
                    (SceneManager.GetActiveScene().name == "SkullRegistrationTREFeature"))
                {
                    txt.text = "" + numOfFidCollected + "/" + totalFid + " fiducials collected.\nRegistered." +
                        "\nPointer tip to target point position:" +
                        "\nx: " + (1000.0f * DataHandler.d.pointerTipErr.x).ToString("F2") +
                        "\ny: " + (1000.0f * DataHandler.d.pointerTipErr.y).ToString("F2") +
                        "\nz: " + (1000.0f * DataHandler.d.pointerTipErr.z).ToString("F2") +
                        "\nEuclidean distance: " + (1000.0f * Mathf.Sqrt(
                            DataHandler.d.pointerTipErr.x * DataHandler.d.pointerTipErr.x +
                            DataHandler.d.pointerTipErr.y * DataHandler.d.pointerTipErr.y +
                            DataHandler.d.pointerTipErr.z * DataHandler.d.pointerTipErr.z
                        )).ToString("F2");
                }

            }
            if (DataHandler.d.isSavedReg == true)
            {
                txt.text = "" + numOfFidCollected + "/" + totalFid + " fiducials collected.\nRegistered.\nSaved.\nSay \"Single!\" or \"Multiple!\"";
            }
        }
    }
}
