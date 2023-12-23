using System;
using System.Collections.Generic;

namespace maze
{
    public class Utils
    {
        public static int Size = 7;
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
        public static string StrForExplorer(object p1, object cellsObj, object weightsObj)
        {
            string cells = (string)cellsObj;
            string weightsStr = (string)weightsObj;
            string[] weights = weightsStr.Split(' ');
            return string.Format("{0} {1} {2} {3} {4} {5} {6} {7} {8}", p1, cells[0], cells[1], cells[2], cells[3],
                                                        weights[0], weights[1], weights[2], weights[3]);
        }
        public static string Str(object p1, object p2, object p3, object p4)
        {
            return string.Format("{0} {1} {2} {3}", p1, p2, p3, p4);
        }

    }
}