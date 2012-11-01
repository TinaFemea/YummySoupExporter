using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.SQLite;
using System.IO;
using System.Windows.Forms;
using YummySoupExporter.Properties;
using System.Linq;

namespace YummySoupExporter
{
    public partial class YSEForm : Form
    {
        public YSEForm()
        {
            InitializeComponent();
        }

        private void inputBrowse_Click(object sender, EventArgs e)
        {
            FileDialog dlg = new OpenFileDialog();
            dlg.Filter = Resources.FileFilter;
            if (DialogResult.OK == dlg.ShowDialog(this))
            {
                inputPath.Text = dlg.FileName;
            }

        }

        private void exportBrowse_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog();
            if (DialogResult.OK == dlg.ShowDialog())
            {
                outputPath.Text = dlg.SelectedPath;
            }
        }

        private string GetFriendlyName(string rowName)
        {
            switch (rowName)
            {
                case "ZHASBEENPREPARED":
                    return "hasBeenPrepared";
                case "ZDIFFICULTY":
                    return "difficulty";
                case "ZRATING":
                    return "rating";
                case "ZCOOKINGTIME":
                    return "cookingTime";
                case "ZINACTIVEPREPTIME":
                    return "inactivePrepTime";
                case "ZPREPTIME":
                    return "prepTime";
                case "ZKEYWORDS":
                    return "keywords";
                case "ZNAME":
                    return "name";
                case "ZYIELD":
                    return "yield";
                case "ZRECIPEDESCRIPTION":
                    return "description";
                case "ZNOTES":
                    return "notes";
                case "ZDIRECTIONS":
                    return "directions";
                case "ZIMPORTEDFROMURL":
                    return "importedFrom";
                case "ZINGREDIENTSARRAY":
                    return "ingredients";
                case "ZATTRIBUTION":
                    return "attribution";
                default:
                    return String.Empty;
            }
        }

       private string ParseIngredientsToJSON(string p)
        {
            List<Ingredient> ingredients = new List<Ingredient>();
            //  What I have here is a mac NSDictionary.  I can't find a "real" C# parser for it, so 
            //  I'm going to parse it by hand and then spit it back out as a parsable JSON string.
            using (StringReader reader = new StringReader(p))
            {
                Ingredient currentIngredient = new Ingredient();    // this is probably a waste of an object creation, but it saves me a lot of null-checking
                    
                while(true)
                {
                    string thisLine = reader.ReadLine();
                    if (thisLine == null)
                        break;
                    thisLine = thisLine.Trim();
                    if (String.IsNullOrEmpty(thisLine))
                        continue;
                    if (thisLine.Equals("(") || thisLine.Equals("("))   // noop - first or last line
                        continue;
                    if (thisLine.Equals("{"))   //  new object
                    {
                        currentIngredient = new Ingredient();
                        continue;
                    }
                    if (thisLine.StartsWith("}"))   //  end of an object
                    {
                        ingredients.Add(currentIngredient);
                        continue;
                    }
                    //  Now, split up the rest.
                    string[] pieces = thisLine.Split(new [] {" = "}, 10, StringSplitOptions.RemoveEmptyEntries);
                    if (!pieces.Any())
                        continue;

                    FillInField(pieces, currentIngredient);
                }
            }

            return JSONReaderWriter<List<Ingredient>>.WriteToString(ingredients);
        }

        private void FillInField(string[] pieces, Ingredient currentIngredient)
        {
            string fieldName = pieces[0].Trim();
            string value;

            if (pieces.Count() == 1)
                value = String.Empty;
            else if (pieces.Count() == 2)
              value = pieces[1].Trim().TrimEnd(';').Trim('\"');
            else
              value = String.Join(" = ", pieces, 1, pieces.Count() - 1).Trim().TrimEnd(';').Trim('\"');
        

            switch (fieldName)
            {
                case "name":
                    currentIngredient.name = value;
                    break;
                case "quantity":
                    currentIngredient.quantity = value;
                    break;
                case "method":
                    currentIngredient.method = value;
                    break;
                case "measurement":
                    currentIngredient.measurement = value;
                    break;
                case "isGroupTitle":
                    currentIngredient.isGroupTitle = true;
                    break;
            }
        }


        private void exportButton_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(outputPath.Text) || !Directory.Exists(outputPath.Text))
            {
                MessageBox.Show(this, Resources.BadOutputPath, Resources.Error, MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                return;
            }

            if (String.IsNullOrEmpty(inputPath.Text) || !File.Exists(inputPath.Text))
            {
                MessageBox.Show(this, Resources.BadInputPath, Resources.Error, MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                return;
            }

            try
            {
                MastercookMX2Writer writer = new MastercookMX2Writer();
                string outputPathString = outputPath.Text;  //store this, so they don't change it on us.
                exportButton.Enabled = false;   //  don't let them keep clicking it.
                SQLiteConnection connection = new SQLiteConnection(String.Format("Data Source={0}", inputPath.Text));
                connection.Open();
                
                SQLiteCommand countCommand = connection.CreateCommand();
                countCommand.CommandText = "select count(*) from ZRECIPES";
                long count = (long)countCommand.ExecuteScalar();

                exportProgressBar.Maximum = (int) count;    //  is anyone ever going to have more then MaxInt recipes?  

                SQLiteCommand getRowCommand = connection.CreateCommand();
                getRowCommand.CommandText = "select * from ZRECIPES";
                SQLiteDataReader reader = getRowCommand.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        StringDictionary dictionary = new StringDictionary();
                        NameValueCollection row = reader.GetValues();
                        foreach (string key in row.Keys)
                        {
                            //  turn the keys into something more generic.  Why the heck are all these columns
                            //  starting with Z?
                            string friendlyName = GetFriendlyName(key);
                            if (!String.IsNullOrEmpty(friendlyName))
                                dictionary.Add(friendlyName, row[key]);

                        }

                        if (!String.IsNullOrEmpty(dictionary["ingredients"]))
                            dictionary["ingredients"] = ParseIngredientsToJSON(dictionary["ingredients"]);

                        writer.WriteOneFile(dictionary, outputPathString);
                        exportProgressBar.PerformStep();
                    }
                }
                
                connection.Close();
            }
            catch (Exception exception)
            {
                MessageBox.Show(this, exception.Message, Resources.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw;
            }
            finally
            {
                exportButton.Enabled = true;
            }
            
        }
       
    }
}
