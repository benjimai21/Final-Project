Shader "Multiply by Solid Color" {

	Properties
	{
		_Color("Main Color", Color) = (1,1,1,1)
		_MainTex("Texture", 2D) = "white" {}
	}

		SubShader
	{
		Tags{ Queue = Transparent }
		Blend Zero SrcColor
		Pass{
		SetTexture[_MainTex]{
		constantColor[_Color]
		Combine texture * constant, texture * constant }
		}
	}

}
