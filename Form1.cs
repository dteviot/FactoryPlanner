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

        private List<Models.Recipe> recipes;

        private const string StarRupture = "Star Rupture";

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
            var recipeText = comboBoxRecipe.SelectedItem;
            var recipe = recipes.Where(r => string.Equals(r.ToString(), recipeText)).FirstOrDefault();
            if (recipe != null)
            {
                var material = (string)comboBoxMaterials.SelectedItem;
                var builder = new ProductionPlanBuilder();
                var plan = builder.BuildPlan(recipes, material, quantity, recipe);
                string formattedPlan = builder.FormatPlan(plan);
                textBoxRecipes.Text = formattedPlan;
            }
        }
    }
}
