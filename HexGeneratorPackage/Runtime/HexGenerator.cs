using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEditor;
using UnityEngine;

public class HexGenerator : MonoBehaviour
{
    public Hex m_hex;
    public float m_tilecount = 5f;
    public Hex[] m_hexes;

    private Vector3[] points = new Vector3[6];
    public Vector3 DirFromAngle(float angle)
    {
        return new Vector3(Mathf.Sin(angle * Mathf.Deg2Rad), 0, Mathf.Cos(angle * Mathf.Deg2Rad));
    }

    //Initialize
    private void InitVariables()
    {
        points = CalculatePoints();
    }

    private void Start()
    {
        InitVariables();
        StartGenerator();
    }

    //Generation
    public void StartGenerator()
    {
        StartCoroutine(Generate());
    }
    public IEnumerator Generate()
    {
        yield return new WaitForSeconds(0.5f);
;

        while(true)
        {
            Hex[] hexes = FindObjectsOfType<Hex>();

            bool check = m_tilecount <= hexes.Length;
            //Debug.Log(string.Format("{0} : {1} <= {2}",check,m_tilecount,hexes.Length));
            if(check) { break; }


            for(int i = 0;i < hexes.Length;i++)
            {
                yield return new WaitForFixedUpdate();
                hexes[i].GenerateTiles();
            }

            yield return new WaitForSeconds(0.5f);
        }

        Debug.Log("Finish Generating");
    }

    //Caluclations
    public Vector3[] CalculatePoints()
    {
        Vector3[] points = new Vector3[6];
        for (int i = 0; i < points.Length; i++)
        {
            points[i] = transform.position + (DirFromAngle(30f + (60f * i)) * m_tilecount);
        }
        return points;
    }
}
