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

        private void label1_Click(object sender, EventArgs e)
        {

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

        public class Ingredient
        {
            public string name;
            public string method;
            public string quantity;
            public string measurement;
            public bool isGroupTitle;
        }
        private string ParseIngredientsToJSON(string p)
        {
            List<Ingredient> ingredients = new List<Ingredient>();
            //  What I have here is a mac NSDictionary.  I can't find a "real" C# parser for it, so 
            //  I'm going to parse it by hand and then spit it back out as a parsable JSON string.
            using (StringReader reader = new StringReader(p))
            {
                while(true)
                {
                    Ingredient currentIngredient = new Ingredient();    // this is probably a waste of an object creation, but it saves me a lot of null-checking
                    string thisLine = reader.ReadLine();
                    if (thisLine == null)
                        break;
                    thisLine = thisLine.Trim();
                    if (String.IsNullOrEmpty(thisLine))
                        continue;
                    if (thisLine.Equals("("))   // noop - first line
                        continue;
                    if (thisLine.Equals("{"))   //  new object
                    {
                        currentIngredient = new Ingredient();
                    }
                    if (thisLine.Equals("}"))   //  end of an object
                    {
                        ingredients.Add(currentIngredient);
                    }
                    //  Now, split up the rest.
                    Regex firstWordFinder = new Regex(@"([^\s]+)( =)");
                    
                }
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
