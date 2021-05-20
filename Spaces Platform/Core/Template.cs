using UnityEngine;
using System.Collections;

namespace Spaces.Core
{
    public class Template
    {
        public string id;
        public string name;

        public Node rootNode;

        private Template()
        {
        }
        
        public static Template CreateTemplate(GameObject root)
        {
            var template = new Template();

            template.rootNode = new Node(root);

            return template;
        }


        public void SaveDataMap()
        { }

        public void Delete()
        { }

        public void Load(string templateJson)
        { }
    }
}