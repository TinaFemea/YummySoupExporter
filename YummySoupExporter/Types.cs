using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace YummySoupExporter
{
    public class Ingredient
    {
        public string name;
        public string method;
        public string quantity;
        public string measurement;
        public bool isGroupTitle;
    }
        
    public class Utils
    {
        public static string RemoveUnicode(string source) //  old XML - didn't support unicode.
        {
            return Regex.Replace(source, @"[^\u0000-\u007F]", string.Empty);
        }

        public static string CleanFileName(string fileName)
        {
            return string.Join("_", fileName.Split(Path.GetInvalidFileNameChars()));
        }
    }
}
