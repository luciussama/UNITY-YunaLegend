using System;
using UnityEngine;

namespace YunaLegend.BearChase.Runtime
{
    /// <summary>
    /// Estrutura serializavel de ossos de personagem para desacoplar BearChase.Runtime
    /// dos assemblies das demais fases (ToyRoom e Arena).
    /// </summary>
    [Serializable]
    public struct BearChaseRigBones
    {
        public Transform Hips;
        public Transform Spine;
        public Transform Chest;
        public Transform Neck;
        public Transform Head;
        public Transform UpperArmL;
        public Transform ForearmL;
        public Transform HandL;
        public Transform UpperArmR;
        public Transform ForearmR;
        public Transform HandR;
        public Transform ThighL;
        public Transform ShinL;
        public Transform FootL;
        public Transform ThighR;
        public Transform ShinR;
        public Transform FootR;

        public bool IsValid => Hips != null && ThighL != null && ThighR != null && UpperArmL != null && UpperArmR != null;
    }
}
