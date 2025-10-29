Shader "Custom/GridDisplay"
{
    Properties
    {
        _MainTex ("Grid Texture (R Channel)", 2D) = "white" {}
        _MinColor ("Min Value Color", Color) = (0,0,0,1)  // Black for 0
        _MaxColor ("Max Value Color", Color) = (1,1,1,1)  // White for 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _MinColor;
            fixed4 _MaxColor;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Sample the R channel (your [0,1] values)
                float intensity = tex2D(_MainTex, i.uv).r;
                
                // Lerp from min to max color based on intensity
                fixed4 col = lerp(_MinColor, _MaxColor, intensity);
                return col;
            }
            ENDCG
        }
    }
}
