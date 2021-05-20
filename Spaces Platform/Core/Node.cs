using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Spaces.Core
{
    [System.Serializable]
    public class NodeWidget
    {
        public string Type;
        public string Value;
        public int ID;
        public string InstanceID;

        public NodeWidget() { }

        public NodeWidget(string type, string value)
        {
            ID = 0;
            Type = type;
            Value = value;
        }

        public NodeWidget(Widget widget)
        {
            ID = widget.GetInstanceID();
            InstanceID = widget.InstanceID;
            NodeReferenceMap.AddReference(widget, this);
        }

        public void Serialize()
        {
            var Referent = NodeReferenceMap.GetObject(this);

            if (Referent)
            {
                Type = Referent.GetType().ToString();
                Value = JsonUtility.ToJson(Referent);
            }
        }

        public bool SerializeObjectReferences()
        {
            bool referencesComplete = false;

            var Referent = NodeReferenceMap.GetObject(this);

            if (Referent)
            {
                var widgetType = Referent.GetType();

                foreach (var info in widgetType.GetMembers().Where(m => m.MemberType == System.Reflection.MemberTypes.Field))
                {
                    var fieldInfo = info.Module.ResolveField(info.MetadataToken);
                    var field = fieldInfo.GetValue(Referent);

                    if (field is ISerializableReference)
                    {
                        var objects = ((ISerializableReference)field).GetReferencedObjects();
                        int completeRefs = 0;

                        foreach (var obj in objects)
                        {
                            var objectID = NodeReferenceMap.GetObjectID(obj);

                            if (!string.IsNullOrEmpty(objectID))
                            {
                                completeRefs++;
                                ((ISerializableReference)field).SetReferenceID(obj, objectID);
                            }
                        }

                        if (objects.Count == completeRefs)
                            referencesComplete = true;
                    }
                }

                Serialize();
            }
            else
            {
                referencesComplete = true;
            }

            return referencesComplete;
        }

        public void Deserialize(GameObject gameObject, Widget existingWidget = null)
        {
            var widgetType = System.Type.GetType(Type);

            if (widgetType == null)
            {
                Debug.LogWarning(gameObject.name + " [Widget Type Unknown] " + Type);
                return;
            }

            var reconstitutedWidget = existingWidget ? existingWidget : gameObject.AddComponent(widgetType);

            if (reconstitutedWidget)
                JsonUtility.FromJsonOverwrite(Value, reconstitutedWidget);

            //Referent = reconstitutedWidget as Widget;

            //if (Referent)
            //    InstanceID = Referent.InstanceID;
        }

        public bool DeserializeObjectReferences()
        {
            bool referencesComplete = false;

            var Referent = NodeReferenceMap.GetObject(InstanceID);

            if (Referent)
            {
                var widgetType = Referent.GetType();

                foreach (var info in widgetType.GetMembers().Where(m => m.MemberType == System.Reflection.MemberTypes.Field))
                {
                    var fieldInfo = info.Module.ResolveField(info.MetadataToken);
                    var field = fieldInfo.GetValue(Referent);

                    if (field is ISerializableReference)
                    {
                        var objectIDs = ((ISerializableReference)field).GetReferenceIDs();

                        int completeRefs = 0;

                        foreach (var ID in objectIDs)
                        {
                            var obj = NodeReferenceMap.GetObject(ID);

                            if (obj)
                            {
                                completeRefs++;
                                ((ISerializableReference)field).SetReferencedObject(ID, obj);
                            }
                        }

                        if (objectIDs.Count == completeRefs)
                            referencesComplete = true;
                    }
                }
            }
            else
            {
                referencesComplete = true;
            }

            return referencesComplete;
        }

        public bool NeedsReferenceSerialization()
        {
            System.Reflection.MemberInfo[] memberInfo = new System.Reflection.MemberInfo[0];
            var Referent = NodeReferenceMap.GetObject(this);

            if (Referent)
            {
                memberInfo = Referent.GetType().GetMembers();
            }
            else if (!string.IsNullOrEmpty(Type))
            {
                var widgetType = System.Type.GetType(Type);

                if (widgetType != null)
                    memberInfo = widgetType.GetMembers();
            }            

            foreach (var info in memberInfo)
            {
                if (info.MemberType == System.Reflection.MemberTypes.Field)
                {
                    var token = info.MetadataToken;
                    var type = info.Module.ResolveField(token);

                    if (type.FieldType.GetInterfaces().Contains(typeof(ISerializableReference)))
                        return true;

                    //Debug.Log("METHOD: " + info + " " + type.FieldType + " " + hasInterface);
                }
            }

            return false;
        }
    }

    internal class NodeReference
    {
        public enum Type
        {
            Node,
            Widget
        }

        public Type type;
        public Node node;
        public NodeWidget widget;

        public string InstanceID
        {
            get
            {
                if (type == Type.Node)
                    return node != null ? node.instanceID : "";
                else
                    return widget.InstanceID;
            }
        }

        public NodeReference(Node node)
        {
            this.type = Type.Node;
            this.node = node;
        }

        public NodeReference(NodeWidget widget)
        {
            this.type = Type.Widget;
            this.widget = widget;
        }
    }

    internal static class NodeReferenceMap
    {
        internal static Dictionary<Object, NodeReference> s_nodeMap;
        internal static List<NodeWidget> s_nodeWidgetReferenceList;

        public static void AddReference(Object objectRef, Node node)
        {
            if (s_nodeMap == null)
                s_nodeMap = new Dictionary<Object, NodeReference>();

            if (s_nodeMap.ContainsKey(objectRef))
                s_nodeMap[objectRef] = new NodeReference(node);
            else
                s_nodeMap.Add(objectRef, new NodeReference(node));
        }

        public static void AddReference(Object objectRef, NodeWidget widget)
        {
            if (s_nodeMap == null)
                s_nodeMap = new Dictionary<Object, NodeReference>();

            if (s_nodeMap.ContainsKey(objectRef))
                s_nodeMap[objectRef] = new NodeReference(widget);
            else
                s_nodeMap.Add(objectRef, new NodeReference(widget));
        }

        public static NodeReference GetNodeReference(Object objectRef)
        {
            if (s_nodeMap == null)
                s_nodeMap = new Dictionary<Object, NodeReference>();

            if (s_nodeMap.ContainsKey(objectRef))
                return s_nodeMap[objectRef];
            else
                return null;
        }

        public static string GetObjectID(Object objectRef)
        {
            if (s_nodeMap != null && s_nodeMap.ContainsKey(objectRef))
            {
                var nodeRef = s_nodeMap[objectRef];

                if (nodeRef != null)
                    return nodeRef.InstanceID;
            }

            return "";
        }

        public static Object GetObject(Node node)
        {
            if (s_nodeMap == null)
                return null;

            return s_nodeMap.Keys.Where(obj => s_nodeMap[obj].type == NodeReference.Type.Node).FirstOrDefault(obj => s_nodeMap[obj].node.instanceID == node.instanceID);
        }

        public static Object GetObject(NodeWidget widget)
        {
            if (s_nodeMap == null)
                return null;

            return s_nodeMap.Keys.Where(obj => s_nodeMap[obj].type == NodeReference.Type.Widget).FirstOrDefault(obj => s_nodeMap[obj].widget.InstanceID == widget.InstanceID);
        }

        public static Object GetObject(string objectID)
        {
            return s_nodeMap.Keys.FirstOrDefault(obj => s_nodeMap[obj].InstanceID == objectID);
        }

        public static void Clear()
        {
            if (s_nodeMap != null)
                s_nodeMap.Clear();
        }

        public static void AddNodeWidget(NodeWidget nodeWidget)
        {
            if (s_nodeWidgetReferenceList == null)
                s_nodeWidgetReferenceList = new List<NodeWidget>();

            if (!s_nodeWidgetReferenceList.Contains(nodeWidget))
                s_nodeWidgetReferenceList.Add(nodeWidget);
        }

        public static void UpdateReferenceIDs()
        {
            if (s_nodeWidgetReferenceList == null)
                s_nodeWidgetReferenceList = new List<NodeWidget>();

            var RemoveList = new List<NodeWidget>();

            foreach(var widget in s_nodeWidgetReferenceList)
            {
                if (widget.SerializeObjectReferences())
                    RemoveList.Add(widget);
            }

            RemoveList.ForEach(w => s_nodeWidgetReferenceList.Remove(w));
        }

        public static void UpdateObjectReferences()
        {
            if (s_nodeWidgetReferenceList == null)
                s_nodeWidgetReferenceList = new List<NodeWidget>();

            var RemoveList = new List<NodeWidget>();

            foreach (var widget in s_nodeWidgetReferenceList)
            {
                if (widget.DeserializeObjectReferences())
                    RemoveList.Add(widget);
            }

            RemoveList.ForEach(w => s_nodeWidgetReferenceList.Remove(w));
        }

    }


    [System.Serializable]
    public class Node
    {
        public string instanceID;
        public int id;
        public string name;

        public Node parent;
        public List<Node> children;

        public Vector3 localPosition;
        public Quaternion localRotation;
        public Vector3 localScale = Vector3.one;

        public List<NodeWidget> widgetMap;

        public Node()
        {
            Initialize();
        }

        public Node(string nodeName = "node", Node parentNode = null)
        {
            Initialize();

            instanceID = System.Guid.NewGuid().ToString();
            name = nodeName;
            parent = parentNode;
        }

        public Node(GameObject gameObject)
        {
            Initialize();

            instanceID = System.Guid.NewGuid().ToString();
            id = gameObject.GetInstanceID();
            name = gameObject.name;
            NodeReferenceMap.AddReference(gameObject, this);

            localPosition = gameObject.transform.position;
            localRotation = gameObject.transform.rotation;
            localScale = gameObject.transform.lossyScale;

            GetComponentsFrom(gameObject);
            CreateChildrenFrom(gameObject);

            //TODO: test for references
            NodeReferenceMap.UpdateReferenceIDs();
        }

        public void Initialize()
        {
            children = new List<Node>();
            widgetMap = new List<NodeWidget>();
        }

        private void CreateChildrenFrom(GameObject gameObject)
        {
            var nodeQueue = new Queue<Node>();
            nodeQueue.Enqueue(this);

            var gameObjectQueue = new Queue<GameObject>();
            gameObjectQueue.Enqueue(gameObject);

            while (gameObjectQueue.Count > 0)
            {
                var currentNode = nodeQueue.Dequeue();
                var currentGameObject = gameObjectQueue.Dequeue();

                foreach (Transform child in currentGameObject.transform)
                {
                    if (child.GetComponent<InstancedAssetWidget>())
                    {
                        //TODO: Handle re-application of instanced object state.
                    }
                    else
                    {
                        nodeQueue.Enqueue(currentNode.CreateChild(child.gameObject));
                        gameObjectQueue.Enqueue(child.gameObject);
                    }
                }
            }
        }

        public Node CreateChild(string name = "node")
        {
            var child = new Node(name, this);
            AddChild(child);
            return child;
        }

        public Node CreateChild(GameObject gameObject)
        {
            var child = CreateChild(gameObject.name);
            child.id = gameObject.GetInstanceID();
            NodeReferenceMap.AddReference(gameObject, child);

            child.localPosition = gameObject.transform.localPosition;
            child.localRotation = gameObject.transform.localRotation;
            child.localScale = gameObject.transform.localScale;

            child.GetComponentsFrom(gameObject);

            return child;
        }

        public void AddChild(Node childNode)
        {
            children.Add(childNode);
            childNode.parent = this;
        }

        public void RemoveChild(Node childNode)
        {
            if (children.Remove(childNode))
                childNode.parent = null;
        }

        public Node GetChild(int index)
        {
            if (children.Count > index)
                return children[index];
            else
                return null;
        }

        public IList<Node> GetChildren()
        {
            return children;
        }

        public IEnumerable<Node> GetDescendants()
        {
            var descendants = new Queue<Node>(children);

            while (descendants.Count > 0)
            {
                var currentNode = descendants.Dequeue();
                currentNode.children.ForEach(node => descendants.Enqueue(node));

                yield return currentNode;
            }
        }

        public bool IsDescendantOf(Node node)
        {
            if (parent == null || node.children.Count == 0)
                return false;
            else
                return node.GetDescendants().Contains(this);
        }

        public void GetComponentsFrom(GameObject gameObject)
        {
            foreach (var component in gameObject.GetComponents<Widget>())
            {
                if (component is MaterialWidget)
                {
                    ((MaterialWidget)component).UpdateWidgetState();
                }
                else if (component is LightWidget)
                {
                    ((LightWidget)component).UpdateWidget();
                }
                else if (component is ParticleWidget)
                {
                    ((ParticleWidget)component).UpdateWidget();
                }

                if (string.IsNullOrEmpty(component.InstanceID))
                    component.GenerateInstanceID();

                var widget = new NodeWidget(component);

                if (widget.NeedsReferenceSerialization())
                    NodeReferenceMap.AddNodeWidget(widget);

                widget.Serialize();

                NodeReferenceMap.AddReference(component, widget);
                widgetMap.Add(widget);
            }
        }

        public void SetWidgetsFor(GameObject gameObject, bool overwriteWidgetSettings = true)
        {
            var usedWidgets = new List<Component>();

            foreach (var widget in widgetMap)
            {
                var widgetType = System.Type.GetType(widget.Type);
                bool existingWidget = false;

                if (widgetType == null)
                {
                    Debug.LogWarning(this.name + " [Widget Type Unknown] " + widget.Type);
                    continue;
                }

                var reconstitutedWidget = gameObject.GetComponents(widgetType).FirstOrDefault(w => !usedWidgets.Contains(w));

                if (reconstitutedWidget)
                    existingWidget = true;
                else
                    reconstitutedWidget = gameObject.AddComponent(widgetType);

                usedWidgets.Add(reconstitutedWidget);
                NodeReferenceMap.AddReference(reconstitutedWidget, widget);

                if (!overwriteWidgetSettings && existingWidget)
                    continue;

                if (widget.NeedsReferenceSerialization())
                    NodeReferenceMap.AddNodeWidget(widget);

                if (reconstitutedWidget)
                    JsonUtility.FromJsonOverwrite(widget.Value, reconstitutedWidget);

                if (reconstitutedWidget is Widget && string.IsNullOrEmpty(((Widget)reconstitutedWidget).InstanceID))
                    ((Widget)reconstitutedWidget).GenerateInstanceID();
            }
        }

        /// <summary>
        /// This is the bootstrapper for RenderNodeTree. It will return the root GameObject in the rendered heirarchy.
        /// </summary>
        /// <param name="node">The root node from which to render the hierarchy.</param>
        /// <param name="target">The Game Object to be used as the root of the hierarchy. If null, target will be generated new.</param>
        /// <returns></returns>
        public static GameObject RenderToGameObjectHierarchy(Node node, GameObject target = null)
        {
            var gameObject = RenderToGameObject(node, target);

            var component = gameObject.GetComponent<MonoBehaviour>();

            if (!component)
                component = gameObject.AddComponent<Widget>();

            if (node.children.Count > 0)
            {
                if (component)
                    component.StartCoroutine(RenderNodeTree(node, gameObject));
                else
                    Debug.LogError(target.name + " [Unable to get component to launch coroutine]");
            }

            return gameObject;
        }

        /// <summary>
        /// This is the bootstrapper for RenderNodeTree. It will return the root GameObject in the rendered heirarchy.
        /// </summary>
        /// <param name="target">The Game Object to be used at the root of the hierarchy. If null, target will be generated new.</param>
        /// <returns></returns>
        public GameObject RenderToGameObjectHierarchy(GameObject target = null)
        {
            return RenderToGameObjectHierarchy(this, target);
        }

        public static GameObject RenderToGameObject(Node node, GameObject target = null)
        {
            if (!target)
                target = new GameObject(node == null ? "Root" : node.name);
            else
                target.name = node.name;

            target.transform.position = node.localPosition;
            target.transform.rotation = node.localRotation;
            target.transform.localScale = node.localScale;

            node.SetWidgetsFor(target);
            NodeReferenceMap.AddReference(target, node);

            return target;
        }

        public GameObject RenderToGameObject(GameObject target = null)
        {
            return RenderToGameObject(this, target);
        }

        public static IEnumerator RenderNodeTree(Node node, GameObject gameObject = null)
        {
            var nodeQueue = new Queue<Node>();
            nodeQueue.Enqueue(node);

            var gameObjectQueue = new Queue<GameObject>();

            if (gameObject)
            {
                gameObjectQueue.Enqueue(gameObject);
                NodeReferenceMap.AddReference(gameObject, node);
            }
            else
                gameObjectQueue.Enqueue(RenderToGameObject(node, gameObject));

            while (gameObjectQueue.Count > 0)
            {
                var currentNode = nodeQueue.Dequeue();
                var currentGameObject = gameObjectQueue.Dequeue();

                foreach (Node childNode in currentNode.GetChildren())
                {
                    nodeQueue.Enqueue(childNode);
                    var childGO = RenderToGameObject(childNode);
                    childGO.transform.SetParent(currentGameObject.transform, false);
                    gameObjectQueue.Enqueue(childGO);
                }

                yield return null;
            }

            NodeReferenceMap.UpdateObjectReferences();
        }

    }
}