Shader "Unlit/Water"
{
    Properties
    {
        //properties dealing with the main water waves
        [Header(Waves)]
        _Waves_Texture("Texture", 2D) = "bump" {}
        _Waves_Strength("Wave Strength", Float) = 1
        _Waves_Speed("Wave Speed", Vector) = (1.0, 1.0, 0.0, 0.0)
        _Waves_SecondaryScale("Secondary Wave Scale", Vector) = (1.0, 1.0, 1.0, 1.0)

            //properties dealing with water reflection
            [Header(Reflection)]
            _Shading_ReflectionColor("Reflection Color", Color) = (1.0, 1.0, 1.0, 1.0)
            [MaterialToggle] _Shading_EnableBoxProjection("Enable Box Projection", Float) = 0
            _Shading_ReflectionLOD("Reflection Mip Level", Float) = 0
            _Shading_FresnelPower("Fresnel Power", Float) = 2.26
            _Shading_FresnelSubtract("Fresnel Subtract", Float) = 0.36
            _Shading_FresnelMultiplier("Fresnel Multiplier", Float) = 1

                //custom section for when a custom reflection is defined
                [Header(Custom Reflection)]
                [MaterialToggle] _Shading_UseCustomCubemap("Use Custom Reflection", Float) = 0
                _Shading_CustomCubeReflection("Custom Cube Reflection", CUBE) = "white" {}
                _Shading_CustomBoxPosition("Custom Box Position", Vector) = (1.0, 0.0, 0.0, 0.0)
                _Shading_CustomBoxSizeMin("Custom Box Size Min", Vector) = (1.0, 0.0, 0.0, 0.0)
                _Shading_CustomBoxSizeMax("Custom Box Size Max", Vector) = (1.0, 0.0, 0.0, 0.0)

                    //properties dealing with water refraction
                    [Header(Refraction)]
                    [MaterialToggle] _Shading_EnableWaterFog("Enable Water Fog", Float) = 1
                    _Shading_RefractionColorShallow("Shallow Refraction Color", Color) = (0.411, 0.9254459, 1.0, 0.2)
                    _Shading_RefractionColorDeep("Deep Refraction Color", Color) = (0.17374, 0.2321556, 0.34, 1.0)
                    [MaterialToggle] _Shading_SolidRefraction("Solid Refraction", Float) = 0
                    _Shading_FogDepth("Water Depth", Float) = 1
                    [MaterialToggle] _Shading_DistortRefraction("Distort Refraction", Float) = 1
                    _Shading_RefractionDistortion("Distortion Amount", range(0,256)) = 50
                    [MaterialToggle] _Shading_ChromaticAbberation("Chromatic Abberation", Float) = 1
                    _Shading_RefractionChannelShift("Refraction Channel Shift", Vector) = (1.0, 1.0, 0.0, 0.0)

                        //shading properties dealing with specular
                        [Header(Specular)]
                        [MaterialToggle] _Shading_EnableSpecular("Enable Specular Reflection", Float) = 1
                        _Shading_Roughness("Specular Roughness", Range(0.0, 1.0)) = 0.001

                            //shading properties dealing with fake subsurface specular
                            [Header(Fake Subsurface Specular)]
                            [MaterialToggle] _Shading_EnableSSSSpecular("Enable Subsurface Specular Reflection", Float) = 1
                            _Shading_SSS_Power("Subsurface Specular Power", Float) = 7.17
                            _Shading_SSS_Strength("Subsurface Specular Strength", Float) = 0.1

                                //properties dealing with water intersection and edge smoothing
                                [Header(Edge Fading)]
                                [MaterialToggle] _Shading_EnableEdgeFading("Enable Edge Fading", Float) = 1
                                _Shading_EdgeFade("Edge Fade Factor", Float) = 5

                                    [Header(Fog)]
                                [MaterialToggle] _Shading_EnableUnityFog("Enable Unity Fog", Float) = 1
    }
        SubShader
                                {
                                    Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }

                                    //blend operation for transparency
                                    Blend SrcAlpha OneMinusSrcAlpha

                                    //no culling, and make sure we are writing to the depth buffer
                                    Cull Off ZTest On ZWrite On

                                    //used for getting the render target for refraction
                                    GrabPass
                                    {
                                        Name "BASE"
                                        Tags{ "RenderType" = "Opaque"  "LightMode" = "ForwardBase" }
                                    }

                                    //the main water shading pass
                                    Pass
                                    {
                                        CGPROGRAM
                                        #pragma vertex vert
                                        #pragma fragment frag

                                        //make fog work
                                        #pragma multi_compile_fog

                                        //include these .cginc files because they have some handy functions
                                        #include "UnityCG.cginc"
                                        #include "UnityStandardUtils.cginc"
                                        #include "UnityStandardBRDF.cginc"

                                        //used for the physically based GGX specular
                                        #define PI 3.141592653589793238462643383279502884197169

                                        //main mesh data
                                        struct appdata
                                        {
                                            float4 vertex : POSITION;
                                            float2 uv : TEXCOORD0;
                                            float4 tangent : TANGENT;
                                            float3 normal : NORMAL;

                                            UNITY_VERTEX_INPUT_INSTANCE_ID //Insert
                                        };

                                //shader vertex data
                                struct v2f
                                {
                                    float4 vertex : SV_POSITION; //vertex positions
                                    float2 uv : TEXCOORD0; //main texture coordinates for waves
                                    float4 uvgrab : TEXCOORD1; //texture coordinates for the refraction texture
                                    float4 posWorld : TEXCOORD2; //world space position 
                                    half3 tspace0 : TEXCOORD3; //tangent space 0
                                    half3 tspace1 : TEXCOORD4; //tangent space 1
                                    half3 tspace2 : TEXCOORD5; //tangent space 2
                                    float4 screenPos : TEXCOORD7; //screen position coordinates for the depth buffer
                                    float3 worldNormal : TEXCOORD8; //world normal of the mesh
                                    UNITY_FOG_COORDS(9) //get fog

                                    UNITY_VERTEX_OUTPUT_STEREO //Insert
                                };

                                //sampler2D _CameraDepthTexture; //unity camera depth texture
                                UNITY_DECLARE_SCREENSPACE_TEXTURE(_CameraDepthTexture);

                                sampler2D _GrabTexture; //camera refraction render texture
                                float4 _GrabTexture_TexelSize; //texel UV size for render texture

                                //wave properties
                                sampler2D _Waves_Texture; //main waves texture
                                float4 _Waves_Texture_ST; //waves tiling and offset
                                float _Waves_Strength; //normal map strength of the waves
                                float4 _Waves_Speed; //the speed at which the waves move (vector4, for control on each x and y axis)
                                float4 _Waves_SecondaryScale; //secondary tiling and offset for the 2nd wave layer

                                //reflection properties
                                float4 _Shading_ReflectionColor; //reflection color tint
                                float _Shading_EnableBoxProjection; //toggle, enables box projected cubemap reflections
                                float _Shading_ReflectionLOD; //reflection cubemap mip map level (for blurry reflections)
                                float _Shading_FresnelPower; //fresnel shading power
                                float _Shading_FresnelSubtract; //fresnel shading subtraction
                                float _Shading_FresnelMultiplier; //fresnel shading multiplier

                                //custom reflection properties
                                float _Shading_UseCustomCubemap; //toggle, enables use of custom reflection cubemaps
                                samplerCUBE _Shading_CustomCubeReflection; //custom reflection probe if defined
                                float4 _Shading_CustomBoxPosition; //world space position for box projection on custom cubemap (if enabled)
                                float4 _Shading_CustomBoxSizeMin; //minimum box size for box projection on custom cubemap (if enabled)
                                float4 _Shading_CustomBoxSizeMax; //maximum box size for box projection on custom cubemap (if enabled)

                                //refraction properties
                                float _Shading_EnableWaterFog;
                                float4 _Shading_RefractionColorShallow; //shallow color tint for water fog (becomes the main solid refracted color if there is no camera refraction distortion enabled)
                                float4 _Shading_RefractionColorDeep; //deep color tint for water fog
                                float _Shading_SolidRefraction; //toggle, ignore alpha channel on shallow color tint
                                float _Shading_FogDepth; //water depth fog multiplier
                                float _Shading_DistortRefraction; //toggle, enable camera refraction distortion
                                float _Shading_RefractionDistortion; //the strength of the refraction distortion
                                float _Shading_ChromaticAbberation; //toggle, enable chromatic abberation on refraction
                                float4 _Shading_RefractionChannelShift; //vector4 (x, y, z are used) for shifting each RGB channel for chromatic abberation

                                //specular properties
                                float _Shading_EnableSpecular; //toggle, enables GGX specular on water
                                float _Shading_Roughness; //roughness amount used for ggx specular

                                //fake subsurface specular properties
                                float _Shading_EnableSSSSpecular; //toggle, enables fake sub surface specular
                                float _Shading_SSS_Power; //the power of the sub surface specular
                                float _Shading_SSS_Strength; //the strength of the sub surface specular

                                //edge fading properties
                                float _Shading_EnableEdgeFading; //toggle, enables edge fading when the water mesh intersects other meshes
                                float _Shading_EdgeFade; //edge fade factor

                                float _Shading_EnableUnityFog;

                                //vertex function (per vertex)
                                v2f vert(appdata v)
                                {
                                    v2f o;

                                    UNITY_SETUP_INSTANCE_ID(v);
                                    UNITY_INITIALIZE_OUTPUT(v2f, o);
                                    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                                    //define our vertex positions
                                    o.vertex = UnityObjectToClipPos(v.vertex);

                                    //define our texture coordinates
                                    o.uv = TRANSFORM_TEX(v.uv, _Waves_Texture);

                                    //define our world position vector
                                    o.posWorld = mul(unity_ObjectToWorld, v.vertex);

                                    #if UNITY_UV_STARTS_AT_TOP
                                        float scale = -1.0;
                                    #else
                                        float scale = 1.0;
                                    #endif

                                    o.uvgrab.xy = (float2(o.vertex.x, o.vertex.y * scale) + o.vertex.w) * 0.5;
                                    o.uvgrab.zw = o.vertex.zw;

                                    //define our texture coordinates for the camera
                                    //o.uvgrab = ComputeScreenPos(o.vertex);

                                    //define our screen position vector for the camera depth buffer
                                    o.screenPos = o.uvgrab;

                                    //get the depth buffer
                                    //COMPUTE_EYEDEPTH(o.screenPos.z);

                                    //the world normal of the mesh
                                    half3 wNormal = UnityObjectToWorldNormal(v.normal);

                                    //the tangents of the mesh
                                    half3 wTangent = UnityObjectToWorldDir(v.tangent.xyz);

                                    //compute bitangent from cross product of normal and tangent
                                    half tangentSign = v.tangent.w * unity_WorldTransformParams.w;
                                    half3 wBitangent = cross(wNormal, wTangent) * tangentSign;

                                    // output the tangent space matrix
                                    o.tspace0 = half3(wTangent.x, wBitangent.x, wNormal.x);
                                    o.tspace1 = half3(wTangent.y, wBitangent.y, wNormal.y);
                                    o.tspace2 = half3(wTangent.z, wBitangent.z, wNormal.z);

                                    //get the world normal from the mesh
                                    o.worldNormal = wNormal;

                                    //unity function for defining fog vectors
                                    UNITY_TRANSFER_FOG(o,o.vertex);

                                    return o;
                                }

                                //Box projection function for the reflection vector
                                float3 BoxProjection(float3 direction, float3 position, float3 cubemapPosition, float3 boxMin, float3 boxMax)
                                {
                                    float3 factors = ((direction > 0 ? boxMax : boxMin) - position) / direction;
                                    float scalar = min(min(factors.x, factors.y), factors.z);
                                    return direction * scalar + (position - cubemapPosition);
                                }

                                //fragment function (per pixel)
                                fixed4 frag(v2f i) : SV_Target
                                {
                                    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

                                //--------------------------- VECTORS -------------------------------
                                float4 worldPosition = i.posWorld;
                                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                                float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
                                float3 halfDirection = normalize(viewDirection + lightDirection);
                                float3 worldNormal = normalize(i.worldNormal.xyz);

                                //--------------------------- WAVES -------------------------------
                                //get our wave UVs
                                float2 waterUVs1 = i.uv.xy + float2(_Time.x * _Waves_Speed.x, _Time.y * _Waves_Speed.y) * _Waves_Speed.w;
                                float2 waterUVs2 = i.uv.xy - float2(_Time.x * _Waves_Speed.x, _Time.y * _Waves_Speed.y) * _Waves_Speed.w;
                                waterUVs2 *= _Waves_SecondaryScale.xy;
                                waterUVs2 *= _Waves_SecondaryScale.w;

                                //sample our wave normals
                                float4 wavesBump1 = tex2D(_Waves_Texture, waterUVs1);
                                float4 wavesBump2 = tex2D(_Waves_Texture, waterUVs2);

                                //combine our normals
                                wavesBump1 += wavesBump2;
                                wavesBump1 /= 2.0f;

                                //add a bump strength for the waves
                                wavesBump1 = lerp(float4(0.5, 0.5, 1, 1), wavesBump1, _Waves_Strength);
                                wavesBump1.xyz = UnpackNormal(wavesBump1);

                                //--------------------------- ADDITIONAL VECTORS -------------------------------

                                //calculate our mesh normals with the wave normals into consideration
                                float3 normalDirection;
                                normalDirection.x = dot(i.tspace0, wavesBump1.xyz);
                                normalDirection.y = dot(i.tspace1, wavesBump1.xyz);
                                normalDirection.z = dot(i.tspace2, wavesBump1.xyz);
                                normalDirection = normalize(normalDirection);

                                //initalize our reflection direction (needs to be initalized so wave normals affect the reflection vector)
                                float3 reflectionDirection = reflect(-viewDirection, normalDirection);

                                float nv = abs(dot(normalDirection, viewDirection)); // This abs allow to limit artifact
                                float nl = saturate(dot(normalDirection, lightDirection));
                                float nh = saturate(dot(normalDirection, halfDirection));
                                float lv = saturate(dot(lightDirection, viewDirection));
                                float lh = saturate(dot(lightDirection, halfDirection));

                                //--------------------------- SHADING -------------------------------
                                //base roughness value
                                float perceptualRoughness = SmoothnessToPerceptualRoughness(_Shading_Roughness);
                                float roughness = PerceptualRoughnessToRoughness(perceptualRoughness);
                                roughness = max(roughness, 0.001);

                                //fresnel shading effect
                                float fresnel = pow(1.0 - max(0.0, dot(normalDirection, viewDirection)) - _Shading_FresnelSubtract, _Shading_FresnelPower);
                                fresnel = saturate(fresnel * _Shading_FresnelMultiplier);

                                //initalize our specular and subsurface values (if they aren't used they will stay at zero)
                                float3 specularColor = float3(0, 0, 0);
                                float subsurfaceSpecular = 0.0f;

                                //specular shading
                                if (_Shading_EnableSpecular > 0)
                                {
                                    float V = SmithJointGGXVisibilityTerm(nl, nv, roughness);
                                    float D = GGXTerm(nh, roughness);

                                    float specularTerm = V * D * UNITY_PI;

                                    //mutliply the light color (from unity provided light) by the ggx specular
                                    specularColor = _LightColor0.rgb * specularTerm;
                                }

                                //fake subsurface specular
                                if (_Shading_EnableSSSSpecular > 0)
                                {
                                    //compute fake subsurface specular
                                    subsurfaceSpecular = pow(saturate(dot(reflect(-lightDirection, worldNormal), viewDirection)), _Shading_SSS_Power) * _Shading_SSS_Strength;

                                    //clamp the values to 0..1
                                    subsurfaceSpecular = saturate(subsurfaceSpecular);
                                }

                                //--------------------------- REFLECTION -------------------------------
                                //initalize our reflection color
                                float3 finalReflection = (0, 0, 0);

                                //if a custom reflection cubemap is defined
                                if (_Shading_UseCustomCubemap > 0)
                                {
                                    //enable box projection
                                    if (_Shading_EnableBoxProjection > 0)
                                    {
                                        //do box projection, since a custom reflection cubemap is defined, the artist needs to author the box projection coordinates and values
                                        reflectionDirection = BoxProjection(reflectionDirection, worldPosition, _Shading_CustomBoxPosition, _Shading_CustomBoxSizeMin, _Shading_CustomBoxSizeMax);
                                    }

                                    //sample the custom reflection cubemap, and if it has mips then use the given mip map level
                                    finalReflection = texCUBElod(_Shading_CustomCubeReflection, float4(reflectionDirection.xyz, _Shading_ReflectionLOD)).rgb;
                                }
                                //if no custom reflection cubemap is defined
                                else
                                {
                                    //enable box projection
                                    if (_Shading_EnableBoxProjection > 0)
                                    {
                                        //do the box projection, since unity gives us a reflection probe that the mesh uses, we can use the box projection coordinates from it
                                        reflectionDirection = BoxProjection(reflectionDirection, worldPosition, unity_SpecCube0_ProbePosition, unity_SpecCube0_BoxMin, unity_SpecCube0_BoxMax);
                                    }

                                    //sample the unity provided reflection cubemap, if it has mip maps then use the given mip map level
                                    finalReflection = UNITY_SAMPLE_TEXCUBE_LOD(unity_SpecCube0, reflectionDirection.xyz, _Shading_ReflectionLOD).rgb;

                                    //decode the reflection probe since its HDR
                                    finalReflection = DecodeHDR(float4(finalReflection, 1), unity_SpecCube0_HDR);
                                }

                                //apply the reflection color tint
                                finalReflection.rgb *= _Shading_ReflectionColor.rgb;

                                //--------------------------- REFRACTION -------------------------------
                                //initalize our refraction color to be the shallow water color
                                float4 refractionColor = _Shading_RefractionColorDeep;

                                //if camera water refraction is enabled
                                if (_Shading_DistortRefraction > 0)
                                {
                                    //calculate the UV offsets for the refraction UVs (include our waves so it distorts the render target)
                                    float2 offset = wavesBump1 * _Shading_RefractionDistortion * _GrabTexture_TexelSize.xy;

                                    #if UNITY_SINGLE_PASS_STEREO
                                    i.uvgrab.xy = TransformStereoScreenSpaceTex(i.uvgrab.xy, i.uvgrab.w);
                                    #endif
                                    #ifdef UNITY_Z_0_FAR_FROM_CLIPSPACE
                                    i.uvgrab.xy = offset * UNITY_Z_0_FAR_FROM_CLIPSPACE(i.uvgrab.z) + i.uvgrab.xy;
                                    #else
                                    i.uvgrab.xy = offset * i.uvgrab.z + i.uvgrab.xy;
                                    #endif

                                    //initalize our refraction color vectors
                                    float cameraRefractionR = 0.0f;
                                    float cameraRefractionG = 0.0f;
                                    float cameraRefractionB = 0.0f;
                                    float4 cameraRefraction = float4(0, 0, 0, 1);

                                    //if chromatic abberation is enabled
                                    if (_Shading_ChromaticAbberation > 0)
                                    {
                                        //get our UV offsets defined by the user
                                        float4 uvOffset = _GrabTexture_TexelSize * _Shading_RefractionChannelShift;

                                        //calculate each UV coordinate for each color channel differently
                                        float4 uv_r = float4(i.uvgrab.x + uvOffset.x, i.uvgrab.y, i.uvgrab.z, i.uvgrab.w);
                                        float4 uv_g = float4(i.uvgrab.x, i.uvgrab.y + uvOffset.y, i.uvgrab.z, i.uvgrab.w);
                                        float4 uv_b = float4(i.uvgrab.x + uvOffset.z, i.uvgrab.y + uvOffset.z, i.uvgrab.z, i.uvgrab.w);

                                        //sample each channel seperately with their respective uv coordinates
                                        cameraRefractionR = tex2Dproj(_GrabTexture, UNITY_PROJ_COORD(uv_r)).r;
                                        cameraRefractionG = tex2Dproj(_GrabTexture, UNITY_PROJ_COORD(uv_g)).g;
                                        cameraRefractionB = tex2Dproj(_GrabTexture, UNITY_PROJ_COORD(uv_b)).b;

                                        //combine each channel into a final refraction color
                                        cameraRefraction = float4(cameraRefractionR, cameraRefractionG, cameraRefractionB, 1.0);
                                    }
                                    //if there is no chromatic abberation enabled
                                    else
                                    {
                                        //sample the camera render target like normal
                                        cameraRefraction = tex2Dproj(_GrabTexture, UNITY_PROJ_COORD(i.uvgrab));
                                    }

                                    //override the refraction color to be our camera refraction texture
                                    refractionColor = cameraRefraction;
                                }

                                float3 finalRefractionColor = refractionColor.rgb;

                                if (_Shading_DistortRefraction > 0)
                                {
                                    finalRefractionColor *= _Shading_RefractionColorShallow.rgb;
                                }

                                if (_Shading_EnableWaterFog > 0)
                                {
                                    finalRefractionColor *= _Shading_RefractionColorShallow.rgb;

                                    //get the camera depth buffer
                                    float cameraDepth = LinearEyeDepth(UNITY_SAMPLE_SCREENSPACE_TEXTURE(_CameraDepthTexture, i.screenPos.xy / i.screenPos.w).r) - i.screenPos.z;

                                    //compute the depth factors for the shallow and deep water fog
                                    float shallowDepthFactor = saturate(sqrt(cameraDepth) * _Shading_FogDepth / 2.0f);
                                    float deepDepthFactor = saturate(sqrt(cameraDepth) * _Shading_FogDepth);

                                    //calculate the color of the shallow water refraction color
                                    float3 shallowRefraction = lerp(refractionColor.rgb * _Shading_RefractionColorShallow, _Shading_RefractionColorDeep, shallowDepthFactor);

                                    //calculate the color of the deep water refraction color
                                    finalRefractionColor = lerp(refractionColor.rgb, shallowRefraction, deepDepthFactor);

                                    refractionColor.a = lerp(0.0f, _Shading_RefractionColorShallow.a, shallowDepthFactor);
                                    refractionColor.a = lerp(refractionColor.a, _Shading_RefractionColorDeep.a, deepDepthFactor);
                                }

                                //if fake subsurface specular is enabled
                                if (_Shading_EnableSSSSpecular > 0)
                                {
                                    //lerp the refraction color with the shallow water color to emulate the sub surface scattering
                                    finalRefractionColor = lerp(finalRefractionColor, _Shading_RefractionColorShallow, subsurfaceSpecular);

                                    refractionColor.a = lerp(refractionColor.a, _Shading_RefractionColorShallow.a, subsurfaceSpecular);
                                }

                                //initalize our final color vector
                                float4 color = float4(0, 0, 0, 0);

                                //combine our final refraction color with our final reflection color by the fresnel factor, and add our specular color (if it was defined)
                                color.rgb = lerp(finalRefractionColor.rgb, finalReflection.rgb, fresnel) + specularColor;

                                //if solid refraction IS NOT enabled
                                if (_Shading_SolidRefraction < 1)
                                {
                                    //lerp from the alpha of the refraction color to no transparency based on the fresnel 
                                    color.a = lerp(refractionColor.a, 1.0f, fresnel);
                                }
                                //if solid refraction IS enbled
                                else
                                {
                                    //no transparency
                                    color.a = 1.0f;
                                }

                                //if distort refraction is enabled
                                if (_Shading_DistortRefraction > 0)
                                {
                                    //solidfy the alpha since we already can see whats past it in the RT
                                    color.a = 1.0f;
                                }
                                else
                                {
                                    //since we don't have a refraction RT texture, we just have to fake it with fog
                                    color.a = refractionColor.a;
                                }

                                //if edge fading is enabled
                                if (_Shading_EnableEdgeFading > 0)
                                {
                                    //sample the camera depth buffer
                                    float cameraSceneZ = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.screenPos)));
                                    //float cameraSceneZ = LinearEyeDepth(UNITY_SAMPLE_SCREENSPACE_TEXTURE(_CameraDepthTexture, i.screenPos.xy / i.screenPos.w).r);

                                    //compute the fade factor
                                    float fade = saturate(_Shading_EdgeFade * (cameraSceneZ - i.screenPos.z));

                                    //multiply the final color transparency with the fade factor
                                    color.a *= fade;
                                }

                                //if unity fog is enabled
                                if (_Shading_EnableUnityFog > 0)
                                {
                                    //apply unity fog color
                                    UNITY_APPLY_FOG(i.fogCoord, color);
                                }

                                return color;
                            }
                        ENDCG
                        }
                                }
                                    SubShader
                            {
                                Blend DstColor Zero

                                Pass
                                {
                                    Name "BASE"

                                    SetTexture[_MainTex]
                                    {
                                        combine texture
                                    }
                                }
                            }
}
