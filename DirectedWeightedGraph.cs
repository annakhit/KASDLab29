using System.Collections.Generic;
using System;
using System.Linq;

public class DirectedWeightedGraph
{
    // ключ - вершина, значение - список ребер,исходящих из нее
    public Dictionary<int, List<Edge>> adjacencyList;

    public DirectedWeightedGraph()
    {
        adjacencyList = new Dictionary<int, List<Edge>>();
    }

    public void AddVertex(int vertex)
    {
        if (!adjacencyList.ContainsKey(vertex))
        {
            adjacencyList.Add(vertex, new List<Edge>());
        }
    }

    public void AddEdge(int from, int to, double weight = 0)
    {
        if (!adjacencyList.ContainsKey(from) || !adjacencyList.ContainsKey(to))
        {
            throw new ArgumentException("Vertices do not exist");
        }

        adjacencyList[from].Add(new Edge(to, weight));
    }

    //"транспонированный" граф (обратные направления ребер)
    public DirectedWeightedGraph Transpose()
    {
        DirectedWeightedGraph transposed = new DirectedWeightedGraph();

        foreach (int v in adjacencyList.Keys)
        {
            transposed.AddVertex(v);
        }

        foreach (int v in adjacencyList.Keys)
        {
            foreach (Edge edge in adjacencyList[v])
            {
                transposed.AddEdge(edge.To, v, edge.Weight);
            }
        }

        return transposed;
    }

    // поиск в глубину (перебор всех ребер из v, если для них конечная вершина To не помещена - рекурс)
    private void DFSGraph(int v, bool[] visited, DirectedWeightedGraph graph, Stack<int> stack)
    {
        visited[v] = true;

        foreach (Edge edge in graph.adjacencyList[v])
        {
            if (!visited[edge.To]) DFSGraph(edge.To, visited, graph, stack);
        }

        stack.Push(v); // порядок обратный порядку завершения обхода
    }

    public List<List<int>> Kosaraju()
    {
        // 1- Поиск в глубину(DFS) в исходном графе и построение стека:
        int size = adjacencyList.Count;
        List<List<int>> stronglyConnectedComponents = new List<List<int>>();
        Stack<int> stack = new Stack<int>();
        bool[] visitedGraph = new bool[size];

        for (int i = 0; i < size; i++)
        {
            // добавление каждой непосещ. вершины  стек в порядке убывания времени обхода
            if (!visitedGraph[i]) DFSGraph(i, visitedGraph, Transpose(), stack);
        }

        // 2 - Поиск в глубину (DFS) в транспонированном графе, используя стек:

        bool[] visitedReversedGraph = new bool[size];

        while (stack.Count > 0) // перебор вершин в  стеке 
        {
            int value = stack.Pop();

            if (!visitedReversedGraph[value])
            {
                Stack<int> component = new Stack<int>(); // с.с.к. 
                DFSGraph(value, visitedReversedGraph, this, component); // все достижимые вершины из текущей value образуют с.с.к.
                stronglyConnectedComponents.Add(component.ToList());
            }
        }

        return stronglyConnectedComponents;
    }

    // преобразование из списка смежности в матрицу весов
    public double[,] GetWeightMatrix()
    {
        int n = adjacencyList.Count;

        double[,] weightMatrix = new double[n, n];

        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                weightMatrix[i, j] = 0;
            }
        }

        int index = 0;

        List<int> vertices = adjacencyList.Keys.ToList();

        foreach (int vertex in vertices)
        {
            int row = vertices.IndexOf(vertex);
            foreach (Edge edge in adjacencyList[vertex])
            {
                int col = vertices.IndexOf(edge.To);
                weightMatrix[row, col] = edge.Weight;
            }
            index++;
        }

        return weightMatrix;
    }


    // алгоритм Эдмондса-Карпа (дополняющий путь - кратчайший по количеству ребер.  Поиск в ширину)
    // source - исток, sink - сток
    public double MaxFlow(int source, int sink)
    {
        double[,] weightMatrix = GetWeightMatrix();

        int n = weightMatrix.GetLength(0);

        // матрица потоков
        double[,] flow = new double[n, n]; 

        // максимальный поток
        double maxFlow = 0;

        while (true) // поиск увеличивающего пути (поиск в ширину)
        {
            // массив родительских вершин
            int[] parent = new int[n];
            for (int i = 0; i < n; i++) { parent[i] = -1; }

            Queue<int> q = new Queue<int>();

            q.Enqueue(source); // добавление в конец очереди (вершины, которые надо посетить)

            parent[source] = -2; // чтобы отличать нач вершину от непосещенных

            while (q.Count > 0)
            {
                int u = q.Dequeue();
                for (int v = 0; v < n; v++) // проверка исходящих из u ребер
                {
                    if (parent[v] == -1 && weightMatrix[u, v] - flow[u, v] > 0)
                    {
                        parent[v] = u;
                        q.Enqueue(v);
                        if (v == sink) break; // если вершина - сток (путь найден)
                    }
                }
                if (parent[sink] != -1) break;
            }

            if (parent[sink] == -1) break;

            double pathFlow = double.MaxValue;

            for (int v = sink; v != source; v = parent[v])
            {
                int u = parent[v];
                // определение мин значения на найденном пути, на которое увелиивается поток
                pathFlow = Math.Min(pathFlow, weightMatrix[u, v] - flow[u, v]);
            }

            for (int v = sink; v != source; v = parent[v])
            {
                int u = parent[v];
                flow[u, v] += pathFlow;
                flow[v, u] -= pathFlow;
            }

            maxFlow += pathFlow;
        }

        return maxFlow;
    }

    private bool IsAdjacent(int u, int v) => adjacencyList[u].Any(e => e.To == v);

    public List<int> FindMaxClique()
    {
        List<int> vertices = adjacencyList.Keys.ToList();
        List<int> maxClique = new List<int>();

        foreach (var item in vertices)
        {
            Console.WriteLine(item);
        }

        void FindClique(List<int> currentClique, int start)
        {
            if (currentClique.Count > maxClique.Count) //если текущая клика больше
            {
                maxClique = new List<int>(currentClique);
            }

            for (int i = start; i < vertices.Count; i++)
            {
                int v = vertices[i];
                // проверка на принадлежность к клике каждой вершины v соседняя ли она со всеми ост вершинами в текущей клике: 
                if (currentClique.All(u => IsAdjacent(u, v) || IsAdjacent(v, u)))
                {
                    currentClique.Add(v); // добавление v к текущей клике
                    FindClique(currentClique, i + 1); // поиск клик содержащих v
                    currentClique.RemoveAt(currentClique.Count - 1); // обратный ход (удаление v для перебора других возможных клик)
                }
            }
        }


        FindClique(new List<int>(), 0);

        return maxClique;
    }
}