using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Spaces.Core
{
    [System.Serializable]
    public class SpaceGraph
    {
        //public AvatarWidget avatar;
        //public string startLocation;
        public List<Node> nodes;
        public List<string> assetIDs;
        public int precedence { get; set; }

        public bool isEmpty
        {
            get
            {
                return nodes.Count == 0;
            }
        }

        public SpaceGraph()
        {
            nodes = new List<Node>();
            assetIDs = new List<string>();
        }

        public SpaceGraph(List<Node> nodeSet)
        {
            nodes = nodeSet;
            assetIDs = new List<string>();
        }


        /// <summary>
        /// Write out the SpaceGraph of a Space.
        /// </summary>
        /// <returns>The resulting SpaceGraph</returns>
        public static SpaceGraph Generate()
        {
            var roots = new List<Transform>();
            var nodeRoots = new List<Node>();
            var assetIDs = new List<string>();

            // Roots List determined by presence of Widget
            foreach (Widget w in GameObject.FindObjectsOfType<Widget>())
            {
                if (!roots.Contains(w.transform.root))
                    roots.Add(w.transform.root);

                // Asset IDs used in Space determined by AssetWidgets
                if (w is IAssetReference)
                {
                    foreach (string assetID in ((IAssetReference)w).GetAssetIDs())
                    {
                        if (!assetIDs.Contains(assetID))
                            assetIDs.Add(assetID);
                    }
                }
            }

            // Node tree created for each Root
            foreach (var root in roots)
            {
                var node = new Node(root.gameObject);
                nodeRoots.Add(node);
            }

            // Graph created with set of node Roots.
            var graph = new SpaceGraph(nodeRoots);

            // List of AssetIDs assigned in new Graph.
            graph.assetIDs = assetIDs;

            // Serialize Graph into json.
            //string json = JsonUtility.ToJson(graph);

            return graph;
        }

        /// <summary>
        /// Deserialize the SpaceGraph to the layout Space.
        /// </summary>
        public void RenderGraph()
        {
            if (!isEmpty)
            { 
                foreach (Node node in nodes)
                {
                    var gameObject = node.RenderToGameObjectHierarchy();
                    gameObject.GetComponentsInChildren<AssetWidget>().ToList().ForEach(w => w.GraphSetBySpace = true);
                }
            }
        }
    }
}