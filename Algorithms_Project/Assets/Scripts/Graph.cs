using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class Graph<T>
{
    private Dictionary<T, List<T>> adjecencyList = new();

    public Graph()
    {
        adjecencyList = new Dictionary<T, List<T>>();
    }

    public void ClearNodes()
    {
        adjecencyList.Clear();
    }

    public void AddNode(T node)
    {
        if (!adjecencyList.ContainsKey(node))
        {
            adjecencyList[node] = new List<T>();
            Debug.Log(node);
            //Debug.Log(adjecencyList[node]);
        }
        //else
        //    Debug.Log("This node already exists");
    }

    public void AddEdge(T fromNode, T toNode)
    {
        if (!adjecencyList.ContainsKey(fromNode) || !adjecencyList.ContainsKey(toNode))
        {
            Debug.Log("Not all specified nodes exist");
            return;
        }
        adjecencyList[fromNode].Add(toNode);
        adjecencyList[toNode].Add(fromNode);
    }

    public List<T> GetNodes()
    {
        return adjecencyList.Keys.ToList();
    }

    public List<T> GetNodes(T node)
    {
        return adjecencyList[node];
    }
}
