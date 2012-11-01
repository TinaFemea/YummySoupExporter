using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace YummySoupExporter
{
    public class Summary
    {
        [XmlElement("Nam")] public string Name;
    }

    public class Recipe
    {
        [XmlAttribute]
        public string name;
        [XmlAttribute]
        [DefaultValue("qwerty")]    //  This is evil.  I need to always have an author tag written, and the serializer won't write out a value equal to the default.
                                    //  So we set the default to something unlikely.
        public string author;

        
    }

    [XmlRootAttribute("mx2")]
    public class MastercookRecipe
    {
        [XmlAttribute]
        public string source = "MasterCook";
        [XmlAttribute]
        public string date = String.Empty;

        [XmlElement("Summ")] public Summary Summary;

        [XmlElement("RcpE")] public Recipe Recipe;
    }

    class MastercookMX2Writer
    {
        private string WriteToString(MastercookRecipe toBeWritten)
        {
            XmlSerializer writer = new XmlSerializer(typeof(MastercookRecipe));
            string returnString;

            using (MemoryStream memoryStream = new MemoryStream())
            {
                writer.Serialize(memoryStream, toBeWritten);
                memoryStream.Position = 0;

                StreamReader sr = new StreamReader(memoryStream);
                returnString = sr.ReadToEnd();
            }

            //  Most readers expect this to look like a "real" Mastercook file.  That mean "no new XML stuff".  Clean up the header.
            returnString = returnString.Replace("<?xml version=\"1.0\"?>", "<?xml version=\"1.0\" standalone=\"yes\"?>");
            returnString =
                returnString.Replace(
                    "<mx2 xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" source=\"MasterCook\" date=\"\">",
                    "<!DOCTYPE mx2 SYSTEM \"mx2.dtd\">\r\n<mx2 source=\"MasterCook\" date=\"\">");
            return returnString;
        }
        
        public void WriteOneFile(StringDictionary inputRecipe, string outputPath)
        {
            MastercookRecipe recipe = new MastercookRecipe();
            recipe.Summary = new Summary {Name = inputRecipe["name"]};
            recipe.Recipe = new Recipe {name = inputRecipe["name"], author = String.Empty};

            string test = WriteToString(recipe);
        }
    }
}
