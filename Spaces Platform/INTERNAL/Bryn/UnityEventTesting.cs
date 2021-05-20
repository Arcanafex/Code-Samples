using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;


public class UnityEventTesting : MonoBehaviour
{
    public UnityEvent OnSomethingOrOther;
    public Spaces.Core.SpaceEvent spaceEvent;

    public void LargeBeets(string myString)
    {
        Debug.Log("String: " + myString);
    }

    private void Start()
    {
        var widget = GetComponent<Spaces.Core.Widget>();
        var memberInfo = widget.GetType().GetMembers();

        foreach (var info in memberInfo)
        {
            if (info.MemberType == System.Reflection.MemberTypes.Field)
            {
                var token = info.MetadataToken;
                var type = info.Module.ResolveField(token);
                var hasInterface = type.FieldType.GetInterfaces().Contains(typeof(Spaces.Core.ISerializableReference));

                Debug.Log("METHOD: " + info + " " + type.FieldType + " " + hasInterface);
            }
        }

        //foreach (var info in memberInfo)
        //{
        //    var type = info.MemberType;

        //    //if (type is Spaces.Core.ISerializableReference)
        //    Debug.Log("MEMBER: " + info + " " + type);
        //}

        //OnSomethingOrOther = new UnityEvent();

        //if (OnSomethingOrOther != null)
        //{
        //    //OnSomethingOrOther.AddListener(myAction);
        //}
        //else
        //{
        //    Debug.LogWarning("No dice");
        //}

        ////OnSomethingOrOther.Invoke();

        //var types = new System.Type[0];// { typeof(string) };
        //                               //var info = UnityEvent.GetValidMethodInfo(this, "LargeBeets", types);
        //                               //info.Invoke(this, new object[]{ "Blubbery"});


        //Object target = null;

        //foreach (var widget in ((GameObject)spaceEvent.actions[0].m_Target).GetComponents<Component>())
        //{
        //    var methods = widget.GetType().GetMethods();
        //    foreach (var info in methods)
        //    {
        //        Debug.Log(info.Name);
        //        if (info.Name == spaceEvent.actions[0].m_MethodName)
        //            target = widget;
        //    }
        //}



        ////var type = System.Type.GetType("UnityEventInputTesting");

        ////var methods = type.GetMethods();
        ////foreach (var info in methods)
        ////{
        ////    Debug.Log(info.Name);
        ////}

        //System.Reflection.MethodInfo method = target.GetType().GetMethod(spaceEvent.actions[0].m_MethodName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);

        ////var info = UnityEvent.GetValidMethodInfo(spaceEvent.actions[0].m_Target, spaceEvent.actions[0].m_MethodName, types);
        //if (method != null)
        //{
        //    Debug.Log("Booya!");
        //    method.Invoke(target, new object[0]);

        //    var action = new UnityAction(delegate { method.Invoke(target, new object[0]); });
        //    OnSomethingOrOther.AddListener(action);
        //}
        //Debug.Log("method info: " + method.Name);

        ////new UnityAction(method.Invoke(target, new object[0]);)
        //OnSomethingOrOther.Invoke();

    }

    private void OnDisable()
    {
        //Debug.Log(JsonUtility.ToJson(this, true));
    }
}