using System;
using System.Collections.Generic;


namespace NodeLife
{
    public static class Utils
    {
        public static bool IsPowerOfTwo(int number)
        {
            return (number & -number) == number;
        }
    }
}
