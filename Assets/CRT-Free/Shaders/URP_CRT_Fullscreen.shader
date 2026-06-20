Shader "Universal Render Pipeline/CRT Fullscreen"
{
    Properties
    {
        _Curvature ("Monitor Curvature", Range(0.00, 0.5)) = 0.15
        _Vignette ("Vignette (Dark Corners)", Range(0, 3)) = 1.5
        _Scanlines ("Scanlines Density", Range(0, 5)) = 1.0
        _ScanlinesOpacity ("Scanlines Opacity", Range(0, 1)) = 0.5
        _ColorShift ("Chromatic Aberration", Range(0, 0.01)) = 0.002
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline" }
        LOD 100
        ZTest Always ZWrite Off Cull Off

        Pass
        {
            Name "CRT Fullscreen Pass"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Fragment
            
            // Wajib untuk URP Full Screen Pass
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            float _Curvature;
            float _Vignette;
            float _Scanlines;
            float _ScanlinesOpacity;
            float _ColorShift;

            half4 Fragment(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float2 uv = input.texcoord;
                
                // 1. Efek Layar Melengkung (Curvature)
                uv = uv * 2.0 - 1.0;
                float2 offset = abs(uv.yx) * _Curvature;
                uv = uv + uv * offset * offset;
                uv = uv * 0.5 + 0.5;

                // Jika melewati batas layar yang melengkung, jadikan hitam (Border)
                if (uv.x < 0.0 || uv.x > 1.0 || uv.y < 0.0 || uv.y > 1.0)
                    return half4(0.05, 0.05, 0.05, 1); // Warna abu-abu sangat gelap untuk pinggiran

                // 2. Efek Chromatic Aberration (Warna RGB sedikit terpisah)
                half r = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv + float2(_ColorShift, 0)).r;
                half g = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv).g;
                half b = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv - float2(_ColorShift, 0)).b;
                half4 color = half4(r, g, b, 1.0);

                // 3. Efek Scanlines (Garis-garis horizontal khas TV tabung)
                float scanline = sin(uv.y * 800.0 * _Scanlines) * 0.1 * _ScanlinesOpacity;
                color.rgb -= scanline;

                // 4. Efek Vignette (Sudut layar lebih gelap)
                float2 dist = uv - 0.5;
                float vignette = 1.0 - dot(dist, dist) * _Vignette;
                color.rgb *= vignette;

                return color;
            }
            ENDHLSL
        }
    }
}
