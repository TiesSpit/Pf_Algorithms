using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class Graph<T>
{
    private Dictionary<T, List<T>> adjacencyList = new();

    public Graph()
    {
        adjacencyList = new Dictionary<T, List<T>>();
    }

    public void Clear()
    {
        adjacencyList.Clear();
        visited.Clear();
    }

    public void AddNode(T node)
    {
        if (!adjacencyList.ContainsKey(node))
        {
            adjacencyList[node] = new List<T>();            
        }
        else
            Debug.Log("This node already exists");
    }

    public void AddEdge(T fromNode, T toNode)
    {
        if (!adjacencyList.ContainsKey(fromNode) || !adjacencyList.ContainsKey(toNode))
        {
            Debug.Log("Not all specified nodes exist");
            return;
        }
        adjacencyList[fromNode].Add(toNode);
        adjacencyList[toNode].Add(fromNode);
    }

    public List<T> GetNodes()
    {
        return adjacencyList.Keys.ToList();
    }

    public List<T> GetNodes(T node)
    {
        return adjacencyList[node];
    }

    public List<T> GetNeighbors(T node)
    {
        return new List<T>(adjacencyList[node]);
    }

    HashSet<T> visited = new HashSet<T>();
    public bool BFS(T startNode)
    {        
        Queue<T> toDo = new Queue<T>();

        toDo.Enqueue(startNode);
        visited.Add(startNode);

        while (toDo.Count > 0)
        {
            T currentNode = toDo.Dequeue();
            visited.Add(currentNode);
            
            //Debug.Log(currenNode);

            foreach (T connection in GetNeighbors(currentNode))
            {
                if (visited.Contains(connection)) continue;

                toDo.Enqueue(connection);
                visited.Add(connection);
            }
        }
        if (visited.Count < GetNodes().Count) return false;
        else return true;
    }

    //public void DFS(T startNode)
    //{        
    //    Stack<T> toDo = new Stack<T>();        

    //    toDo.Push(startNode);
    //    visited.Add(startNode);

    //    while (toDo.Count > 0)
    //    {
    //        T currentNode = toDo.Pop();
    //        visited.Add(currentNode);
            
    //        Debug.Log(currenNode);

    //        foreach (T connection in GetNeighbors(currentNode))
    //        {
    //            if (!visited.Contains(connection))
    //                toDo.Push(connection);
    //        }
    //    }
    //}

    public List<T> GetVisited()
    {
        return visited.ToList();
    }
}
