using System.Collections.Generic;
using UnityEngine;

namespace Uinty2glTF
{
    public class GLTF_BlendShape : GlTF_Writer
    {
        public GlTF_Accessor normalAccessor;
        public GlTF_Accessor positionAccessor;
        public GlTF_Accessor colorAccessor;
        public GlTF_Accessor texCoord0Accessor;
        public GlTF_Accessor texCoord1Accessor;
        public GlTF_Accessor texCoord2Accessor;
        public GlTF_Accessor texCoord3Accessor;
        public GlTF_Accessor lightmapTexCoordAccessor;
        public GlTF_Accessor jointAccessor;
        public GlTF_Accessor weightAccessor;
        public GlTF_Accessor tangentAccessor;
        public int blendShapeIndex;
        public int morphFrame;

        public void Populate(Mesh m)
        {
            //positionAccessor.Populate(m.vertices);

            var deltaVertices = new Vector3[m.vertexCount];
            var deltaNormals = new Vector3[m.vertexCount];
            var deltaTangents = new Vector3[m.vertexCount];
            m.GetBlendShapeFrameVertices(blendShapeIndex, morphFrame, deltaVertices, deltaNormals, deltaTangents);
            positionAccessor.Populate(deltaVertices);
        }

        public override void Write()
        {
            Indent(); jsonWriter.Write("{\n");
            IndentIn();
            if (positionAccessor != null)
            {
                CommaNL();
                Indent(); jsonWriter.Write("\"POSITION\": " + GlTF_Writer.accessors.IndexOf(positionAccessor));
            }
            if (normalAccessor != null)
            {
                CommaNL();
                Indent(); jsonWriter.Write("\"NORMAL\": " + GlTF_Writer.accessors.IndexOf(normalAccessor));
            }
            if (colorAccessor != null)
            {
                CommaNL();
                Indent(); jsonWriter.Write("\"COLOR_0\": " + GlTF_Writer.accessors.IndexOf(colorAccessor));
            }
            if (texCoord0Accessor != null)
            {
                CommaNL();
                Indent(); jsonWriter.Write("\"TEXCOORD_0\": " + GlTF_Writer.accessors.IndexOf(texCoord0Accessor));
            }
            if (texCoord1Accessor != null)
            {
                CommaNL();
                Indent(); jsonWriter.Write("\"TEXCOORD_1\": " + GlTF_Writer.accessors.IndexOf(texCoord1Accessor));
            }
            if (texCoord2Accessor != null)
            {
                CommaNL();
                Indent(); jsonWriter.Write("\"TEXCOORD_2\": " + GlTF_Writer.accessors.IndexOf(texCoord2Accessor));
            }
            if (texCoord3Accessor != null)
            {
                CommaNL();
                Indent(); jsonWriter.Write("\"TEXCOORD_3\": " + GlTF_Writer.accessors.IndexOf(texCoord3Accessor));
            }
            if (lightmapTexCoordAccessor != null)
            {
                CommaNL();
                Indent(); jsonWriter.Write("\"TEXCOORD_4\": " + GlTF_Writer.accessors.IndexOf(lightmapTexCoordAccessor));
            }
            if (jointAccessor != null)
            {
                CommaNL();
                Indent(); jsonWriter.Write("\"JOINTS_0\": " + GlTF_Writer.accessors.IndexOf(jointAccessor));
            }
            if (weightAccessor != null)
            {
                CommaNL();
                Indent(); jsonWriter.Write("\"WEIGHTS_0\": " + GlTF_Writer.accessors.IndexOf(weightAccessor));
            }
            if (tangentAccessor != null)
            {
                CommaNL();
                Indent(); jsonWriter.Write("\"TANGENT\": " + GlTF_Writer.accessors.IndexOf(tangentAccessor));
            }

            jsonWriter.WriteLine();
            IndentOut();
            Indent(); jsonWriter.Write("}");
        }

    }
    
}
