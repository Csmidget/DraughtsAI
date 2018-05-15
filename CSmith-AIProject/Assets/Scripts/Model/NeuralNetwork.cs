using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

public class NeuralNetwork{

    public const int inputLayerSize = 15;
    public const int hiddenLayerSize = 48;
    public const double init_epsilon = 0.1;

    double[,] theta1Arr;
    double[] theta2Arr;

    Matrix<double> theta1; //input - hidden layer weights
    Matrix<double> theta2; //hidden - output layer weights

    Matrix<double> e_theta1; //eligibility trace for theta1;
    Matrix<double> e_theta2; //eligibility trace for theta2;

    //Initialize neuralnetwork from file
    public NeuralNetwork(string fileName)
    {
        //Basic arrays for reading data from txt file into
        theta1Arr = new double[hiddenLayerSize, inputLayerSize + 1];
        theta2Arr = new double[hiddenLayerSize + 1];
        
        if (File.Exists("NetWeights\\" + fileName))
        {
            //used to track which line is being read, if greater than number of rows in theta1 then we must be reading theta2
            int theta1Row = 0;

            StreamReader sr = new StreamReader("NetWeights\\" + fileName);
            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine();

                string[] valueStrings = line.Split(' ');
                List<double> doubleList = new List<double>();
                double d;
                foreach (string s in valueStrings)
                {
                    //Convert each value in the line to a double
                    if (double.TryParse(s, out d))
                    {
                        doubleList.Add(d);
                    }
                }

                for (int i = 0; i < doubleList.Count; i++)
                {
                    if (theta1Row == theta1Arr.GetLength(0))
                    {
                        theta2Arr[i] = doubleList[i];
                    }
                    else
                    {
                        theta1Arr[theta1Row, i] = doubleList[i];                        
                    }
                }
                theta1Row++;
            }
            sr.Close();
        }
        else
        {
            //If we can't find a valid file, initialize with random theta values.
            Debug.Log("No file found with name: " + fileName + ". Initializing with random theta values");
            InitializeRandomTheta();
        }
        
        //Create MathNet matrices based on values read from txt file.
        theta1 = DenseMatrix.OfArray(theta1Arr);
        theta2 = DenseVector.OfArray(theta2Arr).ToRowMatrix();

        //Create blank matrices for eligibility trace values.
        e_theta1 = DenseMatrix.Create(hiddenLayerSize, inputLayerSize + 1, 0);
        e_theta2 = DenseMatrix.Create(hiddenLayerSize + 1, 1, 0);
    }

    // Initialize network with random theta values
    public NeuralNetwork() {

        theta1Arr = new double[hiddenLayerSize, inputLayerSize + 1];
        theta2Arr = new double[hiddenLayerSize + 1];

        //Populate theta1Arr & theta2Arr with random values
        InitializeRandomTheta();

        //convert theta arrays into matrices
        theta1 = DenseMatrix.OfArray(theta1Arr);
        theta2 = DenseVector.OfArray(theta2Arr).ToRowMatrix();

        //blank matrices for eligibility trace
        e_theta1 = DenseMatrix.Create(hiddenLayerSize, inputLayerSize + 1, 0);
        e_theta2 = DenseMatrix.Create(hiddenLayerSize + 1, 1, 0);
    }

    //Writes neural network to NetWeights\fileName.txt
    public bool SaveToFile(string fileName)
    {
        StreamWriter sw = new StreamWriter(fileName + ".txt");
        sw.Write(theta1.ToMatrixString(hiddenLayerSize,inputLayerSize+1));
        sw.Write(theta2.ToMatrixString(1,hiddenLayerSize +1));
        sw.Close();
        Debug.Log("WROTE TO FILE");
        return true;
    }

    //Fills theta1Arr and theta2Arr with random numbers from -0.1:0.1
    void InitializeRandomTheta()
    {

        System.Random random = new MathNet.Numerics.Random.SystemRandomSource();

        //Populate arrays with random values -0.1:0.1, up to 8 sig fig
        for (int i = 0; i < hiddenLayerSize; i++)
        {
            theta2Arr[i] = random.NextDouble() * (2 * init_epsilon) - init_epsilon;

            for (int j = 0; j <= inputLayerSize; j++)
            {
                theta1Arr[i, j] = random.NextDouble() * (2 * init_epsilon) - init_epsilon;

            }
        }
        theta2Arr[hiddenLayerSize] = random.NextDouble() * (2 * init_epsilon) - init_epsilon;     
    }

    public void BackPropagate(FFData prevRun,FFData currRun, double alpha, double lambda)
    {
        theta2 = theta2 + alpha * (currRun.a3[0,0] - prevRun.a3[0,0]) * e_theta2.Transpose();

        theta1 = theta1 + alpha * (currRun.a3[0,0] - prevRun.a3[0,0]) * e_theta1;

        FFData newRun = FeedForward(currRun.input);

        e_theta2 = lambda * e_theta2 + (1 - newRun.a3[0,0]) * newRun.a3[0,0] * newRun.a2.Transpose();

        e_theta1 = lambda * e_theta1 + ((1 - newRun.a3[0,0]) * newRun.a3[0,0] * (((1 - newRun.a2.RemoveColumn(0)).PointwiseMultiply(newRun.a2.RemoveColumn(0))).PointwiseMultiply(theta2.RemoveColumn(0)).Transpose()) * newRun.a1);
    }

    //Calculates partial derivatives for weights. No longer necessary
    //void PartialDerivatives(out Matrix<double> delta1, out Matrix<double> delta2, FFData run, FFData nextRun)
    //{
    //    Matrix<double> d3 = run.a3.Subtract(nextRun.a3);
    //    Matrix<double> tempTheta2 = DenseMatrix.Create(theta2.RowCount, theta2.ColumnCount, 0);
    //    theta2.CopyTo(tempTheta2);
    //    tempTheta2 = tempTheta2.RemoveColumn(0);
    //    Matrix<double> u = MatrixSigmoidGradient(run.z2);
    //    Matrix<double> d2 = (tempTheta2.TransposeThisAndMultiply(d3));
    //    d2 = d2.Transpose().PointwiseMultiply(u);
    //
    //    delta1 = d2.Transpose().Multiply(run.a1);
    //    delta2 = d3.Multiply(run.a2);
    //}

    //Mean squared error cost function. No longer necessary
    //public Vector<double> CostFunction(FFData prevRun, FFData currRun)
    //{
    //    return 0.5 * prevRun.a3.Subtract(currRun.a3);//Mathf.Pow((float)(prevRun.a3.Subtract(currRun.a3), 2);
    //}

    public void ResetTraceValues()
    {
        e_theta1 = DenseMatrix.Create(hiddenLayerSize, inputLayerSize + 1, 0);
        e_theta2 = DenseMatrix.Create(hiddenLayerSize + 1, 1, 0);
    }

    // Update is called once per frame
    public FFData FeedForward (double[] _inputFeatures) {

        FFData data = new FFData();

        data.input = _inputFeatures;

        //Add bias unit to input array
        data.a1 = DenseMatrix.Create(1, 1, 1).Append(DenseVector.OfArray(_inputFeatures).ToRowMatrix());

        //Multiple input array (a1) by weights for each node in hidden layer to produce number at each node
        data.z2 = data.a1.Multiply(theta1.Transpose());

        //Run logistic function on results to squash
        data.a2 = MatrixSigmoid(data.z2);

        //Add bias unit to results of hidden layer
        data.a2 = DenseMatrix.Create(1, 1, 1).Append(data.a2);

        //Multiply results of hidden layer with weights for output node to produce final result
        data.z3 = data.a2.Multiply(theta2.Transpose());

        //Squash final result
        data.a3 = MatrixSigmoid(data.z3);

        //As we are using only 1 output, result is a 1x1 matrix
        return data;
	}

    static Matrix<double> MatrixSigmoidGradient(Matrix<double> _inMat)
    {
        Matrix<double> u = MatrixSigmoid(_inMat);
        return u.PointwiseMultiply(1 - u);
    }

    //Computes sigmoid function for every element in a matrix
    static Matrix<double> MatrixSigmoid(Matrix<double> _inMat)
    {
        Matrix<double> outMat = DenseMatrix.Create(_inMat.RowCount, _inMat.ColumnCount, 0);

        for(int i = 0; i < _inMat.RowCount; i++)
        {
            for (int j = 0; j < _inMat.ColumnCount; j++)
            {
                outMat[i, j] = MathNet.Numerics.SpecialFunctions.Logistic(_inMat[i, j]);
            }
        }

        return outMat;
    }

    public NeuralNetwork Copy()
    {
        NeuralNetwork returnNet = new NeuralNetwork();
        theta1.CopyTo(returnNet.theta1);
        theta2.CopyTo(returnNet.theta2);
        e_theta1.CopyTo(returnNet.e_theta1);
        e_theta2.CopyTo(returnNet.e_theta2);

        return returnNet;
    }
}
