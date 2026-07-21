// Alewinn

#pragma once

#include "Engine/Scripting/Script.h"
#include "Engine/Core/Math/Vector4.h"

class Material;
struct Color;

/// <summary>
/// MatGraphTools Function Library
/// </summary>
API_CLASS(Static) class MATERIALLAB_API MatGraphTools 
{
    DECLARE_SCRIPTING_TYPE_MINIMAL(MatGraphTools);
public:

    API_FUNCTION() static void SetColor(Material* material, Color color);
    API_FUNCTION() static void ModifyMaterialGraph(Material* material, Color color);
};
