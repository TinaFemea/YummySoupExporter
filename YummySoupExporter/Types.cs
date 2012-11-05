using System.IO;
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
    
    public enum CommonFields
    {
        hasBeenPrepared,
        difficulty,
        rating,
        cookingTime,
        inactivePrepTime,
        prepTime,
        keywords,
        name,
        yield,
        description,
        notes,
        directions,
        importedFrom,
        ingredients,
        attribution,
        unknown
    }
    public class Utils
    {
        public static string RemoveUnicode(string source) //  old XML - didn't support unicode.
        {
            string temp = source.Replace(@"\U00bd", @" 1/2").Trim();
            temp = temp.Replace(@"\U00bc", @" 1/4").Trim();
            temp = temp.Replace(@"\U2033", "in");
            temp = temp.Replace(@"\U2019", "'");

            return Regex.Replace(temp, @"[^\u0000-\u007F]", string.Empty);
        }

        public static string CleanFileName(string fileName)
        {
            return string.Join("_", fileName.Split(Path.GetInvalidFileNameChars()));
        }


        public static string StripTags(string source)
        {
            return Regex.Replace(source, "<.*?>", string.Empty);
        }

        public static string CleanText(string source)
        {
            return StripTags(RemoveUnicode(source));
        }
    }
}
