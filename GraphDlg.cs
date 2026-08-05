using FactoryPlanner.Models;
using FactoryPlanner.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FactoryPlanner
{
    public partial class GraphDlg : Form
    {
        public GraphDlg(List<ProductionStep> plan)
        {
            InitializeComponent();
            RenderGraph(plan);
        }

        public void RenderGraph(List<ProductionStep> plan)
        {
            //create a viewer object 
            Microsoft.Msagl.GraphViewerGdi.GViewer viewer = new Microsoft.Msagl.GraphViewerGdi.GViewer();
            //create a graph object 
            Microsoft.Msagl.Drawing.Graph graph = new Microsoft.Msagl.Drawing.Graph("graph");
            PlanToGraph(plan, graph);

            //bind the graph to the viewer 
            viewer.Graph = graph;
            //associate the viewer with the form 
            this.SuspendLayout();
            viewer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Controls.Add(viewer);
            this.ResumeLayout();
        }

        private void PlanToGraph(List<ProductionStep> plan, Microsoft.Msagl.Drawing.Graph graph)
        {
            // give each step unique name
            for(int i = 0; i < plan.Count; i++)
            {
                plan[i].ID = i;
            }

            //create the graph content
            foreach (var step in plan)
            {
                foreach(var flow in step.Inflows)
                {
                    graph.AddEdge(flow.Producer.ID.ToString(), flow.ToString(), step.ID.ToString());
                }
                var node = graph.FindNode(step.ID.ToString());
                node.Label.Text = step.ToString();
                ColourCodeStep(step, node);
            }

            //graph.AddEdge("A", "C").Attr.Color = Microsoft.Msagl.Drawing.Color.Green;
            //graph.FindNode("A").Attr.FillColor = Microsoft.Msagl.Drawing.Color.Magenta;
            //graph.FindNode("B").Attr.FillColor = Microsoft.Msagl.Drawing.Color.MistyRose;
            //Microsoft.Msagl.Drawing.Node c = graph.FindNode("C");
            //c.Attr.FillColor = Microsoft.Msagl.Drawing.Color.PaleGreen;
            //c.Attr.Shape = Microsoft.Msagl.Drawing.Shape.Diamond;
        }

        private void ColourCodeStep(ProductionStep step, Microsoft.Msagl.Drawing.Node node)
        {
            double surplus = step.GetSurplus();
            double productionRate = step.GetProductionRate(step.TargetMaterial.Name);
            if (surplus == 0)
            {
                node.Attr.FillColor = Microsoft.Msagl.Drawing.Color.MistyRose;
            }
            else if (surplus < productionRate * 0.1)
            {
                node.Attr.FillColor = Microsoft.Msagl.Drawing.Color.LightYellow;
            }
            else if(productionRate * 0.5 <= surplus)
            {
                node.Attr.FillColor = Microsoft.Msagl.Drawing.Color.LightGreen;
            }
        }
    }
}
