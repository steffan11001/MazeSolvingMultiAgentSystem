﻿using System;
using System.Collections.Generic;
namespace maze
{
    public class Utils
    {
        public static int Size = 20;
        public static int NoExplorers = 5;
        public static int Delay = 100;
        public static Random RandNoGen = new Random();
        public static void ParseMessage(string content, out string action, out List<string> parameters)
        {
            string[] t = content.Split();
            action = t[0];
            parameters = new List<string>();
            for (int i = 1; i < t.Length; i++)
                parameters.Add(t[i]);
        }
        public static void ParseMessage(string content, out string action, out string parameters)
        {
            string[] t = content.Split();
            action = t[0];
            parameters = "";
            if (t.Length > 1)
            {
                for (int i = 1; i < t.Length - 1; i++)
                    parameters += t[i] + " ";
                parameters += t[t.Length - 1];
            }
        }
        public static string Str(object p1, object p2)
        {
            return string.Format("{0} {1}", p1, p2);
        }
        public static string Str(object p1, object p2, object p3)
        {
            return string.Format("{0} {1} {2}", p1, p2, p3);
        }
        public static string Str(object p1, object weight, object left, object right, object top, object bottom)
        {
            return string.Format("{0} {1} {2} {3} {4} {5}", p1, weight, left, right, top, bottom);
        }
    }
}