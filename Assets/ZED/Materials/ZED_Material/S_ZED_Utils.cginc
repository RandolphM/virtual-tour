//struct ChromaKey {
//	float3 keyColor;
//	float thresh;
//	float slope;
//};
//
//
////Compute the depth of ZED to the Unity scale
//float computeDepthXYZ(float3 colorXYZ) {
//	//reverse Y and Z axes
//	colorXYZ.b = -colorXYZ.b;
//	colorXYZ.g = -colorXYZ.g;
//
//	//If the depth is inf or nan
//	if (colorXYZ.b != colorXYZ.b) return 0.999f;
//	if (isinf(colorXYZ.b)) {
//		if (colorXYZ.b > 0)
//			return 0.01f;
//		else
//			return 0.999f;
//	}
//
//	float4 v = float4(colorXYZ, 1);
//	//Project to unity's coordinate
//	float4 depthXYZVector = mul(UNITY_MATRIX_P, v);
//
//
//	depthXYZVector.b /= (depthXYZVector.w);
//	float depthReal = depthXYZVector.b;
//
//	if (depthReal > 0.99999f || depthReal <= 0) depthReal = 0.99999f;
//
//	return depthReal;
//}
//float Epsilon = 1e-10;
///*Convert the color space*/
////RGB to XYZ
//float3 rgb2xyz(float3 c) {
//	float3 tmp;
//	tmp.x = (c.r > 0.04045) ? pow((c.r + 0.055) / 1.055, 2.4) : c.r / 12.92;
//	tmp.y = (c.g > 0.04045) ? pow((c.g + 0.055) / 1.055, 2.4) : c.g / 12.92,
//		tmp.z = (c.b > 0.04045) ? pow((c.b + 0.055) / 1.055, 2.4) : c.b / 12.92;
//	static const float3x3 mat = float3x3(
//		0.4124, 0.3576, 0.1805,
//		0.2126, 0.7152, 0.0722,
//		0.0193, 0.1192, 0.9505
//		);
//	return 100.0 * mul(tmp, mat);
//}
//
////RGB to YUV
//float3 rgb2yuv(float3 c) {
//	static const float3x3 RGBtoYUV = float3x3(
//		0.257, 0.439, -0.148,
//		0.504, -0.368, -0.291,
//		0.098, -0.071, 0.439
//		);
//
//	return mul(c, RGBtoYUV);
//}
//
//float3 rgb2hcv(in float3 RGB)
//{
//	// Based on work by Sam Hocevar and Emil Persson
//	float4 P = (RGB.g < RGB.b) ? float4(RGB.bg, -1.0, 2.0 / 3.0) : float4(RGB.gb, 0.0, -1.0 / 3.0);
//	float4 Q = (RGB.r < P.x) ? float4(P.xyw, RGB.r) : float4(RGB.r, P.yzx);
//	float C = Q.x - min(Q.w, Q.y);
//	float H = abs((Q.w - Q.y) / (6 * C + Epsilon) + Q.z);
//	return float3(H, C, Q.x);
//}
//
//float3 rgb2hsv(in float3 RGB)
//{
//	float3 HCV = rgb2hcv(RGB);
//	float S = HCV.y / (HCV.z + Epsilon);
//	return float3(HCV.x, S, HCV.z);
//}
//
////XYZ to Lab
//float3 xyz2lab(float3 c) {
//	float3 n = c / float3(95.047, 100, 108.883);
//	float3 v;
//	v.x = (n.x > 0.008856) ? pow(n.x, 1.0 / 3.0) : (7.787 * n.x) + (16.0 / 116.0);
//	v.y = (n.y > 0.008856) ? pow(n.y, 1.0 / 3.0) : (7.787 * n.y) + (16.0 / 116.0);
//	v.z = (n.z > 0.008856) ? pow(n.z, 1.0 / 3.0) : (7.787 * n.z) + (16.0 / 116.0);
//	return float3((116.0 * v.y) - 16.0, 500.0 * (v.x - v.y), 200.0 * (v.y - v.z));
//}
////RGB 2 LAB
//float3 rgb2lab(float3 c) {
//	float3 lab = xyz2lab(rgb2xyz(c));
//	return float3(lab.x / 100.0, 0.5 + 0.5 * (lab.y / 127.0), 0.5 + 0.5 * (lab.z / 127.0));
//}
////Lab to XYZ
//float3 lab2xyz(float3 c) {
//	float fy = (c.x + 16.0) / 116.0;
//	float fx = c.y / 500.0 + fy;
//	float fz = fy - c.z / 200.0;
//	return float3(
//		95.047 * ((fx > 0.206897) ? fx * fx * fx : (fx - 16.0 / 116.0) / 7.787),
//		100.000 * ((fy > 0.206897) ? fy * fy * fy : (fy - 16.0 / 116.0) / 7.787),
//		108.883 * ((fz > 0.206897) ? fz * fz * fz : (fz - 16.0 / 116.0) / 7.787)
//		);
//}
//
////XYZ to RGB
//float3 xyz2rgb(float3 c) {
//	static const float3x3 mat = float3x3(
//		3.2406, -1.5372, -0.4986,
//		-0.9689, 1.8758, 0.0415,
//		0.0557, -0.2040, 1.0570
//		);
//	float3 v = mul(c / 100.0, mat);
//	float3 r;
//	r.x = (v.r > 0.0031308) ? ((1.055 * pow(v.r, (1.0 / 2.4))) - 0.055) : 12.92 * v.r;
//	r.y = (v.g > 0.0031308) ? ((1.055 * pow(v.g, (1.0 / 2.4))) - 0.055) : 12.92 * v.g;
//	r.z = (v.b > 0.0031308) ? ((1.055 * pow(v.b, (1.0 / 2.4))) - 0.055) : 12.92 * v.b;
//	return r;
//}
//
////Lab to RGB
//float3 lab2rgb(float3 c) {
//	return xyz2rgb(lab2xyz(float3(100.0 * c.x, 2.0 * 127.0 * (c.y - 0.5), 2.0 * 127.0 * (c.z - 0.5))));
//}
//
////Algorithm to compute the alpha of a frag depending of the similarity of a color.
////ColorCamera is the color from a texture given by the camera
////Thresh is the degree of similarity
////Slope is the power of the alpha
//float computeAlphaYUV(float3 colorCamera, ChromaKey key) {
//
//	float maskY = 0.2989 * key.keyColor.r + 0.5866 * key.keyColor.g + 0.1145 * key.keyColor.b;
//	float maskCr = 0.7132 * (key.keyColor.r - maskY);
//	float maskCb = 0.5647 * (key.keyColor.b - maskY);
//	
//	float Y = 0.2989 * colorCamera.r + 0.5866 * colorCamera.g + 0.1145 * colorCamera.b;
//	float Cr = 0.7132 * (colorCamera.r - Y);
//	float Cb = 0.5647 * (colorCamera.b - Y);
//
//
//	float blendValue = smoothstep(key.thresh, key.thresh + key.slope, distance(float2(maskCr, maskCb), float2(Cr, Cb)));
//	return blendValue;
//}
////Compute alpha depending of a dominant color
//float computeAlphaDominance(float3 colorCamera, float amount, uint primary_channel, uint other_1, uint other_2, float cutoff) {
//
//	float min = colorCamera[other_1] > colorCamera[other_2] ? colorCamera[other_2] : colorCamera[other_1];
//	float max = colorCamera[other_1] < colorCamera[other_2] ? colorCamera[other_2] : colorCamera[other_1];
//
//	float mean = (colorCamera[other_2] + colorCamera[other_1]) / 2.0f;
//	float p = colorCamera[primary_channel];
//	float dist = amount*((p - max)) + (p - mean);
//
//	if (dist > cutoff*(amount + 1)) dist = 1;
//	if (dist < 0) dist = 0;
//	
//	return 1 -dist;
//
//}

struct ChromaKey {
	float3 keyColor;
	float thresh;
	float slope;
};

float4x4 inverse(float4x4 input)
{
#define minor(a,b,c) determinant(float3x3(input.a, input.b, input.c))
	//determinant(float3x3(input._22_23_23, input._32_33_34, input._42_43_44))

	float4x4 cofactors = float4x4(
		minor(_22_23_24, _32_33_34, _42_43_44),
		-minor(_21_23_24, _31_33_34, _41_43_44),
		minor(_21_22_24, _31_32_34, _41_42_44),
		-minor(_21_22_23, _31_32_33, _41_42_43),

		-minor(_12_13_14, _32_33_34, _42_43_44),
		minor(_11_13_14, _31_33_34, _41_43_44),
		-minor(_11_12_14, _31_32_34, _41_42_44),
		minor(_11_12_13, _31_32_33, _41_42_43),

		minor(_12_13_14, _22_23_24, _42_43_44),
		-minor(_11_13_14, _21_23_24, _41_43_44),
		minor(_11_12_14, _21_22_24, _41_42_44),
		-minor(_11_12_13, _21_22_23, _41_42_43),

		-minor(_12_13_14, _22_23_24, _32_33_34),
		minor(_11_13_14, _21_23_24, _31_33_34),
		-minor(_11_12_14, _21_22_24, _31_32_34),
		minor(_11_12_13, _21_22_23, _31_32_33)
		);
#undef minor
	return transpose(cofactors) / determinant(input);
}

//Compute the depth of ZED to the Unity scale
float computeDepthXYZ(float3 colorXYZ, float scale) {
	//reverse Y and Z axes
	colorXYZ.b = -colorXYZ.b;
	colorXYZ.g = -colorXYZ.g;

	//If the depth is inf or nan
	if (colorXYZ.b != colorXYZ.b) return 0.999f;
	if (isinf(colorXYZ.b)) {
		if (colorXYZ.b > 0)
			return 0.01f;
		else
			return 0.999f;
	}

	float4 v = float4(colorXYZ, 1);

	/********************************************/
	//Deal with the scaling that might be happening on the parent of the camera.  This only supports uniform scaling......
	float4x4 scaling = float4x4(
		scale,
		0,
		0,
		0,

		0,
		scale,
		0,
		0,

		0,
		0,
		scale,
		0,

		0,
		0,
		0,
		1
		);

	v = mul(scaling, v);

	/*******************************************/

	//Project to unity's coordinate
	float4 depthXYZVector = mul(UNITY_MATRIX_P, v);

	depthXYZVector.b /= (depthXYZVector.w);

	float depthReal = depthXYZVector.b;

	if (depthReal > 0.99999f || depthReal <= 0) depthReal = 0.99999f;

	return depthReal;
}
float Epsilon = 1e-10;
/*Convert the color space*/
//RGB to XYZ
float3 rgb2xyz(float3 c) {
	float3 tmp;
	tmp.x = (c.r > 0.04045) ? pow((c.r + 0.055) / 1.055, 2.4) : c.r / 12.92;
	tmp.y = (c.g > 0.04045) ? pow((c.g + 0.055) / 1.055, 2.4) : c.g / 12.92,
		tmp.z = (c.b > 0.04045) ? pow((c.b + 0.055) / 1.055, 2.4) : c.b / 12.92;
	static const float3x3 mat = float3x3(
		0.4124, 0.3576, 0.1805,
		0.2126, 0.7152, 0.0722,
		0.0193, 0.1192, 0.9505
		);
	return 100.0 * mul(tmp, mat);
}

//RGB to YUV
float3 rgb2yuv(float3 c) {
	static const float3x3 RGBtoYUV = float3x3(
		0.257, 0.439, -0.148,
		0.504, -0.368, -0.291,
		0.098, -0.071, 0.439
		);

	return mul(c, RGBtoYUV);
}

float3 rgb2hcv(in float3 RGB)
{
	// Based on work by Sam Hocevar and Emil Persson
	float4 P = (RGB.g < RGB.b) ? float4(RGB.bg, -1.0, 2.0 / 3.0) : float4(RGB.gb, 0.0, -1.0 / 3.0);
	float4 Q = (RGB.r < P.x) ? float4(P.xyw, RGB.r) : float4(RGB.r, P.yzx);
	float C = Q.x - min(Q.w, Q.y);
	float H = abs((Q.w - Q.y) / (6 * C + Epsilon) + Q.z);
	return float3(H, C, Q.x);
}

float3 rgb2hsv(in float3 RGB)
{
	float3 HCV = rgb2hcv(RGB);
	float S = HCV.y / (HCV.z + Epsilon);
	return float3(HCV.x, S, HCV.z);
}

//XYZ to Lab
float3 xyz2lab(float3 c) {
	float3 n = c / float3(95.047, 100, 108.883);
	float3 v;
	v.x = (n.x > 0.008856) ? pow(n.x, 1.0 / 3.0) : (7.787 * n.x) + (16.0 / 116.0);
	v.y = (n.y > 0.008856) ? pow(n.y, 1.0 / 3.0) : (7.787 * n.y) + (16.0 / 116.0);
	v.z = (n.z > 0.008856) ? pow(n.z, 1.0 / 3.0) : (7.787 * n.z) + (16.0 / 116.0);
	return float3((116.0 * v.y) - 16.0, 500.0 * (v.x - v.y), 200.0 * (v.y - v.z));
}
//RGB 2 LAB
float3 rgb2lab(float3 c) {
	float3 lab = xyz2lab(rgb2xyz(c));
	return float3(lab.x / 100.0, 0.5 + 0.5 * (lab.y / 127.0), 0.5 + 0.5 * (lab.z / 127.0));
}
//Lab to XYZ
float3 lab2xyz(float3 c) {
	float fy = (c.x + 16.0) / 116.0;
	float fx = c.y / 500.0 + fy;
	float fz = fy - c.z / 200.0;
	return float3(
		95.047 * ((fx > 0.206897) ? fx * fx * fx : (fx - 16.0 / 116.0) / 7.787),
		100.000 * ((fy > 0.206897) ? fy * fy * fy : (fy - 16.0 / 116.0) / 7.787),
		108.883 * ((fz > 0.206897) ? fz * fz * fz : (fz - 16.0 / 116.0) / 7.787)
		);
}

//XYZ to RGB
float3 xyz2rgb(float3 c) {
	static const float3x3 mat = float3x3(
		3.2406, -1.5372, -0.4986,
		-0.9689, 1.8758, 0.0415,
		0.0557, -0.2040, 1.0570
		);
	float3 v = mul(c / 100.0, mat);
	float3 r;
	r.x = (v.r > 0.0031308) ? ((1.055 * pow(v.r, (1.0 / 2.4))) - 0.055) : 12.92 * v.r;
	r.y = (v.g > 0.0031308) ? ((1.055 * pow(v.g, (1.0 / 2.4))) - 0.055) : 12.92 * v.g;
	r.z = (v.b > 0.0031308) ? ((1.055 * pow(v.b, (1.0 / 2.4))) - 0.055) : 12.92 * v.b;
	return r;
}

//Lab to RGB
float3 lab2rgb(float3 c) {
	return xyz2rgb(lab2xyz(float3(100.0 * c.x, 2.0 * 127.0 * (c.y - 0.5), 2.0 * 127.0 * (c.z - 0.5))));
}

//Algorithm to compute the alpha of a frag depending of the similarity of a color.
//ColorCamera is the color from a texture given by the camera
//Thresh is the degree of similarity
//Slope is the power of the alpha
float computeAlphaYUV(float3 colorCamera, ChromaKey key) {

	float maskY = 0.2989 * key.keyColor.r + 0.5866 * key.keyColor.g + 0.1145 * key.keyColor.b;
	float maskCr = 0.7132 * (key.keyColor.r - maskY);
	float maskCb = 0.5647 * (key.keyColor.b - maskY);

	float Y = 0.2989 * colorCamera.r + 0.5866 * colorCamera.g + 0.1145 * colorCamera.b;
	float Cr = 0.7132 * (colorCamera.r - Y);
	float Cb = 0.5647 * (colorCamera.b - Y);


	float blendValue = smoothstep(key.thresh, key.thresh + key.slope, distance(float2(maskCr, maskCb), float2(Cr, Cb)));
	return blendValue;
}
//Compute alpha depending of a dominant color
float computeAlphaDominance(float3 colorCamera, float amount, uint primary_channel, uint other_1, uint other_2, float cutoff) {

	float min = colorCamera[other_1] > colorCamera[other_2] ? colorCamera[other_2] : colorCamera[other_1];
	float max = colorCamera[other_1] < colorCamera[other_2] ? colorCamera[other_2] : colorCamera[other_1];

	float mean = (colorCamera[other_2] + colorCamera[other_1]) / 2.0f;
	float p = colorCamera[primary_channel];
	float dist = amount*((p - max)) + (p - mean);

	if (dist > cutoff*(amount + 1)) dist = 1;
	if (dist < 0) dist = 0;

	return 1 - dist;

}