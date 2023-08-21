
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace Uinty2glTF
{
    public class GlTF_Mesh : GlTF_Writer
    {
        public List<GlTF_Primitive> primitives = new List<GlTF_Primitive>();
        public List<float> weight = new List<float>();
        public List<string> targetNames = new List<string>();
        //public List<GlTF_Attributes> attributes = new List<GlTF_Attributes>();
        public GlTF_Attributes at = new GlTF_Attributes();
        public int meshIndex = 0;

        public GlTF_Mesh() { primitives = new List<GlTF_Primitive>(); }

        public static string GetNameFromObject(Object o)
        {
            //return "mesh_" + GlTF_Writer.GetNameFromObject(o, true);
            return GlTF_Writer.GetNameFromObject(o, true);
        }

        public void Populate(Mesh m)
        {
            //if (primitives.Count > 0)
            //{
            //    // only populate first attributes because the data are shared between primitives
            //    primitives[0].attributes.Populate(m);
            //}
            //for(int i = 0; i < attributes.Count; i++)
            //{
            //    attributes[i].Populate(m);
            //}
            at.Populate(m);
            //if(weight.Count < m.blendShapeCount)
            //{
            //    for(int i = 0; i < m.blendShapeCount; i++)
            //    {
            //        weight.Add(0);
            //    }
            //}
            //primitives.Remove()
            foreach (GlTF_Primitive p in primitives)
            {
                p.Populate(m);
                p.attributes = at;
                weight.Clear();
                //p.attributes.Populate(m);
                foreach(GLTF_BlendShape bs in p.blendShapes)
                {
                    bs.Populate(m);
                    if(!targetNames.Contains(bs.name))
                    {
                        targetNames.Add(bs.name);
                        weight.Add(0);
                    }
                }
            }
        }

        public override void Write()
        {
            Indent(); jsonWriter.Write("{\n");
            IndentIn();
            Indent(); jsonWriter.Write("\"name\": \"" + name + "\",\n");
            Indent(); jsonWriter.Write("\"primitives\": [\n");
            IndentIn();
            foreach (GlTF_Primitive p in primitives)
            {
                CommaNL();
                Indent(); jsonWriter.Write("{\n");       
                p.Write();
                Indent(); jsonWriter.Write("}");
            }
            jsonWriter.WriteLine();
            IndentOut();
            IndentOut();
            Indent(); jsonWriter.Write("]\n");
            CommaNL();
            if (weight.Count > 0)
            {
                Indent(); jsonWriter.Write("\"weights\": [\n");
                IndentIn();
                for (int i = 0; i < weight.Count; i++)
                {
                    CommaNL();
                    Indent(); jsonWriter.Write(weight[i] + "\n");

                }
                IndentOut();
                Indent(); jsonWriter.Write("]\n");
                CommaNL();

            }
            if (targetNames.Count > 0)
            {
                Indent(); jsonWriter.Write("\"extras\": {\n");
                IndentIn();
                Indent(); jsonWriter.Write("\"targetNames\": [\n");
                for (int i = 0; i < targetNames.Count; i++)
                {
                    CommaNL();
                    Indent(); jsonWriter.Write("\"" + targetNames[i] + "\"\n");

                }
                IndentOut();
                Indent(); jsonWriter.Write("]\n");
                Indent(); jsonWriter.Write("}\n");

            }

            Indent(); jsonWriter.Write("}");
            jsonWriter.WriteLine();
        }
    }
}
