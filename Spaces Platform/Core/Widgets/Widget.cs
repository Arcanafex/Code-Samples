using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Spaces.Core
{
    /// <summary>
    /// Base class for collectively referencing Widgets.
    /// </summary>

    public class Widget : MonoBehaviour
    {
        public enum State
        {
            Edit,
            Play
        }

        [SerializeField]
        private string instanceID;
        public string InstanceID
        {
            get { return instanceID; }
        }

        public State mode { get; private set; }
        protected bool initialized;

        public virtual Widget parentWidget
        {
            get { return transform.parent != null ? transform.parent.GetComponentInParent<Widget>() : null; }
        }

         public virtual Widget[] childWidgets
        {
            get
            {
                List<Widget> children = new List<Widget>();

                foreach (Transform child in transform)
                {
                    children.AddRange(child.GetComponentsInChildren<Widget>());
                }

                return children.ToArray();
            }
        }

        public virtual T GetParentWidgetOfType<T>() where T : Widget
        {
            if (transform.transform.parent != null)
                return transform.parent.GetComponentInParent<T>();
            else
                return default(T);
        }

        public virtual T[] GetChildWidgetsOfType<T>() where T : Widget
        {
            List<T> children = new List<T>();

            foreach (Transform child in transform)
            {
                children.AddRange(child.GetComponentsInChildren<T>());
            }

            return children.ToArray();
        }

        public static T Create<T>() where T : Widget
        {
            var widget = new GameObject().AddComponent<T>();
            return widget;
        }

        protected virtual void Start()
        {
            if (!initialized)
                Initialize();
        }

        public virtual void Initialize()
        {
            //TODO: regenerate if duplicate
            if (string.IsNullOrEmpty(instanceID))
                GenerateInstanceID();

            initialized = true;
        }

        public virtual void GenerateInstanceID()
        {
            instanceID = System.Guid.NewGuid().ToString();
        }

        public virtual void EditMode()
        {
            mode = State.Edit;
        }

        public virtual void PlayMode()
        {
            mode = State.Play;
        }
    }
}