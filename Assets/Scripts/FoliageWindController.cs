using UnityEngine;
using UnityEngine.Rendering;

[ExecuteAlways]
public class FoliageWindController : MonoBehaviour
{
    [Header("Wind Settings")]
    [Range(0, 2)] public float windStrength = 0.5f;
    [Range(0, 5)] public float windSpeed = 1.0f;
    [Range(0, 2)] public float windFrequency = 0.5f;
    public Vector3 windDirection = Vector3.right;

    [Header("Wind Zone Integration")]
    public bool useWindZone = true;
    [Range(0, 2)] public float windZoneMultiplier = 1.0f;

    private MaterialPropertyBlock m_propBlock;
    private Renderer m_renderer;
    private WindZone m_windZone;

    // Shader property IDs
    private static readonly int WindStrengthID = Shader.PropertyToID("_WindStrength");
    private static readonly int WindSpeedID = Shader.PropertyToID("_WindSpeed");
    private static readonly int WindFrequencyID = Shader.PropertyToID("_WindFrequency");
    private static readonly int WindDirectionID = Shader.PropertyToID("_WindDirection");
    private static readonly int WindZoneInfluenceID = Shader.PropertyToID("_WindZoneInfluence");

    void Start()
    {
        m_renderer = GetComponent<Renderer>();
        m_propBlock = new MaterialPropertyBlock();

        if (useWindZone)
        {
            m_windZone = FindFirstObjectByType<WindZone>();
        }
    }

    void Update()
    {
        if (m_renderer == null) return;

        m_renderer.GetPropertyBlock(m_propBlock);

        // Интеграция с Wind Zone
        float finalWindStrength = windStrength;
        Vector3 finalWindDirection = windDirection.normalized;
        float windZoneInfluence = 1.0f;

        if (useWindZone && m_windZone != null)
        {
            finalWindStrength *= m_windZone.windMain * windZoneMultiplier;
            finalWindDirection = m_windZone.transform.forward;
            windZoneInfluence = Mathf.Max(0.5f, m_windZone.windMain);
        }

        // Устанавливаем параметры
        m_propBlock.SetFloat(WindStrengthID, finalWindStrength);
        m_propBlock.SetFloat(WindSpeedID, windSpeed);
        m_propBlock.SetFloat(WindFrequencyID, windFrequency);
        m_propBlock.SetVector(WindDirectionID, new Vector4(
            finalWindDirection.x,
            finalWindDirection.y,
            finalWindDirection.z,
            0
        ));
        m_propBlock.SetFloat(WindZoneInfluenceID, windZoneInfluence);

        m_renderer.SetPropertyBlock(m_propBlock);
    }

    void OnValidate()
    {
        if (Application.isPlaying)
        {
            Update();
        }
        else
        {
            // Обновляем в редакторе
            EditorUpdate();
        }
    }

    void EditorUpdate()
    {
        if (m_renderer == null)
            m_renderer = GetComponent<Renderer>();

        if (m_renderer != null)
        {
            if (m_propBlock == null)
                m_propBlock = new MaterialPropertyBlock();

            Update();
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        // Визуализация направления ветра
        UnityEditor.Handles.color = new Color(0, 1, 1, 0.5f);
        Vector3 position = transform.position;
        Vector3 direction = windDirection.normalized * 2;

        UnityEditor.Handles.ArrowHandleCap(0,
            position,
            Quaternion.LookRotation(direction),
            2.0f,
            EventType.Repaint);

        // Отображение параметров
        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.cyan;
        style.fontSize = 10;

        string info = $"Wind Settings:\n" +
                     $"Strength: {windStrength:F2}\n" +
                     $"Speed: {windSpeed:F2}\n" +
                     $"Freq: {windFrequency:F2}";

        UnityEditor.Handles.Label(position + Vector3.up * 2, info, style);
    }
#endif
}