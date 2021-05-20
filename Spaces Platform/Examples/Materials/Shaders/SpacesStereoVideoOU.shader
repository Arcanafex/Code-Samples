// Shader created with Shader Forge v1.26 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.26;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,lico:1,lgpr:1,limd:0,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:0,bsrc:0,bdst:1,dpts:2,wrdp:True,dith:0,rfrpo:True,rfrpn:Refraction,coma:15,ufog:False,aust:True,igpj:False,qofs:0,qpre:1,rntp:1,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:False,fnfb:False;n:type:ShaderForge.SFN_Final,id:3138,x:32719,y:32712,varname:node_3138,prsc:2|emission-8785-RGB;n:type:ShaderForge.SFN_Tex2d,id:8785,x:32408,y:32806,ptovrint:False,ptlb:MainTex,ptin:_MainTex,varname:node_8785,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:c0f974dc0e902634ab19dd8551dd001c,ntxv:0,isnm:False|UVIN-3229-OUT;n:type:ShaderForge.SFN_TexCoord,id:233,x:31531,y:32597,varname:node_233,prsc:2,uv:0;n:type:ShaderForge.SFN_SwitchProperty,id:370,x:32471,y:32553,ptovrint:False,ptlb:top,ptin:_top,varname:_top_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,on:False|A-4129-OUT,B-3346-OUT;n:type:ShaderForge.SFN_Divide,id:4129,x:32024,y:32365,varname:node_4129,prsc:2|A-233-V,B-8445-OUT;n:type:ShaderForge.SFN_Vector1,id:8445,x:31808,y:32433,varname:node_8445,prsc:2,v1:2;n:type:ShaderForge.SFN_Vector1,id:6550,x:32026,y:32588,varname:node_6550,prsc:2,v1:0.5;n:type:ShaderForge.SFN_Add,id:3346,x:32277,y:32502,varname:node_3346,prsc:2|A-4129-OUT,B-6550-OUT;n:type:ShaderForge.SFN_Append,id:3229,x:32192,y:32806,varname:node_3229,prsc:2|A-233-U,B-370-OUT;proporder:8785-370;pass:END;sub:END;*/

Shader "SPACES/StereoVideoOU" {
    Properties {
        _MainTex ("MainTex", 2D) = "white" {}
        [MaterialToggle] _top ("top", Float ) = 0
    }
    SubShader {
        Tags {
            "RenderType"="Opaque"
        }
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #include "UnityCG.cginc"
            #pragma multi_compile_fwdbase_fullshadows
            #pragma exclude_renderers gles3 metal d3d11_9x xbox360 xboxone ps3 ps4 psp2 
            #pragma target 3.0
            uniform sampler2D _MainTex; uniform float4 _MainTex_ST;
            uniform fixed _top;
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
                float node_4129 = (i.uv0.g/2.0);
                float2 node_3229 = float2(i.uv0.r,lerp( node_4129, (node_4129+0.5), _top ));
                float4 _MainTex_var = tex2D(_MainTex,TRANSFORM_TEX(node_3229, _MainTex));
                float3 emissive = _MainTex_var.rgb;
                float3 finalColor = emissive;
                return fixed4(finalColor,1);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
    CustomEditor "ShaderForgeMaterialInspector"
}
