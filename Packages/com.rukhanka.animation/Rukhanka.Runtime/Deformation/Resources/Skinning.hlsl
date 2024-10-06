#ifndef SKINNING_HLSL_
#define SKINNING_HLSL_

/////////////////////////////////////////////////////////////////////////////////

#define SKINNING_KERNEL_NUM_THREAD_GROUPS_X 1024

/////////////////////////////////////////////////////////////////////////////////

ByteAddressBuffer framePerVertexWorkload;
//  SourceSkinnedMeshVertex
ByteAddressBuffer inputMeshVertexData;
//  BoneInfluence
ByteAddressBuffer inputBoneInfluences;
//  DeformedVertex
ByteAddressBuffer inputBlendShapes;

StructuredBuffer<float3x4> frameSkinMatrices;
StructuredBuffer<float> frameBlendShapeWeights;
RWStructuredBuffer<DeformedVertex> outDeformedVertices;
uint totalSkinnedVerticesCount;
uint voidMeshVertexCount;

/////////////////////////////////////////////////////////////////////////////////

DeformedVertex ApplyBlendShapes(DeformedVertex v, uint meshVertexIndex, MeshFrameDeformationDescription mfd)
{
    if (mfd.baseBlendShapeWeightIndex < 0)
        return v;

    for (int i = 0; i < mfd.meshBlendShapesCount; ++i)
    {
        float blendShapeWeight = frameBlendShapeWeights[mfd.baseBlendShapeWeightIndex + i];
        if (blendShapeWeight == 0)
            continue;

        DeformedVertex blendShapeDelta = DeformedVertex::ReadFromRawBuffer
        (
            inputBlendShapes,
            mfd.baseInputMeshBlendShapeIndex + meshVertexIndex + i * mfd.meshVerticesCount
        );
        blendShapeDelta.Scale(blendShapeWeight * 0.01f);

        v.position += blendShapeDelta.position;
        v.normal += blendShapeDelta.normal;
        v.tangent += blendShapeDelta.tangent;
    }

    return v;
}

/////////////////////////////////////////////////////////////////////////////////

DeformedVertex ApplySkinMatrices
(
    DeformedVertex v,
    uint vertexBoneWeightsOffset,
    uint vertexBoneWeightsCount,
    MeshFrameDeformationDescription mfd
)
{
    if (mfd.baseSkinMatrixIndex < 0)
        return v;

    //  Skinning loop
    DeformedVertex rv = (DeformedVertex)0;
    for (uint i = 0; i < vertexBoneWeightsCount; ++i)
    {
        uint boneInfluenceIndex = i + vertexBoneWeightsOffset;
        BoneInfluence bi = BoneInfluence::ReadFromRawBuffer(inputBoneInfluences, boneInfluenceIndex);
        float3x4 skinMatrix = frameSkinMatrices[bi.boneIndex + mfd.baseSkinMatrixIndex];

        rv.position += mul(skinMatrix, float4(v.position, 1)) * bi.weight;
        rv.normal += mul(skinMatrix, float4(v.normal, 0)) * bi.weight;
        rv.tangent += mul(skinMatrix, float4(v.tangent, 0)) * bi.weight;
    }
    
    return rv;
}

/////////////////////////////////////////////////////////////////////////////////

[numthreads(SKINNING_KERNEL_NUM_THREAD_GROUPS_X, 1, 1)]
void Skinning(uint tid: SV_DispatchThreadID)
{
    if (tid >= totalSkinnedVerticesCount)
    {
        if (tid < totalSkinnedVerticesCount + voidMeshVertexCount)
            outDeformedVertices[tid] = (DeformedVertex)0;
        return;
    }

    uint frameDeformedMeshIndex = framePerVertexWorkload.Load(tid * 4);
    MeshFrameDeformationDescription mfd = frameDeformedMeshes[frameDeformedMeshIndex];
    uint meshVertexIndex = tid - mfd.baseOutVertexIndex;
    uint absoluteInputMeshVertexIndex = meshVertexIndex + mfd.baseInputMeshVertexIndex;
    SourceSkinnedMeshVertex smv = SourceSkinnedMeshVertex::ReadFromRawBuffer(inputMeshVertexData, absoluteInputMeshVertexIndex);
    uint vertexBoneWeightsOffset = GetBoneWeightsOffsetFromPackedUINT(smv.boneWeightsOffsetAndCount);
    uint vertexBoneWeightsCount =  GetBoneWeightsCountFromPackedUINT(smv.boneWeightsOffsetAndCount);

    DeformedVertex rv = (DeformedVertex)0;
    rv.position = smv.position;
    rv.tangent = smv.tangent;
    rv.normal = smv.normal;

    rv = ApplySkinMatrices(rv, vertexBoneWeightsOffset, vertexBoneWeightsCount, mfd);
    rv = ApplyBlendShapes(rv, meshVertexIndex, mfd);

    outDeformedVertices[tid] = rv;
    
}

#endif // SKINNING_HLSL_
