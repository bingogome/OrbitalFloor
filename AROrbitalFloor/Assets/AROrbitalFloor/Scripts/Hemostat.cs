using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Windows.Speech;
using UnityEngine.SceneManagement;


namespace AROF
{
    public class Hemostat : MonoBehaviour
    {
        private Dictionary<string, Action> keywordActions = new Dictionary<string, Action>();
        private KeywordRecognizer keywordRecognizer;

        public float[] pointFeaturePlanned;
        public float[][] pointFeaturesPlanned;
        public float[] pointFeatureImp;
        public float[][] pointFeaturesImp;
        public Tuple<float[][], float[]> regResult;

        Quaternion regR;
        Vector3 regP;

        GameObject[] features;

        bool navigateStart = false;
        bool singleFeature = false;

        GameObject alignIndicator;

        // Start is called before the first frame update
        void Start()
        {
            // load data
            // pointFeaturePlanned = DataHandler.d.pointFeaturePlanned;
            // pointFeaturesPlanned = DataHandler.d.pointFeaturesPlanned;
            // pointFeatureImp = DataHandler.d.pointFeature;
            // pointFeaturesImp = DataHandler.d.pointFeatures;
            regResult = DataHandler.d.regResult;

            keywordActions.Add("navigate", Navigate);
            keywordRecognizer = new KeywordRecognizer(keywordActions.Keys.ToArray());
            keywordRecognizer.OnPhraseRecognized += OnKeywordsRecognized;
            keywordRecognizer.Start();

            alignIndicator = GameObject.FindWithTag("alignCenter");

            if (SceneManager.GetActiveScene().name == "FeaturePointsNav")
            {
                singleFeature = false;
                features = new GameObject[3] {
                    GameObject.FindWithTag("pointFeatureTarget1"),
                    GameObject.FindWithTag("pointFeatureTarget2"),
                    GameObject.FindWithTag("pointFeatureTarget3")
                };
            }
            else if (SceneManager.GetActiveScene().name == "SinglePointNav")
            {
                singleFeature = true;
                features = new GameObject[1] { GameObject.FindWithTag("pointFeatureTarget") };
            }

            regR = RMat2Quat(regResult.Item1);
            regP = new Vector3(regResult.Item2[0], regResult.Item2[1], regResult.Item2[2]);
        }

        private void OnKeywordsRecognized(PhraseRecognizedEventArgs args)
        {
            keywordActions[args.text].Invoke();
        }

        private void Navigate()
        {
            if (SceneManager.GetActiveScene().name == "FeaturePointsNav")
            {
                NavigateFeatures();
            }
            else if (SceneManager.GetActiveScene().name == "SinglePointNav")
            {
                NavigateFeature();
            }
            navigateStart = true;
            DataHandler.d.startNavigate = true;
        }

        private void NavigateFeatures()
        {
            pointFeaturesImp = DataHandler.d.pointFeatures;
            pointFeaturesPlanned = DataHandler.d.pointFeaturesPlanned;

            // visualize the implant feature points
            GameObject pointFeatures1 = GameObject.FindWithTag("pointFeatures1");
            GameObject pointFeatures2 = GameObject.FindWithTag("pointFeatures2");
            GameObject pointFeatures3 = GameObject.FindWithTag("pointFeatures3");

            Vector3 pos1 = new Vector3(pointFeaturesImp[0][0], pointFeaturesImp[0][1], pointFeaturesImp[0][2]);
            Vector3 pos2 = new Vector3(pointFeaturesImp[1][0], pointFeaturesImp[1][1], pointFeaturesImp[1][2]);
            Vector3 pos3 = new Vector3(pointFeaturesImp[2][0], pointFeaturesImp[2][1], pointFeaturesImp[2][2]);

            pointFeatures1.transform.localPosition = pos1;
            pointFeatures2.transform.localPosition = pos2;
            pointFeatures3.transform.localPosition = pos3;

            // visualize the target feature points
            for (int i = 0; i < features.Length; i++)
            {
                Vector3 featurePlannedPos;
                featurePlannedPos = new Vector3(pointFeaturesPlanned[i][0], pointFeaturesPlanned[i][1], pointFeaturesPlanned[i][2]);
                features[i].transform.localPosition = regR * featurePlannedPos + regP;
            }

        }

        private void NavigateFeature()
        {
            pointFeatureImp = DataHandler.d.pointFeature;
            pointFeaturePlanned = DataHandler.d.pointFeaturePlanned;

            // visualize the implant feature point
            GameObject pointFeature = GameObject.FindWithTag("pointFeature");
            Vector3 pointFeaturePos = new Vector3(pointFeatureImp[0], pointFeatureImp[1], pointFeatureImp[2]);
            pointFeature.transform.localPosition = pointFeaturePos;

            // visualize the target feature point
            Vector3 featurePlannedPos;
            featurePlannedPos = new Vector3(pointFeaturePlanned[0], pointFeaturePlanned[1], pointFeaturePlanned[2]);

            features[0].transform.localPosition = regR * featurePlannedPos + regP;

        }

        public Quaternion RegThreePoint(Vector3[] A, Vector3[] B)
        {
            // http://www.cs.hunter.cuny.edu/~ioannis/registerpts_allen_notes.pdf
            Quaternion R = Quaternion.identity;
            Vector3 p = new Vector3(0.0f, 0.0f, 0.0f);

            float[][] RL = GetZero();
            float[][] RR = GetZero();
            Vector3[] basL = CreateBasisThreePoint(A);
            Vector3[] basR = CreateBasisThreePoint(B);
            for (int i = 0; i < 3; i++)
            {
                RL[i] = new float[3] { basL[i].x, basL[i].y, basL[i].z };
                RR[i] = new float[3] { basR[i].x, basR[i].y, basR[i].z };
            }

            RL = Transp(RL);

            float[][] RMat = Mult(RL, RR);
            R = RMat2Quat(RMat);

            // p = A[0] - R * B[0];

            return R;
        }

        public Quaternion RMat2Quat(float[][] RMat)
        {
            Vector3 column3;
            Vector3 column2;
            column3 = new Vector3(RMat[0][2], RMat[1][2], RMat[2][2]);
            column2 = new Vector3(RMat[0][1], RMat[1][1], RMat[2][1]);
            return Quaternion.LookRotation(column3, column2);
        }

        public Vector3[] CreateBasisThreePoint(Vector3[] A)
        {
            Vector3 x = Vector3.Normalize(A[1] - A[0]);
            Vector3 y = Vector3.Normalize((A[2] - A[0]) - Vector3.Dot((A[2] - A[0]), x) * x);
            Vector3 z = Vector3.Cross(x, y);
            return (new Vector3[3] { x, y, z });
        }

        public float[][] GetZero()
        {
            float[][] I = new float[3][];
            I[0] = new float[3] { 0.0f, 0.0f, 0.0f };
            I[1] = new float[3] { 0.0f, 0.0f, 0.0f };
            I[2] = new float[3] { 0.0f, 0.0f, 0.0f };
            return I;
        }

        public float[][] Mult(float[][] X, float[][] Y)
        {
            float[][] A = GetZero();

            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    for (int k = 0; k < 3; k++)
                        A[i][j] += X[i][k] * Y[k][j];

            return A;
        }

        public float[][] Transp(float[][] X)
        {
            float[][] A = GetZero();
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    A[j][i] = X[i][j];
            return A;
        }

        public float DistanceMapping(float dist) // to account for more distance in the center
        {
            if (dist > 0.08f)
                return 0.096f;
            else if (dist < -0.08f)
                return -0.096f;
            else if (dist >= 0.0f)
                return 0.096f - 15.0f * (dist - 0.08f) * (dist - 0.08f);
            else
                return -(0.096f - 15.0f * (dist - 0.08f) * (dist - 0.08f));
        }

        // Update is called once per frame
        void Update()
        {
            if (navigateStart)
            {
                // Math: S_T_d = S_T_C * C_T_H * H_T_d, 
                // S: skull marker; C: cemra; H: hemostat marker; d: feature point
                GameObject markerSkull = GameObject.FindWithTag("markerSkull");
                GameObject markerHemostat = GameObject.FindWithTag("markerHemostat");

                Vector3 S_p_C = -(Quaternion.Inverse(markerSkull.transform.localRotation) * markerSkull.transform.localPosition);
                Quaternion S_R_C = Quaternion.Inverse(markerSkull.transform.localRotation);
                Vector3 C_p_H = markerHemostat.transform.localPosition;
                Quaternion C_R_H = markerHemostat.transform.localRotation;

                Vector3 S_p_H = S_R_C * C_p_H + S_p_C;
                Quaternion S_R_H = S_R_C * C_R_H;

                if (singleFeature)
                {
                    Vector3 featurePlannedPos;
                    featurePlannedPos = new Vector3(pointFeaturePlanned[0], pointFeaturePlanned[1], pointFeaturePlanned[2]);

                    Vector3 H_p_d = new Vector3(pointFeatureImp[0], pointFeatureImp[1], pointFeatureImp[2]);
                    Vector3 S_p_d = S_R_H * H_p_d + S_p_H;

                    Vector3 finalPos = (regR * featurePlannedPos + regP) - S_p_d;
                    DataHandler.d.finalPos = finalPos;
                    finalPos.x = DistanceMapping(finalPos.x);
                    finalPos.y = DistanceMapping(finalPos.y);
                    finalPos.z = DistanceMapping(finalPos.z);
                    alignIndicator.transform.localPosition = finalPos;
                }
                else
                {
                    Vector3[] featurePlannedPos = new Vector3[3];
                    for (int i = 0; i < features.Length; i++)
                        featurePlannedPos[i] = new Vector3(pointFeaturesPlanned[i][0], pointFeaturesPlanned[i][1], pointFeaturesPlanned[i][2]);

                    Vector3[] featurePlannedPosReged = new Vector3[3];
                    for (int i = 0; i < features.Length; i++)
                        featurePlannedPosReged[i] = regR* featurePlannedPos[i] +regP;

                    Vector3[] H_p_d = new Vector3[3];
                    for (int i = 0; i < features.Length; i++)
                        H_p_d[i] = new Vector3(pointFeaturesImp[i][0], pointFeaturesImp[i][1], pointFeaturesImp[i][2]);

                    Vector3[] S_p_d = new Vector3[3];
                    for (int i = 0; i < features.Length; i++)
                        S_p_d[i] = S_R_H * H_p_d[i] + S_p_H;

                    Vector3[] diff = new Vector3[3];
                    for (int i = 0; i < features.Length; i++)
                        diff[i] = (featurePlannedPosReged[i]) - S_p_d[i];

                    Vector3 diffAve = new Vector3(0.0f, 0.0f, 0.0f);
                    for (int i = 0; i < features.Length; i++)
                    {
                        diffAve.x = diffAve.x + diff[i].x;
                        diffAve.y = diffAve.y + diff[i].y;
                        diffAve.z = diffAve.z + diff[i].z;
                    }

                    diffAve.x = diffAve.x / features.Length;
                    diffAve.y = diffAve.y / features.Length;
                    diffAve.z = diffAve.z / features.Length;
                    Vector3 finalPos = diffAve;
                    finalPos.x = DistanceMapping(diffAve.x);
                    finalPos.y = DistanceMapping(diffAve.y);
                    finalPos.z = DistanceMapping(diffAve.z);

                    alignIndicator.transform.localPosition = finalPos;

                    Quaternion diffPose = RegThreePoint(featurePlannedPos, S_p_d);
                    alignIndicator.transform.localRotation = diffPose;

                }
            }

        }
    }
}
