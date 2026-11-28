using NUnit.Framework;
using UnityEngine;
using YunaLegend.Camera25D;

namespace YunaLegend.Tests
{
    public class ResponsiveCameraTests
    {
        [Test] public void PortraitUsesSmallerMinimumAndDesktopKeepsApprovedWidth()
        {
            Assert.AreEqual(7f, SideScroll25DCamera.ResponsiveMinimumWidth(9f/16, 10.5f), .001f);
            Assert.AreEqual(10.5f, SideScroll25DCamera.ResponsiveMinimumWidth(16f/9, 10.5f), .001f);
        }
        [Test] public void PortraitViewWidthUsesActualAspectInsteadOfSquareClamp()
        {
            var go = new GameObject("PortraitCamera");
            try
            {
                var camera = go.AddComponent<Camera>(); camera.aspect = 9f/16;
                var framing = go.AddComponent<SideScroll25DCamera>();
                float expected = 2 * Mathf.Abs(go.transform.position.z) * Mathf.Tan(45f*Mathf.Deg2Rad*.5f) * camera.aspect;
                Assert.AreEqual(expected, framing.ViewWidth, .001f);
            }
            finally { Object.DestroyImmediate(go); }
        }
    }
}
