
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using Uinty2glTF;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Uinty2glTF
{

    public class TranslateCSVtoGLB : GlTF_Writer
    {

        public class MorphTarget
        {
            public string name = null;
            public List<float> weight = new List<float>();
            public int flag = -1;
            public int index = -1;
        }
        public List<GlTF_Channel> channels = new List<GlTF_Channel>();
        public List<GlTF_AnimSampler> animSamplers = new List<GlTF_AnimSampler>();

        public List<List<MorphTarget>> ReadData(string path, string namepath)
        {
            
            var data = new List<List<MorphTarget>>();
            var placeHolder = new List<MorphTarget>();
            var placeHolder2 = new List<MorphTarget>();

            //First line of the .csv file
            StreamReader csvreader = new StreamReader(path);
            string inputLine = csvreader.ReadLine();
            string[] inputArray = inputLine.Split(new char[] { ',' });

            //Standard BS names 
            StreamReader bp_names = new StreamReader(Application.dataPath + "\\Scripts\\StandardNames\\" + namepath + ".csv");//new StreamReader(Application.dataPath.Substring(0, Application.dataPath.LastIndexOf('/')) + "\\Test\\" + namepath + ".csv");
            string names = bp_names.ReadLine();
            string[] namesArray = names.Split(new char[] { ',' });

            //Placeholder for two different BS
            List<string> ExNames = new List<string>();
            List<string> JawNames = new List<string>();
            List<string> ARKitnames = new List<string>();

            //Weight values from .csv file
            string weightReader = csvreader.ReadToEnd();
            string[] weights = weightReader.Split(new char[] { ',', '\n'}) ;
            int length = inputArray.Length;

            

            //two name array to diff expressionBS and JawBS
            for (int i = 0; i < namesArray.Length - 1; i++)
            {
                if (namesArray[i].Contains("ExpressionBlendshapes"))
                {
                    ExNames.Add(namesArray[i]);
                }
                if (namesArray[i].Contains("JawBlendshapes"))
                {
                    JawNames.Add(namesArray[i]);
                }
                if (namesArray[i].Contains("ARKitblendShape"))
                {
                    ARKitnames.Add(namesArray[i]);
                }
            }
            Stopwatch sw = new Stopwatch();
            sw.Start();
            List<List<float>> weightList = new List<List<float>>();
            int weightCount = (weights.Length / length);
            for(int i = 0; i < length; i++)
            {
                int temp = i;
                weightList.Add(new List<float>());
                for(int j = 0; j < (weights.Length / length); j++)
                {
                    weightList[i].Add(float.Parse(weights[temp]));
                    temp += length;
                }
            }
            sw.Stop();

            for (int i = 0; i < namesArray.Length - 1; i++)
            {
                MorphTarget target = new MorphTarget();
                target.name = namesArray[i];
                if (i < ExNames.Count)
                {
                    placeHolder.Add(target);
                }
                else
                {
                    placeHolder2.Add(target);
                }
                if (ARKitnames.Count != 0)
                {
                    placeHolder.Add(target);
                }

                for (int j = 0; j < inputArray.Length; j++)
                {
                    if (i < ExNames.Count)
                    {
                        if (ExNames[i].Contains(inputArray[j]))
                        {
                            placeHolder[i].weight = weightList[j];
                            break;
                        }
                    }
                    else
                    {
                        if (JawNames[i - ExNames.Count].Contains(inputArray[j]))
                        {
                            placeHolder2[i - ExNames.Count].weight = weightList[j];
                            break;
                        }
                    }
                    if (ARKitnames.Count != 0 && ARKitnames[i].Contains(inputArray[j]))
                    {
                        placeHolder[i].weight = weightList[j];
                        break;
                    }

                }
            }

            Debug.Log("Read data: " + (float)sw.ElapsedMilliseconds + "ms");
            data.Add(placeHolder);
            data.Add(placeHolder2);
            return data;
        }
        public void Init(string path, List<MorphTarget> extarget, List<MorphTarget> jawtarget, List<Mesh> meshes, int frame)
        {
            GlTF_Writer writer = new GlTF_Writer();
            writer.Init();
            //GlTF_Node node = new GlTF_Node();
            // Create rootNode
            GlTF_Node correctionNode = new GlTF_Node();
            correctionNode.id = "Test";
            correctionNode.name = "Test";
            GlTF_Writer.nodes.Add(correctionNode);
            GlTF_Writer.nodeNames.Add(correctionNode.name);
            GlTF_Writer.rootNodes.Add(correctionNode);
            GlTF_Animation tempAnim = new GlTF_Animation("test");
            int weightCount = 0;
            foreach (Mesh m in meshes)
            {

                GlTF_Mesh mesh = new GlTF_Mesh();
                mesh.name = GlTF_Writer.cleanNonAlphanumeric(GlTF_Mesh.GetNameFromObject(m));
                GlTF_Accessor positionAccessor = new GlTF_Accessor(GlTF_Accessor.GetNameFromObject(m, "position"), GlTF_Accessor.Type.VEC3, GlTF_Accessor.ComponentType.FLOAT);
                positionAccessor.bufferView = GlTF_Writer.vec3BufferView;
                GlTF_Writer.accessors.Add(positionAccessor);
                GlTF_Accessor normalAccessor = null;
                if (m.normals.Length > 0)
                {
                    normalAccessor = new GlTF_Accessor(GlTF_Accessor.GetNameFromObject(m, "normal"), GlTF_Accessor.Type.VEC3, GlTF_Accessor.ComponentType.FLOAT);
                    normalAccessor.bufferView = GlTF_Writer.vec3BufferView;
                    GlTF_Writer.accessors.Add(normalAccessor);
                }
                GlTF_Primitive primitive = new GlTF_Primitive();
                primitive.name = GlTF_Primitive.GetNameFromObject(m, 0);
                primitive.index = 0;
                GlTF_Accessor indexAccessor = new GlTF_Accessor(GlTF_Accessor.GetNameFromObject(m, "indices_" + 0), GlTF_Accessor.Type.SCALAR, GlTF_Accessor.ComponentType.USHORT);
                indexAccessor.bufferView = GlTF_Writer.ushortBufferView;
                GlTF_Writer.accessors.Add(indexAccessor);
                primitive.indices = indexAccessor;
                var deltaVertices = new Vector3[m.vertexCount];
                var deltaNormals = new Vector3[m.vertexCount];
                var deltaTangents = new Vector3[m.vertexCount];
                GlTF_Node node = new GlTF_Node();
                List<MorphTarget> targets = new List<MorphTarget>();
                if (m.name == "mesh0")
                {
                    node.id = "head_part";
                    node.name = "head_part";
                    targets = extarget;
                }
                else if(m.name == "mesh1")
                {
                    node.id = "tooth_down";
                    node.name = "tooth_down";
                    targets = jawtarget;
                }
                for (int i = 0; i < targets.Count; i++)
                {
                    GlTF_Accessor bspositionAccessor = new GlTF_Accessor(targets[i].name + "_position", GlTF_Accessor.Type.VEC3, GlTF_Accessor.ComponentType.FLOAT);
                    bspositionAccessor.bufferView = GlTF_Writer.vec3BufferView;
                    GlTF_Writer.accessors.Add(bspositionAccessor);


                    GLTF_BlendShape blendShape = new GLTF_BlendShape();
                    m.AddBlendShapeFrame(targets[i].name, 1f, deltaVertices, deltaNormals, deltaTangents);
                    blendShape.blendShapeIndex = i;
                    blendShape.morphFrame = m.GetBlendShapeFrameCount(i) - 1;

                    blendShape.name = targets[i].name;
                    blendShape.positionAccessor = bspositionAccessor;
                    primitive.blendShapes.Add(blendShape);
                    if(weightCount == 0)
                    {
                        weightCount = targets[i].weight.Count;
                    }
                }
                
                GlTF_Accessor timeAccessor = new GlTF_Accessor("_TimeAccessor_", GlTF_Accessor.Type.SCALAR, GlTF_Accessor.ComponentType.FLOAT);
                timeAccessor.bufferView = GlTF_Writer.floatBufferView;
                int timeAccessorIndex = GlTF_Writer.accessors.Count;
                GlTF_Writer.accessors.Add(timeAccessor);

                GlTF_Channel chWeight = new GlTF_Channel("weight", animSamplers.Count);
                GlTF_Target targetWeight = new GlTF_Target();
                targetWeight.id = node.id;
                targetWeight.path = "weights";
                chWeight.target = targetWeight;
                channels.Add(chWeight);

                GlTF_AnimSampler sWeight = new GlTF_AnimSampler(timeAccessorIndex, GlTF_Writer.accessors.Count);
                GlTF_Accessor sweightAccessor = new GlTF_Accessor("_WeightAccessor_", GlTF_Accessor.Type.SCALAR, GlTF_Accessor.ComponentType.FLOAT);
                sweightAccessor.bufferView = GlTF_Writer.floatBufferView;
                GlTF_Writer.accessors.Add(sweightAccessor);
                animSamplers.Add(sWeight);
                Stopwatch sw = new Stopwatch();
                //sw.Start();
                float[] keyframeInput = new float[weightCount];
                float[] weights = new float[weightCount * targets.Count];
                //for (int j = 0; j < weightCount; j++)
                //{
                //}
                int index = 0;
                for (int i = 0; i < weightCount; i++)
                {
                    keyframeInput[i] = (1.0f / frame) * i;
                    for (int j = 0; j < targets.Count; ++j)
                    {
                        if(targets[j].weight.Count == 0)
                        {
                            weights[j + index] = 0;
                        }
                        else if(targets[j].weight.Count != 0)
                        {
                            weights[j + index] = targets[j].weight[i];
                        }
                    }
                    index += targets.Count;
                }
                timeAccessor.Populate(keyframeInput);
                sweightAccessor.Populate(weights);

                //sw.Stop();
                //Debug.Log("Populate accessors: " + sw.ElapsedMilliseconds + "ms");

                GlTF_Writer.nodeNames.Add(node.id);
                GlTF_Writer.nodes.Add(node);

                mesh.primitives.Add(primitive);
                GlTF_Attributes attributes = new GlTF_Attributes();
                attributes.positionAccessor = positionAccessor;
                attributes.normalAccessor = normalAccessor;
                mesh.at = attributes;

                mesh.Populate(m);
                GlTF_Writer.meshes.Add(mesh);
                node.meshIndex = GlTF_Writer.meshes.IndexOf(mesh);
                node.mesh = mesh;

                if (!node.hasParent)
                    correctionNode.childrenNames.Add(node.id);
            }
            tempAnim.channels = channels;
            tempAnim.animSamplers = animSamplers;
            GlTF_Writer.animations.Add(tempAnim);

           
            
            writer.OpenFiles(path);
            writer.Write();
            writer.CloseFiles();

            //Write .glb file
            string glbName = Path.GetDirectoryName(path) + "/" + Path.GetFileNameWithoutExtension(path) + ".glb";
            Packer.Pack(path, glbName);

        }
        public void InitARkit(string path, List<MorphTarget> targets, Mesh m, int frame)
        {
            GlTF_Writer writer = new GlTF_Writer();
            writer.Init();
            //GlTF_Node node = new GlTF_Node();
            // Create rootNode
            GlTF_Node correctionNode = new GlTF_Node();
            correctionNode.id = "Test";
            correctionNode.name = "Test";
            GlTF_Writer.nodes.Add(correctionNode);
            GlTF_Writer.nodeNames.Add(correctionNode.name);
            GlTF_Writer.rootNodes.Add(correctionNode);
            GlTF_Animation tempAnim = new GlTF_Animation("test");
            int weightCount = 0;

            GlTF_Mesh mesh = new GlTF_Mesh();
            mesh.name = GlTF_Writer.cleanNonAlphanumeric(GlTF_Mesh.GetNameFromObject(m));
            GlTF_Accessor positionAccessor = new GlTF_Accessor(GlTF_Accessor.GetNameFromObject(m, "position"), GlTF_Accessor.Type.VEC3, GlTF_Accessor.ComponentType.FLOAT);
            positionAccessor.bufferView = GlTF_Writer.vec3BufferView;
            GlTF_Writer.accessors.Add(positionAccessor);
            GlTF_Accessor normalAccessor = null;
            if (m.normals.Length > 0)
            {
                normalAccessor = new GlTF_Accessor(GlTF_Accessor.GetNameFromObject(m, "normal"), GlTF_Accessor.Type.VEC3, GlTF_Accessor.ComponentType.FLOAT);
                normalAccessor.bufferView = GlTF_Writer.vec3BufferView;
                GlTF_Writer.accessors.Add(normalAccessor);
            }
            GlTF_Primitive primitive = new GlTF_Primitive();
            primitive.name = GlTF_Primitive.GetNameFromObject(m, 0);
            primitive.index = 0;
            GlTF_Accessor indexAccessor = new GlTF_Accessor(GlTF_Accessor.GetNameFromObject(m, "indices_" + 0), GlTF_Accessor.Type.SCALAR, GlTF_Accessor.ComponentType.USHORT);
            indexAccessor.bufferView = GlTF_Writer.ushortBufferView;
            GlTF_Writer.accessors.Add(indexAccessor);
            primitive.indices = indexAccessor;
            var deltaVertices = new Vector3[m.vertexCount];
            var deltaNormals = new Vector3[m.vertexCount];
            var deltaTangents = new Vector3[m.vertexCount];
            GlTF_Node node = new GlTF_Node();
            node.id = "body";
            node.name = "body";

            for (int i = 0; i < targets.Count; i++)
            {
                GlTF_Accessor bspositionAccessor = new GlTF_Accessor(targets[i].name + "_position", GlTF_Accessor.Type.VEC3, GlTF_Accessor.ComponentType.FLOAT);
                bspositionAccessor.bufferView = GlTF_Writer.vec3BufferView;
                GlTF_Writer.accessors.Add(bspositionAccessor);


                GLTF_BlendShape blendShape = new GLTF_BlendShape();
                m.AddBlendShapeFrame(targets[i].name, 1f, deltaVertices, deltaNormals, deltaTangents);
                blendShape.blendShapeIndex = i;
                blendShape.morphFrame = m.GetBlendShapeFrameCount(i) - 1;

                blendShape.name = targets[i].name;
                blendShape.positionAccessor = bspositionAccessor;
                primitive.blendShapes.Add(blendShape);
                if (weightCount == 0)
                {
                    weightCount = targets[i].weight.Count;
                }
            }
            GlTF_Accessor timeAccessor = new GlTF_Accessor("_TimeAccessor_", GlTF_Accessor.Type.SCALAR, GlTF_Accessor.ComponentType.FLOAT);
            timeAccessor.bufferView = GlTF_Writer.floatBufferView;
            int timeAccessorIndex = GlTF_Writer.accessors.Count;
            GlTF_Writer.accessors.Add(timeAccessor);

            GlTF_Channel chWeight = new GlTF_Channel("weight", animSamplers.Count);
            GlTF_Target targetWeight = new GlTF_Target();
            targetWeight.id = node.id;
            targetWeight.path = "weights";
            chWeight.target = targetWeight;
            channels.Add(chWeight);

            GlTF_AnimSampler sWeight = new GlTF_AnimSampler(timeAccessorIndex, GlTF_Writer.accessors.Count);
            GlTF_Accessor sweightAccessor = new GlTF_Accessor("_WeightAccessor_", GlTF_Accessor.Type.SCALAR, GlTF_Accessor.ComponentType.FLOAT);
            sweightAccessor.bufferView = GlTF_Writer.floatBufferView;
            GlTF_Writer.accessors.Add(sweightAccessor);
            animSamplers.Add(sWeight);

            float[] keyframeInput = new float[weightCount];
            float[] weights = new float[weightCount * targets.Count];
            int index = 0;
            for (int i = 0; i < weightCount; ++i)
            {
                keyframeInput[i] = (1.0f / frame) * i;
                for (int j = 0; j < targets.Count; ++j)
                {
                    if (targets[j].weight.Count == 0)
                    {
                        weights[j + index] = 0;
                    }
                    else
                    {
                        weights[j + index] = targets[j].weight[i];
                    }
                }
                index += targets.Count;
            }
            timeAccessor.Populate(keyframeInput);
            sweightAccessor.Populate(weights);


            GlTF_Writer.nodeNames.Add(node.id);
            GlTF_Writer.nodes.Add(node);

            mesh.primitives.Add(primitive);
            GlTF_Attributes attributes = new GlTF_Attributes();
            attributes.positionAccessor = positionAccessor;
            attributes.normalAccessor = normalAccessor;
            mesh.at = attributes;

            mesh.Populate(m);
            GlTF_Writer.meshes.Add(mesh);
            node.meshIndex = GlTF_Writer.meshes.IndexOf(mesh);
            node.mesh = mesh;

            if (!node.hasParent)
                correctionNode.childrenNames.Add(node.id);
            
            tempAnim.channels = channels;
            tempAnim.animSamplers = animSamplers;
            GlTF_Writer.animations.Add(tempAnim);
            writer.OpenFiles(path);
            writer.Write();
            writer.CloseFiles();
            string glbName = Path.GetDirectoryName(path) + "/" + Path.GetFileNameWithoutExtension(path) + ".glb";
            Packer.Pack(path, glbName);
        }
    }

}
