using UnityEngine;

namespace YunaLegend.Illustrated25D.Environment
{
    public sealed class IllustratedBookStackPlatform : MonoBehaviour
    {
        public Texture2D paintedTexture;
        public Mesh contactMesh;
        public Mesh visualMesh;
        public float supportWidth = 1.184211f;
        public float supportTiltDegrees = -8.739244f;
        public float contactOffsetX = 0.134786f;
        public float supportHeightAboveCentre = 0.363763f;
        public float physicalDepth = 1.15f;
    }
}
