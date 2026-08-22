using NightCourt.Core;
using NightCourt.Runtime;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NightCourt.Editor
{
    public static class FaeCottage3DBuilder
    {
        private const string ScenePath="Assets/NightCourt/Scenes/FaeCottage.unity";
        private static readonly Color Cream=new Color(.94f,.86f,.68f), Plum=new Color(.36f,.18f,.44f), Lilac=new Color(.66f,.48f,.78f), Teal=new Color(.20f,.66f,.61f), Rose=new Color(.72f,.25f,.43f), Gold=new Color(.95f,.65f,.20f), Moss=new Color(.30f,.52f,.32f);

        [MenuItem("Night Court/Build Polished 3D Fae Cottage")]
        public static void Build()
        {
            Scene scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            RenderSettings.ambientLight=new Color(.46f,.39f,.56f); RenderSettings.fog=true; RenderSettings.fogColor=new Color(.24f,.18f,.32f); RenderSettings.fogDensity=.012f;
            AddLighting(); AddCottage(); Transform player=AddPlayer(); AddCamera(player); AddDragon(player);AddCraftingAndHousing();
            AddStation("tiny","Hearth Spark","Choose one tiny win",QuestType.Micro,5,new Reward(15,3,3),new Vector3(-4.7f,.55f,-1.6f),Gold);
            AddStation("focus","Moonlit Desk","Begin a focus quest",QuestType.Focus,25,new Reward(40,7,4),new Vector3(4.8f,.55f,1.9f),Lilac);
            AddStation("care","Glowcap Garden","Tend one gentle ritual",QuestType.Ritual,5,new Reward(12,2,7),new Vector3(3.7f,.55f,-3.7f),Teal);
            AddPortals(); AddWorlds();
            new GameObject("GameServices",typeof(GameBootstrap),typeof(RewardPresenter),typeof(CozyHud),typeof(FocusSessionController));
            new GameObject("LifeHubBridge",typeof(LifeHubWebBridge));
            EditorSceneManager.SaveScene(scene,ScenePath); AssetDatabase.SaveAssets(); Debug.Log("Polished 3D Fae Cottage created: "+ScenePath);
        }

        private static void AddLighting()
        {
            GameObject sun=new GameObject("Moonlight",typeof(Light)); Light light=sun.GetComponent<Light>(); light.type=LightType.Directional; light.color=new Color(.72f,.76f,1f); light.intensity=1.15f; light.shadows=LightShadows.Soft; sun.transform.rotation=Quaternion.Euler(48f,-32f,0f);
            GameObject hearth=new GameObject("Hearth Glow",typeof(Light)); Light glow=hearth.GetComponent<Light>(); glow.type=LightType.Point; glow.color=new Color(1f,.48f,.18f); glow.range=9f; glow.intensity=5f; hearth.transform.position=new Vector3(-5f,2f,2.5f);
        }

        private static void AddCottage()
        {
            Part("Floor",PrimitiveType.Cube,new Vector3(0,-.28f,0),new Vector3(15,.5f,12),Cream);
            Part("Back Wall",PrimitiveType.Cube,new Vector3(0,2.3f,5.8f),new Vector3(15,5,.35f),Plum);
            Part("Left Wall",PrimitiveType.Cube,new Vector3(-7.3f,2.3f,0),new Vector3(.35f,5,12),Plum);
            Part("Right Wall",PrimitiveType.Cube,new Vector3(7.3f,2.3f,0),new Vector3(.35f,5,12),Plum);
            Part("Hearth Rug",PrimitiveType.Cylinder,new Vector3(-3.8f,.02f,1.7f),new Vector3(4,.05f,3),Rose);
            Part("Focus Rug",PrimitiveType.Cylinder,new Vector3(3.9f,.02f,1.7f),new Vector3(4,.05f,3),Lilac);
            Part("Garden Rug",PrimitiveType.Cylinder,new Vector3(2.8f,.02f,-3.2f),new Vector3(5,.05f,3),Teal);
            AddFireplace(); AddDesk(); AddGarden(); AddShelves();
            for(int i=0;i<18;i++){float x=-6.4f+(i%9)*1.55f,z=-5.1f+(i/9)*.6f;GameObject star=Part("Floor Star",PrimitiveType.Sphere,new Vector3(x,.08f,z),Vector3.one*.08f,Gold);Object.DestroyImmediate(star.GetComponent<Collider>());}
        }

        private static void AddFireplace()
        {
            Part("Hearth Mantel",PrimitiveType.Cube,new Vector3(-5.1f,1.25f,4.95f),new Vector3(3.1f,2.5f,.8f),new Color(.30f,.16f,.16f));
            Part("Hearth Opening",PrimitiveType.Cube,new Vector3(-5.1f,.75f,4.48f),new Vector3(1.7f,1.3f,.3f),new Color(.08f,.05f,.10f));
            for(int i=0;i<5;i++){GameObject flame=Part("Magic Flame",PrimitiveType.Sphere,new Vector3(-5.55f+i*.23f,.55f+(i%2)*.16f,4.18f),Vector3.one*(.25f+i%2*.08f),i%2==0?Gold:Rose);Object.DestroyImmediate(flame.GetComponent<Collider>());}
        }
        private static void AddDesk()
        {
            Part("Moon Desk",PrimitiveType.Cube,new Vector3(4.7f,1f,4.4f),new Vector3(3.3f,.25f,1.3f),new Color(.27f,.16f,.36f));
            Part("Desk Leg L",PrimitiveType.Cube,new Vector3(3.4f,.45f,4.4f),new Vector3(.25f,1.1f,.8f),Plum);Part("Desk Leg R",PrimitiveType.Cube,new Vector3(6f,.45f,4.4f),new Vector3(.25f,1.1f,.8f),Plum);
            GameObject orb=Part("Focus Orb",PrimitiveType.Sphere,new Vector3(4.7f,1.65f,4.35f),Vector3.one*.58f,Lilac);Object.DestroyImmediate(orb.GetComponent<Collider>());
        }
        private static void AddGarden()
        {
            for(int i=0;i<9;i++){float x=.7f+(i%5)*1.2f,z=-4.8f+(i/5)*1.1f;Part("Glowcap Stem",PrimitiveType.Cylinder,new Vector3(x,.34f,z),new Vector3(.16f,.34f,.16f),Moss);GameObject cap=Part("Glowcap",PrimitiveType.Sphere,new Vector3(x,.76f,z),new Vector3(.58f,.25f,.58f),i%2==0?Teal:Lilac);Object.DestroyImmediate(cap.GetComponent<Collider>());}
        }
        private static void AddShelves()
        {
            Part("Wardrobe",PrimitiveType.Cube,new Vector3(-6.2f,1.35f,-2.5f),new Vector3(1.6f,2.7f,1.2f),new Color(.42f,.23f,.38f));
            for(int i=0;i<4;i++)Part("Book",PrimitiveType.Cube,new Vector3(-6.2f+i*.22f,2.15f,-1.82f),new Vector3(.16f,.6f,.18f),i%2==0?Gold:Teal);
        }
        private static void AddPortals()
        {
            AddPortal("moonbinding_hollow","Moonbinding Hollow",3,new Vector3(-4.6f,1.2f,5.25f),Lilac,new Vector3(28,1,-1));
            AddPortal("emberward_grove","Emberward Grove",5,new Vector3(0f,1.2f,5.25f),Rose,new Vector3(56,1,-1));
            AddPortal("mistcaller_market","Mistcaller Market",10,new Vector3(4.6f,1.2f,5.25f),Teal,new Vector3(84,1,-1));
        }
        private static void AddWorlds()
        {
            AddWorld("Moonbinding Hollow",new Vector3(28,0,0),Lilac,"Elowen",3,"moonbinding_hollow");
            AddWorld("Emberward Grove",new Vector3(56,0,0),Rose,"Pip",5,"emberward_grove");
            AddWorld("Mistcaller Market",new Vector3(84,0,0),Teal,"Mira",10,"mistcaller_market");
            AddWorld("Starweaver Observatory",new Vector3(112,0,0),Gold,"Orin",13,"starweaver_observatory");
        }
        private static void AddWorld(string label,Vector3 centre,Color colour,string npcName,int level,string worldId)
        {
            Part(label+" Island",PrimitiveType.Cylinder,centre+Vector3.down*.55f,new Vector3(12,.55f,12),Color.Lerp(colour,Cream,.45f));
            for(int i=0;i<7;i++){float angle=i*Mathf.PI*2f/7f;Vector3 p=centre+new Vector3(Mathf.Cos(angle)*4.8f,.45f,Mathf.Sin(angle)*4.8f);Part(label+" Crystal",PrimitiveType.Capsule,p,new Vector3(.45f,1.2f,.45f),i%2==0?colour:Gold);}
            Part(label+" Landmark",PrimitiveType.Cylinder,centre+new Vector3(0,1.3f,2.3f),new Vector3(2.1f,1.3f,2.1f),colour);AddLabel(label,centre+new Vector3(0,3.2f,2.3f));
            GameObject npc=Part(npcName,PrimitiveType.Capsule,centre+new Vector3(-2,.9f,-1),new Vector3(.75f,.9f,.75f),Color.Lerp(colour,Color.white,.3f));npc.AddComponent<NpcGuide3D>().Configure(npcName);AddLabel("Talk to "+npcName,npc.transform.position+Vector3.up*1.35f);
            AddPortal("fae_cottage","Return to Cottage",1,centre+new Vector3(3,.2f,-2.5f),Cream,new Vector3(0,1,-.4f));
        }
        private static void AddCraftingAndHousing()
        {
            GameObject bench=Part("Moonwood Crafting Bench",PrimitiveType.Cube,new Vector3(-1.5f,.55f,3.7f),new Vector3(2.4f,1.05f,1.1f),Gold);bench.AddComponent<CraftingStation3D>();AddLabel("Crafting",new Vector3(-1.5f,1.45f,3.7f));
            GameObject holder=new GameObject("Housing Upgrades",typeof(HousingDisplay));
            GameObject shelf=Part("Crafted Glowcap Shelf",PrimitiveType.Cube,new Vector3(-6.1f,1.25f,.7f),new Vector3(1.6f,2.4f,.45f),Teal,holder.transform);
            for(int i=0;i<3;i++){GameObject glow=Part("Shelf Glow",PrimitiveType.Sphere,new Vector3(-.45f+i*.45f,.45f,-.3f),Vector3.one*.22f,i%2==0?Lilac:Gold,shelf.transform);Object.DestroyImmediate(glow.GetComponent<Collider>());}holder.GetComponent<HousingDisplay>().Configure(shelf);
        }
        private static void AddPortal(string id,string label,int level,Vector3 position,Color colour,Vector3 destination)
        {GameObject portal=Part(label+" Portal",PrimitiveType.Cylinder,position,new Vector3(1.35f,.18f,1.35f),colour);portal.AddComponent<WorldPortal3D>().Configure(id,label,level,destination);AddLabel(label+" · Lv"+level,position+Vector3.up*.55f);}

        private static Transform AddPlayer()
        {
            GameObject root=new GameObject("Fae Player",typeof(CharacterController),typeof(CozyPlayerController));root.transform.position=new Vector3(0,.05f,-.4f);CharacterController cc=root.GetComponent<CharacterController>();cc.height=1.65f;cc.radius=.38f;cc.center=new Vector3(0,.82f,0);
            GameObject body=Part("Body",PrimitiveType.Capsule,new Vector3(0,.72f,0),new Vector3(.72f,.72f,.72f),Lilac,root.transform);Object.DestroyImmediate(body.GetComponent<Collider>());
            GameObject head=Part("Head",PrimitiveType.Sphere,new Vector3(0,1.62f,0),Vector3.one*.66f,new Color(.93f,.72f,.63f),root.transform);Object.DestroyImmediate(head.GetComponent<Collider>());
            Part("Cloak",PrimitiveType.Sphere,new Vector3(0,.82f,-.28f),new Vector3(.9f,1.15f,.28f),Plum,root.transform);
            for(int side=-1;side<=1;side+=2){GameObject ear=Part("Fae Ear",PrimitiveType.Sphere,new Vector3(.42f*side,1.68f,0),new Vector3(.42f,.16f,.2f),new Color(.93f,.72f,.63f),root.transform);ear.transform.rotation=Quaternion.Euler(0,0,18f*side);Object.DestroyImmediate(ear.GetComponent<Collider>());}
            return root.transform;
        }
        private static void AddCamera(Transform player)
        {
            GameObject go=new GameObject("Main Camera",typeof(Camera),typeof(AudioListener),typeof(CozyCamera));go.tag="MainCamera";go.transform.position=player.position+new Vector3(0,9,-10);go.transform.LookAt(player.position+Vector3.up*.8f);Camera cam=go.GetComponent<Camera>();cam.fieldOfView=48;cam.backgroundColor=new Color(.16f,.11f,.24f);cam.clearFlags=CameraClearFlags.SolidColor;go.GetComponent<CozyCamera>().SetTarget(player);
        }
        private static void AddDragon(Transform player)
        {
            GameObject root=new GameObject("Aster the Hearth Dragon",typeof(DragonCompanion));root.transform.position=player.position+new Vector3(-1,1,-1);root.GetComponent<DragonCompanion>().SetPlayer(player);
            GameObject body=Part("Dragon Body",PrimitiveType.Sphere,Vector3.zero,new Vector3(.75f,.48f,1f),Rose,root.transform);Object.DestroyImmediate(body.GetComponent<Collider>());
            GameObject head=Part("Dragon Head",PrimitiveType.Sphere,new Vector3(0,.28f,.55f),Vector3.one*.58f,Gold,root.transform);Object.DestroyImmediate(head.GetComponent<Collider>());
            for(int side=-1;side<=1;side+=2){GameObject wing=Part("Wing",PrimitiveType.Sphere,new Vector3(.48f*side,.18f,-.05f),new Vector3(.65f,.12f,.72f),Lilac,root.transform);wing.transform.rotation=Quaternion.Euler(0,0,22f*side);Object.DestroyImmediate(wing.GetComponent<Collider>());}
        }
        private static void AddStation(string id,string label,string fallback,QuestType type,int minutes,Reward reward,Vector3 position,Color colour)
        {
            GameObject root=Part(label,PrimitiveType.Cylinder,position,new Vector3(1.15f,.42f,1.15f),colour);root.AddComponent<TaskStation3D>().Configure(id,fallback,type,minutes,reward);
            GameObject glow=Part("Glow",PrimitiveType.Sphere,position+Vector3.up*.75f,Vector3.one*.42f,Color.Lerp(colour,Color.white,.35f));Object.DestroyImmediate(glow.GetComponent<Collider>());AddLabel(label,position+Vector3.up*1.35f);
        }
        private static void AddLabel(string text,Vector3 position)
        {GameObject go=new GameObject(text,typeof(TextMesh));go.transform.position=position;go.transform.rotation=Quaternion.Euler(35,0,0);TextMesh mesh=go.GetComponent<TextMesh>();mesh.text=text;mesh.anchor=TextAnchor.MiddleCenter;mesh.alignment=TextAlignment.Center;mesh.fontSize=56;mesh.characterSize=.06f;mesh.color=Color.white;}
        private static GameObject Part(string name,PrimitiveType type,Vector3 position,Vector3 scale,Color colour,Transform parent=null)
        {GameObject go=GameObject.CreatePrimitive(type);go.name=name;if(parent){go.transform.SetParent(parent);go.transform.localPosition=position;}else go.transform.position=position;go.transform.localScale=scale;go.GetComponent<Renderer>().sharedMaterial=Material(colour);return go;}
        private static Material Material(Color colour)
        {Shader shader=Shader.Find("Unlit/Color")??Shader.Find("Standard");Material material=new Material(shader);material.color=colour;if(material.HasProperty("_Smoothness"))material.SetFloat("_Smoothness",.35f);return material;}
    }
}
