namespace Antlr.Runtime.Misc
{
    using Antlr.Runtime;
    using System;
    using System.IO;

    public class Stats
    {
        public static double Avg(int[] X)
        {
            double num = 0.0;
            int length = X.Length;
            if (length != 0)
            {
                for (int i = 0; i < length; i++)
                {
                    num += X[i];
                }
                if (num >= 0.0)
                {
                    return (num / ((double) length));
                }
            }
            return 0.0;
        }

        public static string GetAbsoluteFileName(string filename)
        {
            return Path.Combine(Path.Combine(Environment.CurrentDirectory, Constants.ANTLRWORKS_DIR), filename);
        }

        public static int Max(int[] X)
        {
            int num = -2147483648;
            int length = X.Length;
            if (length == 0)
            {
                return 0;
            }
            for (int i = 0; i < length; i++)
            {
                if (X[i] > num)
                {
                    num = X[i];
                }
            }
            return num;
        }

        public static int Min(int[] X)
        {
            int num = 0x7fffffff;
            int length = X.Length;
            if (length == 0)
            {
                return 0;
            }
            for (int i = 0; i < length; i++)
            {
                if (X[i] < num)
                {
                    num = X[i];
                }
            }
            return num;
        }

        public static double Stddev(int[] X)
        {
            int length = X.Length;
            if (length <= 1)
            {
                return 0.0;
            }
            double num2 = Avg(X);
            double d = 0.0;
            for (int i = 0; i < length; i++)
            {
                d += (X[i] - num2) * (X[i] - num2);
            }
            d /= (double) (length - 1);
            return Math.Sqrt(d);
        }

        public static int Sum(int[] X)
        {
            int num = 0;
            int length = X.Length;
            if (length == 0)
            {
                return 0;
            }
            for (int i = 0; i < length; i++)
            {
                num += X[i];
            }
            return num;
        }

        public static void WriteReport(string filename, string data)
        {
            string absoluteFileName = GetAbsoluteFileName(filename);
            FileInfo info = new FileInfo(absoluteFileName);
            info.Directory.Create();
            try
            {
                StreamWriter writer = new StreamWriter(info.FullName, true);
                writer.WriteLine(data);
                writer.Close();
            }
            catch (IOException exception)
            {
                ErrorManager.InternalError("can't write stats to " + absoluteFileName, exception);
            }
        }
    }
}

