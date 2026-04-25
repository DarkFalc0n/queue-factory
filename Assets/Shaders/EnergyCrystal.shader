Shader "Custom/EnergyCrystal"
{
    Properties
    {
        [HDR] _EdgeColor ("Edge Emissive Color", Color) = (0.0, 0.8, 1.0, 2.0)
        _CoreColor ("Core Color", Color) = (0.0, 0.1, 0.2, 1.0)
        _FresnelPower ("Fresnel Power", Range(0.1, 10.0)) = 3.0
    }

    SubShader
    {
        // ForceNoShadowCasting helps prevent shadow rendering
        Tags 
        { 
            "RenderType" = "Opaque" 
            "Queue" = "Geometry"
            "ForceNoShadowCasting" = "True" 
        }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldNormal : NORMAL;
                float3 viewDir : TEXCOORD0;
            };

            float4 _EdgeColor;
            float4 _CoreColor;
            float _FresnelPower;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                
                // Transform normal to world space
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                
                // Calculate world space view direction
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.viewDir = normalize(_WorldSpaceCameraPos - worldPos);
                
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Normalize vectors for accurate dot product
                float3 n = normalize(i.worldNormal);
                float3 v = normalize(i.viewDir);

                // Calculate Fresnel (1 at edges, 0 at center)
                float ndotv = saturate(dot(n, v));
                float fresnel = pow(1.0 - ndotv, _FresnelPower);

                // Lerp between Core and Edge based on Fresnel
                fixed4 col = lerp(_CoreColor, _EdgeColor, fresnel);

                return col;
            }
            ENDCG
        }
    }
    
    // Removing the Fallback ensures no implicit ShadowCaster pass is added,
    // which guarantees this material will not cast shadows.
    Fallback Off
}
