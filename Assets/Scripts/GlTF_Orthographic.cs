﻿
using UnityEngine;
using System.Collections;

namespace Uinty2glTF
{
    public class GlTF_Orthographic : GlTF_Camera
    {
        public float xmag;
        public float ymag;
        public float zfar;
        public float znear;
        public GlTF_Orthographic() { type = "orthographic"; }
        public override void Write()
        {
        }
    }
}