using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

public class MatricesInitializer
{
    private const int  SCAN_THREAD_GROUP_SIZE = 64; // TODO: Move to base class

    private readonly ComputeShader _computeShader;
    private readonly int _numberOfInstances;
    
    private ComputeBuffer _positionsBuffer;
    private ComputeBuffer _rotationsBuffer;
    private ComputeBuffer _scalesBuffer;

    public MatricesInitializer(ComputeShader computeShader, int numberOfInstances)
    {
        _computeShader = computeShader;
        _numberOfInstances = numberOfInstances;

        InitializeMatricesBuffers();
        InitializeTransformBuffers();
        InitializeComputeShader();
        // InitializeMaterialProperties();
    }

    public void Initialize(List<Vector3> positions, List<Vector3> rotations, List<Vector3> scales)
    {
        _positionsBuffer.SetData(positions);
        _rotationsBuffer.SetData(rotations);
        _scalesBuffer.SetData(scales);
    }

    public void Dispatch()
    {
        var groupX = Mathf.Max(1, _numberOfInstances / (2 * SCAN_THREAD_GROUP_SIZE));
        _computeShader.Dispatch(ShaderKernels.MatricesInitializer, groupX, 1, 1);
        
        _positionsBuffer?.Release();
        _rotationsBuffer?.Release();
        _scalesBuffer?.Release();
    }
    
    // TODO: #EDITOR
    public void LogInstanceDrawMatrices(string prefix = "")
    {
        var matrix1 = new Indirect2x2Matrix[_numberOfInstances];
        var matrix2 = new Indirect2x2Matrix[_numberOfInstances];
        var matrix3 = new Indirect2x2Matrix[_numberOfInstances];
        
        ShaderBuffers.MatrixRows01.GetData(matrix1);
        ShaderBuffers.MatrixRows23.GetData(matrix2);
        ShaderBuffers.MatrixRows45.GetData(matrix3);
        
        var stringBuilder = new StringBuilder();
        if (!string.IsNullOrEmpty(prefix))
        {
            stringBuilder.AppendLine(prefix);
        }
        
        for (var i = 0; i < matrix1.Length; i++)
        {
            stringBuilder.AppendLine(
                i + "\n" 
                  + matrix1[i].FirstRow + "\n"
                  + matrix1[i].SecondRow + "\n"
                  + matrix2[i].FirstRow + "\n"
                  + "\n\n"
                  + matrix2[i].SecondRow + "\n"
                  + matrix3[i].FirstRow + "\n"
                  + matrix3[i].SecondRow + "\n"
                  + "\n"
            );
        }
    
        Debug.Log(stringBuilder.ToString());
    }

    private void InitializeMatricesBuffers()
    {
        ShaderBuffers.MatrixRows01 = new ComputeBuffer(_numberOfInstances, Indirect2x2Matrix.Size, ComputeBufferType.IndirectArguments);
        ShaderBuffers.MatrixRows23 = new ComputeBuffer(_numberOfInstances, Indirect2x2Matrix.Size, ComputeBufferType.IndirectArguments);
        ShaderBuffers.MatrixRows45 = new ComputeBuffer(_numberOfInstances, Indirect2x2Matrix.Size, ComputeBufferType.IndirectArguments);
    }

    private void InitializeTransformBuffers()
    {
        _positionsBuffer = new ComputeBuffer(_numberOfInstances, Marshal.SizeOf(typeof(Vector3)), ComputeBufferType.Default);
        _scalesBuffer    = new ComputeBuffer(_numberOfInstances, Marshal.SizeOf(typeof(Vector3)), ComputeBufferType.Default);
        _rotationsBuffer = new ComputeBuffer(_numberOfInstances, Marshal.SizeOf(typeof(Vector3)), ComputeBufferType.Default);
    }

    private void InitializeComputeShader()
    {
        _computeShader.SetBuffer(ShaderKernels.MatricesInitializer, ShaderProperties.Positions,            _positionsBuffer);
        _computeShader.SetBuffer(ShaderKernels.MatricesInitializer, ShaderProperties.Rotations,            _rotationsBuffer);
        _computeShader.SetBuffer(ShaderKernels.MatricesInitializer, ShaderProperties.Scales,               _scalesBuffer);
        _computeShader.SetBuffer(ShaderKernels.MatricesInitializer, ShaderProperties.MatrixRows01, ShaderBuffers.MatrixRows01);
        _computeShader.SetBuffer(ShaderKernels.MatricesInitializer, ShaderProperties.MatrixRows23, ShaderBuffers.MatrixRows23);
        _computeShader.SetBuffer(ShaderKernels.MatricesInitializer, ShaderProperties.MatrixRows45, ShaderBuffers.MatrixRows45);
    }

    // private void InitializeMaterialProperties()
    // {
    //     _meshProperties.Lod0PropertyBlock.SetBuffer(ShaderProperties.MatrixRows01, ShaderBuffers.CulledMatrixRows01);
    //     _meshProperties.Lod1PropertyBlock.SetBuffer(ShaderProperties.MatrixRows01, ShaderBuffers.CulledMatrixRows01);
    //     _meshProperties.Lod2PropertyBlock.SetBuffer(ShaderProperties.MatrixRows01, ShaderBuffers.CulledMatrixRows01);
    //         
    //     _meshProperties.Lod0PropertyBlock.SetBuffer(ShaderProperties.MatrixRows23, ShaderBuffers.CulledMatrixRows23);
    //     _meshProperties.Lod1PropertyBlock.SetBuffer(ShaderProperties.MatrixRows23, ShaderBuffers.CulledMatrixRows23);
    //     _meshProperties.Lod2PropertyBlock.SetBuffer(ShaderProperties.MatrixRows23, ShaderBuffers.CulledMatrixRows23);
    //         
    //     _meshProperties.Lod0PropertyBlock.SetBuffer(ShaderProperties.MatrixRows45, ShaderBuffers.CulledMatrixRows45);
    //     _meshProperties.Lod1PropertyBlock.SetBuffer(ShaderProperties.MatrixRows45, ShaderBuffers.CulledMatrixRows45);
    //     _meshProperties.Lod2PropertyBlock.SetBuffer(ShaderProperties.MatrixRows45, ShaderBuffers.CulledMatrixRows45);
    //     
    //     _meshProperties.ShadowLod0PropertyBlock.SetBuffer(ShaderProperties.MatrixRows01, ShaderBuffers.ShadowsCulledMatrixRows01);
    //     _meshProperties.ShadowLod1PropertyBlock.SetBuffer(ShaderProperties.MatrixRows01, ShaderBuffers.ShadowsCulledMatrixRows01);
    //     _meshProperties.ShadowLod2PropertyBlock.SetBuffer(ShaderProperties.MatrixRows01, ShaderBuffers.ShadowsCulledMatrixRows01);
    //         
    //     _meshProperties.ShadowLod0PropertyBlock.SetBuffer(ShaderProperties.MatrixRows23, ShaderBuffers.ShadowsCulledMatrixRows23);
    //     _meshProperties.ShadowLod1PropertyBlock.SetBuffer(ShaderProperties.MatrixRows23, ShaderBuffers.ShadowsCulledMatrixRows23);
    //     _meshProperties.ShadowLod2PropertyBlock.SetBuffer(ShaderProperties.MatrixRows23, ShaderBuffers.ShadowsCulledMatrixRows23);
    //         
    //     _meshProperties.ShadowLod0PropertyBlock.SetBuffer(ShaderProperties.MatrixRows45, ShaderBuffers.ShadowsCulledMatrixRows45);
    //     _meshProperties.ShadowLod1PropertyBlock.SetBuffer(ShaderProperties.MatrixRows45, ShaderBuffers.ShadowsCulledMatrixRows45);
    //     _meshProperties.ShadowLod2PropertyBlock.SetBuffer(ShaderProperties.MatrixRows45, ShaderBuffers.ShadowsCulledMatrixRows45);
    // }
}