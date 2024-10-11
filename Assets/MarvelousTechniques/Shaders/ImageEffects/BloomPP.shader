Shader "Kirnu/Marvelous/BloomPP" {	
	HLSLINCLUDE

        #include "Packages/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl"

		TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
		TEXTURE2D_SAMPLER2D(_Bloom, sampler_Bloom);
				
		half4 _MainTex_TexelSize;
		half4 _Parameter;
		half4 _BloomColor;
		
		#define INTENSITY _Parameter.w
		#define THRESHHOLD _Parameter.z

        static const half offset[3] = {half( 0.0), half(1.3846153846), half(3.2307692308 )};
        static const half weight[3] = {half( 0.2270270270), half(0.3162162162), half(0.0702702703 )};

        inline half Luminance( half3 c ) {
            return dot( c, half3(0.22, 0.707, 0.071) );
        }

		half4 fragDownsample ( VaryingsDefault i ) : SV_Target {				
            half4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord);	
			color = lerp(0,color,(Luminance(color)-(1-THRESHHOLD))*INTENSITY);

			return _BloomColor*max(color, 0);
		}
						
		half4 fragBloom ( VaryingsDefault i ) : SV_Target{
        
            half4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord);
            return color + SAMPLE_TEXTURE2D(_Bloom, sampler_Bloom, i.texcoord);	
		} 
      
		half4 fragBlur ( VaryingsDefault i ) : SV_Target{
			half2 uv = i.texcoord.xy; 

    		half4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv) * weight[0];
    		for (int l=1; l<3; l++) {
                half2 v = half2(offset[l]*_Parameter.x*_MainTex_TexelSize.x, offset[l]*_Parameter.y*_MainTex_TexelSize.y);
                color += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + v) * weight[l];
                color += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv - v) * weight[l];
    		}

			return color;
		}
			
	ENDHLSL
	
	SubShader {
	  	Lighting Off
		ZTest Always
		Cull Off
		ZWrite Off
		Fog { Mode Off }
	  
	// 0
	Pass {
	
		HLSLPROGRAM
		#pragma vertex VertDefault
		#pragma fragment fragBloom
		
		ENDHLSL
		 
		}

	// 1
	Pass { 
	
		HLSLPROGRAM
		#pragma vertex VertDefault
		#pragma fragment fragDownsample
		
		ENDHLSL
		 
		}

	// 2
	Pass {
		HLSLPROGRAM 
		#pragma vertex VertDefault
		#pragma fragment fragBlur
		
		ENDHLSL 
		}		
	}	
}