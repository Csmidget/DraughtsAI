using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MathNet.Numerics.LinearAlgebra;

//Struct to act as a data dump for a single NN feedforward iteration
public class FFData {

    //Raw input data
    public double[] input;

    //input including bias unit
    public Matrix<double> a1;

    //Raw result of weights * a1
    public Matrix<double> z2;

    //z2 passed through logistic function. Bias unit included
    public Matrix<double> a2;

    //Raw result of weights * a2
    public Matrix<double> z3;

    //z3 passed through logistic function. Output of NN.
    public Matrix<double> a3;

    public FFData Clone()
    {
        FFData returnData = new FFData();

        returnData.input = (double[])input.Clone();
        returnData.a1 = a1.Clone();
        returnData.z2 = z2.Clone();
        returnData.a2 = a2.Clone();
        returnData.z3 = z3.Clone();
        returnData.a3 = a3.Clone();

        return returnData;
    }
}
