using System.Collections;
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
        }
    }

}
