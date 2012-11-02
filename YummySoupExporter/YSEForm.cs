using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.SQLite;
using System.IO;
using System.Text.RegularExpressions;
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


        private CommonFields GetFriendlyName(string rowName)
        {
            switch (rowName)
            {
                case "ZHASBEENPREPARED":
                    return CommonFields.hasBeenPrepared;
                case "ZDIFFICULTY":
                    return CommonFields.difficulty;
                case "ZRATING":
                    return CommonFields.rating;
                case "ZCOOKINGTIME":
                    return CommonFields.cookingTime;
                case "ZINACTIVEPREPTIME":
                    return CommonFields.inactivePrepTime;
                case "ZPREPTIME":
                    return CommonFields.prepTime;
                case "ZKEYWORDS":
                    return CommonFields.keywords;
                case "ZNAME":
                    return CommonFields.name;
                case "ZYIELD":
                    return CommonFields.yield;
                case "ZRECIPEDESCRIPTION":
                    return CommonFields.description;
                case "ZNOTES":
                    return CommonFields.notes;
                case "ZDIRECTIONS":
                    return CommonFields.directions;
                case "ZIMPORTEDFROMURL":
                    return CommonFields.importedFrom;
                case "ZINGREDIENTSARRAY":
                    return CommonFields.ingredients;
                case "ZATTRIBUTION":
                    return CommonFields.attribution;
                default:
                    return CommonFields.unknown;
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

                while (true)
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
                        NormalizeMeasurements(currentIngredient);
                        ingredients.Add(currentIngredient);
                        continue;
                    }
                    //  Now, split up the rest.
                    string[] pieces = thisLine.Split(new[] { " = " }, 10, StringSplitOptions.RemoveEmptyEntries);
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
                    if (!value.Equals("NO", StringComparison.OrdinalIgnoreCase) && !value.Equals("0", StringComparison.OrdinalIgnoreCase))
                        currentIngredient.isGroupTitle = true;
                    break;
            }
        }

        private void NormalizeMeasurements(Ingredient ingred)
        {
            if (!String.IsNullOrEmpty(ingred.measurement))
            {
                bool needsPluralized = false;
                double measurement;
                if (!String.IsNullOrEmpty(ingred.quantity) && double.TryParse(ingred.quantity, out measurement) && (measurement > 1))
                    needsPluralized = true;

                if (ingred.measurement.Equals("tsp", StringComparison.OrdinalIgnoreCase) ||
                    ingred.measurement.Equals("tsp.", StringComparison.OrdinalIgnoreCase))
                    ingred.measurement = needsPluralized ? "teaspoons" : "teaspoon";
                if (ingred.measurement.Equals("Tbsp", StringComparison.OrdinalIgnoreCase) ||
                    ingred.measurement.Equals("tbsp.", StringComparison.OrdinalIgnoreCase))
                    ingred.measurement = needsPluralized ? "tablespoons" : "tablespoon";


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

                exportProgressBar.Maximum = (int)count;    //  is anyone ever going to have more then MaxInt recipes?  

                SQLiteCommand getRowCommand = connection.CreateCommand();
                getRowCommand.CommandText = "select * from ZRECIPES";
                SQLiteDataReader reader = getRowCommand.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        Dictionary<CommonFields, string> dictionary = new Dictionary<CommonFields, string>();
                        NameValueCollection row = reader.GetValues();
                        foreach (string key in row.Keys)
                        {
                            //  turn the keys into something more generic.  Why the heck are all these columns
                            //  starting with Z?
                            CommonFields friendlyName = GetFriendlyName(key);
                            if (friendlyName != CommonFields.unknown)
                                dictionary.Add(friendlyName, Utils.CleanText(row[key]));

                        }

                        if (!String.IsNullOrEmpty(dictionary[CommonFields.ingredients]))
                            dictionary[CommonFields.ingredients] = ParseIngredientsToJSON(dictionary[CommonFields.ingredients]);


                        try
                        {
                            writer.WriteOneFile(dictionary, outputPathString);
                        }
                        catch (Exception exception)
                        {
                            MessageBox.Show(this, exception.Message, Resources.Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
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
