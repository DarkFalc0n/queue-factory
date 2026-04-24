Shader "Custom/ConveyorBelt"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Tiling ("Tiling (XY)", Vector) = (1, 1, 0, 0)
        _MoveSpeed ("Move Speed", Float) = 1.0
        [Toggle] _IsCorner ("Is Corner", Float) = 0
        _CornerPivot ("Corner Pivot (XY)", Vector) = (0.0, 0.0, 0, 0)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
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
            float2 _Tiling;
            float _MoveSpeed;
            float _IsCorner;
            float2 _CornerPivot;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                // Unity macro for transforming UVs using _MainTex_ST if needed
                // But we will use our custom _Tiling and offset logic in the fragment shader
                o.uv = v.uv; 
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;

                if (_IsCorner > 0.5)
                {
                    // Polar Coordinates for Corner wrapping
                    float2 offset = uv - _CornerPivot;
                    
                    // Radius -> maps to texture U (X axis)
                    float radius = length(offset);
                    
                    // atan2(y, x) -> returns angle from -Pi to Pi
                    float angle = atan2(offset.y, offset.x);
                    
                    // Normalize the angle to [0, 1] for a 90 degree turn (PI/2 = 1.570796)
                    float normalizedAngle = angle / 1.57079632679;
                    
                    if (_CornerPivot.x > 0.5)
                    {
                        radius = 1.0 - radius;
                        normalizedAngle = 2.0 - normalizedAngle;
                    }
                    
                    // Remap UVs
                    // x is distance from pivot, y is rotation progression + time movement
                    uv = float2(radius * _Tiling.x, (normalizedAngle * _Tiling.y) + (_Time.y * _MoveSpeed));
                }
                else
                {
                    // Straight movement
                    // We swap X and Y (uv.yx) to rotate the texture 90 degrees, 
                    // because the path is now stretched along the local X axis.
                    uv = uv.yx;
                    uv = uv * _Tiling;
                    
                    // Move the texture along the length of the belt
                    // Since we swapped UVs, the physical length (geometry U) is now mapped to uv.y
                    uv.y -= _Time.y * _MoveSpeed;
                }

                // Sample texture with remapped UVs
                fixed4 col = tex2D(_MainTex, uv);
                return col;
            }
            ENDCG
        }
    }
}
