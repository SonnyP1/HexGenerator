using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Palmmedia.ReportGenerator.Core;
using System.Threading;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Linq;

public enum Terrain
{
    None,
    River,
    Grass,
    Clay,
}

public class Hex : MonoBehaviour
{
    public GameObject m_hexPrefab;
    public Terrain m_terrainType;
    public Material[] m_materials;
    public MeshRenderer m_meshRender;

    [Header("Effects")]
    public GameObject m_selectedEffect;
    private bool _isSelected;
    public bool IsSelected
    {
        set {

            _isSelected = value;
            m_selectedEffect.SetActive(_isSelected);
        }
        get { return _isSelected; }
    }

    public float m_radius = 5f;

    public Terrain Terrain
    {
        set
        {
            m_terrainType = value;
            m_meshRender.material = m_materials[(int)m_terrainType];
        }
    }

    private Vector3[] points;
    private Vector3[] midpoints;
    private List<Vector3> spawnpoints = new List<Vector3>();

    private void Start()
    {
        InitVariables();
    }

    private void InitVariables()
    {
        points = CalculatePoints();
        midpoints = CalculateMidpoints();
        spawnpoints.AddRange(CalculateNewTilePos());
    }

    //Calculation
    public Vector3[] CalculatePoints()
    {
        Vector3[] points = new Vector3[6];
        for (int i = 0; i < points.Length; i++)
        {
            points[i] = transform.position + (DirFromAngle(30f + (60f * i)) * m_radius);
        }
        return points;
    }
    public Vector3[] CalculateMidpoints()
    {
        Vector3[] midpoints = new Vector3[6];
        for (int i = 0; i < midpoints.Length; i++)
        {
            if (i == points.Length - 1)
            {
                midpoints[i] = Vector3.Lerp(points[i], points[0], 0.5f);
                continue;
            }
            midpoints[i] = Vector3.Lerp(points[i], points[i + 1], 0.5f);
        }

        return midpoints;
    }

    public Vector3[] CalculateNewTilePos()
    {
        Vector3[] spawnpoints = new Vector3[6];
        for (int i = 0; i < spawnpoints.Length; i++)
        {
            float distScalar = Vector3.Distance(transform.position, midpoints[i]);

            Vector3 dir = midpoints[i] - transform.position;
            dir.y = 0;
            dir = dir.normalized;

            spawnpoints[i] = midpoints[i] + (dir * distScalar);
        }

        return spawnpoints;
    }
    
    public Terrain PickRandomTerrain()
    {
        int num = Random.Range(1,4);
        Terrain terrain = new Terrain();
        terrain = (Terrain)num;

        return terrain;
    }

    //Generation
    public Hex[] GenerateTiles()
    {
        List<Hex> hexes = new List<Hex>();
        for(int i = 0;i < spawnpoints.Count;i++)
        {
            bool found = false;
            Collider[] cols = Physics.OverlapSphere(spawnpoints[i],0.5f);
            foreach(Collider col in cols)
            {
                if(col.GetComponent<Hex>())
                {
                    found = true;
                    break;
                }
            }

            if(found) { continue; }

            GameObject obj = Instantiate(m_hexPrefab);
            obj.transform.position = spawnpoints[i];
            Hex newHex = obj.GetComponent<Hex>();
            newHex.IsSelected = false;



            //Pick Terrain
            float num = Random.Range(0f, 1f);
            if(num < 0.5f)
            {
                newHex.Terrain = m_terrainType;
            }
            else
            {
                newHex.Terrain = PickRandomTerrain();
            }

            hexes.Add(newHex);
        }

        spawnpoints.Clear();

        return hexes.ToArray();
    }
    public Hex[] GenerateTiles(Vector3[] points)
    {
        List<Hex> hexes = new List<Hex>();
        for (int i = 0; i < points.Length; i++)
        {
            bool found = false;
            Collider[] cols = Physics.OverlapSphere(points[i], 0.5f);
            foreach (Collider col in cols)
            {
                if (col.GetComponent<Hex>())
                {
                    found = true;
                    break;
                }
            }

            if (found) { continue; }

            GameObject obj = Instantiate(m_hexPrefab);
            obj.transform.position = points[i];
            Hex newHex = obj.GetComponent<Hex>();



            //Pick Terrain
            float num = Random.Range(0f, 1f);
            if (num < 0.5f)
            {
                newHex.Terrain = m_terrainType;
            }
            else
            {
                newHex.Terrain = PickRandomTerrain();
            }

            hexes.Add(newHex);
        }

        spawnpoints.Clear();

        return hexes.ToArray();
    }

    //Math
    public Vector3 DirFromAngle(float angle)
    {
        return new Vector3(Mathf.Sin(angle * Mathf.Deg2Rad),0,Mathf.Cos(angle * Mathf.Deg2Rad));
    }



}

#region Editor
#if UNITY_EDITOR
[CustomEditor(typeof(Hex))]
public class HexEditor : Editor
{
    Hex m_hex;
    Vector3[] generate = new Vector3[6];

    public void OnEnable()
    {
        m_hex = (Hex)target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if(GUILayout.Button("Generate"))
        {
            m_hex.GenerateTiles(generate);
        }
    }

    private void OnSceneGUI()
    {
        if(Application.isPlaying) { return; }
        //Correct Material of Hex
        m_hex.m_meshRender.material = m_hex.m_materials[(int)m_hex.m_terrainType];

        //Correct Scale of Hex
        m_hex.transform.localScale = Vector3.one * m_hex.m_radius;

        //Draw Circle
        Handles.DrawWireArc(m_hex.transform.position,Vector3.up,Vector3.right,360f,m_hex.m_radius);

        //Draw Center
        Handles.DrawWireArc(m_hex.transform.position,Vector3.up,Vector3.right,360f,m_hex.m_radius/15f);

        //Create All Points
        Vector3[] points = new Vector3[6];
        for(int i = 0; i < points.Length;i++)
        {
            points[i] =  m_hex.transform.position + (m_hex.DirFromAngle(30f +(60f*i)) * m_hex.m_radius);
        }

        //Draws All Points
        Handles.color = Color.magenta;
        for(int i = 0; i < points.Length;i++)
        {
            if (points[i] == Vector3.zero) { continue; }
            Handles.DrawWireArc(points[i],Vector3.up,Vector3.right,360f,m_hex.m_radius/15f);
        }

        //Draws All Lines
        Handles.color = Color.red;
        for(int i = 0; i< points.Length;i++)
        {
            if(i == points.Length-1) {
                Handles.DrawLine(points[i], points[0]);
                continue; }
            Handles.DrawLine(points[i], points[i+1]);
        }

        //Create All Midpoints
        Vector3[] midpoints = new Vector3[6];
        for(int i =0; i< midpoints.Length;i++)
        {
            if(i == points.Length -1){
                midpoints[i] = Vector3.Lerp(points[i], points[0], 0.5f);
                continue; }
            midpoints[i] = Vector3.Lerp(points[i], points[i+1], 0.5f);
        }


        //Draw All Midpoints
        Handles.color = Color.cyan;
        for(int i=0; i<midpoints.Length;i++)
        {
            Handles.DrawWireArc(midpoints[i],Vector3.up,Vector3.right,360f,m_hex.m_radius/15f);
        }


        //Create New Points to Generate other Hex
        for(int i = 0; i < generate.Length;i++ )
        {
            float distScalar = Vector3.Distance(m_hex.transform.position, midpoints[i]);

            Vector3 dir = midpoints[i] - m_hex.transform.position;
            dir.y = 0;
            dir = dir.normalized;

            generate[i] = midpoints[i] + (dir * distScalar);
        }

        //Draw New Points to Generate other Hex
        Handles.color = Color.yellow;
        for(int i = 0; i< generate.Length;i++)
        {
            Handles.DrawWireArc(generate[i],Vector3.up,Vector3.right,360f,m_hex.m_radius/15f);
        }
    }
}

#endif
#endregion 