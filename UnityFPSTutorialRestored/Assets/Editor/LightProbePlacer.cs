using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

/// <summary>
/// Editor window GUI for assisting in placing Light Probes.
/// </summary>
public class LightProbePlacer : EditorWindow
{
    //GUI related
    private int tabIndex;
    private bool showHelp;
    private static int guiSectionSpacePixels = 10;
    private static string[] tabNames = new string[] { "Step 1", "Step 2", "Step 3", "Step 4", "Cleanup", "Display" };

    //scene related
    //private static LightProbePlacerObject lightProbePlacerObject; //our main script
    private static string lightProbePlacerObjectNewName = "EDITOR_LightProbePlacerObject";
    private static string sceneMeshColliderParentName = "EDITOR_SceneMeshColliders"; //name of the generated meshes group
    private GameObject lightProbePlacerGameObject;

    //main fields
    private LightProbeGroup lightProbeGroup;
    private Vector3 boundingBoxPos;
    private float boundingBoxWidth;
    private float boundingBoxHeight;
    private float boundingBoxLength;
    private int xProbeCount;
    private int yProbeCount;
    private int zProbeCount;
    private float probeIntersectionRadius;
    private float probeGizmoSize;
    private float probeColorDifference;
    private List<GameObject> probePositions;
    private List<GameObject> probesToRemove;
    private bool showCalculatedPositions;
    private bool showChildPositions;
    private bool showIntersectionSphere;

    //add a menu item at the top of the unity editor toolbar
    [MenuItem("Custom Tools/Light Probe Placer")]
    public static void ShowWindow()
    {
        //get the window and open it
        GetWindow(typeof(LightProbePlacer));
    }

    /// <summary>
    /// GUI display function for the window
    /// </summary>
    void OnGUI()
    {
        //if there is an object selected
        if (Selection.activeGameObject)
        {
            //if the selection doesn't have a 'LightProbePlacerObject'
            if (!Selection.activeGameObject.name.Equals(lightProbePlacerObjectNewName))
            {
                //ask the user to initalize
                if (GUILayout.Button("Initalize"))
                {
                    //find a 'LightProbePlacerObject'
                    lightProbePlacerGameObject = GameObject.Find(lightProbePlacerObjectNewName);

                    //if there isn't one in the scene, then create one
                    if (!lightProbePlacerGameObject)
                    {
                        lightProbePlacerGameObject = new GameObject(lightProbePlacerObjectNewName);
                    }

                    //assign it to the selection
                    Selection.activeGameObject = lightProbePlacerGameObject;
                }

                //don't continue with the rest of the function
                return;
            }
            else
            {
                //if the selection does have a 'LightProbePlacerObject' then assign it
                lightProbePlacerGameObject = Selection.activeGameObject;
            }
        }

        //if for whatever reason we still don't have a 'LightProbePlacerObject'
        if (lightProbePlacerGameObject == null)
        {
            //ask the user to initalize
            if (GUILayout.Button("Initalize"))
            {
                //find a 'LightProbePlacerObject'
                lightProbePlacerGameObject = GameObject.Find(lightProbePlacerObjectNewName);

                //if there isn't one in the scene, then create one
                if (!lightProbePlacerGameObject)
                {
                    lightProbePlacerGameObject = new GameObject(lightProbePlacerObjectNewName);
                }

                //assign it to the selection
                Selection.activeGameObject = lightProbePlacerGameObject;
            }

            //don't continue with the rest of the function
            return;
        }

        //if there isn't a light probe group assigned to the scene object then ask the user to assign it
        if (lightProbeGroup == null)
        {
            GUILayout.Label("Assign a 'Light Probe Group'.", EditorStyles.helpBox);

            //object GUI field
            GUILayout.Label("Light Probe Group", EditorStyles.label);
            lightProbeGroup = (LightProbeGroup)EditorGUILayout.ObjectField(lightProbeGroup, typeof(LightProbeGroup), true);//(LightProbeGroup)EditorGUILayout.ObjectField(lightProbeGroup, null);

            //don't continue with the rest of the function
            return;
        }

        //create a toolbar to organize our mess
        tabIndex = GUILayout.Toolbar(tabIndex, tabNames);

        //tab - step 1
        if (tabIndex == 0)
        {
            //tab title
            GUILayout.Space(guiSectionSpacePixels);
            GUILayout.Label("Step 1 - Probe Placement", EditorStyles.whiteLargeLabel);
            GUILayout.Label("This is where we define a volume's dimensions and position to generate probe positions.", EditorStyles.helpBox);
            GUILayout.Space(guiSectionSpacePixels);

            //section title
            GUILayout.Label("[Probe Bounding Box]", EditorStyles.boldLabel);

            if (showHelp)
                GUILayout.Label("Probe Bounding Box - Defining the dimensions and position of the probe volume.", EditorStyles.helpBox);

            GUILayout.Space(guiSectionSpacePixels);

            //section fields
            boundingBoxPos = EditorGUILayout.Vector3Field("Position", boundingBoxPos);
            boundingBoxWidth = EditorGUILayout.FloatField("Width", boundingBoxWidth);
            boundingBoxHeight = EditorGUILayout.FloatField("Height", boundingBoxHeight);
            boundingBoxLength = EditorGUILayout.FloatField("Length", boundingBoxLength);

            //section title
            GUILayout.Space(guiSectionSpacePixels);
            GUILayout.Label("[Probe Volume Resolution]", EditorStyles.boldLabel);

            if (showHelp)
                GUILayout.Label("Probe Volume Resolution - Defining how many probes are going to be sampled within the volume.", EditorStyles.helpBox);

            GUILayout.Space(guiSectionSpacePixels);

            //fields
            xProbeCount = EditorGUILayout.IntField("X Probe Count", xProbeCount);
            yProbeCount = EditorGUILayout.IntField("Y Probe Count", yProbeCount);
            zProbeCount = EditorGUILayout.IntField("Z Probe Count", zProbeCount);

            GUILayout.Space(guiSectionSpacePixels);

            //functions
            if (GUILayout.Button("Generate Probe Positions"))
            {
                GenerateGameObjects();
                GetChildren();
            }
        }
        //tab - step 2
        else if (tabIndex == 1)
        {
            //tab title
            GUILayout.Space(guiSectionSpacePixels);
            GUILayout.Label("Step 2 - Removing Invalid Probes", EditorStyles.whiteLargeLabel);
            GUILayout.Label("This is where after generating probe positions, we do a couple of things in order to remove probes that 'intersect' with geometry, or are probes that float over the void.", EditorStyles.helpBox);
            GUILayout.Space(guiSectionSpacePixels);

            //section title
            GUILayout.Label("[Preparation]", EditorStyles.boldLabel);
            GUILayout.Space(guiSectionSpacePixels);

            //section functions
            if (GUILayout.Button("Generate Scene Mesh Colliders"))
            {
                GenerateSceneMeshColliders();
            }

            if (showHelp)
                GUILayout.Label("Generate Scene Mesh Colliders - Generates physics Mesh Colliders for all gameobjects within the scene that are enabled, and that contribute to the lightmap. This is meant to be used to help with removing invalid probes as physics raycast functions are used for it.", EditorStyles.helpBox);

            //section title
            GUILayout.Space(guiSectionSpacePixels);
            GUILayout.Label("[Removing Probes]", EditorStyles.boldLabel);
            GUILayout.Space(guiSectionSpacePixels);

            //section fields
            probeIntersectionRadius = EditorGUILayout.FloatField("Probe Intersection Radius", probeIntersectionRadius);

            //section functions
            if (GUILayout.Button("Remove Intersecting Probes"))
            {
                GetChildren();
                GetntersectingProbes();
                RemoveProbesFromList();
            }

            if (showHelp)
                GUILayout.Label("Remove Intersecting Probes - Removes probes that intersect scene geometry, or are inside it. This is to prevent incorrect occlusion and shadowing. 'Probe Intersection Radius' determines how far it checks for intersecting geometry.", EditorStyles.helpBox);

            if (GUILayout.Button("Remove Floating Over Void Probes"))
            {
                GetChildren();
                GetFloatingProbes();
                RemoveProbesFromList();
            }

            if (showHelp)
                GUILayout.Label("Remove Floating Over Void Probes - Removes probes that are floating over the void and not scene geometry.", EditorStyles.helpBox);
        }
        //tab - step 3
        else if (tabIndex == 2)
        {
            //tab title
            GUILayout.Space(guiSectionSpacePixels);
            GUILayout.Label("Step 3 - Add Probe Positions to Group", EditorStyles.whiteLargeLabel);
            GUILayout.Label("This is where after generating and removing invalid probes, we add our final probe positions to the given light probe group.", EditorStyles.helpBox);
            GUILayout.Space(guiSectionSpacePixels);

            //section fields
            GUILayout.Label("Light Probe Group", EditorStyles.label);
            lightProbeGroup = (LightProbeGroup)EditorGUILayout.ObjectField(lightProbeGroup, null);

            //section functions
            if (GUILayout.Button("Add New Probes to Light Probe Group"))
            {
                GetChildren();
                AddProbesToGroup(lightProbeGroup);
            }

            if (showHelp)
                GUILayout.Label("Add New Probes to Light Probe Group - Adds the generated light probe positions into the Light Probe Group.", EditorStyles.helpBox);

            if (GUILayout.Button("Clear Existing Light Probe Group positions"))
            {
                ClearProbes(lightProbeGroup);
            }

            if (showHelp)
                GUILayout.Label("Clear Existing Light Probe Group positions - Removes all the existing probe positions with the Light Probe Group.", EditorStyles.helpBox);
        }
        //tab - step 4 (optional)
        else if (tabIndex == 3)
        {
            //tab title
            GUILayout.Space(guiSectionSpacePixels);
            GUILayout.Label("Step 4 - Additional Simplification After Lightmap", EditorStyles.whiteLargeLabel);
            GUILayout.Label("This is an optional step where after lightmapping, we can use the final light probe colors to do a difference check between probes. Removing any probes that happen to share the same lighting if it's too similar. This step assumes that you haven't removed the generated probe positions from the gameobject.", EditorStyles.helpBox);
            EditorGUILayout.HelpBox("This step is optional.", MessageType.Info);
            GUILayout.Space(guiSectionSpacePixels);

            //section fields
            GUILayout.Label("Probe Color Difference Threshold", EditorStyles.label);
            probeColorDifference = EditorGUILayout.Slider(probeColorDifference, 0.0f, 1.0f);

            if (showHelp)
                GUILayout.Label("Probe Color Difference Threshold - Calculates the colors between probes and if the difference is below the threshold it will be removed.", EditorStyles.helpBox);

            //section functions
            if (GUILayout.Button("Simplify Probes After Lightmap"))
            {
                SimplifyProbesAfterLightmap();
            }

            if (showHelp)
                GUILayout.Label("Simplify Probes After Lightmap - Removes probes that don't have much of a difference in color between eachother. This is to help reduce the size of light probe data.", EditorStyles.helpBox);
        }
        //tab - cleanup
        else if (tabIndex == 4)
        {
            //tab title
            GUILayout.Space(guiSectionSpacePixels);
            GUILayout.Label("Cleanup", EditorStyles.whiteLargeLabel);
            GUILayout.Space(guiSectionSpacePixels);

            //functions
            if (GUILayout.Button("Remove Generated Scene Mesh Colliders"))
            {
                RemoveSceneMeshColliders();
            }

            if (showHelp)
                GUILayout.Label("Remove Generated Scene Mesh Colliders - Removes the generated scene mesh colliders by this script that is used for invalidating bad probe positions.", EditorStyles.helpBox);

            if (GUILayout.Button("Remove Generated Probe Positions"))
            {
                RemoveChildren();
            }

            if (showHelp)
                GUILayout.Label("Remove Generated Probe Positions - Removes the generated game object children that are apart of the gameobject.", EditorStyles.helpBox);
        }
        //tab - display
        else if (tabIndex == 5)
        {
            //tab title
            GUILayout.Space(guiSectionSpacePixels);
            GUILayout.Label("Display", EditorStyles.whiteLargeLabel);
            GUILayout.Space(guiSectionSpacePixels);

            showHelp = EditorGUILayout.Toggle("Show Help Boxes", showHelp);

            if (GUILayout.Button("Update Children"))
            {
                GetChildren();
            }

            //section title
            GUILayout.Space(guiSectionSpacePixels);
            GUILayout.Label("[Info]", EditorStyles.boldLabel);
            GUILayout.Space(guiSectionSpacePixels);

            //section info
            GUILayout.Label("Calculated Probe Count: " + GetCalculatedProbeCount().ToString(), EditorStyles.label);
            GUILayout.Label("Current Probe Count: " + lightProbePlacerGameObject.transform.childCount.ToString(), EditorStyles.label);

            //section title
            GUILayout.Space(guiSectionSpacePixels);
            GUILayout.Label("[Unity Gizmo Display Settings]", EditorStyles.boldLabel);
            GUILayout.Space(guiSectionSpacePixels);

            //section fields
            probeGizmoSize = EditorGUILayout.FloatField("Probe Gizmo Size", probeGizmoSize);
            showCalculatedPositions = EditorGUILayout.Toggle("Show Calculated Positions", showCalculatedPositions);
            showChildPositions = EditorGUILayout.Toggle("Show Generated Positions", showChildPositions);
            showIntersectionSphere = EditorGUILayout.Toggle("Show Intersection Sphere", showIntersectionSphere);
        }
    }


    /// <summary>
    /// Generates child game objects positioned in a volume.
    /// </summary>
    public void GenerateGameObjects()
    {
        //just used for info
        int count = 1;

        //3d loop for our volume
        for (int x = -xProbeCount / 2; x <= xProbeCount / 2; x++)
        {
            //get the x offset
            float x_offset = boundingBoxWidth / xProbeCount;

            for (int y = -yProbeCount / 2; y <= yProbeCount / 2; y++)
            {
                //get the y offset
                float y_offset = boundingBoxHeight / yProbeCount;

                for (int z = -zProbeCount / 2; z <= zProbeCount / 2; z++)
                {
                    //get the z offset
                    float z_offset = boundingBoxLength / zProbeCount;

                    //get our new probe position
                    Vector3 probePosition = new Vector3(boundingBoxPos.x + (x * x_offset), boundingBoxPos.y + (y * y_offset), boundingBoxPos.z + (z * z_offset));

                    //create a gameobject
                    GameObject newGameObject = new GameObject("probe position " + count);

                    //set its position and parent it to the main gameobject
                    newGameObject.transform.position = probePosition;
                    newGameObject.transform.SetParent(lightProbePlacerGameObject.transform);

                    //increment the counter variable
                    count++;
                }
            }
        }
    }

    /// <summary>
    /// returns an array of all the gameobject positions in 'probePositions'
    /// </summary>
    /// <returns></returns>
    public List<Vector3> GetProbePositions()
    {
        //initalize an array
        List<Vector3> positions = new List<Vector3>();

        //loop through all of the child gameobjects
        foreach (GameObject probePosition in probePositions)
        {
            //add its position to the array
            positions.Add(probePosition.transform.position);
        }

        //return the array
        return positions;
    }

    /// <summary>
    /// uses 'probePositions' and fills up the array with each gameobject child's position.
    /// </summary>
    public void GetChildren(bool clearArray = true)
    {
        //initalize our list of probe position gameobjects if its null
        if (probePositions == null)
            probePositions = new List<GameObject>();

        //clear the array
        if (clearArray)
            probePositions.Clear();

        //loop through all of the children
        for (int i = 0; i < lightProbePlacerGameObject.transform.childCount; i++)
        {
            //add the position of the gameobject child
            probePositions.Add(lightProbePlacerGameObject.transform.GetChild(i).gameObject);
        }
    }

    /// <summary>
    /// uses 'probePositions' and does a physics overlap sphere check to get probes that intresect with scene objects. If it does then the gameobject of that probe is added to 'probesToRemove'.
    /// </summary>
    public void GetntersectingProbes()
    {
        //get all of the game object children
        GetChildren();

        //loop through all of the children
        foreach (GameObject probe in probePositions)
        {
            //get a list of any colliders that are within the intersection radius
            List<Collider> colliders = new List<Collider>(Physics.OverlapSphere(probe.transform.position, probeIntersectionRadius));

            //if there are any colliders we found
            if (colliders.Count > 0)
            {
                //add the probe gameobject to the list of probes that will be removed
                probesToRemove.Add(probe);
            }
        }
    }

    /// <summary>
    /// Get probes that are floating over the void
    /// </summary>
    public void GetFloatingProbes()
    {
        //get all of the game object children
        GetChildren();

        //loop through the children
        foreach (GameObject probe in probePositions)
        {
            //create a downward pointing ray
            Ray ray = new Ray(probe.transform.position, Vector3.down);

            //do an infinte raycast downward
            if (Physics.Raycast(ray, int.MaxValue) == false)
            {
                //if we didn't hit anything then add it to the probes to remove
                probesToRemove.Add(probe);
            }
        }
    }

    /// <summary>
    /// Deletes any probes that are in 'probesToRemove'
    /// </summary>
    public void RemoveProbesFromList()
    {
        //loop through the array backwards
        //for(int i = probesToRemove.Count - 1; i >= 0; --i)
        for (int i = 0; i < probesToRemove.Count; i++)
        {
            DestroyImmediate(probesToRemove[i].gameObject);
        }
    }

    /// <summary>
    /// Adds all of the child gameobject probes to a Light Probe Group
    /// </summary>
    /// <param name="lightProbeGroup"></param>
    public void AddProbesToGroup(LightProbeGroup lightProbeGroup)
    {
        //get the probe positions from the light probe group
        List<Vector3> groupProbePositions = new List<Vector3>(lightProbeGroup.probePositions);

        //get our gameobject children
        List<Vector3> newProbePositions = GetProbePositions();

        //loop through our children
        foreach (Vector3 newPosition in newProbePositions)
        {
            //add the position to the group
            groupProbePositions.Add(newPosition);
        }

        //assign the position array to the light probe group
        lightProbeGroup.probePositions = groupProbePositions.ToArray();
    }

    /// <summary>
    /// Removes all light probe positions with a light probe group.
    /// </summary>
    /// <param name="lightProbeGroup"></param>
    public void ClearProbes(LightProbeGroup lightProbeGroup)
    {
        //initalize an empty array over it
        lightProbeGroup.probePositions = new Vector3[0];
    }

    /// <summary>
    /// Deletes all child gameobjects
    /// </summary>
    public void RemoveChildren()
    {
        //loop through the array backwards so we can remove as we go
        for (int i = lightProbePlacerGameObject.transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(lightProbePlacerGameObject.transform.GetChild(i).gameObject);
        }
    }

    /// <summary>
    /// Get the amount of calculated probes
    /// </summary>
    /// <returns></returns>
    public int GetCalculatedProbeCount()
    {
        int amount = 0;

        for (int x = -xProbeCount / 2; x <= xProbeCount / 2; x++)
        {
            for (int y = -yProbeCount / 2; y <= yProbeCount / 2; y++)
            {
                for (int z = -zProbeCount / 2; z <= zProbeCount / 2; z++)
                {
                    amount++;
                }
            }
        }

        return amount;
    }

    /// <summary>
    /// Remove our generated scene mesh collider object and its children
    /// </summary>
    public void RemoveSceneMeshColliders()
    {
        //find the gameobject (should have the same name since we should be the only one making it)
        GameObject sceneMeshColliders = GameObject.Find(sceneMeshColliderParentName);

        //if we didn't find it then don't do anything
        if (!sceneMeshColliders)
            return;

        //if we found it, then destroy it
        DestroyImmediate(sceneMeshColliders);
    }

    /// <summary>
    /// Generate a scene mesh collider object and 
    /// </summary>
    public void GenerateSceneMeshColliders()
    {
        //find the gameobject (should have the same name since we should be the only one making it)
        GameObject meshColliderParent = GameObject.Find(sceneMeshColliderParentName);

        //if we found an existing one then don't do anything
        if (meshColliderParent)
            return;

        //create a new gameobject
        meshColliderParent = new GameObject(sceneMeshColliderParentName);

        //find all of the mesh renderers that are in the scene
        List<MeshRenderer> meshRenderersInScene = new List<MeshRenderer>(FindObjectsOfType<MeshRenderer>());

        //loop through all of the mesh renderers that we found
        foreach (MeshRenderer meshRenderer in meshRenderersInScene)
        {
            //get the static flags of the mesh renderer gameobject
            StaticEditorFlags staticFlags = GameObjectUtility.GetStaticEditorFlags(meshRenderer.gameObject);

            //check if the mesh renderer is enabled, if the gameobject is active, and if its marked for contributing to the lightmap
            if (meshRenderer.enabled && meshRenderer.gameObject.activeInHierarchy && staticFlags == StaticEditorFlags.ContributeGI)
            {
                //create the base gameobject that will contain the collider
                GameObject colliderBase = new GameObject(meshRenderer.gameObject.name);

                //set its transformation to match the original
                colliderBase.transform.position = meshRenderer.transform.position;
                colliderBase.transform.rotation = meshRenderer.transform.rotation;
                colliderBase.transform.SetParent(meshColliderParent.transform);
                colliderBase.transform.localScale = meshRenderer.transform.lossyScale;

                //get the mesh filter component to get the mesh
                MeshFilter meshFilter = meshRenderer.GetComponent<MeshFilter>();

                //add a mesh collider component and assign the mesh
                MeshCollider meshCollider = colliderBase.AddComponent<MeshCollider>();
                meshCollider.sharedMesh = meshFilter.sharedMesh;
            }
        }
    }

    /// <summary>
    /// Gets the difference between two colors. If the difference is greater than 'probeColorDifference' it returns true.
    /// </summary>
    /// <param name="one"></param>
    /// <param name="two"></param>
    /// <returns></returns>
    public bool GetColorDifference(Color one, Color two)
    {
        //get the differences from each channel
        float r_difference = Mathf.Abs(one.r - two.r);
        float g_difference = Mathf.Abs(one.g - two.g);
        float b_difference = Mathf.Abs(one.b - two.b);
        float a_difference = Mathf.Abs(one.a - two.a);

        //check each difference and return true if one of them is greater
        bool result = (r_difference > probeColorDifference) || (g_difference > probeColorDifference) || (b_difference > probeColorDifference) || (a_difference > probeColorDifference);

        return result;
    }

    /// <summary>
    /// Gets the difference between two probes. If the difference is greater than 'probeColorDifference' it returns true.
    /// </summary>
    /// <param name="probe1"></param>
    /// <param name="probe2"></param>
    /// <returns></returns>
    public bool GetDifferenceBetweenProbes(SphericalHarmonicsL2 probe1, SphericalHarmonicsL2 probe2)
    {
        //probe directions to evaluate
        Vector3[] probeDirections = new Vector3[]
        {
            Vector3.up,
            Vector3.down,
            Vector3.forward,
            Vector3.back,
            Vector3.left,
            Vector3.right
        };

        //create our arrays that will store the colors from the sampled directions
        Color[] probe1Colors = new Color[probeDirections.Length];
        Color[] probe2Colors = new Color[probeDirections.Length];

        //evaluate the colors from both probes
        probe1.Evaluate(probeDirections, probe1Colors);
        probe2.Evaluate(probeDirections, probe2Colors);

        //our local result
        bool result = false;

        //check the color differences of the sampled directions between both probes
        for (int i = 0; i < probeDirections.Length; i++)
        {
            //if the difference is still under the threshold, keep checking
            if (!result)
            {
                //get the result of the check
                result = GetColorDifference(probe1Colors[i], probe2Colors[i]);
            }
        }

        //return the result
        return result;
    }

    /// <summary>
    /// Removes probes that are the 'same' visually.
    /// </summary>
    public void SimplifyProbesAfterLightmap()
    {
        //get a temp mesh renderer component for sampling a light probe
        MeshRenderer tempMesh = lightProbePlacerGameObject.GetComponent<MeshRenderer>();

        if (!tempMesh)
            tempMesh = lightProbePlacerGameObject.AddComponent<MeshRenderer>();

        //get the probe positions from the light probe group
        List<Vector3> probePositions = new List<Vector3>(lightProbeGroup.probePositions);

        //initalize an array of probe indexes to remove later
        List<int> probeIndexesToRemove = new List<int>();

        //loop through the probe positions in the light probe group
        for (int i = 0; i < probePositions.Count; i++)
        {
            //make sure that we don't go beyond the bounds of the array
            if (i + 1 < probePositions.Count)
            {
                //get the current probe position and the next one
                Vector3 probePosition1 = probePositions[i];
                Vector3 probePosition2 = probePositions[i + 1];

                //initalize our probe objects
                SphericalHarmonicsL2 probe1;
                SphericalHarmonicsL2 probe2;

                //get the probes at the given positions
                LightProbes.GetInterpolatedProbe(probePosition1, tempMesh, out probe1);
                LightProbes.GetInterpolatedProbe(probePosition2, tempMesh, out probe2);

                //look at the difference between the two. If there is no difference then add it to the probes to remove
                if (GetDifferenceBetweenProbes(probe1, probe2) == false)
                {
                    //add the index
                    probeIndexesToRemove.Add(i);
                }
            }
        }

        //if there are no elements in the probes to remove then we don't have to do anything else
        if (probeIndexesToRemove.Count <= 0)
            return;

        //loop through all of the indexes in the array
        foreach (int index in probeIndexesToRemove)
        {
            //make sure the index we're trying to remove at is not out of bounds
            if (index < probePositions.Count)
            {
                //remove the probe prosition from the light probe group at the given index
                probePositions.RemoveAt(index);

                //destroy the probe from the children
                DestroyImmediate(lightProbePlacerGameObject.transform.GetChild(index).gameObject);

                Debug.Log(index);
            }
        }

        //assign the modified probe positions array to the light probe group
        lightProbeGroup.probePositions = probePositions.ToArray();
    }

    // Window has been selected
    void OnFocus()
    {
        // Remove delegate listener if it has previously
        // been assigned.
        SceneView.duringSceneGui -= OnSceneGUI;
        // Add (or re-add) the delegate.
        SceneView.duringSceneGui += OnSceneGUI;
    }

    void OnDestroy()
    {
        // When the window is destroyed, remove the delegate
        // so that it will no longer do any drawing.
        SceneView.duringSceneGui -= OnSceneGUI;
    }


    void OnSceneGUI(SceneView sceneView)
    {
        Vector3 directionToCamera_boundingBoxPos = sceneView.camera.transform.position - boundingBoxPos;

        //draw the position and dimensions of the volume
        Handles.color = Color.yellow;
        Handles.DrawSolidDisc(boundingBoxPos, directionToCamera_boundingBoxPos, probeGizmoSize);
        Handles.DrawWireCube(boundingBoxPos, new Vector3(boundingBoxWidth, boundingBoxHeight, boundingBoxLength));

        //if the user wants to see the calculated positions
        if (showCalculatedPositions)
        {
            //make it white to diffrentiate it 
            Handles.color = Color.white;

            for (int x = -xProbeCount / 2; x <= xProbeCount / 2; x++)
            {
                float x_offset = boundingBoxWidth / xProbeCount;

                for (int y = -yProbeCount / 2; y <= yProbeCount / 2; y++)
                {
                    float y_offset = boundingBoxHeight / yProbeCount;

                    for (int z = -zProbeCount / 2; z <= zProbeCount / 2; z++)
                    {
                        float z_offset = boundingBoxLength / zProbeCount;

                        //get our probe position
                        Vector3 probePosition = new Vector3(boundingBoxPos.x + (x * x_offset), boundingBoxPos.y + (y * y_offset), boundingBoxPos.z + (z * z_offset));

                        //normal to camera
                        Vector3 directionToCamera_probePosition = sceneView.camera.transform.position - probePosition;

                        //draw a sphere at the calculated position
                        Handles.DrawSolidDisc(probePosition, directionToCamera_probePosition, probeGizmoSize);

                        //if the user wants to see the intersection radius then draw a wireframe sphere to represent it
                        if (showIntersectionSphere)
                            Handles.DrawWireDisc(probePosition, directionToCamera_probePosition, probeIntersectionRadius);
                    }
                }
            }
        }

        //if the user wants to see the child game object positions
        if (showChildPositions)
        {
            //make it green to diffrentiate it 
            Handles.color = Color.green;

            //get all of the children
            List<Transform> children = new List<Transform>(lightProbePlacerGameObject.transform.GetComponentsInChildren<Transform>());

            //if there is a reference of the main gameobject then remove it
            if (children.Contains(lightProbePlacerGameObject.transform))
                children.Remove(lightProbePlacerGameObject.transform);

            //loop through the children
            foreach (Transform child in children)
            {
                //normal to camera
                Vector3 directionToCamera_childPosition = sceneView.camera.transform.position - child.position;

                //draw a sphere at the calculated position
                Handles.DrawSolidDisc(child.position, directionToCamera_childPosition, probeGizmoSize);

                //if the user wants to see the intersection radius then draw a wireframe sphere to represent it
                if (showIntersectionSphere)
                    Handles.DrawWireDisc(child.position, directionToCamera_childPosition, probeIntersectionRadius);
            }
        }

        HandleUtility.Repaint();
    }
}
