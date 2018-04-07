using System;

namespace JPEG
{
	public class DCT
	{
	    public static double[,] DCT2D(double[,] input, double[,] coefficients, double[,] tansponseCoefficients)
	    {
	        var height = input.GetLength(0);
	        var width = input.GetLength(1);
	        var beta = Beta(height, width);
	        var temp = new double[height, width];
            MultiMatrix(coefficients, input, temp);
	        var result = new double[height, width];
            MultiMatrix(temp, tansponseCoefficients , result);
            for (var i = 0; i < width; i++)
                for (var j = 0; j < height; j++)
                    result[i, j] = result[i, j] * beta;
            return result;

	    }

        public static void IDCT2D(double[,] coeffs, double[,] output, double[,] DCTCoefficients, double[,] transponseCoefficients)
		{
		    var beta = Beta(coeffs.GetLength(0), coeffs.GetLength(1));
		    var c = new double[coeffs.GetLength(0), coeffs.GetLength(1)];
            MultiMatrix(transponseCoefficients, coeffs, c);
            MultiMatrix(c, DCTCoefficients, output);
            for (var i = 0; i < coeffs.GetLength(1); i++)
                for (var j = 0; j < coeffs.GetLength(0); j++)
                    output[i, j] = output[i, j] * beta;
        }


        public static double[,] GetCoefficientsMatrix(int w, int h)
	    {
	        var result = new double[w, h];
            for(var i=0; i < w; i++)
            {
                var alpha = Alpha(i);
                for (var j = 0; j < h; j++)
                    result[i, j] = alpha* Math.Cos(Math.PI * (2*j + 1d) / 16 * i);
                }
	        return result;

	    }

	    private static double Alpha(int u) => u == 0 ? 1 / Math.Sqrt(2) : 1;

		private static double Beta(int height, int width) => 1d / width + 1d / height;

	    public static double[,] MultiMatrix(double[,] a, double[,] b, double[,] output)
	    {
	        for (var i = 0; i < output.GetLength(0); i++)
	            for (var j = 0; j < output.GetLength(1); j++)
	            {
	                output[i, j] = 0;
	                for (var k = 0; k < a.GetLength(1); k++)
	                    output[i, j] = output[i, j] + a[i, k] * b[k, j];
	            }
	        return output;
	    }

	    public static double[,] Transpose(double[,] matrix)
	    {
	        var result = new double[matrix.GetLength(1), matrix.GetLength(0)];
	        for (var i = 0; i < matrix.GetLength(0); i++)
	            for (var j = 0; j < matrix.GetLength(1); j++)
	                result[j, i] = matrix[i, j];
	        return result;
	    }
    }
}