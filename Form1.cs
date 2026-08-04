using FactoryPlanner.Models;
using FactoryPlanner.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FactoryPlanner
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            InitComboBoxes();
        }

        private void InitComboBoxes()
        {
            comboBoxGame.Items.Clear();
            comboBoxGame.Items.Add("StarRupture");
            comboBoxGame.SelectedIndex = 0;
        }

        private void ParseStarRuptureHtml()
        {
            recipes = StarRuptureHtmlParser.ParseStarRupture();
            string json = Recipe.Serialize(recipes);
            System.Diagnostics.Trace.WriteLine(json);
        }

        private void comboBoxMaterials_SelectedIndexChanged(object sender, EventArgs e)
        {
            var name = comboBoxMaterials.Text;
            var sb = new StringBuilder();
            comboBoxRecipe.Items.Clear();
            foreach (var recipe in recipes.Where(r => r.Outputs.Any(o => o.Name == name)))
            {
                comboBoxRecipe.Items.Add(recipe.ToString());
            }
            comboBoxRecipe.SelectedIndex = 0;
        }

        private void PopulateMaterials()
        {
            var names = new HashSet<string>();
            foreach (var recipe in recipes)
            {
                foreach (var material in recipe.Inputs)
                {
                    names.Add(material.Name);
                }
            }
            comboBoxMaterials.Items.Clear();
            var temp = names.ToArray();
            Array.Sort(temp);
            comboBoxMaterials.Items.AddRange(temp);
            comboBoxMaterials.SelectedIndex = 0;
        }

        private void comboBoxGame_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxGame.SelectedIndex == 0)
            {
                recipes = StarRuptureHtmlParser.LoadRecipes();
            }

            PopulateMaterials();
        }

        private void buttonGo_Click(object sender, EventArgs e)
        {
            var quantity = (double)numericUpDownQuantity.Value;
            var recipe = FindSelectedRecipe();
            if (recipe != null)
            {
                var material = (string)comboBoxMaterials.SelectedItem;
                var builder = new ProductionPlanBuilder();
                plan = builder.BuildPlan(recipes, material, quantity, recipe);
                string formattedPlan = builder.FormatPlan(plan);
                textBoxRecipes.Text = formattedPlan;
            }
        }

        private void buttonGraph_Click(object sender, EventArgs e)
        {
            using (var dlg = new GraphDlg(plan))
            {
                dlg.ShowDialog(this);
            }
        }

        private List<Models.Recipe> recipes;
        private List<ProductionStep> plan;

        private const string StarRupture = "Star Rupture";

        private void comboBoxRecipe_SelectedIndexChanged(object sender, EventArgs e)
        {
            var recipe = FindSelectedRecipe();
            if (recipe != null)
            {
                numericUpDownQuantity.Value = (decimal)recipe.Outputs[0].Rate;
            }
        }

        private Recipe FindSelectedRecipe()
        {
            var recipeText = comboBoxRecipe.SelectedItem;
            return recipes.Where(r => string.Equals(r.ToString(), recipeText)).FirstOrDefault();
        }
    }
}
