using FactoryPlanner.Models;
using FactoryPlanner.Services;
using System.Reflection.PortableExecutable;

namespace Tests
{
    [TestClass]
    public sealed class ProductionPlanBuilderTests
    {
        [TestMethod]
        public void SingleStep_VolumeMoreThanOneMachine()
        {
            var builder = new ProductionPlanBuilder();
            var plan = builder.BuildPlan(GetRecipies(), "Calcium Ore", 61);
            Assert.HasCount(1, plan);
            Assert.AreEqual(2, plan[0].MachinesRequired);
            Assert.HasCount(0, plan[0].Inflows);
            Assert.HasCount(1, plan[0].Outflows);
        }

        [TestMethod]
        public void MultipleStep_UseSurplus()
        {
            var builder = new ProductionPlanBuilder();
            var steps = builder.BuildPlan(GetRecipies(), "Iron Plate", 15);
            Assert.HasCount(3, steps);
            ValidateStep(steps[0], 1, "Iron Plate", 20, 5);
            ValidateStep(steps[1], 1, "Iron Ingot", 30, 0);
            ValidateStep(steps[2], 1, "Iron Ore", 60, 10);

            var flows = steps[0].Inflows;
            Assert.HasCount(2, flows);
            Assert.AreEqual(30, flows[0].Rate);
            Assert.AreEqual(20, flows[1].Rate);

            flows = steps[1].Inflows;
            Assert.HasCount(1, flows);
            Assert.AreEqual(30, flows[0].Rate);
        }

        public void ValidateStep(ProductionStep step, int machines, string targetMaterial, int productionRate, int surplus)
        {
            Assert.AreEqual(machines, step.MachinesRequired);
            Assert.AreEqual(targetMaterial, step.TargetMaterial.Name);
            Assert.AreEqual(productionRate, step.GetProductionRate(targetMaterial));
            Assert.AreEqual(surplus, step.GetSurplus(targetMaterial));
        }

        [TestMethod]
        public void MultipleStep_IncreaseMachines()
        {
            var builder = new ProductionPlanBuilder();
            var steps = builder.BuildPlan(GetRecipies(), "Iron Component", 1);
            Assert.HasCount(5, steps);
            ValidateStep(steps[0], 1, "Iron Component", 60, 59);
            ValidateStep(steps[1], 1, "Iron Plate", 20, 0);
            ValidateStep(steps[2], 3, "Iron Ingot", 90, 0);
            ValidateStep(steps[3], 3, "Iron Ore", 180, 30);
            ValidateStep(steps[4], 2, "Iron Rod", 40, 0);

            // into Iron Component
            var flows = steps[0].Inflows;
            Assert.HasCount(2, flows);
            Assert.AreEqual(20, flows[0].Rate);
            Assert.AreEqual(40, flows[1].Rate);

            // into IronPlate
            flows = steps[1].Inflows;
            Assert.HasCount(2, flows);
            Assert.AreEqual(30, flows[0].Rate);
            Assert.AreEqual(20, flows[1].Rate);

            // into IronIngot
            flows = steps[2].Inflows;
            Assert.HasCount(1, flows);
            Assert.AreEqual(90, flows[0].Rate);

            // into Iron Rod
            flows = steps[4].Inflows;
            Assert.HasCount(2, flows);
            Assert.AreEqual(60, flows[0].Rate);
            Assert.AreEqual(40, flows[1].Rate);
        }

        private List<Recipe> GetRecipies()
        {
            return new List<Recipe> {
                new Recipe("Ore Excavator", 2, 2,
                    new List<RecipeMaterial>() { },
                    new List<RecipeMaterial>() { new RecipeMaterial("Calcium Ore", 60.0)}),
                new Recipe("Ore Excavator", 2, 2,
                    new List<RecipeMaterial>() { },
                    new List<RecipeMaterial>() { new RecipeMaterial("Iron Ore", 60.0)}),
                new Recipe("Smelter", 4, 2,
                    new List<RecipeMaterial>() { new RecipeMaterial("Iron Ore", 30.0)},
                    new List<RecipeMaterial>() { new RecipeMaterial("Iron Ingot", 30.0) }),
                new Recipe("Forge", 3, 1,
                    new List<RecipeMaterial>() { new RecipeMaterial("Iron Ingot", 30.0), new RecipeMaterial("Iron Ore", 20.0) },
                    new List<RecipeMaterial>() { new RecipeMaterial("Iron Plate", 20.0)}),
                new Recipe("Forge", 3, 1,
                    new List<RecipeMaterial>() { new RecipeMaterial("Iron Ingot", 30.0), new RecipeMaterial("Iron Ore", 20.0) },
                    new List<RecipeMaterial>() { new RecipeMaterial("Iron Rod", 20.0)}),
                new Recipe("Welder", 3, 3,
                    new List<RecipeMaterial>() { new RecipeMaterial("Iron Plate", 20.0), new RecipeMaterial("Iron Rod", 40.0) },
                    new List<RecipeMaterial>() { new RecipeMaterial("Iron Component", 60.0)}),
                };
        }
    }
}
