using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace NightCourt.Editor
{
    public static class ScenePreviewCapture
    {
        public static void Capture()
        {
            EditorSceneManager.OpenScene("Assets/NightCourt/Scenes/FaeCottage.unity");
            Camera camera=Camera.main; if(camera==null)throw new InvalidDataException("Preview camera missing.");
            var texture=new RenderTexture(1280,720,24);camera.targetTexture=texture;camera.Render();RenderTexture.active=texture;
            var image=new Texture2D(1280,720,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1280,720),0,0);image.Apply();
            File.WriteAllBytes("/private/tmp/nightcourt-3d-cottage-preview.png",image.EncodeToPNG());
            camera.targetTexture=null;RenderTexture.active=null;Object.DestroyImmediate(texture);Object.DestroyImmediate(image);
            Debug.Log("3D cottage preview captured.");
        }
    }
}
