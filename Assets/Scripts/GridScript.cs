using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GridScript : MonoBehaviour
{
    public Transform cellPrefab, lampPrefab;
    public Material wallMaterial, floorMaterial, entranceMaterial, exitMaterial;
    // size of the grid
    public Vector3 Size;
    // using 2d matrix to easily get to certain position in grid instead of using list 
    public Transform[,] Grid;

    // list used for Prim's algorithm, empty at start
    public List<Transform> Set;


    //  AdjSet{
    //       [ 0 ] all adjacent cell with weight 0
    //       [ 1 ] all adjacent cell with weight 1
    //...
    //       [ 9 ] all adjacent cell with weight 9
    //  }
    public List<List<Transform>> AdjSet;

    public InputField heightInput, widthInput;
    public Button generateButton, startPlayButton;
    public float height, width;
    void Start()
    {
        heightInput.gameObject.SetActive(true);
        widthInput.gameObject.SetActive(true);
        generateButton.gameObject.SetActive(true);
        startPlayButton.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(0);
        }
    }

    public void StartGeneration()
    {
        height = float.Parse(heightInput.text);
        width = float.Parse(widthInput.text);
        heightInput.gameObject.SetActive(false);
        widthInput.gameObject.SetActive(false);
        generateButton.gameObject.SetActive(false);
        Size.Set(width, 10, height);
        CreateGrid();
        SetRandomWeights();
        SetAdjacents();
        SetStart(Grid[0, 0]);

        // reccursively look for lowest weight adjacent to all the cells in the set.
        // if cell has two open neighbours, is disqualified
        FindNext();
    }

    // Create grid here
    private void CreateGrid()
    {
        // initialize grid with height and width
        Grid = new Transform[(int)Size.x, (int)Size.z];
        for (int x = 0; x < Size.x; x++)
        {
            for (int z = 0; z < Size.z; z++)
            {
                Transform newCell;
                Vector3 posToInstantiate = new Vector3(x, 0, z);
                newCell = (Transform) Instantiate(cellPrefab, posToInstantiate, Quaternion.identity);
                // set cell parent the grid game object so hierarchy is cleaner
                newCell.parent = transform;
                // set name of gameobject to its' actual position
                newCell.name = string.Format("({0},{1})", x, z);
                newCell.GetComponent<CellScript>().Position = posToInstantiate;
                // add new cell to transform matrix
                Grid[x, z] = newCell;
            }
        }

        // Setup camera so the entire maze is seen
        Camera.main.orthographic = true;
        Camera.main.orthographicSize = Mathf.Max(Size.x, Size.z)/2 + 5f;
        Camera.main.transform.position = Grid[(int) (Size.x / 2), (int) (Size.z / 2)].position + Vector3.up*10f;
    }
    
    // Adding random weghts to each cell
    private void SetRandomWeights()
    {
        foreach(Transform child in transform)
        {
            int weight = Random.Range(0, 10);
            child.GetComponentInChildren<TextMesh>().text = weight.ToString();
            child.GetComponent<CellScript>().Weight = weight;
        }
    }

    // each cell will have to know the adjacent cells to it in order for the algorithm to work
    // we will not do diagonals, only left, right, up and down
    private void SetAdjacents()
    {
        for (int x = 0; x < Size.x; x++)
        {
            for (int z = 0; z < Size.z; z++)
            {
                Transform cell = Grid[x, z];
                // make sure that the adjacent actually exists, since some cell might not have (corners, edges)
                CellScript cellScript = cell.GetComponent<CellScript>();
                if (x - 1 >= 0)
                {
                    // furthest to the left, no adjacent left
                    cellScript.Adjacents.Add(Grid[x - 1, z]);
                }
                if (x + 1 < Size.x)
                {
                    // furthest to the right, no adjacent right
                    cellScript.Adjacents.Add(Grid[x + 1, z]);
                }
                if (z - 1 >= 0)
                {
                    // furthest down, no adjacent down
                    cellScript.Adjacents.Add(Grid[x, z - 1]);
                }
                if (z + 1 < Size.z)
                {
                    // furthest up, no adjacent up
                    cellScript.Adjacents.Add(Grid[x, z + 1]);
                }
                cellScript.Adjacents.Sort(SortByWeight);
            }
        }
    }

    private int SortByWeight(Transform inputA, Transform inputB)
    {
        int aWeight = inputA.GetComponent<CellScript>().Weight;
        int bWeight = inputB.GetComponent<CellScript>().Weight;
        return aWeight.CompareTo(bWeight);
    }

    private void SetStart(Transform startCell)
    {
        Set = new List<Transform>();
        AdjSet = new List<List<Transform>>();
        for (int i = 0; i < 10; i++)
        {
            AdjSet.Add(new List<Transform>());
        }
        // set green color to start
        Grid[0, 0].GetComponent<Renderer>().material = entranceMaterial;
        Grid[0, 0].GetComponentInChildren<TextMesh>().text = "";
        AddToSet(Grid[0, 0]);
    }

    void AddToSet(Transform toAdd)
    {
        Set.Add(toAdd);
        //For every adjacent of toAdd object:
        foreach (Transform adj in toAdd.GetComponent<CellScript>().Adjacents)
        {
            adj.GetComponent<CellScript>().AdjacentsOpened++;
            // ff
            // a) The Set does not contain the adjacent
            //   (cells in the Set are not valid to be entered as
            //   adjacentCells as well).
            //  and
            // b) The AdjSet does not already contain the adjacent cell.
            // then..
            if (!Set.Contains(adj) && !(AdjSet[adj.GetComponent<CellScript>().Weight].Contains(adj)))
            {
                //.. Add this new cell into the proper AdjSet sub-list.
                AdjSet[adj.GetComponent<CellScript>().Weight].Add(adj);
            }
        }
    }

    void FindNext()
    {
        Transform nextCell;

        do
        {
            // algorithm stops when each adjacency sub list is empty, bool to keep track
            bool empty = true;
            // also remember which list has least elements
            int lowestList = 0;
            for (int i = 0; i < 10; i++)
            {
                // find lowest sub list
                lowestList = i;
                if (AdjSet[i].Count > 0)
                {
                    empty = false;
                    break;
                }
            }
            // if there is no lowest sub list, we are done
            if (empty)
            {
                Debug.Log("We're Done, " + Time.timeSinceLevelLoad + " seconds taken");
                CancelInvoke("FindNext");
                // exit is last cell opened, mark it red
                Set[Set.Count - 1].GetComponent<Renderer>().material = exitMaterial;
                // every cell that is not opened is moved up and colored black so they will be walls
                foreach (Transform cell in Grid)
                {
                    if (!Set.Contains(cell))
                    {
                        cell.Translate(Vector3.up);
                        cell.GetComponent<Renderer>().material = wallMaterial;
                        cell.GetComponentInChildren<TextMesh>().text = "";
                    }
                }
                CreateMazeEdges();
                // after generation is done, this bool will allow user to move throught the maze

                startPlayButton.gameObject.SetActive(true);
                return;
            }
            // if not done, find first element in smallest adjacency sub list
            nextCell = AdjSet[lowestList][0];
            // since we do not want the same cell in both AdjSet and Set,
            // remove this 'next' variable from AdjSet.
            AdjSet[lowestList].Remove(nextCell);
        } while (nextCell.GetComponent<CellScript>().AdjacentsOpened >= 2);
        nextCell.GetComponent<Renderer>().material = floorMaterial; 
        nextCell.GetComponentInChildren<TextMesh>().text = "";
        if (Random.value > 0.97) //%3 percent chance to spawn lamp
        {
            if (Random.value < 0.5f)
            {
                Transform newLamp = (Transform)Instantiate(lampPrefab, new Vector3(nextCell.position.x - 0.4f, 0.5f, nextCell.position.z - 0.4f), Quaternion.identity);
                newLamp.Rotate(0, 45, 0);
            } else
            {
                Transform newLamp = (Transform)Instantiate(lampPrefab, new Vector3(nextCell.position.x + 0.4f, 0.5f, nextCell.position.z + 0.4f), Quaternion.identity);
                newLamp.Rotate(0, -135, 0);
            }
        }
        AddToSet(nextCell);
        // Recursively call this function
        Invoke("FindNext", 0);
    }

    private void CreateMazeEdges()
    {
        int xMarginLeft = -1;
        int xMarginRight = (int)Size.x;
        for (int z = -1; z < Size.z + 1; z++)
        {
            Transform marginCellLeft, marginCellRight;
            Vector3 posToInstantiateLeft = new Vector3(xMarginLeft, 1, z);
            Vector3 posToInstantiateRight = new Vector3(xMarginRight, 1, z);
            marginCellLeft = (Transform)Instantiate(cellPrefab, posToInstantiateLeft, Quaternion.identity);
            marginCellRight = (Transform)Instantiate(cellPrefab, posToInstantiateRight, Quaternion.identity);
            // set cell parent the grid game object so hierarchy is cleaner
            marginCellLeft.parent = transform;
            marginCellRight.parent = transform;
            // set name of gameobject to its' actual position
            marginCellLeft.name = string.Format("({0},{1})", xMarginLeft, z);
            marginCellLeft.GetComponent<CellScript>().Position = posToInstantiateLeft;
            marginCellLeft.GetComponentInChildren<TextMesh>().text = "";
            marginCellLeft.GetComponent<Renderer>().material = wallMaterial;

            marginCellRight.name = string.Format("({0},{1})", xMarginRight, z);
            marginCellRight.GetComponent<CellScript>().Position = posToInstantiateRight;
            marginCellRight.GetComponentInChildren<TextMesh>().text = "";
            marginCellRight.GetComponent<Renderer>().material = wallMaterial;
        }
        int yMarginBottom = -1;
        int yMarginTop = (int) Size.z;
        for (int x = -1; x < Size.x + 1; x++)
        {
            Transform marginCellBottom, marginCellTop;
            Vector3 posToInstantiateBottom = new Vector3(x, 1, yMarginBottom);
            Vector3 posToInstantiateTop = new Vector3(x, 1, yMarginTop);
            marginCellBottom = (Transform)Instantiate(cellPrefab, posToInstantiateBottom, Quaternion.identity);
            marginCellTop = (Transform)Instantiate(cellPrefab, posToInstantiateTop, Quaternion.identity);
            // set cell parent the grid game object so hierarchy is cleaner
            marginCellBottom.parent = transform;
            marginCellTop.parent = transform;
            // set name of gameobject to its' actual position
            marginCellBottom.name = string.Format("({0},{1})", x, yMarginBottom);
            marginCellBottom.GetComponent<CellScript>().Position = posToInstantiateBottom;
            marginCellBottom.GetComponentInChildren<TextMesh>().text = "";
            marginCellBottom.GetComponent<Renderer>().material = wallMaterial;

            marginCellTop.name = string.Format("({0},{1})", x, yMarginTop);
            marginCellTop.GetComponent<CellScript>().Position = posToInstantiateTop;
            marginCellTop.GetComponentInChildren<TextMesh>().text = "";
            marginCellTop.GetComponent<Renderer>().material = wallMaterial;
        }
    }
}
