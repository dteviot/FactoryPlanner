using FactoryPlanner.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FactoryPlanner.Services
{
    public class MaterialFlow
    {
        public MaterialFlow(ProductionStep producer, ProductionStep consumer, string materialName, double rate)
        {
            Producer = producer;
            Consumer = consumer;
            MaterialName = materialName;
            Rate = rate;
            producer.Outflows.Add(this);
            if (consumer != null)
            {
                consumer.Inflows.Add(this);
            }
        }

        public void IncreaseFlow(double rateIncrease)
        {
            Producer.IncreaseProduction(MaterialName, rateIncrease);
            Rate += rateIncrease;
        }

        public override string ToString()
        {
            return $"{Rate:F2}× {MaterialName}";
        }

        public ProductionStep Producer { get; set; }
        public ProductionStep Consumer { get; set; }
        public string MaterialName { get; set; }
        public double Rate { get; set; }
    }

    /// <summary>
    /// Represents a single step in a production plan
    /// </summary>
    public class ProductionStep
    {
        public int Depth { get; set; }

        public int ID { get; set; }

        /// <summary>
        /// The recipe used in this step
        /// </summary>
        public Recipe Recipe { get; set; }

        /// <summary>
        /// Number of machines required to achieve the target rate
        /// </summary>
        public double MachinesRequired { get; set; }

        /// <summary>
        /// Steps that provide input materials to this step
        /// </summary>
        public List<MaterialFlow> Inflows { get; set; } = new List<MaterialFlow>();

        /// <summary>
        /// Steps that take output materials from this step
        /// </summary>
        public List<MaterialFlow> Outflows { get; set; } = new List<MaterialFlow>();

        /// <summary>
        /// Target material produced by this step
        /// </summary>
        public Models.RecipeMaterial TargetMaterial { get; set; }

        /// <summary>
        /// Creates a production step for a recipe
        /// </summary>
        public ProductionStep(Recipe recipe, string targetMaterial, double targetRate, int depth)
        {
            Depth = depth;
            Recipe = recipe;

            // Calculate output rate for the target material
            TargetMaterial = recipe.Outputs.FirstOrDefault(o => o.Name == targetMaterial);
            if (TargetMaterial == null)
            {
                throw new ArgumentException($"Recipe {recipe.Name} does not produce {targetMaterial}");
            }

            // Calculate machine requirements
            double outputPerMachinePerMinute = TargetMaterial.Rate;

            // Calculate machines needed
            MachinesRequired = Math.Ceiling(targetRate / outputPerMachinePerMinute);
        }

        /// <summary>
        /// Gets the required input rate for a specific material
        /// </summary>
        public double GetInputRate(string materialName)
        {
            return GetRate(materialName, Recipe.Inputs);
        }

        public double GetProductionRate(string materialName)
        {
            return GetRate(materialName, Recipe.Outputs);
        }

        public double GetOutflowRate(string materialName)
        {
            double outflowRate = 0;
            foreach (var mat in Outflows.Where(f => f.MaterialName == materialName))
            {
                outflowRate += mat.Rate;
            }
            return outflowRate;
        }

        public double GetSurplus(string materialName)
        {
            double surplus = GetProductionRate(materialName) - GetOutflowRate(materialName);
            System.Diagnostics.Trace.Assert(0 <= surplus);
            return surplus;
        }

        public double GetSurplus(){ return GetSurplus(TargetMaterial.Name); }

        public override string ToString()
        {
            var name = TargetMaterial.Name;
            return $"{MachinesRequired:F2}× {Recipe.Machine} → {GetProductionRate(name):F2}:{GetOutflowRate(name):F2}/min {name} )";
        }

        public double GetRate(string materialName, List<RecipeMaterial> collection)
        {
            var material = collection.FirstOrDefault(i => i.Name == materialName);
            if (material == null)
                throw new Exception($"Material {material} not found in recipe.");

            // Calculate input rate based on machine count
            return material.Rate * MachinesRequired;
        }

        public MaterialFlow AddFlow(ProductionStep consumer, string materialName, double rate)
        {
            var existingFlow = Outflows.Where(f => f.Consumer == consumer && f.MaterialName == materialName).FirstOrDefault();
            System.Diagnostics.Trace.Assert(existingFlow == null || rate == 0);
            return (existingFlow == null)
                ? new MaterialFlow(this, consumer, materialName, rate)
                : existingFlow;
        }

        // Increase production to allow supplying additional rate of minimumNewFlow
        public void IncreaseProduction(string materialName, double minimumNewFlow)
        {
            // calc how many machines to add
            double neededIncrease = minimumNewFlow - GetSurplus(materialName);
            if (neededIncrease <= 0)
            {
                return;
            }

            var target = Recipe.Outputs.First(o => o.Name == materialName);
            double newMachines = Math.Ceiling(neededIncrease / target.Rate);
            MachinesRequired += newMachines;

            // increase inflows to support new production
            foreach(var flow in Inflows)
            {
                var inputMaterial = Recipe.Inputs.FirstOrDefault(o => o.Name == flow.MaterialName);
                flow.IncreaseFlow(newMachines * inputMaterial.Rate);
            }
        }
    }

    /// <summary>
    /// Builds production plans for factory games
    /// </summary>
    public class ProductionPlanBuilder
    {
        /// <summary>
        /// Builds a production plan for a target material at a specific rate
        /// </summary>
        /// <param name="recipes">Available recipes</param>
        /// <param name="targetMaterial">Material to produce</param>
        /// <param name="targetRatePerMinute">Target production rate per minute</param>
        /// <returns>A list of production steps required to achieve the target</returns>
        public List<ProductionStep> BuildPlan(List<Recipe> recipes, string targetMaterial, double targetRatePerMinute, Recipe selectedRecipe = null)
        {
            if (recipes == null || recipes.Count == 0)
                throw new ArgumentException("Recipes list cannot be empty");

            if (string.IsNullOrEmpty(targetMaterial))
                throw new ArgumentException("Target material must be specified");

            if (targetRatePerMinute <= 0)
                throw new ArgumentException("Target rate must be positive");

            var plan = new List<ProductionStep>();
            var visitedMaterials = new HashSet<string>();

            BuildPlanRecursive(recipes, targetMaterial, targetRatePerMinute, plan, visitedMaterials, 0, selectedRecipe);
            // add dummy outflow for tracking desired quantity
            plan[0].AddFlow(null, targetMaterial, targetRatePerMinute);

            return plan;
        }

        /// <summary>
        /// Recursively builds the production plan
        /// </summary>
        private ProductionStep BuildPlanRecursive(List<Recipe> recipes, string material, double requiredRate,
            List<ProductionStep> plan, HashSet<string> visitedMaterials, int depth, Recipe selectedRecipe)
        {
            // Prevent infinite recursion  => TODO THIS IS PROBALY WRONG. WILL NEED TO UPDATE PLAN
            if (visitedMaterials.Contains(material))
            {
                throw new Exception($"Recursive loop detected for {material}");
            }

            visitedMaterials.Add(material);

            if (selectedRecipe == null)
            {
                selectedRecipe = FindRecipeForMaterial(recipes, material);
            }

            // Create production step for this material
            var step = new ProductionStep(selectedRecipe, material, requiredRate, depth);
            plan.Add(step);

            // Process input requirements for this step
            foreach (var input in selectedRecipe.Inputs)
            {
                // Calculate required input rate for this material
                double inputRate = step.GetInputRate(input.Name);

                if (inputRate > 0)
                {
                    var substep = FindExistingSubstep(plan, input.Name);
                    if (substep == null)
                    {
                        // Recursively build plan for this input material
                        substep = BuildPlanRecursive(recipes, input.Name, inputRate, plan, visitedMaterials, depth + 1, null);
                        substep.AddFlow(step, input.Name, inputRate);
                    }
                    else
                    {
                        var flow = substep.AddFlow(step, input.Name, 0);
                        flow.IncreaseFlow(inputRate);
                    }
                }
            }

            visitedMaterials.Remove(material);
            return step;
        }

        public Recipe FindRecipeForMaterial(List<Recipe> recipes, string materialName)
        {
            // Find recipes that produce this material
            var producingRecipes = recipes
                .Where(r => r.Outputs.Any(o => o.Name == materialName))
                .ToList();

            var prefered = producingRecipes.FirstOrDefault(r => r.Preferred);
            if (prefered != null)
            {
                return prefered;
            }

            // find recipe with greatest production
            double max = 0;
            foreach (var recipe in producingRecipes)
            {
                var material = recipe.Outputs.First(o => o.Name == materialName);
                if (max < material.Rate)
                {
                    max = material.Rate;
                    prefered = recipe;
                }
            }

            if (prefered == null)
            {
                throw new Exception($"No recipe found for '{materialName}'.");
            }

            return prefered;
        }

        public ProductionStep FindExistingSubstep(List<ProductionStep> plan, string materialName)
        {
            return plan.Where(s => s.TargetMaterial.Name == materialName).FirstOrDefault();
        }

        /// <summary>
        /// Calculates total machines required for a production plan
        /// </summary>
        public static Dictionary<string, double> CalculateMachineRequirements(List<ProductionStep> plan)
        {
            var machineCounts = new Dictionary<string, double>();

            foreach (var step in plan)
            {
                var machine = step.Recipe.Machine;
                if (machineCounts.ContainsKey(machine))
                {
                    machineCounts[machine] += step.MachinesRequired;
                }
                else
                {
                    machineCounts[machine] = step.MachinesRequired;
                }
            }

            return machineCounts;
        }

        public static List<RecipeMaterial> CalcRawMaterials(List<ProductionStep> plan)
        {
            var raw = new List<RecipeMaterial>();
            foreach(var step in plan)
            {
                if (step.Recipe.Inputs.Count == 0)
                {
                    foreach(var material in step.Recipe.Outputs)
                    {
                        raw.Add(new RecipeMaterial(material.Name, material.Rate * step.MachinesRequired));
                    }
                }
            }
            return raw;
        }

        public List<RecipeMaterial> CalcSurplus(List<ProductionStep> plan)
        {
            var surplus = new List<RecipeMaterial>();
            foreach (var step in plan)
            {
                foreach (var material in step.Recipe.Outputs)
                {
                    var excess = step.GetSurplus(material.Name);
                    if (0 < excess)
                    {
                        surplus.Add(new RecipeMaterial(material.Name, excess));
                    }
                }
            }
            return surplus;
        }

        /// <summary>
        /// Prints a formatted production plan
        /// </summary>
        public string FormatPlan(List<ProductionStep> plan)
        {
            var output = new System.Text.StringBuilder();
            output.AppendLine("=== Production Steps ===");
            output.AppendLine();
            foreach(var step in plan)
            {
                output.Append(FormatStep(step));
            }
            output.AppendLine("=== Raw Materials ===");
            foreach (var material in CalcRawMaterials(plan))
            {
                output.AppendLine(material.ToString());
            }
            output.AppendLine("=== Surplus ===");
            foreach (var material in CalcSurplus(plan))
            {
                output.AppendLine(material.ToString());
            }
            return output.ToString();

        }



        private string FormatStep(ProductionStep step)
        {
            int depth = step.Depth;
            var output = new System.Text.StringBuilder();

            var stepDepths = new Dictionary<ProductionStep, int>();
            output.AppendLine($"{new string(' ', depth * 2)}{new string('-', depth)}>{step}");
            foreach (var flow in step.Inflows)
            {
                output.AppendLine($"{new string(' ', (depth + 1) * 2)}  - {flow.ToString()}");
            }
            return output.ToString();
        }


        private string FormatStepsRecursive(ProductionStep step, int depth)
        {
            var output = new System.Text.StringBuilder();

            output.AppendLine($"{new string(' ', depth * 2)}{new string('-', depth)}>{step}");
            foreach (var flow in step.Inflows)
            {
                var s = FormatStepsRecursive(flow.Producer, depth + 1);
                output.Append(s);
            }
            return output.ToString();
        }
    }
}