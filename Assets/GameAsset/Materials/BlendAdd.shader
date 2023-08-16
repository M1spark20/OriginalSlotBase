Shader "Hidden/BlendAdd"
{
    Properties
    {
        [HideInInspector] _MainTex ("Texture", 2D) = "white" {}
        _Color ("Action Color", Color) = (1,1,1,1)
        _Weight ("Effect Weight", Range(0,255)) = 255
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always
        // 加算ブレンド
        Blend One One

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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            // ここから下だけ編集する
            sampler2D _MainTex;
            fixed4 _Color;
            int _Weight;

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                // 白バックにAction Colorを反映する
                col.rgb = col.rgb * _Color;
                // 色をEffect Weight分暗くする
                float w = float(_Weight) / 255;
                col.rgb = col.rgb * w;
                return col;
            }
            ENDCG
        }
    }
}
