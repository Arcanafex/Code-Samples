// Shader created with Shader Forge v1.26 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.26;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,lico:0,lgpr:1,limd:0,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:0,bsrc:3,bdst:7,dpts:2,wrdp:False,dith:0,rfrpo:True,rfrpn:Refraction,coma:15,ufog:False,aust:True,igpj:True,qofs:0,qpre:3,rntp:2,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:False,fnfb:False;n:type:ShaderForge.SFN_Final,id:3138,x:32719,y:32712,varname:node_3138,prsc:2|emission-7937-RGB,alpha-479-OUT;n:type:ShaderForge.SFN_Tex2d,id:7937,x:32254,y:32770,ptovrint:False,ptlb:MainTex,ptin:_MainTex,varname:node_7937,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:8b765a5db28a54c12b1d80fa3fa0186e,ntxv:0,isnm:False|UVIN-2792-UVOUT;n:type:ShaderForge.SFN_Panner,id:2792,x:32048,y:32770,varname:node_2792,prsc:2,spu:0,spv:1|UVIN-1038-UVOUT,DIST-1751-OUT;n:type:ShaderForge.SFN_TexCoord,id:1038,x:31794,y:32695,varname:node_1038,prsc:2,uv:0;n:type:ShaderForge.SFN_ValueProperty,id:3582,x:31624,y:33010,ptovrint:False,ptlb:ScrollSpeed,ptin:_ScrollSpeed,varname:node_3582,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0.2;n:type:ShaderForge.SFN_Time,id:5312,x:31545,y:32852,varname:node_5312,prsc:2;n:type:ShaderForge.SFN_Multiply,id:1751,x:31757,y:32852,varname:node_1751,prsc:2|A-5312-TSL,B-3582-OUT;n:type:ShaderForge.SFN_Tex2d,id:6181,x:32061,y:33010,ptovrint:False,ptlb:Fader,ptin:_Fader,varname:node_6181,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:2ffef47ad907a4ec891865c8733bc48b,ntxv:2,isnm:False;n:type:ShaderForge.SFN_Multiply,id:2904,x:32425,y:32980,varname:node_2904,prsc:2|A-7937-A,B-6181-R;n:type:ShaderForge.SFN_Clamp01,id:479,x:32546,y:32876,varname:node_479,prsc:2|IN-2904-OUT;proporder:7937-3582-6181;pass:END;sub:END;*/

Shader "SPACES/SpacesScrollY_Unlit_transparent" {
    Properties {
        _MainTex ("MainTex", 2D) = "white" {}
        _ScrollSpeed ("ScrollSpeed", Float ) = 0.2
        _Fader ("Fader", 2D) = "black" {}
        [HideInInspector]_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
    }
    SubShader {
        Tags {
            "IgnoreProjector"="True"
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #include "UnityCG.cginc"
            #pragma multi_compile_fwdbase
            #pragma exclude_renderers metal d3d11_9x xbox360 xboxone ps3 ps4 psp2 
            #pragma target 3.0
            uniform float4 _TimeEditor;
            uniform sampler2D _MainTex; uniform float4 _MainTex_ST;
            uniform float _ScrollSpeed;
            uniform sampler2D _Fader; uniform float4 _Fader_ST;
            struct VertexInput {
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.pos = mul(UNITY_MATRIX_MVP, v.vertex );
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
////// Lighting:
////// Emissive:
                float4 node_5312 = _Time + _TimeEditor;
                float2 node_2792 = (i.uv0+(node_5312.r*_ScrollSpeed)*float2(0,1));
                float4 _MainTex_var = tex2D(_MainTex,TRANSFORM_TEX(node_2792, _MainTex));
                float3 emissive = _MainTex_var.rgb;
                float3 finalColor = emissive;
                float4 _Fader_var = tex2D(_Fader,TRANSFORM_TEX(i.uv0, _Fader));
                return fixed4(finalColor,saturate((_MainTex_var.a*_Fader_var.r)));
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
    CustomEditor "ShaderForgeMaterialInspector"
}
