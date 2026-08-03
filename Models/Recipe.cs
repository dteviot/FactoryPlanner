using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace FactoryPlanner.Models
{
    public class Recipe
    {
        public Recipe() { }

        public Recipe(string machine, int duration, int outputBatchSize, List<RecipeMaterial> inputs, List<RecipeMaterial> oututs)
        {
            Machine = machine;
            Duration = duration;
            OutputBatchSize = outputBatchSize;
            Inputs = inputs;
            Outputs = oututs;
        }

        public string Name { get; set; } = "";
        public string Machine { get; set; } = "";
        public int Duration { get; set; } = 0;
        public int OutputBatchSize { get; set; } = 0;
        public bool Preferred { get; set; } = false;
        public List<RecipeMaterial> Inputs { get; set; } = new List<RecipeMaterial>();
        public List<RecipeMaterial> Outputs { get; set; } = new List<RecipeMaterial>();

        public override string ToString()
        {
            string inputs = String.Join(", ", this.Inputs.Select(i => i.ToString()).ToArray());
            return $"{Outputs[0]}: {Machine}: {inputs}";
        }

        public static string Serialize(List<Recipe> recipes)
        {
            var settings = new JsonSerializerSettings()
            {
                DefaultValueHandling = DefaultValueHandling.Ignore
            };
            return JsonConvert.SerializeObject(recipes, Newtonsoft.Json.Formatting.Indented, settings);
        }

        public static List<Recipe> Deerialize(string json)
        {
            return JsonConvert.DeserializeObject<List<Recipe>>(json);
        }
    }
}
