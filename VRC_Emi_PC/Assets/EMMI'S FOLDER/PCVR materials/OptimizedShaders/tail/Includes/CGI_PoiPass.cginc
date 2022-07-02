#ifndef POI_PASS
#define POI_PASS
#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "UnityPBSLighting.cginc"
#include "AutoLight.cginc"
#include "UnityShaderVariables.cginc"
#ifdef POI_META_PASS
	#include "UnityMetaPass.cginc"
#endif
#include "CGI_PoiMacros.cginc"
#include "CGI_PoiDefines.cginc"
#include "CGI_FunctionsArtistic.cginc"
#include "CGI_Poicludes.cginc"
#include "CGI_PoiHelpers.cginc"
#include "CGI_PoiBlending.cginc"
#ifdef _SUNDISK_NONE
	#include "CGI_PoiRandom.cginc"
#endif
#ifdef _REQUIRE_UV2
	#include "CGI_PoiMirror.cginc"
#endif
#include "CGI_PoiPenetration.cginc"
#include "CGI_PoiVertexManipulations.cginc"
#include "CGI_PoiSpawnInVert.cginc"
#include "CGI_PoiV2F.cginc"
#ifdef BLOOM_LOW
	#include "CGI_PoiBulge.cginc"
#endif
#include "CGI_PoiVert.cginc"
#ifdef TESSELATION
	#include "CGI_PoiTessellation.cginc"
#endif
#include "CGI_PoiDithering.cginc"
#ifdef _PARALLAXMAP
	#include "CGI_PoiParallax.cginc"
#endif
#ifdef COLOR_GRADING_LOG_VIEW
	#include "CGI_PoiAudioLink.cginc"
#endif
#ifdef USER_LUT
	#include "CGI_PoiUVDistortion.cginc"
#endif
#ifdef VIGNETTE
	#include "CGI_PoiRGBMask.cginc"
#endif
#include "CGI_PoiData.cginc"
#ifdef _SPECULARHIGHLIGHTS_OFF
	#include "CGI_PoiBlackLight.cginc"
#endif
#include "CGI_PoiSpawnInFrag.cginc"
#ifdef WIREFRAME
	#include "CGI_PoiWireframe.cginc"
#endif
#ifdef DISTORT
	#include "CGI_PoiDissolve.cginc"
#endif
#ifdef DEPTH_OF_FIELD
	#include "CGI_PoiHologram.cginc"
#endif
#ifdef BLOOM_LENS_DIRT
	#include "CGI_PoiIridescence.cginc"
#endif
#ifdef FUR
#endif
#ifdef VIGNETTE_MASKED
	#include "CGI_PoiLighting.cginc"
#endif
#include "CGI_PoiMainTex.cginc"
#ifdef TONEMAPPING_CUSTOM
	#include "CGI_PoiPathing.cginc"
#endif
#ifdef GEOM_TYPE_BRANCH
	#include "CGI_PoiDecal.cginc"
#endif
#ifdef CHROMATIC_ABERRATION
	#include "CGI_PoiVoronoi.cginc"
#endif
#ifdef _DETAIL_MULX2
	#include "CGI_PoiPanosphere.cginc"
#endif
#ifdef EFFECT_BUMP
	#include "CGI_PoiMSDF.cginc"
#endif
#ifdef GRAIN
	#include "CGI_PoiDepthColor.cginc"
#endif
#ifdef _SUNDISK_HIGH_QUALITY
	#include "CGI_PoiFlipbook.cginc"
#endif
#ifdef _GLOSSYREFLECTIONS_OFF
	#include "CGI_PoiRimLighting.cginc"
#endif
#ifdef _MAPPING_6_FRAMES_LAYOUT
	#include "CGI_PoiEnvironmentalRimLighting.cginc"
#endif
#ifdef VIGNETTE_CLASSIC
	#include "CGI_PoiBRDF.cginc"
#endif
#ifdef _METALLICGLOSSMAP
	#include "CGI_PoiMetal.cginc"
#endif
#ifdef _COLORADDSUBDIFF_ON
	#include "CGI_PoiMatcap.cginc"
#endif
#ifdef _SPECGLOSSMAP
	#include "CGI_PoiSpecular.cginc"
#endif
#ifdef BLOOM
	#include "CGI_PoiVideo.cginc"
#endif
#ifdef _TERRAIN_NORMAL_MAP
	#include "CGI_PoiSubsurfaceScattering.cginc"
#endif
#include "CGI_PoiBlending.cginc"
#include "CGI_PoiGrab.cginc"
#ifdef _SUNDISK_SIMPLE
	#include "CGI_PoiGlitter.cginc"
#endif
#ifdef _EMISSION
	#include "CGI_PoiEmission.cginc"
#endif
#ifdef _COLORCOLOR_ON
	#include "CGI_PoiClearCoat.cginc"
#endif
#include "CGI_PoiAlphaToCoverage.cginc"
#ifdef _COLOROVERLAY_ON
	#include "CGI_PoiDebug.cginc"
#endif
#include "CGI_PoiFrag.cginc"
#endif
