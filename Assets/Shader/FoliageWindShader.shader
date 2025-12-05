Shader "Custom/FoliageWindShader"
{
    Properties
    {
        // Основные свойства
        [MainColor] _BaseColor("Base Color", Color) = (1,1,1,1)
        [MainTexture] _MainTex("Albedo (RGB) Alpha (A)", 2D) = "white" {}
        _Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5
        
        // Настройки ветра
        _WindStrength("Wind Strength", Range(0, 2)) = 0.5
        _WindSpeed("Wind Speed", Range(0, 5)) = 1.0
        _WindFrequency("Wind Frequency", Range(0, 2)) = 0.5
        _WindDirection("Wind Direction", Vector) = (1,0,0,0)
    }
    
    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Geometry"
        }
        
        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        
        struct Attributes
        {
            float4 positionOS : POSITION;
            float3 normalOS : NORMAL;
            float2 uv : TEXCOORD0;
            float4 color : COLOR;
        };
        
        struct Varyings
        {
            float2 uv : TEXCOORD0;
            float3 normalWS : TEXCOORD1;
            float3 positionWS : TEXCOORD2;
            float4 positionCS : SV_POSITION;
            float4 color : TEXCOORD3;
        };
        
        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);
        
        CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_ST;
            float4 _BaseColor;
            float _Cutoff;
            
            float _WindStrength;
            float _WindSpeed;
            float _WindFrequency;
            float4 _WindDirection;
        CBUFFER_END
        
        float3 ApplyWind(float3 positionOS, float3 positionWS, float2 uv, float4 vertexColor)
        {
            float time = _Time.y * _WindSpeed;
            
            // Базовое покачивание
            float windEffect = sin(time + positionWS.x * _WindFrequency) * _WindStrength;
            
            // Учитываем высоту
            windEffect *= uv.y;
            
            // Учитываем цвет вершины для разной жесткости
            windEffect *= (1.0 - 0.5 * vertexColor.r);
            
            // Применяем направление ветра
            float3 windOffset = normalize(_WindDirection.xyz) * windEffect * 0.1;
            
            return positionOS + windOffset;
        }
        ENDHLSL
        
        // Основной проход
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            
            Cull Off
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT
            
            Varyings vert(Attributes input)
            {
                Varyings output;
                
                // Применяем ветер
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 windPositionOS = ApplyWind(input.positionOS.xyz, positionWS, input.uv, input.color);
                
                // Обновляем позицию
                positionWS = TransformObjectToWorld(windPositionOS);
                
                VertexPositionInputs vertexInput = GetVertexPositionInputs(windPositionOS);
                output.positionCS = vertexInput.positionCS;
                output.positionWS = positionWS;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.color = input.color;
                
                return output;
            }
            
            half4 frag(Varyings input) : SV_Target
            {
                // Читаем текстуру
                half4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                
                // Альфа-клиппинг
                clip(texColor.a - _Cutoff);
                
                // Применяем цвет
                half3 albedo = texColor.rgb * _BaseColor.rgb;
                
                // Освещение
                Light mainLight = GetMainLight();
                float3 normalWS = normalize(input.normalWS);
                float NdotL = saturate(dot(normalWS, mainLight.direction));
                float3 lighting = albedo * mainLight.color * (NdotL * 0.5 + 0.5);
                
                return half4(lighting, 1.0);
            }
            ENDHLSL
        }
        
        // Проход теней
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            
            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull Off
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW
            
            struct ShadowAttributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };
            
            struct ShadowVaryings
            {
                float2 uv : TEXCOORD0;
                float4 positionCS : SV_POSITION;
            };
            
            float3 _LightDirection;
            float3 _LightPosition;
            
            ShadowVaryings vert(ShadowAttributes input)
            {
                ShadowVaryings output;
                
                // Применяем ветер (такой же как в основном проходе)
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 windPositionOS = ApplyWind(input.positionOS.xyz, positionWS, input.uv, input.color);
                
                positionWS = TransformObjectToWorld(windPositionOS);
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
                
                #if _CASTING_PUNCTUAL_LIGHT_SHADOW
                    float3 lightDirectionWS = normalize(_LightPosition - positionWS);
                #else
                    float3 lightDirectionWS = _LightDirection;
                #endif
                
                float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, lightDirectionWS));
                
                #if UNITY_REVERSED_Z
                    positionCS.z = min(positionCS.z, UNITY_NEAR_CLIP_VALUE);
                #else
                    positionCS.z = max(positionCS.z, UNITY_NEAR_CLIP_VALUE);
                #endif
                
                output.positionCS = positionCS;
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                return output;
            }
            
            half4 frag(ShadowVaryings input) : SV_Target
            {
                half4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                clip(texColor.a - _Cutoff);
                return 0;
            }
            ENDHLSL
        }
    }
    
    FallBack "Universal Render Pipeline/Simple Lit"
}