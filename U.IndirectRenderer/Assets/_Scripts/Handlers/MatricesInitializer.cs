using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using IndirectRendering;

public class MatricesInitializer
{
    private const int SCAN_THREAD_GROUP_SIZE = 64; // TODO: Move to base class

    private readonly ComputeShader _computeShader;
    private readonly int _numberOfInstances;

    private readonly RendererDataContext _context;

    public MatricesInitializer(ComputeShader computeShader, int numberOfInstances, RendererDataContext context)
    {
        _computeShader = computeShader;
        _numberOfInstances = numberOfInstances;
        _context = context;

        InitializeTransformBuffers();
        InitializeComputeShader();
    }

    public void Initialize(List<Vector3> positions, List<Vector3> rotations, List<Vector3> scales)
    {
        _context.Transform.PositionsBuffer.SetData(positions);
        _context.Transform.RotationsBuffer.SetData(rotations);
        _context.Transform.ScalesBuffer.SetData(scales);
    }

    public void Dispatch()
    {
        var groupX = Mathf.Max(1, _numberOfInstances / (2 * SCAN_THREAD_GROUP_SIZE));
        _computeShader.Dispatch(ShaderKernels.MatricesInitializer, groupX, 1, 1);

        // _positionsBuffer?.Release();
        // _rotationsBuffer?.Release();
        // _scalesBuffer?.Release();
    }

    // TODO: #EDITOR
    public void LogInstanceDrawMatrices(string prefix = "")
    {
        var matrix1 = new Indirect2x2Matrix[_numberOfInstances];
        var matrix2 = new Indirect2x2Matrix[_numberOfInstances];
        var matrix3 = new Indirect2x2Matrix[_numberOfInstances];

        _context.Transform.MatrixRows01.GetData(matrix1);
        _context.Transform.MatrixRows23.GetData(matrix2);
        _context.Transform.MatrixRows45.GetData(matrix3);

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

    private void InitializeTransformBuffers()
    {
    }

    private void InitializeComputeShader()
    {
        _computeShader.SetBuffer(ShaderKernels.MatricesInitializer, ShaderProperties.Positions, _context.Transform.PositionsBuffer);
        _computeShader.SetBuffer(ShaderKernels.MatricesInitializer, ShaderProperties.Rotations, _context.Transform.RotationsBuffer);
        _computeShader.SetBuffer(ShaderKernels.MatricesInitializer, ShaderProperties.Scales, _context.Transform.ScalesBuffer);
        _computeShader.SetBuffer(ShaderKernels.MatricesInitializer, ShaderProperties.MatrixRows01, _context.Transform.MatrixRows01);
        _computeShader.SetBuffer(ShaderKernels.MatricesInitializer, ShaderProperties.MatrixRows23, _context.Transform.MatrixRows23);
        _computeShader.SetBuffer(ShaderKernels.MatricesInitializer, ShaderProperties.MatrixRows45, _context.Transform.MatrixRows45);
    }
}