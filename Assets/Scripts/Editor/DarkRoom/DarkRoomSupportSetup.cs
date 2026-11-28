using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System;
using System.IO;
using System.Linq;
using YunaLegend.DarkRoom;
using YunaLegend.Camera25D;

namespace YunaLegend.EditorScripts
{
    public static class DarkRoomSupportSetup
    {
        [Serializable] class SupportReport { public Entry[] platforms; public bool legacyGameOverField; }
        [Serializable] class Entry
        {
            public int index,areas,segments;
            public float minX,maxX,minY,maxY;
            public Vector3 primary;
            public string[] colliders;
        }
        public static void WriteReport(string output)
        {
            var markers=UnityEngine.Object.FindObjectsByType<DarkRoomPlatformMarker>(FindObjectsInactive.Include,FindObjectsSortMode.None);
            var player=UnityEngine.Object.FindFirstObjectByType<Yuna25DPlayerController>();
            var report=new SupportReport {
                legacyGameOverField=new SerializedObject(player).FindProperty("gameOverSfx")!=null,
                platforms=markers.OrderBy(m=>m.spec.index).Select(m=>new Entry {
                    index=m.spec.index,areas=m.Support.Regions.Count,segments=m.Support.Segments.Count,
                    minX=m.Support.MinX,maxX=m.Support.MaxX,minY=m.Support.MinY,maxY=m.Support.MaxY,
                    primary=m.GetTopCenterWorld(),
                    colliders=m.transform.Find("Contacto_Fisico").GetComponentsInChildren<Collider>(true).Select(c=>c.GetType().Name).ToArray()
                }).ToArray()};
            File.WriteAllText(Path.Combine(output,"support-geometry.json"),JsonUtility.ToJson(report,true));
        }
        public static void ApplyAndSave()
        {
            EditorSceneManager.OpenScene("Assets/Scenes/DarkRoom_Gameplay.unity");
            foreach(var marker in UnityEngine.Object.FindObjectsByType<DarkRoomPlatformMarker>(FindObjectsInactive.Include,FindObjectsSortMode.None))
            {
                marker.spec.contact=DarkRoomPlatformGeometry.GetContact(marker.spec.index);
                var contact=marker.transform.Find("Contacto_Fisico");
                foreach(var c in contact.GetComponents<Collider>())UnityEngine.Object.DestroyImmediate(c);
                while(contact.childCount>0)UnityEngine.Object.DestroyImmediate(contact.GetChild(0).gameObject);
                contact.localPosition=Vector3.zero;contact.localRotation=Quaternion.identity;contact.localScale=Vector3.one;
                DarkRoomPlatformArt.BuildContact(marker.spec,contact);
                marker.InvalidateSupport();EditorUtility.SetDirty(marker);
            }
            var side=UnityEngine.Object.FindFirstObjectByType<SideScroll25DCamera>();side.RouteDistanceScale=1.05f;
            side.SnapToRouteFraming(0);EditorUtility.SetDirty(side);
            // Save removes the retired gameOverSfx serialized property as well.
            EditorUtility.SetDirty(UnityEngine.Object.FindFirstObjectByType<Yuna25DPlayerController>());
            EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            Debug.Log("SUPPORT_SETUP_SAVED: real supports; camera distance1.05; no GameOver clip; no audio generator.");
        }
    }
}
