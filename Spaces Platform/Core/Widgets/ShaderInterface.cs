using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Spaces.Core
{
    public class ShaderInterface : ScriptableObject
    {
        public enum ShaderPropertyType
        {
            Color,
            Vector,
            Float,
            Range,
            TexEnv
        }

        [System.Serializable]
        public struct ShaderRangeProperty
        {
            public float def;
            public float min;
            public float max;
        }

        [System.Serializable]
        public class ShaderProperty
        {
            public string name;
            public ShaderPropertyType type;
            public string description;
            public UnityEngine.Rendering.TextureDimension texDim;
            public ShaderRangeProperty range;
            public bool hidden;
        }

        public List<ShaderProperty> properties;

        private Shader m_shader;
        public Shader shader
        {
            get { return m_shader; }
            set
            {
                m_shader = value;
                shaderName = m_shader.name;
            }
        }
        public string shaderName;

        public static ShaderInterface CreateInterface(Shader shader)
        {
            var shaderInterface = CreateInstance<ShaderInterface>();
            shaderInterface.shader = shader;
            shaderInterface.properties = new List<ShaderProperty>();
            return shaderInterface;
        }

        public static ShaderInterface CreateInterface(string shaderID)
        {
            var shaderInterface = CreateInstance<ShaderInterface>();
            shaderInterface.properties = new List<ShaderProperty>();
            return shaderInterface;
        }

        public static ShaderInterface GetInterface(string internalShader)
        {
            var shaderDef = UnityClient.UserSession.Instance.m_settings.shaders.FirstOrDefault(s => s.name == internalShader);
            return shaderDef != null ? shaderDef.shaderInterface : null;
        }

        public static ShaderInterface GetInterface(Shader shader)
        {
            var shaderDef = UnityClient.UserSession.Instance.m_settings.shaders.FirstOrDefault(s => s.shader.name == shader.name);
            return shaderDef != null ? shaderDef.shaderInterface : null;
        }

    }
}