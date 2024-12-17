public class Edge
{
    public int To { get; set; }
    public double Weight { get; set; }

    public Edge(int to, double weight)
    {
        To = to;
        Weight = weight;
    }
}
