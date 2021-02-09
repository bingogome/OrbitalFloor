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

        public Tuple<float[][], float[]> Register(float[][] A, float[][] B)
        {
            int n = 3;        // Length of Point Sets

            // Calculate centroids of S and M point sets
            float[] aCentroid = new float[3] { 0.0f, 0.0f, 0.0f };
            float[] bCentroid = new float[3] { 0.0f, 0.0f, 0.0f };

            for (int i = 0; i < n; i++)
            {
                aCentroid[0] += A[i][0];
                aCentroid[1] += A[i][1];
                aCentroid[2] += A[i][2];
                bCentroid[0] += B[i][0];
                bCentroid[1] += B[i][1];
                bCentroid[2] += B[i][2];
            }
            aCentroid[0] = (1.0f / n) * aCentroid[0];
            aCentroid[1] = (1.0f / n) * aCentroid[1];
            aCentroid[2] = (1.0f / n) * aCentroid[2];
            bCentroid[0] = (1.0f / n) * bCentroid[0];
            bCentroid[1] = (1.0f / n) * bCentroid[1];
            bCentroid[2] = (1.0f / n) * bCentroid[2];

            // Point Deviations from centroid
            float[][] aTilda = GetZero(n);
            float[][] bTilda = GetZero(n);

            for (int i = 0; i < n; i++)
            {
                aTilda[i][0] = A[i][0] - aCentroid[0];
                bTilda[i][0] = B[i][0] - bCentroid[0];

                aTilda[i][1] = A[i][1] - aCentroid[1];
                bTilda[i][1] = B[i][1] - bCentroid[1];

                aTilda[i][2] = A[i][2] - aCentroid[2];
                bTilda[i][2] = B[i][2] - bCentroid[2];
            }

            //Find R that minimizes SSE
            // H Matrix for Singular Value Decomposition
            float[][] H = GetZero();
            int x = 0; int y = 1; int z = 2;

            // Build H
            for (int i = 0; i < n; i++)
            {
                H[0][0] += aTilda[i][x] * bTilda[i][x];
                H[0][1] += aTilda[i][x] * bTilda[i][y];
                H[0][2] += aTilda[i][x] * bTilda[i][z];

                H[1][0] += aTilda[i][y] * bTilda[i][x];
                H[1][1] += aTilda[i][y] * bTilda[i][y];
                H[1][2] += aTilda[i][y] * bTilda[i][z];

                H[2][0] += aTilda[i][z] * bTilda[i][x];
                H[2][1] += aTilda[i][z] * bTilda[i][y];
                H[2][2] += aTilda[i][z] * bTilda[i][z];
            }

            // SVD Decomposition
            Tuple<float[][], float[][], float[][]> USV = SVDSim3By3(H);
            // Calculate Rotation Matrix R
            float[][] R = Mult(USV.Item3, Transp(USV.Item1));

            // Use Section IV of K. Arun et. al to change mirror and make valid matrix
            // If this does not work, this algorithm cannot be used
            if (Det(R) < 0)
            {
                float[][] V = USV.Item3;
                V[0][0] = -V[0][0]; V[0][1] = -V[0][1]; V[0][2] = -V[0][2];
                V[1][0] = -V[1][0]; V[1][1] = -V[1][1]; V[1][2] = -V[1][2];
                V[2][0] = -V[2][0]; V[2][1] = -V[2][1]; V[2][2] = -V[2][2];

                R = Mult(V, Transp(USV.Item1));
            }

            // Find translation vector - Compute p, Translation
            // p = b_centroid - R * a_centroid;
            float[] p;
            p = new float[3] { 0.0f, 0.0f, 0.0f };
            p[0] = bCentroid[0] - (
                R[0][0] * aCentroid[0] + R[0][1] * aCentroid[1] + R[0][2] * aCentroid[2]);
            p[1] = bCentroid[1] - (
                R[1][0] * aCentroid[0] + R[1][1] * aCentroid[1] + R[1][2] * aCentroid[2]);
            p[2] = bCentroid[2] - (
                R[2][0] * aCentroid[0] + R[2][1] * aCentroid[1] + R[2][2] * aCentroid[2]);

            return Tuple.Create(R, p);
        }

        public float Det(float[][] R)
        {
            float det = 0.0f;
            det = R[0][2] * (R[1][0] * R[2][1] - R[1][1] * R[2][0]) - R[0][1] * (R[1][0] * R[2][2] - R[1][2] * R[2][0]) + R[0][0] * (R[1][1] * R[2][2] - R[1][2] * R[2][1]);
            return det;
        }

        public float[][] GetEye()
        {
            float[][] I = new float[3][];
            I[0] = new float[3] { 1.0f, 0.0f, 0.0f };
            I[1] = new float[3] { 0.0f, 1.0f, 0.0f };
            I[2] = new float[3] { 0.0f, 0.0f, 1.0f };
            return I;
        }

        public float[][] GetZero()
        {
            float[][] I = new float[3][];
            I[0] = new float[3] { 0.0f, 0.0f, 0.0f };
            I[1] = new float[3] { 0.0f, 0.0f, 0.0f };
            I[2] = new float[3] { 0.0f, 0.0f, 0.0f };
            return I;
        }

        public float[][] GetZero(int n)
        {
            float[][] I = new float[n][];
            for (int i = 0; i < n; i++)
            {
                I[i] = new float[3] { 0.0f, 0.0f, 0.0f };
            }
            return I;
        }

        public float[][] GetDiag(float[] v)
        {
            float[][] D = GetZero();
            D[0][0] = v[0];
            D[1][1] = v[1];
            D[2][2] = v[2];
            return D;
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

        public Tuple<float[][], float[][]> QRSim3By3(float[][] A)
        {
            float[][] Q = GetZero();
            float[][] R = GetZero();

            float[][] X = GetZero();
            float[][] Y = GetZero();
            float[][] K = GetEye();

            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    X[i][j] = A[i][j];

            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    Y[i][j] = A[i][j];

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < i; j++)
                {
                    K[j][i] = (X[0][i] * Y[0][j] + X[1][i] * Y[1][j] + X[2][i] * Y[2][j]) / (Y[0][j] * Y[0][j] + Y[1][j] * Y[1][j] + Y[2][j] * Y[2][j]);
                    Y[0][i] = Y[0][i] - K[j][i] * Y[0][j];
                    Y[1][i] = Y[1][i] - K[j][i] * Y[1][j];
                    Y[2][i] = Y[2][i] - K[j][i] * Y[2][j];
                }
            }

            float[] vecY = new float[3] { 0.0f, 0.0f, 0.0f };

            for (int i = 0; i < 3; i++)
            {
                float n = (float)Math.Sqrt(Y[0][i] * Y[0][i] + Y[1][i] * Y[1][i] + Y[2][i] * Y[2][i]);
                Q[0][i] = Y[0][i] / n;
                Q[1][i] = Y[1][i] / n;
                Q[2][i] = Y[2][i] / n;
                vecY[i] = n;
            }

            float[][] diagY = GetDiag(vecY);
            R = Mult(diagY, K);

            return Tuple.Create(Q, R);
        }

        public Tuple<float[][], float[][], float[][]> SVDSim3By3(float[][] A)
        {
            // Editted from the matlab code: Faiz Khan
            // https://github.com/Sable/mcbench-benchmarks/blob/master/12674-simple-svd/svdsim.m

            // If needed, change the "3" to a variable. This method does 3by3 just for simplicity
            float[][] U = new float[3][];
            float[][] S = new float[3][];
            float[][] V = new float[3][];
            float[][] Q = new float[3][];

            float tol = 2.2204E-16f * 1024; // eps floating-point relative accuracy 
            int loopmax = 300;
            int loopcount = 0;

            U = GetEye();
            V = GetEye();
            S = Transp(A);

            float err = float.MaxValue;

            while (err > tol && loopcount < loopmax)
            {
                Tuple<float[][], float[][]> tulp = QRSim3By3(Transp(S));
                Q = tulp.Item1; S = tulp.Item2;
                U = Mult(U, Q);
                tulp = QRSim3By3(Transp(S));
                Q = tulp.Item1; S = tulp.Item2;
                V = Mult(V, Q);

                // e = triu(S,1)
                float[][] e = GetZero();
                e[0][1] = S[0][1];
                e[0][2] = S[0][2];
                e[1][2] = S[1][2];

                float E = (float)Math.Sqrt(e[0][1] * e[0][1] + e[0][2] * e[0][2] + e[1][2] * e[1][2]);
                float F = (float)Math.Sqrt(S[0][0] * S[0][0] + S[1][1] * S[1][1] + S[2][2] * S[2][2]);

                if (F == 0)
                    F = 1;
                err = E / F;
                loopcount++;
            }

            float[] SS = { S[0][0], S[1][1], S[2][2] };
            for (int i = 0; i < 3; i++)
            {
                float SSi = Math.Abs(SS[i]);
                S[i][i] = SSi;
                if (SS[i] < 0)
                {
                    U[0][i] = -U[0][i];
                    U[1][i] = -U[1][i];
                    U[2][i] = -U[2][i];
                }
            }

            return Tuple.Create(U, S, V);
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

        public Quaternion RMat2Quat(float[][] RMat)
        {
            Vector3 column3;
            Vector3 column2;
            column3 = new Vector3(RMat[0][2], RMat[1][2], RMat[2][2]);
            column2 = new Vector3(RMat[0][1], RMat[1][1], RMat[2][1]);
            return Quaternion.LookRotation(column3, column2);
        }

        //public Quaternion RegThreePoint(Vector3[] A, Vector3[] B) // Works in some cases, does not work well
        //{
        //    // http://www.cs.hunter.cuny.edu/~ioannis/registerpts_allen_notes.pdf
        //    Quaternion R = Quaternion.identity;
        //    Vector3 p = new Vector3(0.0f, 0.0f, 0.0f);

        //    float[][] RL = GetZero();
        //    float[][] RR = GetZero();
        //    Vector3[] basL = CreateBasisThreePoint(A);
        //    Vector3[] basR = CreateBasisThreePoint(B);
        //    for (int i = 0; i < 3; i++)
        //    {
        //        RL[i] = new float[3] { basL[i].x, basL[i].y, basL[i].z };
        //        RR[i] = new float[3] { basR[i].x, basR[i].y, basR[i].z };
        //    }

        //    RL = Transp(RL);

        //    float[][] RMat = Mult(RL, RR);
        //    R = RMat2Quat(RMat);

        //    // p = A[0] - R * B[0];

        //    return R;
        //}

        //public Vector3[] CreateBasisThreePoint(Vector3[] A)
        //{
        //    Vector3 x = Vector3.Normalize(A[1] - A[0]);
        //    Vector3 y = Vector3.Normalize(Vector3.Normalize((A[2] - A[0]) - Vector3.Dot((A[2] - A[0]), x) * x));
        //    Vector3 z = Vector3.Normalize(Vector3.Cross(x, y));
        //    return (new Vector3[3] { x, y, z });
        //}

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
                    DataHandler.d.finalPos = finalPos;
                    finalPos.x = DistanceMapping(diffAve.x);
                    finalPos.y = DistanceMapping(diffAve.y);
                    finalPos.z = DistanceMapping(diffAve.z);

                    alignIndicator.transform.localPosition = finalPos;


                    float[][] S_p_dFloat = GetZero(3);
                    float[][] featurePlannedPosFloat = GetZero(3);
                    for(int i=0;i< features.Length; i++)
                    {
                        S_p_dFloat[i][0] = S_p_d[i].x;
                        S_p_dFloat[i][1] = S_p_d[i].y;
                        S_p_dFloat[i][2] = S_p_d[i].z;
                        featurePlannedPosFloat[i][0] = featurePlannedPos[i].x;
                        featurePlannedPosFloat[i][1] = featurePlannedPos[i].y;
                        featurePlannedPosFloat[i][2] = featurePlannedPos[i].z;
                    }
                    Tuple<float[][], float[]> diffPose = Register(S_p_dFloat, featurePlannedPosFloat);
                    Quaternion diffPoseR = RMat2Quat(diffPose.Item1);
                    DataHandler.d.diffPoseR = diffPoseR;
                    alignIndicator.transform.localRotation = diffPoseR;

                }

                float distToTarget = 1000.0f * Mathf.Sqrt(
                    DataHandler.d.finalPos.x * DataHandler.d.finalPos.x +
                    DataHandler.d.finalPos.y * DataHandler.d.finalPos.y +
                    DataHandler.d.finalPos.z * DataHandler.d.finalPos.z
                    );
                if (distToTarget > 30.0f)
                    distToTarget = 30.0f;
                GameObject alignCenter1 = GameObject.FindWithTag("alignCenter1");
                GameObject alignCenter2 = GameObject.FindWithTag("alignCenter2");
                GameObject alignCenter3 = GameObject.FindWithTag("alignCenter3");
                alignCenter1.GetComponent<Renderer>().material.SetColor("_Color", Color.Lerp(Color.green, Color.red, distToTarget/30.0f));
                alignCenter2.GetComponent<Renderer>().material.SetColor("_Color", Color.Lerp(Color.green, Color.red, distToTarget / 80.0f));
                alignCenter3.GetComponent<Renderer>().material.SetColor("_Color", Color.Lerp(Color.green, Color.red, distToTarget / 80.0f));

            }

        }
    }
}
