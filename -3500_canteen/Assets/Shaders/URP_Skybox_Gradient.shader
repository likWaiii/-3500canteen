Shader "Custom/URP/SkyboxGradient"
{
    Properties
    {
        _TopColor ("顶部颜色", Color) = (0.2, 0.5, 0.8, 1.0)
        _MiddleColor ("中间颜色", Color) = (0.4, 0.6, 0.9, 1.0)
        _BottomColor ("底部颜色", Color) = (0.6, 0.4, 0.3, 1.0)
        
        _TopHeight ("顶部高度", Range(-1, 1)) = 0.8
        _BottomHeight ("底部高度", Range(-1, 1)) = -0.2
        
        _Exponent ("渐变指数", Range(0.1, 10)) = 1.0
    }
    
    SubShader
    {
        Tags { "RenderType"="Background" "RenderPipeline" = "UniversalRenderPipeline" "Queue"="Background" }
        LOD 100
        
        Pass
        {
            Cull Off
            ZWrite Off
            ZTest Always
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 texcoord : TEXCOORD0;
            };
            
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 direction : TEXCOORD0;
            };
            
            // 属性
            half4 _TopColor;
            half4 _MiddleColor;
            half4 _BottomColor;
            half _TopHeight;
            half _BottomHeight;
            half _Exponent;
            
            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.direction = input.texcoord;
                return output;
            }
            
            half4 frag(Varyings input) : SV_Target
            {
                // 归一化方向向量
                float3 dir = normalize(input.direction);
                
                // 获取Y坐标作为高度值 (-1 到 1)
                float height = dir.y;
                
                // 计算顶部和中间颜色的混合因子
                float topBlend = saturate(pow((height - _BottomHeight) / (_TopHeight - _BottomHeight), _Exponent));
                
                // 计算最终颜色
                half4 finalColor;
                
                if (height > _TopHeight)
                {
                    // 高于顶部高度，使用顶部颜色
                    finalColor = _TopColor;
                }
                else if (height < _BottomHeight)
                {
                    // 低于底部高度，使用底部颜色
                    finalColor = _BottomColor;
                }
                else if (height > 0)
                {
                    // 高于地平线，混合顶部和中间颜色
                    finalColor = lerp(_MiddleColor, _TopColor, topBlend);
                }
                else
                {
                    // 低于地平线，混合底部和中间颜色
                    finalColor = lerp(_BottomColor, _MiddleColor, topBlend);
                }
                
                return finalColor;
            }
            ENDHLSL
        }
    }
    
    FallBack "Skybox/Procedural"
}
