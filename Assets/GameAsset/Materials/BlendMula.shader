Shader "Hidden/BlendMula"
{
    Properties
    {
        [HideInInspector] _MainTex ("Texture", 2D) = "white" {}
        _Weight ("Effect Weight", Range(0,255)) = 255
    }
    SubShader
    {
    	Tags { "RenderType"="Transparent" "Queue" = "Transparent" }
        // No culling or depth
        Cull Off ZWrite Off ZTest Always
        // 乗算ブレンド(白黒マスク向け/白部分を暗くする)
        Blend Zero SrcColor

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }

            // ここから下だけ編集する
            sampler2D _MainTex;
            fixed4 _Color;
            int _Weight;

            fixed4 frag (v2f i) : SV_Target
            {
            	// 発色は頂点カラーで制御する
                fixed4 col = tex2D(_MainTex, i.uv) * i.color;
                col.rgb *= col.a;
                
                // 色を反転させる。上記操作で黒の色が変わらないため、
                // ここで反転させると白となり乗算部分をすり抜ける
                col.rgb = 1 - col.rgb;
                return col;
            }
            ENDCG
        }
    }
}
