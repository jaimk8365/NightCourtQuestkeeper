using NightCourt.Core;
using NightCourt.Runtime;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NightCourt.Editor
{
    public static class FaeCottageSceneBuilder
    {
        private const string ScenePath = "Assets/NightCourt/Scenes/FaeCottage.unity";

        [MenuItem("Night Court/Build Fae Cottage Prototype")]
        public static void Build()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            AddCamera();
            AddRoom("Hearth", new Vector2(16, 4), new Vector2(30, 6), new Color(.98f, .75f, .35f));
            AddRoom("Closet", new Vector2(6, 9), new Vector2(10, 4), new Color(.72f, .58f, .92f));
            AddRoom("Workbench", new Vector2(24, 9), new Vector2(14, 4), new Color(.30f, .78f, .70f));
            AddRoom("Glowcap Garden", new Vector2(16, 14), new Vector2(30, 5), new Color(.38f, .72f, .43f));
            AddNpc("Elowen · choose one tiny spark", new Vector2(13, 4));
            AddNpc("Orrin · build one piece", new Vector2(23, 9));
            AddNpc("Aster · Hearth Dragon", new Vector2(22, 3));
            AddNode("node_hearth", "Put away five things", QuestType.Micro, 5, new Reward(12, 3, 2), new Vector2(16, 5));
            AddNode("node_closet", "Start one laundry step", QuestType.Micro, 10, new Reward(15, 3, 2), new Vector2(5, 8));
            AddNode("node_workbench", "Shape one project piece", QuestType.Focus, 20, new Reward(35, 5, 3), new Vector2(25, 8));
            AddNode("node_garden", "Tend one recurring ritual", QuestType.Ritual, 5, new Reward(10, 2, 3), new Vector2(4, 13));
            AddNode("node_dragon", "Care for Aster", QuestType.PetCare, 5, new Reward(8, 1, 8), new Vector2(22, 4));
            new GameObject("GameServices", typeof(GameBootstrap), typeof(RewardPresenter));
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            Debug.Log($"Fae Cottage prototype created at {ScenePath}");
        }

        private static void AddCamera()
        {
            var cameraObject = new GameObject("Main Camera", typeof(Camera));
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(16, 9, -10);
            Camera camera = cameraObject.GetComponent<Camera>();
            camera.orthographic = true; camera.orthographicSize = 10;
            camera.backgroundColor = new Color(.08f, .05f, .13f);
        }

        private static void AddRoom(string name, Vector2 position, Vector2 size, Color colour)
        {
            GameObject room = GameObject.CreatePrimitive(PrimitiveType.Quad);
            room.name = name; room.transform.position = position; room.transform.localScale = size;
            room.GetComponent<MeshRenderer>().sharedMaterial = NewMaterial(colour);
            Object.DestroyImmediate(room.GetComponent<MeshCollider>());
            AddLabel(name, position + Vector2.up * (size.y * .35f), 0.7f);
        }

        private static void AddNode(string id, string title, QuestType type, int minutes, Reward reward, Vector2 position)
        {
            var node = new GameObject(title, typeof(CircleCollider2D), typeof(TaskNodeBehaviour));
            node.transform.position = new Vector3(position.x, position.y, -1);
            node.GetComponent<CircleCollider2D>().radius = .7f;
            node.GetComponent<TaskNodeBehaviour>().Configure(id, title, type, minutes, reward);
            AddLabel("✨ " + title, position, .42f);
        }

        private static void AddNpc(string text, Vector2 position) => AddLabel("🧚 " + text, position, .46f);

        private static void AddLabel(string text, Vector2 position, float size)
        {
            var label = new GameObject(text, typeof(TextMesh));
            label.transform.position = new Vector3(position.x, position.y, -2);
            TextMesh mesh = label.GetComponent<TextMesh>();
            mesh.text = text; mesh.characterSize = size; mesh.anchor = TextAnchor.MiddleCenter;
            mesh.color = Color.white; mesh.fontSize = 40;
        }

        private static Material NewMaterial(Color colour)
        {
            var material = new Material(Shader.Find("Unlit/Color")); material.color = colour; return material;
        }
    }
}
