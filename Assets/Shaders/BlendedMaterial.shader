Shader "Custom/BlendedMaterial"
{
    Properties
    {
        _MainTex("Texture 0", 2D) = "white" {}
        _MainNormal("Normal Texture 0", 2D) = "bump" {}
        _Tex1("Texture 1", 2D) = "white" {}
        _Normal1("Normal Texture 1", 2D) = "bump" {}
        _Tex2("Texture 2", 2D) = "white" {}
        _Normal2("Normal Texture 2", 2D) = "bump" {}
        _Tex3("Texture 3", 2D) = "white" {}
        _Normal3("Normal Texture 3", 2D) = "bump" {}

        _BlendWeight0 ("Blend Weight 0", Range(0, 1)) = 1
        _BlendWeight1 ("Blend Weight 1", Range(0, 1)) = 0
        _BlendWeight2 ("Blend Weight 2", Range(0, 1)) = 0
        _BlendWeight3 ("Blend Weight 3", Range(0, 1)) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"            

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 positionWS : TEXCOORD2;
            };
            
            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _MainNormal_ST;
                float4 _Tex1_ST;
                float4 _Normal1_ST;
                float4 _Tex2_ST;
                float4 _Normal2_ST;
                float4 _Tex3_ST;
                float4 _Normal3_ST;
                float _BlendWeight0;
                float _BlendWeight1;
                float _BlendWeight2;
                float _BlendWeight3;
            CBUFFER_END

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_MainNormal);
            SAMPLER(sampler_MainNormal);
            TEXTURE2D(_Tex1);
            SAMPLER(sampler_Tex1);
            TEXTURE2D(_Normal1);
            SAMPLER(sampler_Normal1);
            TEXTURE2D(_Tex2);
            SAMPLER(sampler_Tex2);
            TEXTURE2D(_Normal2);
            SAMPLER(sampler_Normal2);
            TEXTURE2D(_Tex3);
            SAMPLER(sampler_Tex3);
            TEXTURE2D(_Normal3);
            SAMPLER(sampler_Normal3);

            Varyings Vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                output.positionHCS = TransformObjectToHClip(input.positionOS);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.positionWS = TransformObjectToWorld(input.positionOS);
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float weightSum = _BlendWeight0 + _BlendWeight1 + _BlendWeight2 + _BlendWeight3;

                half4 c0 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv * _MainTex_ST.xy + _MainTex_ST.zw) * (_BlendWeight0 / weightSum);
                half4 c1 = SAMPLE_TEXTURE2D(_Tex1, sampler_Tex1, input.uv * _Tex1_ST.xy + _Tex1_ST.zw) * (_BlendWeight1 / weightSum);
                half4 c2 = SAMPLE_TEXTURE2D(_Tex2, sampler_Tex2, input.uv * _Tex2_ST.xy + _Tex2_ST.zw) * (_BlendWeight2 / weightSum);
                half4 c3 = SAMPLE_TEXTURE2D(_Tex3, sampler_Tex3, input.uv * _Tex3_ST.xy + _Tex3_ST.zw) * (_BlendWeight3 / weightSum);

                half4 finalColor = c0 + c1 + c2 + c3;

                half3 n0 = UnpackNormalScale(SAMPLE_TEXTURE2D(_MainNormal, sampler_MainNormal, input.uv * _MainNormal_ST.xy + _MainNormal_ST.zw), 1.0) * (_BlendWeight0 / weightSum);
                half3 n1 = UnpackNormalScale(SAMPLE_TEXTURE2D(_Normal1, sampler_Normal1, input.uv * _Normal1_ST.xy + _Normal1_ST.zw), 1.0) * (_BlendWeight1 / weightSum);
                half3 n2 = UnpackNormalScale(SAMPLE_TEXTURE2D(_Normal2, sampler_Normal2, input.uv * _Normal2_ST.xy + _Normal2_ST.zw), 1.0) * (_BlendWeight2 / weightSum);
                half3 n3 = UnpackNormalScale(SAMPLE_TEXTURE2D(_Normal3, sampler_Normal3, input.uv * _Normal3_ST.xy + _Normal3_ST.zw), 1.0) * (_BlendWeight3 / weightSum);

                half3 finalNormal = normalize(n0 + n1 + n2 + n3);

                
                // Calculate tangent based on UV gradients
                float3 dpdx = ddx(input.positionHCS.xyz);
                float3 dpdy = ddy(input.positionHCS.xyz);
                float2 duvdx = ddx(input.uv);
                float2 duvdy = ddy(input.uv);
                
                float3 tangent = normalize(dpdx * duvdy.y - dpdy * duvdx.y);
                float3 bitangent = normalize(cross(input.normalWS, tangent) * (duvdx.x * duvdy.y - duvdy.x * duvdx.y));
                
                InputData inputData = (InputData)0;
                inputData.positionWS = input.positionHCS.xyz;
                inputData.normalWS = TransformTangentToWorld(finalNormal, half3x3(tangent, bitangent, input.normalWS));
                inputData.viewDirectionWS = normalize(_WorldSpaceCameraPos - GetWorldSpaceViewDir(input.positionHCS.xyz));

                half metallic = 0.0;
                half3 specular = half3(0.5, 0.5, 0.5);
                half smoothness = 0.5;
                half occlusion = 1.0;
                half3 emission = half3(0.0, 0.0, 0.0);

                

                half4 color = UniversalFragmentPBR(inputData, finalColor.rgb, metallic, specular, smoothness, occlusion, emission, finalColor.a);
                return color;
            }
            ENDHLSL
        }
    }
    FallBack "Diffuse"
}
