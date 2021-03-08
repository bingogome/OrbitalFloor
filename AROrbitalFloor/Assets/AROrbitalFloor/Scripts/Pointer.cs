using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Windows.Speech;
using UnityEngine.SceneManagement;

namespace AROF
{
    public class Pointer : MonoBehaviour
    {
        private Dictionary<string, Action> keywordActions = new Dictionary<string, Action>();
        private KeywordRecognizer keywordRecognizer;

        private float[][] pointCloudDig = new float[7][];
        private float[][] pointCloudModel = new float[7][];
        private float[] pointFeature;
        private float[][] pointFeatures = new float[3][];

        private float[] pointFeaturePlanned;
        private float[][] pointFeaturesPlanned = new float[3][];
        private float[] targetTREpoint;

        Tuple<float[][], float[]> regResult;
        Tuple<float[][], float[]> regInv;

        int currDigIndex;

        // Start is called before the first frame update
        void Start()
        {
            if (SceneManager.GetActiveScene().name == "SkullRegistrationTRE")
            {
                pointCloudModel = new float[6][];
                pointCloudDig = new float[6][];
            }
            // Debug.Log(SceneManager.GetActiveScene().name== "SkullRegistration");
            keywordActions.Add("digitize", Digitize);
            keywordActions.Add("register", Register);
            keywordActions.Add("save", Save);
            keywordActions.Add("single", SinglePoint);
            keywordActions.Add("multiple", MultiplePoint);

            keywordRecognizer = new KeywordRecognizer(keywordActions.Keys.ToArray());
            keywordRecognizer.OnPhraseRecognized += OnKeywordsRecognized;
            keywordRecognizer.Start();

            pointCloudModel[0] = new float[] { 62.2834f / 1000.0f, -101.639f / 1000.0f, 137.095f / 1000.0f };
            pointCloudModel[1] = new float[] { 71.8019f / 1000.0f, -134.585f / 1000.0f, 176.091f / 1000.0f };
            pointCloudModel[2] = new float[] { 1.41721f / 1000.0f, -44.5984f / 1000.0f, 153.749f / 1000.0f };
            pointCloudModel[3] = new float[] { 1.14525f / 1000.0f, -60.4037f / 1000.0f, 191.706f / 1000.0f };
            pointCloudModel[4] = new float[] { 1.32943f / 1000.0f, -175.998f / 1000.0f, 211.543f / 1000.0f };
            pointCloudModel[5] = new float[] { -59.9893f / 1000.0f, -109.185f / 1000.0f, 171.305f / 1000.0f };
            if (SceneManager.GetActiveScene().name != "SkullRegistrationTRE")
                pointCloudModel[6] = new float[] { -53.9599f / 1000.0f, -117.288f / 1000.0f, 120.412f / 1000.0f };

            pointFeaturePlanned = new float[] { 29.281f / 1000.0f, -77.8222f / 1000.0f, 114.428f / 1000.0f };
            pointFeaturesPlanned[0] = new float[] { 20.4428f / 1000.0f, -89.0086f / 1000.0f, 120.142f / 1000.0f };
            pointFeaturesPlanned[1] = new float[] { 39.3685f / 1000.0f, -74.5371f / 1000.0f, 110.041f / 1000.0f };
            pointFeaturesPlanned[2] = new float[] { 23.9793f / 1000.0f, -57.9638f / 1000.0f, 111.789f / 1000.0f };

            currDigIndex = 0;

            if (SceneManager.GetActiveScene().name != "SkullRegistrationTRE")
                pointCloudDig = GetZero(7);
            else
                pointCloudDig = GetZero(6);
            pointFeatures = GetZero(3);

            if (SceneManager.GetActiveScene().name == "SkullRegistrationTRE")
                targetTREpoint = new float[] { -53.9599f / 1000.0f, -117.288f / 1000.0f, 120.412f / 1000.0f };
            else
                targetTREpoint = new float[] { 28.8796f / 1000.0f, -81.6048f / 1000.0f, 115.405f / 1000.0f };

            // test svd
            //float[][] A = GetZero();
            //A[0][0] = 3.3f; A[0][1] = 4.2f; A[0][2] = 1f;
            //A[1][0] = 1.5f; A[1][1] = 2.0f; A[1][2] = 6.8f;
            //A[2][0] = 1.7f; A[2][1] = 11.0f; A[2][2] = 1f;
            //Tuple<float[][], float[][]> aaaa = QRSim3By3(A);
            //Tuple<float[][], float[][], float[][]> tulp = SVDSim3By3(A);
            //Debug.Log("" + aaaa.Item1[0][0] + " " + aaaa.Item1[0][1] + " " + aaaa.Item1[0][2]);

        }

        private void OnKeywordsRecognized(PhraseRecognizedEventArgs args)
        {
            keywordActions[args.text].Invoke();
        }

        private void SinglePoint()
        {
            SceneManager.LoadScene("SinglePointNav");
        }

        private void MultiplePoint()
        {
            SceneManager.LoadScene("FeaturePointsNav");
        }

        private void Digitize()
        {
            if(SceneManager.GetActiveScene().name == "SkullRegistration")
            {
                FiducialDigitize();
            }
            else if ((SceneManager.GetActiveScene().name == "SkullRegistrationTRE") || 
                (SceneManager.GetActiveScene().name == "SkullRegistrationTREFeature"))
            {
                FiducialDigitize();
            }
            else if(SceneManager.GetActiveScene().name == "SinglePointNav")
            {
                FeatureDigitize();
                Save();
                DataHandler.d.isDigedFeature = true;
            }
            else if (SceneManager.GetActiveScene().name == "FeaturePointsNav")
            {
                FeaturesDigitize();
                DataHandler.d.numOfFeatCollected = currDigIndex;
                if (currDigIndex == 3)
                {
                    DataHandler.d.isDigedFeatures = true;
                    Save();
                }
            }
        }

        private void Register()
        {
            float[][] A = pointCloudModel;
            float[][] B = pointCloudDig;
            regResult = Register(A, B);
            //Debug.Log("" + regResult.Item1[0][0] + " " + regResult.Item1[0][1] + " " + regResult.Item1[0][2]);
            //Debug.Log("" + regResult.Item1[1][0] + " " + regResult.Item1[1][1] + " " + regResult.Item1[1][2]);
            //Debug.Log("" + regResult.Item1[2][0] + " " + regResult.Item1[2][1] + " " + regResult.Item1[2][2]);
            //Debug.Log("" + regResult.Item2[0] + " " + regResult.Item2[1] + " " + regResult.Item2[2]);
            SaveRegisterDataToHandler();
            DataHandler.d.isRegistered = true;
            if ((SceneManager.GetActiveScene().name == "SkullRegistrationTRE") || 
                (SceneManager.GetActiveScene().name == "SkullRegistrationTREFeature"))
                regInv = InverseT(regResult);
        }

        private void Save()
        {
            if (SceneManager.GetActiveScene().name == "SkullRegistration")
            {
                SaveRegisterDataToHandler();
                //Debug.Log("Registration Saved");
                DataHandler.d.isSavedReg = true;
            }
            else if (SceneManager.GetActiveScene().name == "SinglePointNav")
            {
                SaveFeatureDataToHandler();
                //Debug.Log("Single feature point Saved");
            }
            else if (SceneManager.GetActiveScene().name == "FeaturePointsNav")
            {
                SaveFeaturesDataToHandler();
                //Debug.Log("Feature points Saved");
            }
        }

        public Vector3 Digitize(GameObject reference)
        {
            // pointer tip position offset: (-144,-2.5,0) mm
            Vector3 tipOffset = new Vector3(-144.0f / 1000f, -2.5f / 1000f, 0.0f);

            Quaternion rotPointer = transform.localRotation;
            Vector3 posPointer = transform.localPosition;
            Quaternion rotReference = reference.transform.localRotation;
            Vector3 posReference = reference.transform.localPosition;

            // math: S - reference, P - pointer, C - camera, d - digitized point
            // S_T_d = S_T_P * P_t_d
            // S_T_d = S_T_C * C_T_P * P_t_d
            Quaternion C_R_P = rotPointer;
            Vector3 C_t_P = posPointer;
            Quaternion C_R_S = rotReference;
            Vector3 C_t_S = posReference;
            Vector3 P_t_d = tipOffset;

            Quaternion S_R_C = Quaternion.Inverse(C_R_S);
            Vector3 S_t_C = -(S_R_C * C_t_S);

            Quaternion S_R_P = S_R_C * C_R_P;
            Vector3 S_t_P = S_R_C * C_t_P + S_t_C;

            Vector3 currentDig = S_R_P * P_t_d + S_t_P;

            return currentDig;
        }

        public void FiducialDigitize()
        {
            GameObject reference = GameObject.FindWithTag("markerSkull");

            Vector3 currentDig = Digitize(reference);

            pointCloudDig[currDigIndex][0] = currentDig.x;
            pointCloudDig[currDigIndex][1] = currentDig.y;
            pointCloudDig[currDigIndex][2] = currentDig.z;
            currDigIndex += 1;
            //Debug.Log("" + currentDig[0] + " " + currentDig[1] + " " + currentDig[2]);
            DataHandler.d.numOfFidCollected = currDigIndex;
       
        }

        public void FeatureDigitize()
        {
            GameObject reference = GameObject.FindWithTag("markerHemostat");
            Vector3 currentDig = Digitize(reference);
            pointFeature = new float[3] { currentDig.x, currentDig.y, currentDig.z};
            //Debug.Log("" + pointFeature[0] + " " + pointFeature[1] + " " + pointFeature[2]);
        }

        public void FeaturesDigitize()
        {
            GameObject reference = GameObject.FindWithTag("markerHemostat");
            Vector3 currentDig = Digitize(reference);

            pointFeatures[currDigIndex % 3][0] = currentDig.x;
            pointFeatures[currDigIndex % 3][1] = currentDig.y;
            pointFeatures[currDigIndex % 3][2] = currentDig.z;
            currDigIndex += 1;
            //Debug.Log("" + currentDig[0] + " " + currentDig[1] + " " + currentDig[2]);

        }

        public Tuple<float[][], float[]> Register(float[][] A, float[][] B)
        {
            int n;
            if (SceneManager.GetActiveScene().name != "SkullRegistrationTRE")
                n = 7;        // Length of Point Sets
            else
                n = 6;

            // Calculate centroids of S and M point sets
            float[] aCentroid = new float[3] { 0.0f, 0.0f, 0.0f };
            float[] bCentroid = new float[3] { 0.0f, 0.0f, 0.0f };

            for (int i = 0; i < n; i++) {
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
            for(int i = 0; i < n; i++)
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
            for(int i = 0; i < n; i++)
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

            float[] vecY = new float[3] {0.0f, 0.0f, 0.0f };

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

            while (err>tol && loopcount<loopmax)
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
            for(int i = 0; i < 3; i++)
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

        public Tuple<float[][], float[]> InverseT(Tuple<float[][], float[]> A)
        {
            float[][] RInv = Transp(A.Item1);
            float[] p = A.Item2;
            float[] pInv = new float[3] { 0.0f, 0.0f, 0.0f };
            pInv[0] = -(RInv[0][0] * p[0] + RInv[0][1] * p[1] + RInv[0][2] * p[2]);
            pInv[1] = -(RInv[1][0] * p[0] + RInv[1][1] * p[1] + RInv[1][2] * p[2]);
            pInv[2] = -(RInv[2][0] * p[0] + RInv[2][1] * p[1] + RInv[2][2] * p[2]);
            return Tuple.Create(RInv, pInv);
        }

        public float[] TMultP(Tuple<float[][], float[]> A, float[] p)
        {
            float[] res = new float[3] { 0.0f, 0.0f, 0.0f };
            res[0] = A.Item1[0][0] * p[0] + A.Item1[0][1] * p[1] + A.Item1[0][2] * p[2];
            res[1] = A.Item1[1][0] * p[0] + A.Item1[1][1] * p[1] + A.Item1[1][2] * p[2];
            res[2] = A.Item1[2][0] * p[0] + A.Item1[2][1] * p[1] + A.Item1[2][2] * p[2];
            for(int i = 0; i < 3; i++)
            {
                res[i] += A.Item2[i];
            }
            return res;
        }

        public void SaveRegisterDataToHandler()
        {
            DataHandler.d.regResult = regResult;
            DataHandler.d.pointCloudDig = pointCloudDig;
            DataHandler.d.pointCloudModel = pointCloudModel;
        }

        public void SaveFeatureDataToHandler()
        {
            DataHandler.d.pointFeature = pointFeature;
            DataHandler.d.pointFeaturePlanned = pointFeaturePlanned;
        }

        public void SaveFeaturesDataToHandler()
        {
            DataHandler.d.pointFeatures = pointFeatures;
            DataHandler.d.pointFeaturesPlanned = pointFeaturesPlanned;
        }
        
        // Update is called once per frame
        void Update()
        {
            if ((SceneManager.GetActiveScene().name == "SkullRegistrationTRE") || (SceneManager.GetActiveScene().name == "SkullRegistrationTREFeature"))
            {
                if (DataHandler.d.isRegistered == true)
                {
                    GameObject reference = GameObject.FindWithTag("markerSkull");
                    Vector3 currentDig = Digitize(reference);
                    float[] pointerTipPosWRTModelOrigin = TMultP(regInv, new float[3] {currentDig.x, currentDig.y, currentDig.z }); 
                    DataHandler.d.pointerTipErr = new Vector3(
                        pointerTipPosWRTModelOrigin[0] - targetTREpoint[0],
                        pointerTipPosWRTModelOrigin[1] - targetTREpoint[1],
                        pointerTipPosWRTModelOrigin[2] - targetTREpoint[2]
                    );
                }
            }
        }
    }

}