Shader "CRTShader"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _DecalTex("Decal Texture", 2D) = "white" {}
        _NoiseTex("Noise Texture", 2D) = "white" {}
    }
        
    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #include "UnityCG.cginc"
            #pragma enable_d3d11_debug_symbols
            #pragma vertex vert
            #pragma fragment frag

            struct a2v
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float2 decaluv : TEXCOORD1;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;

            // Screen Jump
            int _ScreenJumpOnOff;
            float _ScreenJumpOffset;

            // Frickering
            int _FlickeringOnOff;
            float _FlickeringStrength;
            float _FlickeringCycle;

            // Slippage
            int _SlippageOnOff;
            float _SlippageNoiseOnOff;
            float _SlippageStrength;
            float _SlippageSize;
            float _SlippageInterval;
            float _SlippageScrollSpeed;

            // Chromatic Aberration
            int _ChromaticAberrationOnOff;
            float _ChromaticAberrationStrength;

            // Multiple Ghost
            int _MultipleGhostOnOff;
            float _MultipleGhostStrength;

            // Scanline
            int _ScanlineOnOff;
            float _ScanlineFrequency;
            int _ScanlineNoiseOnOff;

            // Monochorme
            int _MonochromeOnOff;

            // Film Dirt
            int _FilmDirtOnOff;
            sampler2D _FilmDirtTex;
            float4 _FilmDirtTex_ST;

            // Decal
            int _DecalTexOnOff;
            float2 _DecalTexPos;
            float2 _DecalTexScale;
            sampler2D _DecalTex;
            float4 _DecalTex_ST;

            float GetRandom(float value);
            float EaseIn(float t0, float t1, float t);

            v2f vert(a2v v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.decaluv = TRANSFORM_TEX(v.uv, _DecalTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv;

                // Screen Jump
                uv.y = frac(uv.y + _ScreenJumpOffset * _ScreenJumpOnOff);

                // Frickering
                float flickeringNoise = GetRandom(_Time.y);
                float flickeringMask = pow(abs(sin(i.uv.y * _FlickeringCycle + _Time.y)), 10);
                uv.x = uv.x + (flickeringNoise * flickeringMask * _FlickeringStrength * _FlickeringOnOff);

                // Slippage
                float scrollSpeed = _Time.x * _SlippageScrollSpeed;
                float slippageMask = pow(abs(sin(i.uv.y * _SlippageInterval + scrollSpeed)), _SlippageSize);
                float stepMask = round(sin(i.uv.y * _SlippageInterval + scrollSpeed - 1));
                uv.x = uv.x + (slippageMask * stepMask * _SlippageStrength * _SlippageNoiseOnOff) * _SlippageOnOff;

                // Chromatic Aberration
                float red = tex2D(_MainTex, float2(uv.x - _ChromaticAberrationStrength * _ChromaticAberrationOnOff, uv.y)).r;
                float green = tex2D(_MainTex, float2(uv.x, uv.y)).g;
                float blue = tex2D(_MainTex, float2(uv.x + _ChromaticAberrationStrength * _ChromaticAberrationOnOff, uv.y)).b;
                float4 color = float4(red, green, blue, 1);

                // Multiple Ghost
                float4 ghost1 = tex2D(_MainTex, uv - float2(1, 0) * _MultipleGhostStrength * _MultipleGhostOnOff);
                float4 ghost2 = tex2D(_MainTex, uv - float2(1, 0) * 2 * _MultipleGhostStrength * _MultipleGhostOnOff);
                color = color * 0.8 + ghost1 * 0.15 + ghost2 * 0.05;

                // Decal
                float4 decal = tex2D(_DecalTex, (i.decaluv - _DecalTexPos) * _DecalTexScale) * _DecalTexOnOff;
                color = color * (1 - decal.a) + decal;

                // Scanline
                float scanline = sin((i.uv.y + _Time.x) * _ScanlineFrequency) * 0.05;
                color -= scanline * _ScanlineOnOff;
                if (pow(sin(uv.y + _Time.y * 2), 300) * _ScanlineNoiseOnOff >= 0.999)
                    color *= GetRandom(uv.y);

                // Monochorme
                color.rgb += (0.299f * color.r + 0.587f * color.g + 0.114f * color.b) * _MonochromeOnOff;

                // Film Dirt
                float filmDirtTime = _Time.x;
                float2 centeredUV = -1.0 + 2.0 * uv;
                float2 noiseLookupCoords = centeredUV + filmDirtTime * 1000;
                float3 noiseSample =
                    tex2D(_FilmDirtTex, 0.1 * noiseLookupCoords.xy).xyz +
                    tex2D(_FilmDirtTex, 0.01 * noiseLookupCoords.xy).xyz +
                    tex2D(_FilmDirtTex, 0.004 * noiseLookupCoords.xy).xyz;
                float threshold = 0.6;
                float smoothRadius = 0.1;
                float mul1 = smoothstep(threshold - smoothRadius, threshold + smoothRadius, noiseSample.x);
                float mul2 = smoothstep(threshold - smoothRadius, threshold + smoothRadius, noiseSample.y);
                float mul3 = smoothstep(threshold - smoothRadius, threshold + smoothRadius, noiseSample.z);

                float seed = tex2D(_FilmDirtTex, float2(filmDirtTime * 0.35, filmDirtTime)).x;
                float result = clamp(0, 1, seed + 0.7);
                result += 0.06 * EaseIn(19.2, 19.4, filmDirtTime);

                float band = 0.05;
                if (_FilmDirtOnOff == 1)
                {
                    if (0.3 < seed && 0.3 + band > seed)
                        color *= mul1 * result;
                    else if (0.6 < seed && 0.6 + band > seed)
                        color *= mul2 * result;
                    else if (0.9 < seed && 0.9 + band > seed)
                        color *= mul3 * result;
                }

                return color;
            }

            float GetRandom(float x)
            {
                return frac(sin(dot(x, float2(12.9898, 78.233))) * 43758.5453);
            }

            float EaseIn(float t0, float t1, float t)
            {
                return 2.0 * smoothstep(t0, 2.0 * t1 - t0, t);
            }
            ENDCG
        }
    }
}
