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
   

    [XmlRootAttribute("mx2")]
    public class MastercookRecipe
    {
        public class Summary
        {
            [XmlElement("Nam")]
            public string Name;
        }

        public class Ingredient
        {
            [XmlAttribute]
            public string name;
            [XmlAttribute]
            public string unit;
            [XmlAttribute]
            public string qty;
            [XmlAttribute]
            public string code;

            [XmlElement("IPrp")]
            public string prep;
        }
        public class Recipe
        {
            [XmlAttribute]
            public string name;
            [XmlAttribute]
            [DefaultValue("qwerty")]    //  This is evil.  I need to always have an author tag written, and the serializer won't write out a value equal to the default.
            //  So we set the default to something unlikely.
            public string author;

            [XmlArray("CatS")]
            [XmlArrayItem("CatT")]
            public string[] categories;


            [XmlElement("IngR")] 
            public Ingredient[] ingredients;

            
            [XmlArray("DirS")]
            [XmlArrayItem("DirT")]
            public string[] directions;
        }
        [XmlAttribute]
        public string source = "MasterCook";
        [XmlAttribute]
        public string date = String.Empty;

        [XmlElement("Summ")] public Summary summary;

        [XmlElement("RcpE")] public Recipe recipe;
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
            if (!inputRecipe.ContainsKey("name") || String.Equals(inputRecipe["name"], "New Recipe"))
                return;

            MastercookRecipe recipe = new MastercookRecipe();
            recipe.summary = new MastercookRecipe.Summary {Name = inputRecipe["name"]};
            recipe.recipe = new MastercookRecipe.Recipe {name = inputRecipe["name"], author = String.Empty};
            List<MastercookRecipe.Ingredient> outputIngredients = new List<MastercookRecipe.Ingredient>();
            
            if (inputRecipe.ContainsKey("ingredients"))
            {
                List<Ingredient> inputIngredients =
                    JSONReaderWriter<List<Ingredient>>.ReadFromString(inputRecipe["ingredients"]);
                foreach (Ingredient inputIngredient in inputIngredients)
                {
                    MastercookRecipe.Ingredient outputIngredient = new MastercookRecipe.Ingredient
                                                                       {
                                                                           code =
                                                                               inputIngredient.isGroupTitle ? "G" : "I",
                                                                           name = inputIngredient.name,
                                                                           prep = inputIngredient.method,
                                                                           qty = inputIngredient.quantity,
                                                                           unit = inputIngredient.measurement
                                                                       };
                    outputIngredients.Add(outputIngredient);
                }
                recipe.recipe.ingredients = outputIngredients.ToArray();
            }
            if (inputRecipe.ContainsKey("keywords"))
                recipe.recipe.categories = inputRecipe["keywords"].Split(new []{','}, StringSplitOptions.RemoveEmptyEntries);


            if (inputRecipe.ContainsKey("directions"))
                recipe.recipe.directions = inputRecipe["directions"].Split(new []{"\n\n"}, StringSplitOptions.RemoveEmptyEntries);

            string output = WriteToString(recipe);


            string fileName = Path.Combine(outputPath, Utils.CleanFileName(inputRecipe["name"] + ".mx2"));
            int counter = 0;
            while(File.Exists(fileName))
                fileName = Path.Combine(outputPath, Utils.CleanFileName(inputRecipe["name"] + counter++.ToString() + ".mx2"));

            using (FileStream stream = new FileStream(fileName, FileMode.Create))
            {
                byte[] outputBytes = Encoding.ASCII.GetBytes(output);
                stream.Write(outputBytes, 0, outputBytes.Count());
            }
        }

        

    }
}
