using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace KASDLab29
{
    public partial class Form : System.Windows.Forms.Form
    {
        private readonly Dictionary<string, Point> nodes = new Dictionary<string, Point>();
        private readonly List<Tuple<string, string, string>> edges = new List<Tuple<string, string, string>>();
        private ListBox listbox;
        public Form()
        {
            InitializeComponent();
            VisualizationGraph();
            CalculateParams();
        }

        public void VisualizationGraph()
        {
            nodes.Add("0", new Point(100, 100));
            nodes.Add("1", new Point(300, 100));
            nodes.Add("2", new Point(100, 300));
            nodes.Add("3", new Point(500, 100));
            nodes.Add("4", new Point(500, 300));
            edges.Add(new Tuple<string, string, string>("0", "2", "5"));
            edges.Add(new Tuple<string, string, string>("1", "0", "4"));
            edges.Add(new Tuple<string, string, string>("2", "1", "1"));
            edges.Add(new Tuple<string, string, string>("3", "1", "4"));
            edges.Add(new Tuple<string, string, string>("3", "4", "1"));
            edges.Add(new Tuple<string, string, string>("4", "3", "11"));
        }

        public void CalculateParams()
        {
            DirectedWeightedGraph graph = new DirectedWeightedGraph();

            graph.AddVertex(0);
            graph.AddVertex(1);
            graph.AddVertex(2);
            graph.AddVertex(3);
            graph.AddVertex(4);

            graph.AddEdge(0, 2, 5);
            graph.AddEdge(1, 0, 4);
            graph.AddEdge(2, 1, 1);

            graph.AddEdge(3, 1, 5);
            graph.AddEdge(3, 4, 1);
            graph.AddEdge(4, 3, 11);

            listbox = new ListBox();
            listbox.Location = new Point(550, 100);
            listbox.Size = new Size(300, 100);
            this.Controls.Add(listbox);

            foreach (List<int> component in graph.Kosaraju())
            {
                listbox.Items.Add($"Компоненты сильной связности: {string.Join(", ", component)}");
            }

            listbox.Items.Add($"Максимальный поток: {graph.MaxFlow(4, 2)}");

            listbox.Items.Add($"Максимальная клика: {string.Join(", ", graph.FindMaxClique())}");
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;
            Pen pen = new Pen(Color.Black, 2);
            Brush brush = new SolidBrush(Color.LightBlue);

            pen.CustomEndCap = new AdjustableArrowCap(5, 5);

            foreach (KeyValuePair<string, Point> kvp in nodes)
            {
                g.FillEllipse(brush, kvp.Value.X - 20, kvp.Value.Y - 20, 40, 40);
                g.DrawString(kvp.Key, this.Font, Brushes.Black, kvp.Value);
            }

            foreach (Tuple<string, string, string> edge in edges)
            {
                Point start = nodes[edge.Item1];
                Point end = nodes[edge.Item2];

                g.DrawLine(pen, start, end);

                int x = end.X > start.X ? start.X + end.X  : start.X + end.X - 40;
                int y = end.Y > start.Y ? start.Y + end.Y + 40 : start.Y + end.Y - 40;

                g.DrawString(edge.Item3, this.Font, Brushes.Black, new Point(x / 2, y / 2));
            }

            pen.Dispose();
            brush.Dispose();
        }
    }
}
