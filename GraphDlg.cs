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
        public GraphDlg(ProductionStep step)
        {
            InitializeComponent();
            RenderGraph(step);
        }

        public void RenderGraph(ProductionStep step)
        {
            //create a viewer object 
            Microsoft.Msagl.GraphViewerGdi.GViewer viewer = new Microsoft.Msagl.GraphViewerGdi.GViewer();
            viewer.Click += GraphObject_Click;
            //create a graph object 
            Microsoft.Msagl.Drawing.Graph graph = new Microsoft.Msagl.Drawing.Graph("graph");
            PlanToGraph(step, graph);

            //bind the graph to the viewer 
            viewer.Graph = graph;
            //associate the viewer with the form 
            this.SuspendLayout();
            viewer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Controls.Add(viewer);
            this.ResumeLayout();
        }

        private void GraphObject_Click(object sender, EventArgs e)
        {
            var viewer = sender as Microsoft.Msagl.GraphViewerGdi.GViewer;
            MouseEventArgs mouseEvent = (MouseEventArgs)e;

            Microsoft.Msagl.Drawing.Node selectedNode = viewer.SelectedObject as Microsoft.Msagl.Drawing.Node;
            if (selectedNode != null)
            {
                var dlg = new GraphDlg(selectedNode.UserData as ProductionStep);
                dlg.ShowDialog(this);
            }
        }

        private void PlanToGraph(ProductionStep step, Microsoft.Msagl.Drawing.Graph graph)
        {
            var substeps = new HashSet<ProductionStep>();
            AddStepToGraph(step, substeps, graph);

            //graph.AddEdge("A", "C").Attr.Color = Microsoft.Msagl.Drawing.Color.Green;
            //graph.FindNode("A").Attr.FillColor = Microsoft.Msagl.Drawing.Color.Magenta;
            //graph.FindNode("B").Attr.FillColor = Microsoft.Msagl.Drawing.Color.MistyRose;
            //Microsoft.Msagl.Drawing.Node c = graph.FindNode("C");
            //c.Attr.FillColor = Microsoft.Msagl.Drawing.Color.PaleGreen;
            //c.Attr.Shape = Microsoft.Msagl.Drawing.Shape.Diamond;
        }

        private void AddStepToGraph(ProductionStep step, HashSet<ProductionStep> substeps, Microsoft.Msagl.Drawing.Graph graph)
        {
            if (!substeps.Contains(step))
            {
                substeps.Add(step);
                foreach (var flow in step.Inflows)
                {
                    graph.AddEdge(flow.Producer.ID.ToString(), flow.GraphLabel(), step.ID.ToString());
                    AddStepToGraph(flow.Producer, substeps, graph);
                }
                var node = graph.FindNode(step.ID.ToString());
                node.UserData = step;
                node.Label.Text = step.GraphLabel();
                ColourCodeStep(step, node);
            }
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
