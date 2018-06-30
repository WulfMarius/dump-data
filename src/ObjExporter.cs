using UnityEngine;
using System.IO;
using System.Text;

public class ObjExporterScript
{
    private static int StartIndex = 0;

    public static void Start()
    {
        StartIndex = 0;
    }

    public static void End()
    {
        StartIndex = 0;
    }

    public static string MeshToString(Mesh m, Material[] mats, Transform t)
    {
        if (!m.isReadable)
        {
            Debug.Log("Skipping mesh " + t.name + " because it is not readable.");
            return "";
        }

        Vector3 s = t.localScale;
        Vector3 p = t.localPosition;
        Quaternion r = t.localRotation;

        int numVertices = 0;
        if (!m)
        {
            return "####Error####";
        }

        StringBuilder sb = new StringBuilder();

        foreach (Vector3 vv in m.vertices)
        {
            Vector3 v = t.TransformPoint(vv);
            numVertices++;
            sb.Append(string.Format("v {0} {1} {2}\n", v.x, v.y, -v.z));
        }
        sb.Append("\n");
        foreach (Vector3 nn in m.normals)
        {
            Vector3 v = r * nn;
            sb.Append(string.Format("vn {0} {1} {2}\n", -v.x, -v.y, v.z));
        }
        sb.Append("\n");
        foreach (Vector3 v in m.uv)
        {
            sb.Append(string.Format("vt {0} {1}\n", v.x, v.y));
        }
        for (int material = 0; material < m.subMeshCount; material++)
        {
            sb.Append("\n");
            sb.Append("usemtl ").Append(mats[material].name).Append("\n");
            sb.Append("usemap ").Append(mats[material].name).Append("\n");

            int[] triangles = m.GetTriangles(material);
            for (int i = 0; i < triangles.Length; i += 3)
            {
                sb.Append(string.Format("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}\n",
                    triangles[i] + 1 + StartIndex, triangles[i + 1] + 1 + StartIndex, triangles[i + 2] + 1 + StartIndex));
            }
        }

        StartIndex += numVertices;
        return sb.ToString();
    }
}

public class ObjExporter
{
    public static void DoExport(GameObject gameObject, bool makeSubmeshes)
    {
        string meshName = gameObject.name.Replace(':', '_');
        string fileName = "C:\\Users\\st_dg\\OneDrive\\TLD\\exported-meshes\\" + meshName + ".obj";
        Debug.Log("Exporting " + fileName);

        ObjExporterScript.Start();

        StringBuilder stringBuilder = new StringBuilder();

        stringBuilder.Append("#" + meshName + ".obj"
                            + "\n#" + System.DateTime.Now.ToLongDateString()
                            + "\n#" + System.DateTime.Now.ToLongTimeString()
                            + "\n#-------"
                            + "\n\n");

        Transform t = gameObject.transform;

        Transform originalParent = gameObject.transform.parent;
        Vector3 originalLocalPosition = t.localPosition;
        Quaternion originalLocalRotation = t.localRotation;
        Vector3 originalLocalScale = t.localScale;

        t.parent = null;
        t.position = Vector3.zero;
        t.localScale = Vector3.one;
        t.localRotation = Quaternion.identity;

        if (!makeSubmeshes)
        {
            stringBuilder.Append("g ").Append(t.name).Append("\n");
        }
        stringBuilder.Append(processTransform(t, makeSubmeshes));

        WriteToFile(stringBuilder.ToString(), fileName);

        t.parent = originalParent;
        t.localPosition = originalLocalPosition;
        t.localRotation = originalLocalRotation;
        t.localScale = originalLocalScale;

        ObjExporterScript.End();
        Debug.Log("Exported Mesh: " + fileName);
    }

    static string processTransform(Transform t, bool makeSubmeshes)
    {
        Debug.Log("   Exporting " + t.gameObject.name);

        StringBuilder meshString = new StringBuilder();

        meshString.Append("#" + t.name
                        + "\n#-------"
                        + "\n");

        if (makeSubmeshes)
        {
            meshString.Append("g ").Append(t.name).Append("\n");
        }

        MeshFilter mf = t.GetComponent<MeshFilter>();
        Renderer renderer = t.GetComponent<Renderer>();
        if (mf && renderer)
        {
            meshString.Append(ObjExporterScript.MeshToString(mf.sharedMesh, renderer.sharedMaterials, t));
        }

        SkinnedMeshRenderer skinnedMeshRenderer = t.GetComponent<SkinnedMeshRenderer>();
        if (skinnedMeshRenderer)
        {
            meshString.Append(ObjExporterScript.MeshToString(skinnedMeshRenderer.sharedMesh, skinnedMeshRenderer.sharedMaterials, t));
        }

        for (int i = 0; i < t.childCount; i++)
        {
            meshString.Append(processTransform(t.GetChild(i), makeSubmeshes));
        }

        return meshString.ToString();
    }

    static void WriteToFile(string s, string filename)
    {
        using (StreamWriter sw = new StreamWriter(filename))
        {
            sw.Write(s);
        }
    }
}