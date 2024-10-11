//----------------------------------------------
//            Marvelous Techniques
// Copyright © 2019 - Arto Vaarala, Kirnu Interactive
// http://www.kirnuarp.com
//----------------------------------------------
Shader "Kirnu/Marvelous/ScreenTextureBlendPP"
{
    HLSLINCLUDE

        #include "Packages/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl"
        TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
        TEXTURE2D_SAMPLER2D(_Gradient, sampler_Gradient);

        int _BlendMode;
        int _B1;
        int _B2;
        int _B3;
        int _B4;
            
        float _BlendIntensity;
        float _VignetteIntensity;
        float _VignetteMax;

	ENDHLSL 

	SubShader
	{
		Lighting Off
		ZTest Always
		Cull Off
		ZWrite Off
		Fog { Mode Off }
	 	
	 	Pass 
	 	{
	  		HLSLPROGRAM

	  		#define USE_MAIN_TEX;
	  		#pragma vertex VertDefault
	  		#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
	    	
	    	#pragma shader_feature B1
	    	#pragma shader_feature B2 
	    	#pragma shader_feature B3
	    	#pragma shader_feature B4
	    	
	    	#define BLEND_MODE(s,s2,s3,s4) str(s,s2,s3,s4)
     		#define str(s,s2,s3,s4) applyBlend_##s##s2##s3##s4
     		
 			#ifdef B1
				#define B1_2 1
			#else
				#define B1_2 0
			#endif
			
			#ifdef B2
				#define B2_2 1
			#else
				#define B2_2 0
			#endif
			
			#ifdef B3
				#define B3_2 1
			#else
				#define B3_2 0
			#endif
			
			#ifdef B4
				#define B4_2 1
			#else
				#define B4_2 0
			#endif
	    	
            /*
*
* Blend mode script by Aubergine
* http://forum.unity3d.com/threads/free-photoshop-blends.121661/
*
*/
half3 Darken (half3 a, half3 b) { return half3(min(a.rgb, b.rgb)); }
half3 Multiply (half3 a, half3 b) { return (a * b); }
half3 ColorBurn (half3 a, half3 b) { return (1-(1-a)/b); }
half3 LinearBurn (half3 a, half3 b) { return (a+b-1); }
half3 Lighten (half3 a, half3 b) { return half3(max(a.rgb, b.rgb)); }
half3 Screen (half3 a, half3 b) { return (1-(1-a)*(1-b)); }
half3 ColorDodge (half3 a, half3 b) { return (a/(1-b)); }
half3 LinearDodge (half3 a, half3 b) { return (a+b); }
half3 Overlay (half3 a, half3 b) {
    half3 r = half3(0,0,0);
    if (a.r > 0.5) { r.r = 1-(1-2*(a.r-0.5))*(1-b.r); }
    else { r.r = (2*a.r)*b.r; }
    if (a.g > 0.5) { r.g = 1-(1-2*(a.g-0.5))*(1-b.g); }
    else { r.g = (2*a.g)*b.g; }
    if (a.b > 0.5) { r.b = 1-(1-2*(a.b-0.5))*(1-b.b); }
    else { r.b = (2*a.b)*b.b; }
    return r;
}
half3 SoftLight (half3 a, half3 b) {
    half3 r = half3(0,0,0);
    if (b.r > 0.5) { r.r = a.r*(1-(1-a.r)*(1-2*(b.r))); }
    else { r.r = 1-(1-a.r)*(1-(a.r*(2*b.r))); }
    if (b.g > 0.5) { r.g = a.g*(1-(1-a.g)*(1-2*(b.g))); }
    else { r.g = 1-(1-a.g)*(1-(a.g*(2*b.g))); }
    if (b.b > 0.5) { r.b = a.b*(1-(1-a.b)*(1-2*(b.b))); }
    else { r.b = 1-(1-a.b)*(1-(a.b*(2*b.b))); }
    return r;
}
half3 HardLight (half3 a, half3 b) {
    half3 r = half3(0,0,0);
    if (b.r > 0.5) { r.r = 1-(1-a.r)*(1-2*(b.r)); }
    else { r.r = a.r*(2*b.r); }
    if (b.g > 0.5) { r.g = 1-(1-a.g)*(1-2*(b.g)); }
    else { r.g = a.g*(2*b.g); }
    if (b.b > 0.5) { r.b = 1-(1-a.b)*(1-2*(b.b)); }
    else { r.b = a.b*(2*b.b); }
    return r;
}
half3 VividLight (half3 a, half3 b) {
    half3 r = half3(0,0,0);
    if (b.r > 0.5) { r.r = 1-(1-a.r)/(2*(b.r-0.5)); }
    else { r.r = a.r/(1-2*b.r); }
    if (b.g > 0.5) { r.g = 1-(1-a.g)/(2*(b.g-0.5)); }
    else { r.g = a.g/(1-2*b.g); }
    if (b.b > 0.5) { r.b = 1-(1-a.b)/(2*(b.b-0.5)); }
    else { r.b = a.b/(1-2*b.b); }
    return r;
}
half3 LinearLight (half3 a, half3 b) {
    half3 r = half3(0,0,0);
    if (b.r > 0.5) { r.r = a.r+2*(b.r-0.5); }
    else { r.r = a.r+2*b.r-1; }
    if (b.g > 0.5) { r.g = a.g+2*(b.g-0.5); }
    else { r.g = a.g+2*b.g-1; }
    if (b.b > 0.5) { r.b = a.b+2*(b.b-0.5); }
    else { r.b = a.b+2*b.b-1; }
    return r;
}
half3 PinLight (half3 a, half3 b) {
    half3 r = half3(0,0,0);
    if (b.r > 0.5) { r.r = max(a.r, 2*(b.r-0.5)); }
    else { r.r = min(a.r, 2*b.r); }
    if (b.g > 0.5) { r.g = max(a.g, 2*(b.g-0.5)); }
    else { r.g = min(a.g, 2*b.g); }
    if (b.b > 0.5) { r.b = max(a.b, 2*(b.b-0.5)); }
    else { r.b = min(a.b, 2*b.b); }
    return r;
}
half3 Difference (half3 a, half3 b) { return (abs(a-b)); }
half3 Exclusion (half3 a, half3 b) { return (0.5-2*(a-0.5)*(b-0.5)); }

	    	half3 applyBlend_0000(half3 original,half3 gradient){
	    		return Darken(original, gradient).rgb;
	    	}
            
	    	half3 applyBlend_0001(half3 original,half3 gradient){
	    		return Multiply(original, gradient).rgb;
	    	}
	    	half3 applyBlend_0010(half3 original,half3 gradient){
	    		return ColorBurn(original, gradient).rgb;
	    	}
	    	half3 applyBlend_0011(half3 original,half3 gradient){
	    		return LinearBurn(original, gradient).rgb;
	    	}
	    	half3 applyBlend_0100(half3 original,half3 gradient){
	    		return Lighten(original, gradient).rgb;
	    	}
	    	half3 applyBlend_0101(half3 original,half3 gradient){
	    		return Screen(original, gradient).rgb;
	    	}
	    	half3 applyBlend_0110(half3 original,half3 gradient){
	    		return ColorDodge(original, gradient).rgb;
	    	}
	    	half3 applyBlend_0111(half3 original,half3 gradient){
	    		return LinearDodge(original, gradient).rgb;
	    	}
	    	half3 applyBlend_1000(half3 original,half3 gradient){
	    		return Overlay(original, gradient).rgb;
	    	}
	    	half3 applyBlend_1001(half3 original,half3 gradient){
	    		return SoftLight(original, gradient).rgb;
	    	}
	    	half3 applyBlend_1010(half3 original,half3 gradient){
	    		return HardLight(original, gradient).rgb;
	    	}
	    	half3 applyBlend_1011(half3 original,half3 gradient){
	    		return VividLight(original, gradient).rgb;
	    	}
	    	half3 applyBlend_1100(half3 original,half3 gradient){
	    		return LinearLight(original, gradient).rgb;
	    	}
	    	half3 applyBlend_1101(half3 original,half3 gradient){
	    		return PinLight(original, gradient).rgb;
	    	}
	    	half3 applyBlend_1110(half3 original,half3 gradient){
	    		return Difference(original, gradient).rgb;
	    	}
	    	half3 applyBlend_1111(half3 original,half3 gradient){
	    		return Exclusion(original, gradient).rgb;
	    	}
	    		
	  		half4 frag (VaryingsDefault i) : COLOR
	  		{
	   			half3 original = SAMPLE_TEXTURE2D (_MainTex, sampler_MainTex, i.texcoord).rgb;
				half4 gradient = SAMPLE_TEXTURE2D(_Gradient, sampler_MainTex, i.texcoord);

				half4 effect = half4(BLEND_MODE(B4_2,B3_2,B2_2,B1_2)(original,gradient.rgb),0);	
	   			effect = half4(lerp(original,effect.xyz,_BlendIntensity),0);
	   			half2 coords = i.texcoord;
				half2 uv = i.texcoord;
		
				coords = (coords - 0.5) * 2.0;		
				half coordDot = dot (coords,coords); 
				float mask = 1.0 - coordDot  * _VignetteMax * _VignetteIntensity;
	   			effect.rgb *= mask;
				
	   			return effect;
	  		}
	  		
	  		ENDHLSL
	 	}
	}
}
