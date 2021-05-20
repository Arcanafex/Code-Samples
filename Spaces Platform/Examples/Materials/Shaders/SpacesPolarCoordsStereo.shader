// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Shader created with Shader Forge v1.26 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.26;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,lico:0,lgpr:1,limd:0,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:2,bsrc:0,bdst:1,dpts:2,wrdp:True,dith:0,rfrpo:True,rfrpn:Refraction,coma:15,ufog:False,aust:True,igpj:False,qofs:0,qpre:1,rntp:1,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:False,fnfb:False;n:type:ShaderForge.SFN_Final,id:3138,x:32962,y:32717,varname:node_3138,prsc:2|emission-2930-RGB;n:type:ShaderForge.SFN_ComponentMask,id:6427,x:30977,y:32281,varname:node_6427,prsc:2,cc1:0,cc2:1,cc3:2,cc4:-1|IN-6938-OUT;n:type:ShaderForge.SFN_ArcTan2,id:3380,x:31810,y:32688,varname:node_3380,prsc:2,attp:2|A-6427-B,B-4912-OUT;n:type:ShaderForge.SFN_Append,id:9323,x:32431,y:32761,varname:node_9323,prsc:2|A-3380-OUT,B-7277-OUT;n:type:ShaderForge.SFN_Tex2d,id:2930,x:32634,y:32765,ptovrint:False,ptlb:MainTex,ptin:_MainTex,varname:_mainTex,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:59f6c9038436ebc4aaf4c864fbd269e6,ntxv:0,isnm:False|UVIN-9323-OUT,MIP-8020-OUT;n:type:ShaderForge.SFN_FragmentPosition,id:2340,x:30360,y:32298,varname:node_2340,prsc:2;n:type:ShaderForge.SFN_Normalize,id:2792,x:30555,y:32298,varname:node_2792,prsc:2|IN-2340-XYZ;n:type:ShaderForge.SFN_ArcCos,id:9717,x:31236,y:32220,varname:node_9717,prsc:2|IN-6427-G;n:type:ShaderForge.SFN_Pi,id:3947,x:31236,y:32119,varname:node_3947,prsc:2;n:type:ShaderForge.SFN_Divide,id:9726,x:31434,y:32220,varname:node_9726,prsc:2|A-9717-OUT,B-3947-OUT;n:type:ShaderForge.SFN_Vector1,id:8020,x:32425,y:32955,cmnt:mip 0 to fix seams,varname:node_8020,prsc:2,v1:0;n:type:ShaderForge.SFN_SwitchProperty,id:7277,x:32343,y:32425,ptovrint:False,ptlb:top,ptin:_top,varname:node_7277,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,on:False|A-3175-OUT,B-2394-OUT;n:type:ShaderForge.SFN_Divide,id:3175,x:31896,y:32237,varname:node_3175,prsc:2|A-9726-OUT,B-5822-OUT;n:type:ShaderForge.SFN_Vector1,id:5822,x:31680,y:32305,varname:node_5822,prsc:2,v1:2;n:type:ShaderForge.SFN_Vector1,id:1839,x:31898,y:32460,varname:node_1839,prsc:2,v1:0.5;n:type:ShaderForge.SFN_Add,id:2394,x:32149,y:32374,varname:node_2394,prsc:2|A-3175-OUT,B-1839-OUT;n:type:ShaderForge.SFN_Vector1,id:2322,x:30521,y:32443,varname:node_2322,prsc:2,v1:-1;n:type:ShaderForge.SFN_Multiply,id:6938,x:30781,y:32287,varname:node_6938,prsc:2|A-2792-OUT,B-2322-OUT;n:type:ShaderForge.SFN_Multiply,id:4912,x:31085,y:32746,varname:node_4912,prsc:2|A-6427-R,B-2322-OUT;proporder:2930-7277;pass:END;sub:END;*/

Shader "SPACES/PolarCoordsStereo" {
    Properties {
        _MainTex ("MainTex", 2D) = "white" {}
        [MaterialToggle] _top ("top", Float ) = 0.25
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
            Cull Off
            
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #include "UnityCG.cginc"
            #pragma multi_compile_fwdbase_fullshadows
            #pragma exclude_renderers d3d11_9x xbox360 xboxone ps3 ps4 psp2 
            #pragma target 3.0
            #pragma glsl
            uniform sampler2D _MainTex; uniform float4 _MainTex_ST;
            uniform fixed _top;
            struct VertexInput {
                float4 vertex : POSITION;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float4 posWorld : TEXCOORD0;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                o.pos = mul(UNITY_MATRIX_MVP, v.vertex );
                return o;
            }
            float4 frag(VertexOutput i, float facing : VFACE) : COLOR {
                float isFrontFace = ( facing >= 0 ? 1 : 0 );
                float faceSign = ( facing >= 0 ? 1 : -1 );
////// Lighting:
////// Emissive:
                float node_2322 = (-1.0);
                float3 node_6427 = (normalize(i.posWorld.rgb)*node_2322).rgb;
                float node_3175 = ((acos(node_6427.g)/3.141592654)/2.0);
                float2 node_9323 = float2(((atan2(node_6427.b,(node_6427.r*node_2322))/6.28318530718)+0.5),lerp( node_3175, (node_3175+0.5), _top ));
                float4 _MainTex_var = tex2Dlod(_MainTex,float4(TRANSFORM_TEX(node_9323, _MainTex),0.0,0.0));
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
