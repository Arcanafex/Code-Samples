using UnityEngine;
using UnityEditor;

namespace Spaces.Core
{
    //[CustomPropertyDrawer(typeof(SpaceEvent.SerializableCall))]
    public class SerializableCallDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            base.OnGUI(position, property, label);
        }
    }

    //[CustomPropertyDrawer(typeof(SpaceEvent.SerializableArgument))]
    public class SerializableArgumentDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            base.OnGUI(position, property, label);
        }
    }

}