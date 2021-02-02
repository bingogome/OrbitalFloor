using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AROF
{
    // a global class storing data
    public class DataHandler : MonoBehaviour
    {
        public static DataHandler d;
        public Tuple<float[][], float[]> regResult;
        public float[][] pointCloudDig = new float[7][]; // point cloud of fiducials (on skull) wrt skull marker
        public float[][] pointCloudModel = new float[7][]; // point cloud of fiducials (on skull) wrt skull model origin
        public float[] pointFeature; // point coordinate of the point feature (on implant) wrt implant marker
        public float[][] pointFeatures = new float[3][]; // points coordinates of the point features (on implant) wrt implant marker
        public float[] pointFeaturePlanned; // point coordinate of the planned feature (on skull) wrt skull model origin
        public float[][] pointFeaturesPlanned = new float[3][]; // points coordinates of the planned features (on skull) wrt skull model origin

        public int numOfFidCollected;
        public bool isRegistered = false;
        public bool isSavedReg = false;
        public bool isDigedFeature = false;
        public bool isDigedFeatures = false;
        public bool startNavigate = false;
        public int numOfFeatCollected=0;

        public Vector3 finalPos;

        void Awake()
        {
            if (d == null)
            {
                DontDestroyOnLoad(gameObject);
                d = this;
            }
            else if (d != this)
            {
                Destroy(gameObject);
            }
        }

    }
}

