using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;


namespace JPEG.Utilities
{
    public static class MathEx
    {
        //public static double Sum(int from, int to, Func<int, double> function)
        //{
        //    return Enumerable.Range(from, to).Sum(function);
        //}
		
        //public static double SumByTwoVariables(int from1, int to1, int from2, int to2, Func<int, int, double> function)
        //{
        //    return Sum(from1, to1, x => Sum(from2, to2, y => function(x, y)));
        //}

        //public static double SumByTwoVariables(List<int> e1, List<int> e2,Func<int, int, double> function)
        //{
        //    // return Sum(e1, x => Sum(e2, y => function(x, y)));
        //    var result = 0.0;
        //    for (var i = 0; i < e1.Count; i++)
        //    {
        //        result += Sum(e1, y => function(e1[i], y));

        //    }
        //    return result;
        //}

        public static double SumByTwoVariables(List<int> e1, List<int> e2, Func<double, double> f,Func<int, int, double, double> function)
        {
           // return Sum(e1, x => Sum(e2, y => function(x, y)));
            var result = 0.0;
            for (var i = 0; i < e1.Count; i++)
            {
                var l = f(e1[i]);
                result += Sum(e1, y => function(e1[i], y, l));

            }
            return result;
        }

        public static double Sum(List<int> e, Func<int, double> function)
        {
            var result = 0.0;
            for (var i = 0; i < e.Count; i++)
                result += function(e[i]);
            return result;
        }

    }
}