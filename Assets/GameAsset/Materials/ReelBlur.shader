Shader "Hidden/ReelBlur"
{
    Properties
    {
        [HideInInspector] _MainTex ("Texture", 2D) = "white" {}
        _BlurRange ("Range", Float) = 0
    }
    SubShader
    {
    	// アルファブレンド設定
    	Tags { "RenderType"="Transparent" "Queue" = "Transparent" }
    	Blend SrcAlpha OneMinusSrcAlpha
    	
        // No culling or depth
        Cull Off ZWrite Off ZTest Always
        
        // 各pathでの共通コードをCGINCLUDE～ENDCGに書く
        CGINCLUDE
        
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
            
            half4x4 GetAffine(half3 xyz){
                return half4x4(1, 0, 0, xyz.x,
                               0, 1, 0, xyz.y,
                               0, 0, 1, xyz.z,
                               0, 0, 0, 1);
            }

            sampler2D _MainTex;
            Float _BlurRange;
            
        ENDCG
		
		// 描画1回目
        Pass
        {
            CGPROGRAM
            v2f vert (appdata v)
            {
                v2f o;
                //v.vertex = mul(GetAffine(float3(0, 0, 0)), v.vertex);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }
            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv) * i.color;
                col.rgb *= col.a;
                //col.a = col.a * 1.0;
                return col;
            }
            ENDCG
        }
        
		// 2回目
        Pass
        {
            CGPROGRAM
            v2f vert (appdata v)
            {
                v2f o;
                v.vertex = mul(GetAffine(float3(0, _BlurRange / 2, 0)), v.vertex);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }
            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv) * i.color;
                col.a = col.a * 0.5;
                return col;
            }
            ENDCG
        }
        
		// 3回目
        Pass
        {
            CGPROGRAM
            v2f vert (appdata v)
            {
                v2f o;
                v.vertex = mul(GetAffine(float3(0, _BlurRange, 0)), v.vertex);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }
            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv) * i.color;
                col.a = col.a * 0.25;
                return col;
            }
            ENDCG
        }
    }
}
