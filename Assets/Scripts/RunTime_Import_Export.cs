using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using TMPro;
using Uinty2glTF;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

public class RunTime_Import_Export : MonoBehaviour
{
    private enum nameFiles
    {
        ExpressionAndJawNames,
        ARkitNames
    }
    void Start()
    {
        var importSettings = new Siccity.GLTFUtility.ImportSettings();
        Debug.Log(Application.persistentDataPath);
        Siccity.GLTFUtility.Importer.LoadFromFileAsync(Application.dataPath + "\\characterMain 1.glb", importSettings, (g, ac) =>
        {
            GameObject root = g;
            Siccity.GLTFUtility.Importer.LoadFromFileAsync(Application.dataPath + "\\sing.glb", importSettings, (g, clip) =>
            {
                var animation = root.AddComponent<Animation>();
                animation.wrapMode = WrapMode.Loop;
                animation.playAutomatically = true;
                clip[0].legacy = true;
                animation.AddClip(clip[0], clip[0].name);
                animation.Play(clip[0].name);
                // DestroyImmediate(g);

            });

        });

        Stopwatch sw = Stopwatch.StartNew();
        sw.Start();
        string path = Directory.GetParent(Application.dataPath).FullName + "/export" + "/character.gltf";
        string glbName = Path.GetDirectoryName(path) + "/" + Path.GetFileNameWithoutExtension(path) + ".glb";
        Packer.Pack(path, glbName);
        sw.Stop();
        Debug.Log("Finish!" + sw.ElapsedMilliseconds + "ms");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Return))
        {

            Stopwatch sw = new Stopwatch();
            sw.Start();
            TranslateCSVtoGLB animation = new TranslateCSVtoGLB();
            string inputFileName = "sing";
            string outputFileName = "sing";
            string path = null;

            //Only have 2 different nameFiles(Expression&jaw, ARkit)
            nameFiles namepath = nameFiles.ExpressionAndJawNames;

            List<List<TranslateCSVtoGLB.MorphTarget>> ARktiTarget = null;
            List<List<TranslateCSVtoGLB.MorphTarget>> data = null;
#region Create Mesh to store BS
            var mesh = new Mesh
            {
                name = "mesh0"
            };
            mesh.vertices = new Vector3[]
            {
                Vector3.zero * 0.001f, Vector3.right * 0.001f, Vector3.up * 0.001f
            };
            mesh.triangles = new int[]
            {
                0, 1, 2
            };
            mesh.normals = new Vector3[]
            {
                Vector3.back, Vector3.back, Vector3.back
            };

            var mesh2 = new Mesh
            {
                name = "mesh1"
            };
            mesh2.vertices = new Vector3[]
            {
                Vector3.zero * 0.001f, Vector3.right * 0.001f, Vector3.up * 0.001f
            };
            mesh2.triangles = new int[]
            {
                0, 1, 2
            };
            mesh2.normals = new Vector3[]
            {
                Vector3.back, Vector3.back, Vector3.back
            };

            List<Mesh> meshes = new List<Mesh>
            {
                mesh,
                mesh2
            };
            #endregion
            if (inputFileName == null || outputFileName == null)
            {
                Debug.Log("file name can't be empty");
            }
            else
            {
                //Output file path
                path = Application.dataPath + "\\Scripts\\Export\\" + outputFileName + ".gltf";
                switch (namepath)
                {
                    case nameFiles.ARkitNames:
                        ARktiTarget = animation.ReadData(Application.dataPath + "\\" + inputFileName + ".csv", namepath.ToString());
                        animation.InitARkit(path, ARktiTarget[0], mesh, 24);
                        break;
                    case nameFiles.ExpressionAndJawNames:
                        data = animation.ReadData(Application.dataPath + "\\" + inputFileName + ".csv", namepath.ToString());
                        animation.Init(path, data[0], data[1], meshes, 24);
                        break;

                }
            }
            sw.Stop();
            Debug.Log("Finish translate in: " + sw.ElapsedMilliseconds + "ms");
        }
    }

}
