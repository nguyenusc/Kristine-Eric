// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "BubblesMat"
{
	Properties
	{
		_Cutoff( "Mask Clip Value", Float ) = 0.03
		_TextureSample0("Texture Sample 0", 2D) = "white" {}
		_BaseColor("BaseColor", Color) = (0,0,0,0)
		_InsideColor("InsideColor", Color) = (0,0,0,0)
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "TransparentCutout"  "Queue" = "AlphaTest+0" "IsEmissive" = "true"  }
		Cull Off
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf Standard keepalpha noshadow nolightmap  vertex:vertexDataFunc 
		struct Input
		{
			float3 worldPos;
			float3 worldNormal;
			float4 vertexColor : COLOR;
			half2 uv_texcoord;
		};

		uniform half4 _InsideColor;
		uniform half4 _BaseColor;
		uniform sampler2D _TextureSample0;
		uniform float _Cutoff = 0.03;

		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float3 ase_vertexNormal = v.normal.xyz;
			float3 appendResult185 = (half3(ase_vertexNormal.x , 0.0 , ase_vertexNormal.z));
			float4 temp_output_155_0 = ( ( ( v.color * 2 ) + -1.0 ) + v.color.a );
			float4 lerpResult151 = lerp( float4( 1,1,1,0 ) , float4( 0,0,0,1 ) , saturate( temp_output_155_0 ));
			float3 ase_vertex3Pos = v.vertex.xyz;
			v.vertex.xyz = ( ( ( half4( appendResult185 , 0.0 ) * ( 1.0 - lerpResult151 ) ) * float4( 0.1981132,0.1981132,0.1981132,0 ) ) + half4( ase_vertex3Pos , 0.0 ) ).rgb;
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float3 ase_worldPos = i.worldPos;
			half3 ase_worldViewDir = normalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			half3 ase_worldNormal = i.worldNormal;
			float fresnelNdotV190 = dot( ase_worldNormal, ase_worldViewDir );
			float fresnelNode190 = ( 0.3 + 0.3 * pow( 1.0 - fresnelNdotV190, 1.0 ) );
			o.Emission = ( ( fresnelNode190 * _InsideColor ) + _BaseColor ).rgb;
			o.Smoothness = 1.0;
			o.Alpha = 1;
			float4 temp_output_155_0 = ( ( ( i.vertexColor * 2 ) + -1.0 ) + i.vertexColor.a );
			float4 lerpResult151 = lerp( float4( 1,1,1,0 ) , float4( 0,0,0,1 ) , saturate( temp_output_155_0 ));
			clip( ( lerpResult151 * ( ( 1.0 - temp_output_155_0 ) * tex2D( _TextureSample0, i.uv_texcoord ) ) ).r - _Cutoff );
		}

		ENDCG
	}
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=16200
-1202;-1189;1204;994;-29.93017;233.2651;1.3;True;True
Node;AmplifyShaderEditor.VertexColorNode;18;-882.1777,128.0582;Float;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;161;-677.722,163.5416;Float;False;Constant;_Float0;Float 0;4;0;Create;True;0;0;False;0;-1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ScaleNode;160;-677.1263,86.93124;Float;False;2;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;158;-490.7517,95.59093;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;155;-337.5602,148.7192;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SaturateNode;157;-133.4713,104.6915;Float;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.NormalVertexDataNode;174;120.6755,-119.1817;Float;False;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;151;79.56453,56.32284;Float;False;3;0;COLOR;1,1,1,0;False;1;COLOR;0,0,0,1;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;187;-501.8193,430.8495;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;185;319.9188,-94.77448;Float;False;FLOAT3;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.OneMinusNode;178;290.3373,59.49604;Float;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.FresnelNode;190;215.562,-576.994;Float;False;Standard;WorldNormal;ViewDir;True;5;0;FLOAT3;0,0,1;False;4;FLOAT3;0,0,0;False;1;FLOAT;0.3;False;2;FLOAT;0.3;False;3;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;177;463.5561,29.52613;Float;False;2;2;0;FLOAT3;0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.OneMinusNode;172;-137.4335,211.3314;Float;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;162;-226.0144,395.4802;Float;True;Property;_TextureSample0;Texture Sample 0;1;0;Create;True;0;0;False;0;None;c3a863161c533f74faad79a170a71cee;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;193;212.2803,-414.5029;Float;False;Property;_InsideColor;InsideColor;3;0;Create;True;0;0;False;0;0,0,0,0;0.2302866,0.4925482,0.6509434,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;192;477.2133,-458.5557;Float;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;171;117.7188,285.6267;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.PosVertexDataNode;175;608.6035,276.921;Float;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;195;661.3054,180.467;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0.1981132,0.1981132,0.1981132,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;196;424.5572,-287.653;Float;False;Property;_BaseColor;BaseColor;2;0;Create;True;0;0;False;0;0,0,0,0;0.1174795,0.4528302,0.2439812,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;197;673.9574,-367.5532;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;165;592.0953,429.7287;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;202;839.3823,103.2413;Float;False;Constant;_Smoothness;Smoothness;4;0;Create;True;0;0;False;0;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;176;839.0147,242.5425;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT3;0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;8;1021.474,-10.84511;Half;False;True;2;Half;ASEMaterialInspector;0;0;Standard;BubblesMat;False;False;False;False;False;False;True;False;False;False;False;False;False;False;False;False;False;False;False;False;Off;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Masked;0.03;True;False;0;False;TransparentCutout;;AlphaTest;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;False;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Absolute;0;;0;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;160;0;18;0
WireConnection;158;0;160;0
WireConnection;158;1;161;0
WireConnection;155;0;158;0
WireConnection;155;1;18;4
WireConnection;157;0;155;0
WireConnection;151;2;157;0
WireConnection;185;0;174;1
WireConnection;185;2;174;3
WireConnection;178;0;151;0
WireConnection;177;0;185;0
WireConnection;177;1;178;0
WireConnection;172;0;155;0
WireConnection;162;1;187;0
WireConnection;192;0;190;0
WireConnection;192;1;193;0
WireConnection;171;0;172;0
WireConnection;171;1;162;0
WireConnection;195;0;177;0
WireConnection;197;0;192;0
WireConnection;197;1;196;0
WireConnection;165;0;151;0
WireConnection;165;1;171;0
WireConnection;176;0;195;0
WireConnection;176;1;175;0
WireConnection;8;2;197;0
WireConnection;8;4;202;0
WireConnection;8;10;165;0
WireConnection;8;11;176;0
ASEEND*/
//CHKSM=4FE9AB52ACC806FD7B6948B5A0DF6D6F7035C33C