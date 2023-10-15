using UnityEngine;

public class IndirectRendererComponent : MonoBehaviour
{
    [SerializeField] 
    private IndirectRendererConfig _config;
    
    [Space] 
    [SerializeField] 
    private IndirectRendererSettings _settings;
    
    [Space] 
    [SerializeField] 
    private InstanceProperties[] _instances;

    private IndirectRenderer _renderer;

    private void Start()
    {
        foreach (var instance in _instances)
        {
            for (var i = 0; i < 128; i++)
            {
                for (var j = 0; j < 128; j++)
                {
                    var data = new InstanceProperties.TransformDto
                    {
                        Position = new Vector3
                        {
                            x = i,
                            y = .5f,
                            z = j
                        },
                        
                        Rotation = new Vector3
                        {
                            x = 0f,
                            y = 0f,
                            z = 0f
                        },
                        
                        Scale = new Vector3
                        {
                            x = .75f,
                            y = .75f,
                            z = .75f
                        }
                    };

                    data.Position += instance.Offset.Position;
                    data.Rotation += instance.Offset.Rotation;
                    data.Scale += instance.Offset.Scale;
                    
                    instance.Transforms.Add(data);
                }
            }
        }

        _renderer = new IndirectRenderer(_instances, _config, _settings);
    }

    private void OnDestroy()
    {
        _renderer.Dispose();
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            return;
        }
        
        _renderer.DrawGizmos();
    }
}