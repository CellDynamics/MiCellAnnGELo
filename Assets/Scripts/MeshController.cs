using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine.UI;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class MeshController : MonoBehaviour
{
    public Slider progressBar, minR, maxR, minG, maxG, alphaSlider;
    public Text progressText, currentFrameText;
    // Stores prefab to duplicate for markers and buttons to disable/enable
    public GameObject markerPrefab, loadTimeseries, loadAnnotations, 
                      saveAnnotations, updateColours;
    // Stores an array of meshes (one for each frame of the loaded timeseries)
    private Mesh[] meshes;
    // Stores an arary of Color32s indexed by [meshNumber, colourMode][vertex] for each mesh
    private Color32[,][] meshColours;
    // Stores a list of vertices that have a marker
    private HashSet<int>[] markers;
    // Stores the colour mode (0 = original, 1 = patches, 2 = scaled, 3 = overlay)
    private int colourMode = 2;
    public float speedInterval = 0.01f;
    private float timeBetweenFrames = 0.1f, timeOfLastUpdate = 0f, frameCutoff = 0.01f;
    // Current frame number being displayed (index to meshes array)
    private int index = 0;
    private bool paused = false, fileError = false;
    private Shader trans, opaque;
    private Material cellMat;

    private void Start()
    {
        // Find shaders and material to use for the cell
        trans = Shader.Find("Custom/TransVertexColour");
        opaque = Shader.Find("Custom/OpaqueVertexColour");
        cellMat = GetComponent<MeshRenderer>().material;
    }

    //-------------------------------------------------------------------------------------------
    //                  PROCESSING LOADED MESHES                                                |
    //-------------------------------------------------------------------------------------------

    private void DisplayMesh()
    {
        // Check that there are meshes loaded and the current index has a mesh
        if (meshes != null && meshes[index] != null)
        {
            // Set colours to use for the mesh
            meshes[index].colors32 = meshColours[index, colourMode];
            // Set object's mesh to display and use for the collider
            GetComponent<MeshFilter>().mesh = meshes[index];
            GetComponent<MeshCollider>().sharedMesh = meshes[index];
            // Set the text to show the mesh being displayed
            currentFrameText.text = "Currently displaying frame: " + (index + 1) 
                                    + " / " + meshes.Length;
            // Change markers to be the current mesh's markers
            ChangeMarkers();
        }
    }

    private IEnumerator ChangeMesh()
    {
        if (meshes != null)
        {
            while (true)
            {
                // Loop through indices
                while (index < meshes.Length)
                {
                    DisplayMesh();
                    // Wait before increasing index again
                    yield return new WaitForSeconds(timeBetweenFrames);
                    index++;
                }
                index = 0;
            }
        }
    }

    private void UnloadMarkers()
    {
        foreach (Transform child in transform)
            GameObject.Destroy(child.gameObject);
    }

    private void ChangeMarkers()
    {
        UnloadMarkers();
        if (markers[index] != null) 
        {
            foreach (var vert in markers[index])
            {
                var obj = Instantiate(markerPrefab, transform);
                obj.transform.localPosition = meshes[index].vertices[vert];
            }
        }
    }

    public void UpdateColours()
    {
        StopCoroutines();
        StartCoroutine(UpdateColoursCoroutine());
    }

    public IEnumerator UpdateColoursCoroutine()
    {
        if (meshes != null)
        {
            SetButtonState(false);
            timeOfLastUpdate = Time.realtimeSinceStartup;
            var rScale = 255.0f / maxR.value;
            var gScale = 255.0f / maxG.value;

            var rMin = minR.value * rScale;
            var gMin = minG.value * gScale;

            progressBar.value = 0;
            progressText.text = "Updating: 0 / " + meshes.Length;

            for (int frameNo = 0; frameNo < meshes.Length; frameNo++)
            {
                if (Time.realtimeSinceStartup - timeOfLastUpdate > 0.01)
                {
                    yield return null;
                    timeOfLastUpdate = Time.realtimeSinceStartup;
                }
                progressBar.value = (float)(frameNo + 1) / meshes.Length;
                progressText.text = "Updating: " + (frameNo + 1) + " / " + meshes.Length;
                UpdateColour(frameNo, rScale, gScale, rMin, gMin);
            }
            progressText.text = "Colours updated";
            SetButtonState(true);
            DisplayMesh();
        }
    }

    public void UpdateColour(int frameNo, float rScale, float gScale, float rMin, float gMin)
    {
        var vertCount = meshColours[frameNo, 2].Length / 2;
        var aVal = alphaSlider.value;
        var aValB = System.Convert.ToByte(aVal);
        if (aVal == 255)
            cellMat.shader = opaque;
        else
            cellMat.shader = trans;
        for (int vertexNo = 0; vertexNo < vertCount; vertexNo++)
        {
            var r = meshColours[frameNo, 0][vertexNo].r * rScale;
            var g = meshColours[frameNo, 0][vertexNo].g * gScale;
            r = r > 255 ? 255 : r;
            g = g > 255 ? 255 : g;
            var colour = meshColours[frameNo, 2][vertexNo];
            if (r < rMin && g < gMin)
                colour = Color.grey;
            else
            {
                colour.b = 0;
                if (r >= rMin)
                    colour.r = System.Convert.ToByte(r);
                else
                    colour.r = 0;

                if (g >= gMin)
                    colour.g = System.Convert.ToByte(g);
                else
                    colour.g = 0;
            }
            colour.a = aValB;
            meshColours[frameNo, 2][vertexNo] = colour;
            meshColours[frameNo, 2][vertexNo + vertCount] = colour;
            meshColours[frameNo, 1][vertexNo].a = aValB;
            meshColours[frameNo, 1][vertexNo + vertCount].a = aValB;

            if (meshColours[frameNo, 1][vertexNo].r == 0)
            {
                colour = Color.blue;
                colour.a = aValB;
            }
            meshColours[frameNo, 3][vertexNo] = colour;
            meshColours[frameNo, 3][vertexNo + vertCount] = colour;
        }
    }

    //-------------------------------------------------------------------------------------------
    //                  USER INPUT                                                              |
    //-------------------------------------------------------------------------------------------

    public void SwitchColourMode()
    {
        if (meshes != null)
        {
            // Cycle to next colour mode
            colourMode = colourMode == 1 ? 2 : (colourMode == 2 ? 3 : 1);
            // Set colour to current mesh to new colour mode
            meshes[index].colors32 = meshColours[index, colourMode];
        }
    }
    
    public void ResetCellPosition()
    {
        // Reset transform to base values
        transform.rotation = Quaternion.identity;
        transform.position = new Vector3(-2, 0, 0);
        transform.localScale = new Vector3(1, 1, 1);
    }

    public void ChangeSpeed(bool decrease)
    {
        // Alter speed by changing time between switching frames
        if (decrease)
            timeBetweenFrames += speedInterval;
        else
        {
            if (timeBetweenFrames >= speedInterval)
               timeBetweenFrames -= speedInterval;
        }
    }

    public void ToBegining()
    {
        // Reset mesh index to 0
        Stop();
        index = 0;
        DisplayMesh();
    }

    public void FrameSkip(int noFrames)
    {
        if (meshes != null)
        {
            Stop();
            // Change index by noFrames
            index += noFrames;
            // Ensure index still within bounds
            while (index < 0)
                index += meshes.Length;
            if (index >= meshes.Length)
                index %= meshes.Length;
            DisplayMesh();
        }
    }

    public void PlayPause()
    {
        if (meshes != null)
        {
            // Stop any current ChangeMesh coroutine
            StopCoroutine("ChangeMesh");
            DisplayMesh();
            // Invert paused
            paused = !paused;
            // Start new coroutine if not paused
            if (!paused)
                StartCoroutine("ChangeMesh");
        }
    }

    public void Stop()
    {
        // Stop playing
        StopCoroutine("ChangeMesh");
        paused = true;
        DisplayMesh();
    }

    public void SetAlpha(RaycastHit hit, float rad)
    {
        // Get number of external vertices in mesh
        int noVert = meshes[index].vertices.Length / 2;
        // Scale radius to world scale
        rad /= transform.localScale.x;
        // Get point to paint around in a local vector
        var hitPoint = transform.InverseTransformPoint(hit.point);
        // Store list of vertices and their colours
        var verts = meshes[index].vertices;
        var vertCols1 = meshColours[index, 1];
        var vertCols2 = meshColours[index, 2];
        var vertCols3 = meshColours[index, 3];
        // Get value of alpha slider and convert to bytes
        var alpha = System.Convert.ToByte(alphaSlider.value); 
        // If alpha is less than 100% switch to transparent shader
        if (alpha != 255)
            cellMat.shader = trans;
        
        // Loop through external vertices 
        // (internal are offset so can just update those if the external is updated)
        for (int vert = 0; vert < noVert; vert++)
        {
            // Check if vertex is within the radius
            if (Vector3.Distance(verts[vert], hitPoint) < rad)
            {
                // Set alpha for all three colour modes for both internal and external verts
                vertCols1[vert].a = alpha;
                vertCols1[vert + noVert].a = alpha;
                vertCols2[vert].a = alpha;
                vertCols2[vert + noVert].a = alpha;
                vertCols3[vert].a = alpha;
                vertCols3[vert + noVert].a = alpha;
            }
        }
        // Set mesh colours to the updated versions
        meshColours[index, 1] = vertCols1;
        meshColours[index, 2] = vertCols2;
        meshColours[index, 3] = vertCols3;

        DisplayMesh();
    }

    public void SetColour(RaycastHit hit, Color32 colour, float rad)
    {
        // Check not in scaled colour mode
        if (colourMode != 2)
        {
            // Get number of external vertices in mesh
            int noVert = meshes[index].vertices.Length / 2;
            // Scale radius to world scale
            rad /= transform.localScale.x;
            // Get point to paint around in a local vector
            var hitPoint = transform.InverseTransformPoint(hit.point);
            // Store list of vertices and their colours
            var verts = meshes[index].vertices;
            var vertCols1 = meshColours[index, 1];
            var vertCols2 = meshColours[index, 2];
            var vertCols3 = meshColours[index, 3];
            // Get value of alpha slider and convert to bytes
            var alpha = System.Convert.ToByte(alphaSlider.value);
            // If alpha is less than 100% switch to transparent shader
            if (alpha != 255)
                cellMat.shader = trans;
            // Update colour with alpha
            colour.a = alpha;
            // Check if colour is red
            bool red = colour.b == 0;

            // Loop through external vertices 
            // (internal are offset so can just update those if the external is updated)
            for (int vert = 0; vert < noVert; vert++)
            {
                // Check if vertex is within radius to colour
                if (Vector3.Distance(verts[vert], hitPoint) < rad)
                {
                    // Set the vertex to the colour
                    vertCols1[vert] = colour;
                    vertCols1[vert + noVert] = colour;
                    // If colour is red update overlay colour mode with scaled value
                    if (red)
                    {
                        // Get scaled colour value for the vertex and update alpha
                        var colour2 = vertCols2[vert];
                        colour2.a = alpha;
                        // Set the overlay value
                        vertCols3[vert] = colour2;
                        vertCols3[vert + noVert] = colour2;
                    } 
                    else
                    {
                        // Otherwise set to blue
                        vertCols3[vert] = colour;
                        vertCols3[vert + noVert] = colour;
                    }
                }
            }
            // Update colour arrays
            meshColours[index, 1] = vertCols1;
            meshColours[index, 3] = vertCols3;
            DisplayMesh();
        }
    }

    public void ToggleMarker(RaycastHit hit)
    {
        // Get number of external vertices
        int noVert = meshes[index].vertices.Length / 2;
        // Get the closest vertex hit by the ray
        var vertex = GetClosestVertexHit(hit);
        // If vertex is an internal vertex covert to external
        vertex = vertex <= noVert ? vertex : vertex - noVert;
        // Get the position of the vertex
        var pos = meshes[index].vertices[vertex];
        // Check if there already is a marker there
        if (markers[index].Contains(vertex))
        {
            // Remove the marker and unload it
            markers[index].Remove(vertex);
            int i = 0;
            // Search through children to find the marker
            while (i < transform.childCount)
            {
                var marker = transform.GetChild(i++);
                if (marker.localPosition == pos)
                {
                    GameObject.Destroy(marker.gameObject);
                    break;
                }
            }
        }
        else
        {
            // Otherwise create marker at that position and add it to the list
            var obj = Instantiate(markerPrefab, transform);
            obj.transform.localPosition = pos;
            markers[index].Add(vertex);
        }
    }

    private int GetClosestVertexHit(RaycastHit hit)
    {
        Vector3 baryCoords = hit.barycentricCoordinate;
        int triIndex = hit.triangleIndex * 3;
        // Work out which vertex of triangle is closest using barycentric coordinates
        if (baryCoords.x > baryCoords.y)
        {
            if (baryCoords.x > baryCoords.z)
                return meshes[index].triangles[triIndex];
            else
                return meshes[index].triangles[triIndex + 2];
        }
        else if (baryCoords.y > baryCoords.z)
            return meshes[index].triangles[triIndex + 1];
        else
            return meshes[index].triangles[triIndex + 2];
    }

    //-------------------------------------------------------------------------------------------
    //                  FILE INTERACTION                                                        |
    //-------------------------------------------------------------------------------------------

    private int GetFileNo(string filePath)
    {
        // Search the filename for a number
        string num = "";
        int meshIndex = 0;
        // Gather each number of the filename into a string
        for (int i = 0; i < filePath.Length; i++)
        {
            if (filePath[i] == Path.DirectorySeparatorChar)
                num = "";
            if (char.IsDigit(filePath[i]))
                num += filePath[i];
        }
        // Convert the string to an int and minus 1 to make an index
        if (num.Length > 0)
            meshIndex = int.Parse(num) - 1;
        return meshIndex;
    }

    public void LoadTimeseries(string dirPath)
    {
        // Stop any current coroutines that may interfere
        StopCoroutines();
        fileError = false;
        StartCoroutine(ImportMeshes(dirPath));
    }

    private void ClearTimeseries()
    {
        // Reset index
        index = 0;
        // Remove current model from scene
        GetComponent<MeshFilter>().mesh = null;
        GetComponent<MeshCollider>().sharedMesh = null;
        // Reset shader
        cellMat.shader = opaque;
        // Reset position and remove any markers
        ResetCellPosition();
        UnloadMarkers();
        // Set arrays to null
        meshes = null;
        meshColours = null;
        markers = null;
        // Set status text
        currentFrameText.text = "No time series currently loaded";
        progressText.text = "";
    }

    private IEnumerator ImportMeshes(string dirPath)
    {
        // Disable UI buttons during load
        SetButtonState(false);
        // Clear current timeseries
        ClearTimeseries();
        // Get the filepaths to any .PLYs in dirPath
        string[] frames = Directory.GetFiles(dirPath, "*.ply");
        // Reset loading bar
        progressBar.value = 0;
        progressText.text = "Loading: 0 / " + frames.Length;
        if (frames.Length > 0)
        {
            // Otherwise load meshes from files
            // Initialise all arrays
            meshes = new Mesh[frames.Length];
            meshColours = new Color32[frames.Length, 4][];
            markers = new HashSet<int>[frames.Length];
            currentFrameText.text = "Currently displaying frame: 0 / " + meshes.Length;
            // Loop through each file to import
            for (int i = 0; i < frames.Length; i++)
            {
                // Get the mesh's index from the filename
                int meshIndex = GetFileNo(frames[i]);
                // Create a new marker list
                markers[i] = new HashSet<int>();
                // Import file
                yield return StartCoroutine(ImportMesh(frames[i], meshIndex));
                // Check for exception
                if (fileError)
                {
                    ClearTimeseries();
                    // Show error message
                    progressBar.value = 0;
                    progressText.text = "Error loading frame number " + (meshIndex + 1);
                    // Renable buttons
                    SetButtonState(true);
                    // Stop import coroutine
                    yield break;
                }
                // Change progress bar
                progressBar.value = (float)(i + 1) / frames.Length;
                progressText.text = "Loading: " + (i + 1) + " / " + frames.Length;
                // Display just loaded mesh
                index = meshIndex;
                DisplayMesh();
            }
            progressText.text = "Time series loaded";
        } 
        else
        {
            progressText.text = "No time series files found in directory";
        }
        // Renable buttons
        SetButtonState(true);
    }

    private IEnumerator ImportMesh(string filename, int meshIndex)
    {
        // Initialise mesh property lists
        var triangles = new int[0];
        var vertices = new Vector3[0];
        int vCount = 0;
        string entireText;

        try
        {
            // Read file into a string
            StreamReader stream = File.OpenText(filename);
            entireText = stream.ReadToEnd();
            stream.Close();
        }
        catch (Exception)
        {
            // Flag error
            fileError = true;
            // Stop import
            yield break;
        }
        yield return null;
        // Update last update time
        timeOfLastUpdate = Time.realtimeSinceStartup;

        using (StringReader reader = new StringReader(entireText))
        {
            string currentText = reader.ReadLine();
            char[] splitIdentifier = {' '};
            string[] brokenString;

            // Read past header
            while (currentText != null && !currentText.Equals("end_header"))
            {
                // Yield control if time has passed 0.01s
                if (Time.realtimeSinceStartup - timeOfLastUpdate > frameCutoff)
                {
                    yield return null;
                    timeOfLastUpdate = Time.realtimeSinceStartup;
                }
                // Check if line contains mesh property info
                if (currentText.StartsWith("element"))
                {
                    try
                    {
                        // Break up string into words
                        brokenString = currentText.Split(splitIdentifier);
                        // Check if line describes number of vertices or triangles
                        if (brokenString[1].Equals("vertex"))
                        {
                            // Initialise vertex and colour arrays
                            vertices = new Vector3[System.Convert.ToInt32(brokenString[2]) * 2];
                            vCount = vertices.Length / 2;
                            meshColours[meshIndex, 0] = new Color32[vertices.Length];
                            meshColours[meshIndex, 1] = new Color32[vertices.Length];
                            meshColours[meshIndex, 2] = new Color32[vertices.Length];
                            meshColours[meshIndex, 3] = new Color32[vertices.Length];
                        }
                        else
                        {
                            // Initialise triangle array
                            triangles = new int[System.Convert.ToInt32(brokenString[2]) * 3 * 2];
                        }
                    }
                    catch (Exception)
                    {
                        // Flag error
                        fileError = true;
                        // Stop import
                        yield break;
                    }
                }
                // Read next line
                currentText = reader.ReadLine();
            } 
            // Check number of vertices and triagles found
            if (vertices.Length == 0 || triangles.Length == 0)
            {
                // Flag error
                fileError = true;
                // Stop import
                yield break;
            }
            // Read end header
            currentText = reader.ReadLine();

            // Loop through vertex lines
            for (int i = 0; i < vCount; i++)
            {
                // Yield control if time has passed 0.01s
                if (Time.realtimeSinceStartup - timeOfLastUpdate > frameCutoff)
                {
                    yield return null;
                    timeOfLastUpdate = Time.realtimeSinceStartup;
                }

                try
                {
                    // Split line into word strings
                    brokenString = currentText.Split(splitIdentifier);
                    // Get vertex vector
                    var vertVect = new Vector3(System.Convert.ToSingle(brokenString[0]) / 100,
                                               System.Convert.ToSingle(brokenString[1]) / 100,
                                               System.Convert.ToSingle(brokenString[2]) / 100);
                    // Set positions for both internal and external vertices
                    vertices[i] = vertVect;
                    vertices[i + vCount] = vertVect;
                    // Get base colour of vertex
                    var colour = new Color32(System.Convert.ToByte(brokenString[3]), System.Convert.ToByte(brokenString[4]), 0, 255);
                    // Set colours
                    meshColours[meshIndex, 0][i] = colour;
                    meshColours[meshIndex, 0][i + vCount] = colour;
                    meshColours[meshIndex, 2][i] = colour;
                    meshColours[meshIndex, 2][i + vCount] = colour;
                    // Check if blue channel (patch flag) is 0
                    if (brokenString[5] == "0")
                    {
                        // Set blue
                        meshColours[meshIndex, 1][i] = Color.blue;
                        meshColours[meshIndex, 1][i + vCount] = Color.blue;
                        meshColours[meshIndex, 3][i] = Color.blue;
                        meshColours[meshIndex, 3][i + vCount] = Color.blue;
                    }
                    else
                    {
                        // Set red
                        meshColours[meshIndex, 1][i] = Color.red;
                        meshColours[meshIndex, 1][i + vCount] = Color.red;
                        meshColours[meshIndex, 3][i] = colour;
                        meshColours[meshIndex, 3][i + vCount] = colour;
                    }
                }
                catch (Exception)
                {
                    // Flag error
                    fileError = true;
                    // Stop import
                    yield break;
                }
                // Read next line
                currentText = reader.ReadLine();
            }

            // Loop through triangles
            var triCount = triangles.Length / 2;
            for (int i = 0; i < triCount; i = i + 3)
            {
                // Yield control if time has passed 0.01s
                if (Time.realtimeSinceStartup - timeOfLastUpdate > frameCutoff)
                {
                    yield return null;
                    timeOfLastUpdate = Time.realtimeSinceStartup;
                }
                // Get vertices for triangle
                brokenString = currentText.Split(splitIdentifier);
                try
                {
                    var vert1 = System.Convert.ToInt32(brokenString[1]);
                    var vert2 = System.Convert.ToInt32(brokenString[2]);
                    var vert3 = System.Convert.ToInt32(brokenString[3]);
                    // Set triangle vertices
                    triangles[i] = vert2;
                    triangles[i + 1] = vert1;
                    triangles[i + 2] = vert3;
                    // Internal triangles have reversed winding order
                    triangles[i + triCount] = vert1 + vCount;
                    triangles[i + 1 + triCount] = vert2 + vCount;
                    triangles[i + 2 + triCount] = vert3 + vCount;
                }
                catch (Exception)
                {
                    // Flag error
                    fileError = true;
                    // Stop import
                    yield break;
                }
                currentText = reader.ReadLine();
            }
        }
        // Create new mesh and set properties
        Mesh mesh = new Mesh();
        // Increase index format to 32bit to allow for needed number of vertices
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        var norms = mesh.normals;
        // Inverse normals for internal vertices
        for (int i = 0; i < vCount; i++)
            norms[i + vCount] = -norms[i];
        // Set normals
        mesh.normals = norms;
        // Set mesh in meshes list
        meshes[meshIndex] = mesh;
    }

    public void LoadTimeseriesAnnotation(string dirPath)
    {
        if (meshes != null)
        {
            // Stop any coroutines that may interfere
            Stop();
            // Check if path is a single .csv, otherwise expects a folder
            if (dirPath.EndsWith(".csv"))
            {
                try
                {
                    ImportMarkers(dirPath);
                }
                catch (Exception)
                {
                    // Wipe markers
                    for (int i = 0; i < markers.Length; i++)
                        markers[i] = new HashSet<int>();
                    progressBar.value = 0;
                    progressText.text = "Failed to load markers";
                }
            }
            else
                StartCoroutine(ImportColours(dirPath));
        }
    }

    private void ImportMarkers(string filename)
    {
        // Loop through each frame and create an empty marker set
        for (int i = 0; i < markers.Length; i++)
            markers[i] = new HashSet<int>();

        // Read the file into a string
        StreamReader stream = File.OpenText(filename);
        string entireText = stream.ReadToEnd();
        stream.Close();

        using (StringReader reader = new StringReader(entireText))
        {
            string currentLine;
            // Loop through every line of the csv
            while ((currentLine = reader.ReadLine()) != null)
            {
                // Split csv up
                string[] splitString = currentLine.Split(',');
                // Get vertex of marker
                var vert = System.Convert.ToInt32(splitString[1]);
                // Get frame of marker
                var frame = System.Convert.ToInt32(splitString[0]) - 1;
                // Check frame exists then create a marker
                if (frame < markers.Length)
                    markers[frame].Add(vert);
            }
        } 

        progressBar.value = 1;
        progressText.text = "Markers loaded";
        ChangeMarkers();
    }

    private IEnumerator ImportColours(string dirPath)
    {
        // Disable UI buttons
        SetButtonState(false);
        timeOfLastUpdate = Time.realtimeSinceStartup;
        string[] csvs = Directory.GetFiles(dirPath, "*.csv");
        // Set loading bar
        progressBar.value = 0;
        progressText.text = "Loading: 0 / " + csvs.Length;
        for (int i = 0; i < csvs.Length; i++)
        {
            // Yield if current update has taken over 0.01s
            if (Time.realtimeSinceStartup - timeOfLastUpdate > 0.01)
            {
                yield return null;
                timeOfLastUpdate = Time.realtimeSinceStartup;
            }
            // Import a frame's colouring
            try
            {
                ImportCellColour(csvs[i]);
            }
            catch (Exception)
            {
                // Show error message
                progressBar.value = 0;
                progressText.text = "Error importing colour to frame number " + (GetFileNo(csvs[i]) + 1);
                // Renable buttons
                SetButtonState(true);
                // Stop import coroutine
                yield break;
            }
            // Update loading bar
            progressBar.value = (float)(i + 1) / csvs.Length;
            progressText.text = "Loading: " + (i + 1) + " / " + csvs.Length;
        }
        progressText.text = "Colourmap loaded";
        colourMode = 1;
        // Renable buttons
        SetButtonState(true);
        DisplayMesh();
    }

    private void ImportCellColour(string filename)
    {
        // Get index of the frame the file corresponds to
        int meshIndex = GetFileNo(filename);

        if (meshIndex < meshes.Length)
        {
            // Get number of external vertices
            int noVert = meshes[meshIndex].vertices.Length / 2;

            // Read file to string
            StreamReader stream = File.OpenText(filename);
            string entireText = stream.ReadToEnd();
            stream.Close();

            using (StringReader reader = new StringReader(entireText))
            {
                string currentText;
                int i = -1;
                // Read every line (each line is a vertex) until end of file or vertex limit reached
                while ((currentText = reader.ReadLine()) != null && ++i < noVert)
                {
                    // If patch flag is 1, set vertex to red, otherwise set to blue
                    if (currentText[0] != '0')
                    {
                        var colour = meshColours[meshIndex, 2][i];
                        colour.a = 255;
                        meshColours[meshIndex, 1][i] = Color.red;
                        meshColours[meshIndex, 1][i + noVert] = Color.red;
                        meshColours[meshIndex, 3][i] = colour;
                        meshColours[meshIndex, 3][i + noVert] = colour;
                    }
                    else
                    {
                        meshColours[meshIndex, 1][i] = Color.blue;
                        meshColours[meshIndex, 1][i + noVert] = Color.blue;
                        meshColours[meshIndex, 3][i] = Color.blue;
                        meshColours[meshIndex, 3][i + noVert] = Color.blue;
                    }
                    // Increment to next vertex
                }
            }
        }
    }

    public void SaveAnnotations(string dirPath)
    {
        if (meshes != null)
        {
            bool error = false;
            Stop();
            try
            {
                ExportMarkers(dirPath);
            }
            catch (Exception)
            {
                error = true;
                progressBar.value = 0;
                progressText.text = "Failed to export markers to file";
            }
            if (!error)
                StartCoroutine(ExportColours(dirPath));
        }
    }

    private void ExportMarkers(string filename)
    {
        // Create filepath for marker annotations
        filename += Path.DirectorySeparatorChar + "Marker_Annotations.csv";
        // Create the file, or overwrite if the file exists.
        using (FileStream fs = File.Create(filename))
        using (var sr = new StreamWriter(fs))
        {
            // Loop through each marker
            for (int i = 0; i < markers.Length; i++)
            {
                // Add 1 to i to create frame num for output
                int frame = i + 1;
                // Loop through vertices
                foreach (var vertex in markers[i])
                {
                    // Write marker to csv
                    var pos = meshes[i].vertices[vertex] * 100;
                    sr.WriteLine(string.Format("{0},{1},{2},{3},{4}", frame, vertex, pos.x, pos.y, pos.z));
                }
            }
        }   
    }

    private IEnumerator ExportColours(string dirPath)
    {
        // Set buttons to be inactive
        SetButtonState(false);
        timeOfLastUpdate = Time.realtimeSinceStartup;
        // Get the ply files in the directory
        string[] frames = Directory.GetFiles(dirPath, "*.ply");
        // Reset progress bar
        progressBar.value = 0;
        progressText.text = "Saving: 0 / " + meshes.Length;
        // Check if number of plys = number of frames
        if (frames.Length == meshes.Length)
        {
            // Assume plys are source files for loaded timeseries
            string tempPath = dirPath + Path.DirectorySeparatorChar + "temp.ply";
            // Export every frame
            for (int frameNo = 0; frameNo < frames.Length; frameNo++)
            {
                // If update has taken longer than 0.01s yield
                if (Time.realtimeSinceStartup - timeOfLastUpdate > 0.01)
                {
                    yield return null;
                    timeOfLastUpdate = Time.realtimeSinceStartup;
                }
                // Get frame number
                int meshNo = GetFileNo(frames[frameNo]);
                // Read existing ply into string
                string entireText;
                try
                {
                    StreamReader stream = File.OpenText(frames[frameNo]);
                    entireText = stream.ReadToEnd();
                    stream.Close();
                }
                catch (Exception)
                {
                    progressBar.value = 0;
                    progressText.text = "Failed to export colours of frame " + (frameNo + 1);
                    yield break;
                }
                yield return null;
                using (StringReader reader = new StringReader(entireText))
                // Write to temp file
                using (FileStream fs = File.Create(tempPath))
                using (StreamWriter sr = new StreamWriter(fs))
                {
                    // Read/rewrite to past header
                    string currentLine = reader.ReadLine();
                    while (!currentLine.Equals("end_header"))
                    {
                        if (Time.realtimeSinceStartup - timeOfLastUpdate > 0.01)
                        {
                            yield return null;
                            timeOfLastUpdate = Time.realtimeSinceStartup;
                        }
                        try { 
                            // Write header contents
                            sr.WriteLine(currentLine);
                            currentLine = reader.ReadLine();
                        }
                        catch (Exception)
                        {
                            progressBar.value = 0;
                            progressText.text = "Failed to export colours of frame " + (frameNo + 1);
                            yield break;
                        }
                    }
                    try { 
                        // Write/read end header
                        sr.WriteLine(currentLine);
                        currentLine = reader.ReadLine();
                    }
                    catch (Exception)
                    {
                        progressBar.value = 0;
                        progressText.text = "Failed to export colours of frame " + (frameNo + 1);
                        yield break;
                    }

                    // Loop through every vertex
                    for (int i = 0; i < meshColours[meshNo, 1].Length / 2; i++)
                    {
                        // Yield to maintain framerate
                        if (Time.realtimeSinceStartup - timeOfLastUpdate > 0.01)
                        {
                            yield return null;
                            timeOfLastUpdate = Time.realtimeSinceStartup;
                        }
                        // Turn the current lined to a char array
                        char[] charArray = currentLine.ToCharArray();
                        // Replace last character based off 
                        charArray[charArray.Length - 1] = meshColours[meshNo, 1][i].b == 0 ? '1' : '0';
                        // Convert back to string
                        currentLine = new string(charArray);
                        try { 
                            // Write line
                            sr.WriteLine(currentLine);
                            currentLine = reader.ReadLine();
                        }
                        catch (Exception)
                        {
                            progressBar.value = 0;
                            progressText.text = "Failed to export colours of frame " + (frameNo + 1);
                            yield break;
                        }
                    }
                    // Write the rest of the file to temp
                    do
                    {
                        if (Time.realtimeSinceStartup - timeOfLastUpdate > 0.01)
                        {
                            yield return null;
                            timeOfLastUpdate = Time.realtimeSinceStartup;
                        }
                        try { 
                            sr.WriteLine(currentLine);
                        }
                        catch (Exception)
                        {
                            progressBar.value = 0;
                            progressText.text = "Failed to export colours of frame " + (frameNo + 1);
                            yield break;
                        }
                    } while ((currentLine = reader.ReadLine()) != null);
                }

                try {
                    // Replace original file with the temp file
                    File.Delete(frames[frameNo]);
                    File.Move(tempPath, frames[frameNo]);
                }
                catch (Exception)
                {
                    progressBar.value = 0;
                    progressText.text = "Failed to export colours of frame " + (frameNo + 1);
                    yield break;
                }

                // Update progress bar
                progressBar.value = (float)(frameNo + 1) / frames.Length;
                progressText.text = "Saving: " + (frameNo + 1) + " / " + frames.Length;
            }
        } 
        else
        {
            // Write colour patches to a csv folder
            // Create folder for colour patches
            dirPath += Path.DirectorySeparatorChar + "Colour_Patches";
            try { 
                Directory.CreateDirectory(dirPath);
            }
            catch (Exception)
            {
                progressBar.value = 0;
                progressText.text = "Failed to export colours";
                yield break;
            }
            // Loop through every mesh
            for (int meshNo = 0; meshNo < meshes.Length; meshNo++)
            {
                // Yield to maintain framerate
                if (Time.realtimeSinceStartup - timeOfLastUpdate > 0.01)
                {
                    yield return null;
                    timeOfLastUpdate = Time.realtimeSinceStartup;
                }
                // Create file for the mesh's colour patches
                string filename = dirPath + Path.DirectorySeparatorChar + "Frame" + (meshNo + 1) + ".csv";
                using (FileStream fs = File.Create(filename))
                using (StreamWriter sr = new StreamWriter(fs))
                {
                    // Loop every vertex
                    for (int i = 0; i < meshColours[meshNo, 1].Length / 2; i++)
                    {
                        // Yield to maintain framerate
                        if (Time.realtimeSinceStartup - timeOfLastUpdate > 0.01)
                        {
                            yield return null;
                            timeOfLastUpdate = Time.realtimeSinceStartup;
                        }
                        try { 
                            // Write colour to the csv
                            sr.WriteLine(meshColours[meshNo, 1][i].b == 0 ? 1 : 0);
                        }
                        catch (Exception)
                        {
                            progressBar.value = 0;
                            progressText.text = "Failed to export colours of frame " + (meshNo + 1);
                            yield break;
                        }
                    }
                }
                // Update progress bar
                progressBar.value = (float)(meshNo + 1) / meshes.Length;
                progressText.text = "Saving: " + (meshNo + 1) + " / " + meshes.Length;
            }
        }
        progressText.text = "Annotations saved";
        SetButtonState(true);
    }

    private void StopCoroutines()
    {
        StopAllCoroutines();
        paused = true;
    }

    private void SetButtonState(bool active)
    {
        loadTimeseries.SetActive(active);
        loadAnnotations.SetActive(active);
        saveAnnotations.SetActive(active);
        updateColours.SetActive(active);
    }
}