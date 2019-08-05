using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Cache;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using UnityEngine;

public static class Utility {
    public static System.Random rngGen = new System.Random();

    /// <summary>
    /// Low becomes High, High becomes low.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="maxValue"></param>
    /// <returns>Value between 0-1</returns>
    public static double NormalizedInverseSigmoid(double value, double maxValue = 1f)
    {
        double normalize = value / maxValue;

        double sigmoid = 1f / (1f + Math.Pow(normalize / (1 - normalize), 3));

        return Clamp(sigmoid, 0, 1);
    }

    /// <summary>
    /// Low becomes low, High becomes high.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="maxValue"></param>
    /// <returns>Value between 0-1</returns>
    public static double NormalizedSigmoid(double value, double maxValue = 1f)
    {
        double normalize = value / maxValue;

        double sigmoid = 1f / (1f + Math.Pow(normalize / (1 - normalize), -3));

        return Clamp(sigmoid, 0, 1);
    }

    /// <summary>
    /// Low becomes low, High becomes high.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="maxValue"></param>
    /// <returns>Value between 0-1</returns>
    public static double NormalizedExponential(double value, double maxValue = 1f)
    {
        double normalize = value / maxValue;

        return Clamp(normalize * normalize, 0, 1);
    }

    /// <summary>
    /// Low becomes High, High becomes low.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="maxValue"></param>
    /// <returns>Value between 0-1</returns>
    public static double NormalizedInverseExponential(double value, double maxValue = 1f)
    {
        double normalize = value / maxValue;

        return Clamp(-(normalize * normalize) + 1, 0, 1);
    }

    /// <summary>
    /// Low is low, High is High.
    /// High rate of change when low, lowers rate of change when high.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="maxValue"></param>
    /// <returns>Value between 0-1</returns>
    public static double NormalizedParabolic(double value, double maxValue = 1f)
    {
        double normalize = value / maxValue;

        return Clamp(-((normalize - 1) * (normalize - 1)) + 1, 0,1);
    }

    /// <summary>
    /// Low is low, High is low.
    /// Peak at 0.5.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="maxValue"></param>
    /// <returns></returns>
    public static double NormalizedQuadratic(double value, double maxValue = 1f)
    {
        double normalize = value / maxValue;

        return Clamp(-(normalize * normalize) + normalize, 0,1);
    }

    /// <summary>
    /// Clamps the provided value so that it is not less than min and not more than max.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="val"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    public static T Clamp<T>(this T val, T min, T max) where T : IComparable<T>
    {
        if (val.CompareTo(min) < 0) return min;
        else if (val.CompareTo(max) > 0) return max;
        else return val;
    }

    public static double Lerp(double from, double to, double perc)
    {
        perc = Clamp(perc, 0.0, 1.0);
        return from+((to - from) * perc);
    }

    public static void TryAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
    {
        if (!dictionary.ContainsKey(key))
        {
            dictionary.Add(key, value);
        }
    }

    /// <summary>
    /// Replaces the values of x with the values of y.
    /// Requires [StructLayout(LayoutKind.Sequential)] or (LayoutKind.Explicit).
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="x"></param>
    /// <param name="y"></param>
    public static void Replace<T>(T x, T y)
    where T : class
    {
        // replaces 'x' with 'y'
        if (x == null) throw new ArgumentNullException("x");
        if (y == null) throw new ArgumentNullException("y");

        var size = Marshal.SizeOf(typeof(T));
        var ptr = Marshal.AllocHGlobal(size);
        Marshal.StructureToPtr(y, ptr, false);
        Marshal.PtrToStructure(ptr, x);
        Marshal.FreeHGlobal(ptr);
    }

    public static void Shuffle<T>(List<T> list)
    {
        List<T> shuffled = new List<T>(list.Count);
        while (list.Count > 0)
        {
            int index = Utility.rngGen.Next(0, list.Count);
            shuffled.Add(list[index]);
            list.RemoveAt(index);
        }

        list.AddRange(shuffled);
    }

    public static bool scientificNotation = false;
    public static string FormatBigNumber(double number)
    {
        //Debug.Log("Formatting " + number.ToString("0"));
        int k = 0;
        while(Math.Abs(number / Math.Pow(1000.0, k+1)) >= 1)
        {
            k++;
        }

        if(k > 0)
        {
            string s = (number / Math.Pow(1000.0, k)).ToString("N1");
            if (scientificNotation)
            {
                return  s + "e" + k * 3;
            } else
            {
                switch (k)
                {
                    case 1:
                        return s + "K";
                    case 2:
                        return s + "M";
                    case 3:
                        return s + "B";
                    case 4:
                        return s + "T";
                    case 5:
                        return s + "q";
                    case 6:
                        return s + "Q";
                    case 7:
                        return s + "s";
                    case 8:
                        return s + "S";
                    case 9:
                        return s + "O";
                    default:
                        //Fallback on scientific notation
                        return s + "e" + k * 3;
                }
            }
        }

        return number.ToString("N1");
    }

    public static DateTime? GetNistTime()
    {
        DateTime? dateTime = null;

        //Try fetching 3 times
        try
        {
            for (int i = 0; i < 3; i++)
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://nist.time.gov/actualtime.cgi");
                request.Method = "GET";
                request.Accept = "text/html, application/xhtml+xml, */*";
                request.UserAgent = "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.1; Trident/6.0)";
                request.ContentType = "application/x-www-form-urlencoded";
                request.CachePolicy = new RequestCachePolicy(RequestCacheLevel.NoCacheNoStore); //No caching
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    using (StreamReader stream = new StreamReader(response.GetResponseStream()))
                    {
                        string html = stream.ReadToEnd();//<timestamp time=\"1395772696469995\" delay=\"1395772696469995\"/>
                        string time = Regex.Match(html, @"(?<=\btime="")[^""]*").Value;
                        double milliseconds = Convert.ToInt64(time) / 1000.0;
                        dateTime = new DateTime(1970, 1, 1).AddMilliseconds(milliseconds);
                    }
                    return dateTime;
                }
                else
                {
                    Debug.LogError("Couldnt get date/time " + response.StatusCode.ToString());
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }

        return dateTime;
    }
}
