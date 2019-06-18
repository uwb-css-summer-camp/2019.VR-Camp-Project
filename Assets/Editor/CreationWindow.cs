using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UWBsummercampAPI;
using Valve.VR.InteractionSystem;

public class DirNode
{
    public DirNode [] childDirs {get; set;}
    public string name { get; set; }
    public bool foldOut { get; set; }
}

[InitializeOnLoad]
public class CreationWindow : EditorWindow
{
    //Active Object
    static GameObject selectedObject;
    static MonoBehaviour selectedScript;
    static string scriptTypeLabel;

    //Active Scene
    static Scene selectedScene;
    static int selectedSceneIdx;
    static int newSceneIdx;
    static string scenePath = "Scenes/";
    static string[] sceneNames;

    //scripts Handle
    static int selectedScriptIdx;
    static int newScriptIdx;
    static string customScriptsPath = "API/UWBsummercampAPI/CustomScripts/";
    static string[] customScriptNames;
    static bool justAwake = true;

    //Network Variables -- Excluded
    //static networkManagerSummerCamp netManager;
    //static string roomName;
    //static bool isPrivate;
    //static int goalPoints

    //Folding Section
    static bool objectFold = true;
    static bool scriptFold = true;
    static bool levelFold = false;
    //static bool networkFold = true;
    static bool createSettingsFold = true;
    static bool primitiveObjFold = true;
    static bool customObjFold = true;
    static bool customScriptsFold = true;

    //GUI Specific Variables
    //static string networkStatus
    //static GUIStyle networkStatusStyle
    static Vector2 scrollPane;

    //specific creation settings
    static bool physicalObject = false;
    static bool teleportObject = true;
    static Material defaultMaterial = null;

    //string[] dirs;
    //bool[] dirFold;
    DirNode root;
    //Prefabs to ignore in Custom Objects button population
    static List<string> ignoreName = new List<string>() { "Cube", "Sphere", "Capsule", "Cylinder", "Plane", "Quad",
        "PhysicCube", "PhysicSphere", "PhysicCapsule", "PhysicCylinder", "PhysicPlane", "PhysicQuad", "Head",
        "HandLeft", "HandRight", "LeftHand", "RightHand", "HoloHead", "ViveHead", "VirtualCamera"};

    //Add menu item named "My Window" to the Window menu
    [MenuItem("Window/[UWB] Creation Menu", false, 3)]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(CreationWindow));
    }

    void dirPath(ref DirNode root)
    {
        string [] currDirs = Directory.GetDirectories(root.name);
        //Debug.Log(currDirs.Length);
        root.childDirs = new DirNode[currDirs.Length];
        for(int i = 0; i < currDirs.Length; i++)
        {
            //Debug.Log(currDirs[i]);
            root.childDirs[i] = new DirNode();
            root.childDirs[i].name = currDirs[i];
            root.childDirs[i].name.Replace("\\", "/");
            root.childDirs[i].foldOut = false;
            root.childDirs[i].childDirs = null;
            dirPath(ref root.childDirs[i]);
        }
    }

    void printDirs(ref DirNode root)
    {
        Debug.Log(root.name);
        for(int i = 0; i < root.childDirs.Length; i++)
        {
            printDirs(ref root.childDirs[i]);
        }
    }


    void Awake()
    {

        //dirs = Directory.GetDirectories("Assets/Resources/");
        //dirFold = new bool[dirs.Length];

        //    class DirNode
        //    {
        //    DirNode[] childDirs;
        //    string name;
        //    bool foldOut = false;
        //      }
        root = new DirNode();
        root.name = "Assets/Resources/";
        dirPath(ref root);
        //printDirs(ref root);


    //networkStatusStyle = newGUIStyle();
    //networkStatusStyle.fontStyle = FontStyle.Bold;
    //networkStatusStyle.normal.textColor = Color.black;

    justAwake = true;
        //RefreshNetworkManager();

        selectedScene = EditorSceneManager.GetActiveScene();
        RefreshSceneNames();
        RefreshScriptNames();
    }

    //This happens more than once, including when the play button is pressed
    //  just before the game is actually playing via Application.isPlaying()
    private void OnEnable()
    {
        Awake();
    }

    //RefreshNetworkManager() -- Excluded
    //RefreshNetworkConnection() -- Excluded
    
    void ChangeScene()
    {
        //newScene is a global static variable to prevent instantiation every frame
        selectedSceneIdx = newSceneIdx;

        //If changes to the scene have been made, save them
        if(selectedScene.isDirty)
        {
            EditorSceneManager.SaveScene(selectedScene);
        }

        Debug.Log(Application.dataPath + "/" + scenePath + sceneNames[selectedSceneIdx] + ".unity");

        selectedScene = EditorSceneManager.OpenScene(Application.dataPath + "/" +
            scenePath + sceneNames[selectedSceneIdx] + ".unity");
    }

    void RefreshSceneNames()
    {
        string[] searchFolders = new string[1];
        searchFolders[0] = "Assets/" + scenePath.TrimEnd('/');
        string[] guids = AssetDatabase.FindAssets("t:Scene", searchFolders);
        sceneNames = new string[guids.Length];

        for(int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            string sceneName = path.Split('/').Last().Split('.').First();
            sceneNames[i] = sceneName;
        }

        //foreach(var scene in sceneNames)
        //{
        //    Debug.Log(scene);
        //}
    }

    void RefreshScriptNames()
    {
        string searchFolders = "Assets/" + customScriptsPath.TrimEnd('/');
        DirectoryInfo dir = new DirectoryInfo(searchFolders);
        FileInfo[] info = dir.GetFiles("*.cs");

        customScriptNames = new string[info.Length + 1];
        customScriptNames[0] = "No Script Selected";

        for(int i = 0; i < info.Length; i++)
        {
            customScriptNames[i + 1] = info[i].Name.Substring(0, info[i].Name.Length - 3);
        }
    }

    //This ensures the menu will update whenever an object would normally
    //  update. This means that object names and scripts update properly
    private void OnInspectorUpdate()
    {
        this.Repaint();
    }

    //This describes the look of the window, as well as the functionality of all
    //  of the components, such as what a particular button does
    private void OnGUI()
    {
        //Allows the entire menu to be scrollable if too small
        scrollPane = GUILayout.BeginScrollView(scrollPane);
        
        #region Selected Object stats
        if(Selection.activeGameObject)
        {
            selectedObject = Selection.activeGameObject;
            selectedScript = null;
            scriptTypeLabel = "Object";

            MonoBehaviour[] scriptsOnObject = selectedObject.GetComponents<coreObjectsBehavior>();
            if(scriptsOnObject.Length != 0)
            {
                selectedScript = scriptsOnObject[0];
            }

            //if(scriptsOnObject.Length == 0)
            //{
            //    scriptsOnObject = selectedObject.GetComponents<coreCharacterBehavior>();

            //    if(scriptsOnObject.Length != 0)
            //    {
            //        scriptTypeLabel = "Player";
            //    }
            //}
            //else
            //{
            //    selectedScript = scriptsOnObject[0];
            //}
        }
        else
        {
            selectedObject = null;
            selectedScript = null;
        }
        #endregion

        #region Object Menu
        
        //check if object selected, if not, don't display window
        if(selectedObject == null)
        {
            GUILayout.BeginVertical();
            EditorGUILayout.LabelField("No Object Selected");
            GUILayout.EndVertical();
            objectFold = false;
        }
        else
        {
            objectFold = EditorGUILayout.InspectorTitlebar(objectFold, selectedObject);
        }

        if (objectFold)
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.Space();
            if (selectedObject != null)
            {
                selectedObject.name = EditorGUILayout.TextField(scriptTypeLabel + " Name",
                    selectedObject.name, EditorStyles.objectField);
            }
            EditorGUILayout.Space();
            GUILayout.EndVertical();
        }
        #endregion

        EditorGUILayout.Space();

        #region Script Menu

        if(selectedScript != null && selectedObject != null)
        {
            scriptFold = EditorGUILayout.InspectorTitlebar(scriptFold, selectedScript);
        }
        else
        {
            scriptFold = EditorGUILayout.Foldout(scriptFold, "Scripts", true);
        }

        if (scriptFold)
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.Space();

            if(selectedScript == null && selectedObject != null)
            {
                if(GUILayout.Button("Create Script"))
                {
                    NewScriptWindow.Init(selectedObject);
                }

                #region Custom Objects Menu
                customScriptsFold = EditorGUILayout.Foldout(customScriptsFold, "Custom Scripts", true);
                if(customScriptsFold)
                {
                    GUILayout.BeginVertical(EditorStyles.helpBox);
                    selectedScriptIdx = EditorGUILayout.Popup("Select Custom Script", newScriptIdx, customScriptNames);

                    if(newScriptIdx != selectedScriptIdx)
                    {
                        selectedObject.AddComponent(System.Type.GetType(customScriptNames[selectedScriptIdx] +
                            ", Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"));                
                    }
                    EditorGUILayout.Space();
                    GUILayout.EndVertical();
                }
                #endregion
            }
            else if(selectedScript != null)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(scriptTypeLabel + " Script");
                if(GUILayout.Button("Open Script"))
                {
                    OpenComponentInVisualStudio(selectedScript, 1);
                }
                GUILayout.EndHorizontal();
            }
            EditorGUILayout.Space();
            GUILayout.EndVertical();
        }
        #endregion

        EditorGUILayout.Space();

        #region Level Menu
        levelFold = EditorGUILayout.Foldout(levelFold, "Level Selection", true);
        if(levelFold)
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);
            newSceneIdx = EditorGUILayout.Popup("Level Selection", selectedSceneIdx, sceneNames);
            //if(newSceneIdx != selectedScriptIdx)
            //{
            //    ChangeScene();
            //}
            EditorGUILayout.Space();
            GUILayout.EndVertical();
        }

        if(newSceneIdx != selectedSceneIdx)
        {
            ChangeScene();
        }
        #endregion
        EditorGUILayout.Space();

        #region Creation Menu
        createSettingsFold = EditorGUILayout.Foldout(createSettingsFold, "Creation Settings", true);
        if(createSettingsFold)
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.Space();
            //add physics button
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Enable Physics");
            physicalObject = EditorGUILayout.Toggle(physicalObject);
            GUILayout.EndHorizontal();
            //add teleportable button
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Enable Teleportable");
            teleportObject = EditorGUILayout.Toggle(teleportObject);
            GUILayout.EndHorizontal();

            EditorGUILayout.Space();
            GUILayout.EndVertical();
        }
        #endregion

        EditorGUILayout.Space();

        if(GUILayout.Button("Create Empty Object"))
        {
            Debug.Log("create empty object");
            GameObject empty = new GameObject();
            empty.name = "EmptyObject";
            empty.tag = "Empty";
        }

        #region Primitive Object Menu
        primitiveObjFold = EditorGUILayout.Foldout(primitiveObjFold, "Primitive Objects", true);
        if(primitiveObjFold)
        {
            //each primitive object requires its own special method
            GUILayout.BeginVertical(EditorStyles.helpBox);

            if(GUILayout.Button("Cube"))
            {
                CreateCube();
            }
            else if(GUILayout.Button("Sphere"))
            {
                CreateSphere();
            }
            else if(GUILayout.Button("Capsule"))
            {
                CreateCapsule();
            }
            else if(GUILayout.Button("Cylinder"))
            {
                CreateCylinder();
            }
            else if(GUILayout.Button("Plane"))
            {
                CreatePlane();
            }
            else if(GUILayout.Button("Quad"))
            {
                CreateQuad();
            }
            GUILayout.EndVertical();
        }
        #endregion

        EditorGUILayout.Space();

        #region Custom Objects Menu

        //for(int i = 0; i < dirs.Length; i++)
        //{

        //}



        customObjFold = EditorGUILayout.Foldout(customObjFold, "Custom Objects", true);
        List<string> filesAdded = new List<string>();
        createMenus(ref root, ref filesAdded);

        //if (customObjFold)
        //{
        //    GUILayout.BeginVertical(EditorStyles.helpBox);

        //    string[] searchFolders = new string[1];
        //    searchFolders[0] = "Assets/Resources";
        //    string[] guids = AssetDatabase.FindAssets("t:Prefab", searchFolders);

        //    for(int i = 0; i < guids.Length; i++)
        //    {
        //        string path = AssetDatabase.GUIDToAssetPath(guids[i]);
        //        string btnName = path.Split('/').Last().Split('.').First();
        //        string objName = path.Substring(path.IndexOf('/', path.IndexOf('/') + 1) + 1);

        //        if(objName.Equals(""))
        //        {
        //            objName = path.Split('/').Last();
        //        }
        //        objName = objName.Split('.').First();

        //        if(btnName.Length > 6 && btnName.Substring(0,6).Equals("Physic"))
        //        {
        //            goto skipButton; //Essentially 'continue'
        //        }

        //        for(int j = 0; j < ignoreName.Count; j++)
        //        {
        //            if(btnName == ignoreName[j])
        //            {
        //                goto skipButton; //cannot add 'continue' properly for this
        //            }
        //        }

        //        if(GUILayout.Button(btnName))
        //        {
        //            CreateObj(objName, path);
        //        }
        //        skipButton:;
        //    }
            //GUILayout.EndVertical();
        //}
        #endregion

        GUILayout.EndScrollView();
    }

    public static void createMenus(ref DirNode root, ref List<string> filesAdded)
    {
        for(int n = 0; n < root.childDirs.Length; n++)
        {
            string currentName = root.childDirs[n].name;
            currentName = currentName.Replace("\\", "/");
            int lastIndex = currentName.LastIndexOf("/") > currentName.LastIndexOf("\\") ?
                currentName.LastIndexOf("/") : currentName.LastIndexOf("\\");
            //Debug.Log("Last Index: " + lastIndex);
            //Debug.Log("Current Name Length: " + currentName.Length);
            root.childDirs[n].foldOut = EditorGUILayout.Foldout(root.childDirs[n].foldOut, currentName.Substring(lastIndex + 1), true);
            //root.childDirs[n].foldOut = EditorGUILayout.Foldout(root.childDirs[n].foldOut, currentName, true);
            if (root.childDirs[n].foldOut)
            {
                GUILayout.BeginVertical(EditorStyles.helpBox);

                createMenus(ref root.childDirs[n], ref filesAdded);
                //if(root.childDirs[n].childDirs != null)
                //{
                //    GUILayout.EndVertical();
                //    continue;
                //} 

                string[] searchFolders = new string[1];
                searchFolders[0] = currentName;
                string[] guids = AssetDatabase.FindAssets("t:Prefab", searchFolders);
               
                for (int i = 0; i < guids.Length; i++)
                {

                    string path = AssetDatabase.GUIDToAssetPath(guids[i]);

                    int pathLastIndex = path.LastIndexOf("/") > path.LastIndexOf("\\") ?
                        path.LastIndexOf("/") : path.LastIndexOf("\\");
                    //if(filesAdded.IndexOf(path) != -1)
                    //{
                    //    continue;
                    //}
                    //filesAdded.Add(path);
                    //Debug.Log("Asset Path: " + path);

                    //Debug.Log("Current Name: " + currentName);
                    string checkPath = path.Substring(0, pathLastIndex);
                    //Debug.Log("CheckPath: " + checkPath);
                    if (checkPath != currentName) { continue; }

                    string btnName = path.Split('/').Last().Split('.').First();
                    string objName = path.Substring(path.IndexOf('/', path.IndexOf('/') + 1) + 1);

                    if (objName.Equals(""))
                    {
                        objName = path.Split('/').Last();
                    }
                    objName = objName.Split('.').First();

                    if (btnName.Length > 6 && btnName.Substring(0, 6).Equals("Physic"))
                    {
                        goto skipButton; //Essentially 'continue'
                    }

                    for (int j = 0; j < ignoreName.Count; j++)
                    {
                        if (btnName == ignoreName[j])
                        {
                            goto skipButton; //cannot add 'continue' properly for this
                        }
                    }

                    if (GUILayout.Button(btnName))
                    {
                        CreateObj(objName, path);
                    }
                skipButton:;
                }
                GUILayout.EndVertical();
            }

       
        }
    }

    public static void CreateObj(string objName, string path)
    {
        GameObject asset = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject)) as GameObject;
        GameObject obj = Instantiate(asset) as GameObject;

        //required that objects with non-kinematic rigidbody 
        //have convex mesh collider
        if(obj.GetComponent<MeshCollider>())
        {
            obj.GetComponent<MeshCollider>().convex = true;
        }
        else
        {
            obj.AddComponent<MeshCollider>();
            obj.GetComponent<MeshCollider>().convex = true;
        }

        MeshCollider[] childColliders = obj.GetComponentsInChildren<MeshCollider>();
        foreach(var col in childColliders)
        {
            col.convex = true;
        }

        //create teleport area
        //GameObject teleportArea = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject)) as GameObject;
        //addTeleportToObject(teleportArea, obj);
        InitShape(obj, objName);
    }

    #region Create Primitive Methods
    [MenuItem("GameObject/3D Object/Cube", false, 0)]
    public static void CreateCube()
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);

        //add teleport area
       // GameObject teleportArea = GameObject.CreatePrimitive(PrimitiveType.Cube);
        //addTeleportToObject(teleportArea, cube);

        InitBasicShape(cube, "Cube");
    }

    [MenuItem("GameObject/3D Object/Sphere", false, 1)]
    public static void CreateSphere()
    {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);

        //add teleport area
        //GameObject teleportArea = GameObject.CreatePrimitive(PrimitiveType.Sphere);
       // addTeleportToObject(teleportArea, sphere);

        InitBasicShape(sphere, "sphere");
    }

    [MenuItem("GameObject/3D Object/Capsule", false, 2)]
    public static void CreateCapsule()
    {
        GameObject capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        InitBasicShape(capsule, "capsule");
    }

    [MenuItem("GameObject/3D Object/Cylinder", false, 3)]
    public static void CreateCylinder()
    {
        GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        InitBasicShape(cylinder, "cylinder");
    }

    [MenuItem("GameObject/3D Object/Plane", false, 4)]
    public static void CreatePlane()
    {
        GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        //GameObject teleportArea = GameObject.CreatePrimitive(PrimitiveType.Plane);
        //addTeleportToObject(teleportArea, plane);
        InitBasicShape(plane, "plane");
    }

    [MenuItem("GameObject/3D Object/Quad", false, 5)]
    public static void CreateQuad()
    {
        if(physicalObject)
        {
            Debug.LogWarning("No available collider for [Quad]; skipping creation");
            return;
        }

        GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        InitBasicShape(quad, "quad");

        //quads have a different default spawn behavior
        quad.transform.LookAt(GetViewCenterWorldPos(0));
        quad.transform.Rotate(0, 180, 0);
    }
    #endregion

    //enables object to be teleported to by making the child of the object overlay the
    //parent and be the teleport area
    //public static void addTeleportToObject(GameObject child, GameObject parent)
    //{
    //    child.AddComponent<TeleportArea>();
    //    //position child so it exactly overlays the parent
    //    child.transform.localScale = parent.transform.localScale;
    //    child.transform.parent = parent.transform;
    //    child.transform.position = parent.transform.position;
    //    //teleport area is slightly higher than parent object to avoid collisions
    //    child.transform.position += new Vector3(0, .01f, 0);
    //}

    public static void InitBasicShape(GameObject obj, string prefabName)
    {
        obj.transform.localScale = new Vector3(1f, 1f, 1f);
        Renderer objRenderer = obj.GetComponent<Renderer>();
        if(defaultMaterial == null)
        {
            defaultMaterial = Resources.Load("BasicAssets/SpawnedObjectMaterial") as Material;
        }
        objRenderer.material = defaultMaterial;

        InitShape(obj, prefabName);
    }

    //all objects that are created through creation window use this method
    public static void InitShape(GameObject obj, string prefabName)
    {
        if(Selection.activeTransform != null)
        {
            //eliminates parent-child relationship, still allows initial position to be the same
            obj.transform.position = Selection.activeTransform.position;
        }
        else
        {
            Vector3 tmpPos = new Vector3(0, 1, 0);
            obj.transform.position = tmpPos;
        }

        //-------------commented out to handle controller collisions-----------
        //if(physicalObject)
        //{
        //    if(obj.GetComponent<Rigidbody>() == null)
        //    {
        //        obj.AddComponent<Rigidbody>();
        //    }
        //    obj.name = "Physic" + obj.name;
        //}
        //-------------------------------------------------------------------

        if(obj.GetComponent<Rigidbody>() == null)
        {
            obj.AddComponent<Rigidbody>();
            if(physicalObject)
            {
                obj.name = "Physic" + obj.name;
            }
            else
            {
                obj.GetComponent<Rigidbody>().isKinematic = true;
            }
        }


        //Add script that makes the object teleportable
        if(teleportObject)
        {
            obj.AddComponent<Teleportable>();
        }

        //allows game to register when player touches this object
        obj.AddComponent<Interactable>();
        
        //obj.tag = "GameObject";
        

        //make child of object being created that allows teleportation to the object
        //addTeleportArea((GameObject)obj);
    }

    ////add teleportation area to the object
    //private static void addTeleportArea(GameObject obj, bool isPrimitive)
    //{
    //    GameObject teleportArea;
    //    if(isPrimitive)
    //    {
    //        //teleportArea = GameObject.CreatePrimitive(pr)
    //    }
    //}

    private static Vector3 GetViewCenterWorldPos()
    {
        Vector3 worldPos;

        try
        {
            Ray worldRay = SceneView.lastActiveSceneView.camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 1.0f));
            worldPos = worldRay.GetPoint(5f);
        }
        catch
        {
            Debug.LogWarning("Creating objects without Scene window open will create them at (0,0,0");
            worldPos = new Vector3(0, 0, 0);
        }
        return worldPos;
    }

    private static Vector3 GetViewCenterWorldPos(float distance)
    {
        Ray worldRay = SceneView.lastActiveSceneView.camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 1.0f));
        Vector3 worldPos = worldRay.GetPoint(distance);
        return worldPos;
    }

    public static void OpenComponentInVisualStudio(MonoBehaviour component, int gotoLine)
    {
        string[] fileNames = Directory.GetFiles(Application.dataPath, component.GetType().ToString() +
            ".cs", SearchOption.AllDirectories);

        if(fileNames.Length > 0)
        {
            string relativepath = "Assets" + fileNames[0].Substring(Application.dataPath.Length);
            AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath<TextAsset>(relativepath) as TextAsset, 1);
        }
        else
        {
            Debug.LogError("Error in [OpenComponentInVisualStudio()]: File Not Found");
        }
    }
}
