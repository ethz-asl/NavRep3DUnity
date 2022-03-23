// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Unlit/Depth"
{
    Properties
    {
    }

    CGINCLUDE
    #include "UnityCG.cginc"
    ENDCG

    SubShader
    {
        Tags 
        {
			"DisableBatching" = "true"
            "RenderType"="Opaque"
            "Queue" = "Overlay"  
        }

        Pass 
        {
            Cull Back
            CGPROGRAM
            #pragma vertex vert             
            #pragma fragment frag
            
                         struct vertOut 
             {
                 float4 pos : SV_POSITION;
                 float2 tex : TEXCOORD0;
                 float3 vpos : TEXCOORD2;
				 float3 wpos : TEXCOORD3;
             };
             
 
             vertOut vert(appdata_full input)
             {
                 vertOut output;
 
                 
                 fixed4 pos = input.vertex; 
 
                 output.wpos = mul (unity_ObjectToWorld, input.vertex).xyz;
                 output.pos = UnityObjectToClipPos (pos);
				 output.vpos = UnityObjectToViewPos(pos);
                 output.tex = input.texcoord;
                 return output;
             }
 
             fixed4 frag(vertOut input) : COLOR0
             {
				// took me hours to understand that _CameraDepthTexture works only if it wants to
				// this way, it always work.
				float dist = sqrt(input.vpos.x*input.vpos.x
				 			  + input.vpos.y*input.vpos.y
							  + input.vpos.z*input.vpos.z);
				float maxdist  = 100.;
				float dist01 = min(1., dist / maxdist);
				int distinc = round(dist01 * 256 * 256 * 256);
				// Splitting the depth value into 2 8-bit-part : 
				// The most significant bits for the green channel
				// The least significant bits for the red channel
				int msb = distinc / 256 / 256;
				int ssb = (distinc / 256) % 256;
				int lsb = distinc % 256;
				fixed4 output = fixed4(msb / 256.,ssb / 256.,lsb / 256.,1.);
                return output;
             }
            ENDCG
        }  
   }
}
