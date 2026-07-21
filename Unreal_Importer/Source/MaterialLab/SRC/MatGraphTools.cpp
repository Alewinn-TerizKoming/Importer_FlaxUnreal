#include "MatGraphTools.h"

#include "Engine/Core/Log.h"
#include "Engine/Content/Assets/Material.h"

#include "Engine/Visject/ShaderGraph.h"
#include "Engine/Serialization/MemoryReadStream.h"
#include "Engine/Serialization/MemoryWriteStream.h"

#include "Engine/Content/Storage/ContentStorageManager.h"

// Test graph type
class MaterialGraph : public ShaderGraph<>
{
};


void MatGraphTools::SetColor(Material* material, Color color)
{
    if (!material)
    {
        LOG(Error, "Material is null");
        return;
    }

    ModifyMaterialGraph(material, color);
}

void MatGraphTools::ModifyMaterialGraph(Material* material, Color color)
{
    BytesContainer data = material->LoadSurface(true);

    LOG(Warning, "Original surface size: {0}", data.Length());

    MemoryReadStream readStream(data.Get(), data.Length());

    MaterialGraph graph;

    if (graph.Load(&readStream, true))
    {
        LOG(Error, "Cannot load material graph");
        return;
    }

    LOG(Warning, "Nodes before: {0}", graph.Nodes.Count());

    // Create parameter
    auto& param = graph.Parameters.AddOne();

    param.Identifier = Guid::New();
    param.Name = TEXT("BaseColor");
    param.Type = VariantType::Color;
    param.Value = color;
    param.IsPublic = true;

    LOG(Warning, "Added parameter ID={0}", param.Identifier);

    // Create Parameter Get node
    auto& paramNode = graph.Nodes.AddOne();
    paramNode.ID = graph.Nodes.Count();

    // Parameters group = 6
    // Get Parameter node = 1
    paramNode.Type = GRAPH_NODE_MAKE_TYPE(6, 1);

    paramNode.Values.Resize(1);
    paramNode.Values[0] = param.Identifier;

    // Output box (GetBox(1) in C#)
    paramNode.Boxes.Resize(2);
    paramNode.Boxes[0] = ShaderGraphBox(&paramNode, 0, VariantType::Color);
    paramNode.Boxes[1] = ShaderGraphBox(&paramNode, 1, VariantType::Color);

    LOG(Warning, "Added parameter node ID={0} Type={1}", paramNode.ID, paramNode.Type);


    // Create connection Parameter -> Root
    auto& rootNode = graph.Nodes[0];

    auto& rootInput = rootNode.Boxes[1];
    auto& parameterOutput = paramNode.Boxes[0];

    parameterOutput.Connections.Add(&rootInput);
    rootInput.Connections.Add(&parameterOutput);

    LOG(Warning, "Connected parameter node to root");


    // Serialize
    MemoryWriteStream writeStream(512);

    if (graph.Save(&writeStream, true))
    {
        LOG(Error, "Cannot save material graph");
        return;
    }

    BytesContainer newData;
    newData.Copy(Span<byte>((byte*)writeStream.GetHandle(), writeStream.GetPosition()));

    LOG(Warning, "New surface size: {0}", newData.Length());

    MaterialInfo info = material->GetInfo();
    info.ShadingModel = MaterialShadingModel::Lit;

    if (material->SaveSurface(newData, info))
    {
        LOG(Error, "SaveSurface failed");
        return;
    }

    //
    // IMPORTANT:
    // Keep this order.
    readStream.Flush();
    writeStream.Flush();

    newData.Release();
    data.Release();

    // Rechargement
    material->Reload();
    material->WaitForLoaded();

    // Sauvegarde finale
    material->Save(material->GetPath());

}