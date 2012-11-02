using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace YummySoupExporter
{

    //  Huge thanks to the author of http://krecipes.sourcearchive.com/documentation/2.0~beta2-3/mx2exporter_8cpp_source.html.  
    //  Showed me a lot of less-commonly used tags.

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

        public class Yield
        {
            [XmlAttribute]
            public string unit;
            [XmlAttribute]
            public string qty;
            
        }
        public class Recipe
        {
            [XmlAttribute]
            public string name;
            [XmlAttribute]
            [DefaultValue("qwerty")]    //  This is evil.  I need to always have an author tag written, and the serializer won't write out a value equal to the default.
            //  So we set the default to something unlikely.
            public string author;

            [XmlElement("Serv")] 
            public string servings;

            [XmlElement("PrpT")]
            public string prepTime;

            [XmlArray("CatS")]
            [XmlArrayItem("CatT")]
            public string[] categories;


            [XmlElement("IngR")] 
            public Ingredient[] ingredients;

            
            [XmlArray("DirS")]
            [XmlArrayItem("DirT")]
            public string[] directions;

            [XmlElement("Srce")]
            public string source;

            [XmlElement("Yield")]
            public Yield yield;

            [XmlArray("RatS")]
            [XmlArrayItem("RatE")]
            public string[] rating;  // only ever has one.  See note above.

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
        
        public void WriteOneFile(Dictionary<CommonFields, string> inputRecipe, string outputPath)
        {
            if (!inputRecipe.ContainsKey(CommonFields.name) || String.Equals(inputRecipe[CommonFields.name], "New Recipe"))
                return;

            MastercookRecipe recipe = new MastercookRecipe{
                                              summary = new MastercookRecipe.Summary {Name = inputRecipe[CommonFields.name]},
                                              recipe = new MastercookRecipe.Recipe
                                                      {name = inputRecipe[CommonFields.name], author = String.Empty}
                                          };
            if (inputRecipe.ContainsKey(CommonFields.ingredients))
            {
                List<Ingredient> inputIngredients =
                    JSONReaderWriter<List<Ingredient>>.ReadFromString(inputRecipe[CommonFields.ingredients]);
                recipe.recipe.ingredients = inputIngredients.Select(inputIngredient => new MastercookRecipe.Ingredient{
                                                                                               code = inputIngredient.isGroupTitle ? "S"  : "I",
                                                                                               name = inputIngredient.name,
                                                                                               prep = inputIngredient.method,
                                                                                               qty = inputIngredient.isGroupTitle ? "" : inputIngredient.quantity,
                                                                                               unit = inputIngredient.measurement
                                                                                           }).ToArray();
            }

            if (inputRecipe.ContainsKey(CommonFields.prepTime))
                recipe.recipe.prepTime = inputRecipe[CommonFields.prepTime];


            if (inputRecipe.ContainsKey(CommonFields.keywords))
                recipe.recipe.categories = inputRecipe[CommonFields.keywords].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);


            if (inputRecipe.ContainsKey(CommonFields.directions))
                recipe.recipe.directions = inputRecipe[CommonFields.directions].Split(new[] { "\n\n" }, StringSplitOptions.RemoveEmptyEntries);

            if (inputRecipe.ContainsKey(CommonFields.importedFrom))
                recipe.recipe.source = inputRecipe[CommonFields.importedFrom];

            if (inputRecipe.ContainsKey(CommonFields.yield))
            {
                string[] words = inputRecipe[CommonFields.yield].Split(new []{' '}, StringSplitOptions.RemoveEmptyEntries);    //  we don't have a seperate quantity and unit.  
                if (words.Count() == 1)
                    recipe.recipe.yield = new MastercookRecipe.Yield { qty = "1", unit = words[0] };
                else if (words.Any())
                    recipe.recipe.yield = new MastercookRecipe.Yield
                                              {
                                                  qty = String.Join(" ", words, 0, words.Count() - 1),
                                                  unit = words[words.Count() - 1]
                                              };
            }

            if (inputRecipe.ContainsKey(CommonFields.rating))
            {
                int rating;
                if (int.TryParse(inputRecipe[CommonFields.rating], out rating) && (rating != 0))
                    recipe.recipe.rating = new []{(rating*2).ToString()}; // mastercook uses a 10-point scale.
            }
            string output = WriteToString(recipe);


            string fileName = Path.Combine(outputPath, Utils.CleanFileName(inputRecipe[CommonFields.name] + ".mx2"));
            int counter = 1;
            while(File.Exists(fileName))
                fileName = Path.Combine(outputPath, Utils.CleanFileName(String.Format("{0}_{1}.mx2", inputRecipe[CommonFields.name], counter++)));

            using (FileStream stream = new FileStream(fileName, FileMode.Create))
            {
                byte[] outputBytes = Encoding.ASCII.GetBytes(output);
                stream.Write(outputBytes, 0, outputBytes.Count());
            }
        }

        

    }
}
