using FactoryPlanner.Models;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FactoryPlanner.Services
{
    // reads HTML from internet and extracts recipe information from it
    internal class StarRuptureHtmlParser
   {
        public static List<Models.Recipe> LoadRecipes()
        {
            string json = Utils.ReadResource("FactoryPlanner.Data.StarRuptureRecipes.json");
            return Recipe.Deerialize(json);
        }

        public static List<Models.Recipe> ParseStarRupture()
        {
            var recipies = new List<Models.Recipe>();
            HtmlWeb web = new HtmlWeb();
            foreach (string resourceUrl in SourceDocumentsUrls)
            {
                var parser = new StarRuptureHtmlParser(web.Load(resourceUrl));
                recipies.AddRange(parser.GetRecipes());
            }
            return recipies;
        }

        public StarRuptureHtmlParser(HtmlDocument doc)
        {
            htmlDoc = doc;
        }

        public string GetMachineName()
        {
            var node = htmlDoc.DocumentNode.SelectSingleNode("//h1");
            return node.InnerText;
        }

        public List<Models.Recipe> GetRecipes()
        {
            var table = htmlDoc.DocumentNode.SelectSingleNode("//tbody");
            var rows = table.SelectNodes("tr");
            return rows.Select(row => RowToRecipe(row)).ToList();
        }

        private Models.Recipe RowToRecipe(HtmlAgilityPack.HtmlNode row)
        {
            var cells = row.SelectNodes("td").ToList();
            int itemsPerMinute = GetInnerTextAsInt(cells[3]);
            int duration = GetInnerTextAsInt(cells[2]);
            int batchesPerMinute = 60 / duration;
            var recipe = new Models.Recipe()
            {
                Machine = GetMachineName(),
                Duration = duration,
                OutputBatchSize = itemsPerMinute / batchesPerMinute,
            };
            recipe.Outputs.Add(GetRecipeMaterial(cells[0], batchesPerMinute));
            System.Diagnostics.Trace.Assert(recipe.Outputs[0].Rate == itemsPerMinute);
            var inputs = cells[1].Descendants("section")
                .Select(s => GetRecipeMaterial(s, batchesPerMinute))
                .ToList();
            recipe.Inputs.AddRange(inputs);
            return recipe;
        }

        private RecipeMaterial GetRecipeMaterial(HtmlAgilityPack.HtmlNode cell, int batchesPerMinute)
        {
            return new RecipeMaterial(GetMaterialName(cell), GetTabularValue(cell) * batchesPerMinute);
        }

        private string GetMaterialName(HtmlAgilityPack.HtmlNode cell)
        {
            return cell.Descendants("a").First().InnerText;
        }

        private int GetTabularValue(HtmlAgilityPack.HtmlNode cell)
        {
            var num = cell.Descendants("div").Where(d => d.GetAttributeValue("class", "").Contains("tabular-nums")).First();
            return GetInnerTextAsInt(num);
        }

        private int GetInnerTextAsInt(HtmlAgilityPack.HtmlNode cell)
        {
            return int.Parse(cell.InnerText.Replace("x", ""));
        }

        private HtmlAgilityPack.HtmlDocument htmlDoc = new HtmlAgilityPack.HtmlDocument();

        private static readonly string[] SourceDocumentsUrls =
        {
            "https://starrupture.tools/buildings/assembler",
            "https://starrupture.tools/buildings/synthetizer",        // Compounder
            "https://starrupture.tools/buildings/synthetizer-tier2",  // Compounder V2
            "https://starrupture.tools/buildings/factory",            // Constructorizer
            "https://starrupture.tools/buildings/factory-tier2",      // Constructorizer V2
            "https://starrupture.tools/buildings/crafter",            // Fabricator
            "https://starrupture.tools/buildings/crafter-tier2",      // Fabricator V2
            "https://starrupture.tools/buildings/military-assembler", // Facturer
            "https://starrupture.tools/buildings/furnace",
            "https://starrupture.tools/buildings/furnace-tier2",
            "https://starrupture.tools/buildings/hammer",             // Mega Press
            "https://starrupture.tools/buildings/refinery",           // Pressurizer
            "https://starrupture.tools/buildings/forge",              // Pyro Forge
            "https://starrupture.tools/buildings/pressurizer",        // Refinery
            "https://starrupture.tools/buildings/smelter"
        };

        private const string RawMaterials = "FactoryPlanner.Data.RawMaterials.json";
    }
}
