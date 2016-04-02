Shader "Custom/CaptureShader" {
	SubShader {
		Pass {
			Color (1,1,1,0) Material { Diffuse (1,1,1,0) Ambient (1,1,1,0) }
			Lighting Off
			SetTexture [_MainTex]
		}
	}
}