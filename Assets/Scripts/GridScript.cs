using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GridScript : MonoBehaviour
{
    public Transform CellPrefab;
    // size of the grid
    public Vector3 Size;
    // using 2d matrix to easily get to certain position in grid instead of using list 
    public Transform[,] Grid;

    // list used for Prim's algorithm, empty at start
    // http://en.wikipedia.org/wiki/Prim%27s_algorithm
    public List<Transform> Set;


    //  AdjSet{
    //       [ 0 ] is a list of all the cells
    //         that have a weight of 0, and are
    //         adjacent to the cells in Set.
    //       [ 1 ] is a list of all the cells
    //         that have a weight of 1, and are
    //         adjacent to the cells in Set.
    //       [ 2 ] is a list of all the cells
    //         that have a weight of 2, and are
    //         adjacent to the cells in Set.
    //     etc...
    //       [ 9 ] is a list of all the cells
    //         that have a weight of 9, and are
    //         adjacent to the cells in Set.
    //  }
    public List<List<Transform>> AdjSet;

    void Start()
    {
        CreateGrid();
        SetRandomWeights();
        SetAdjacents();
        SetStart(Grid[0,0]); 

        // reccursively look for lowest weight adjacent to all the cells in the set.
        // if cell has two open neighbours, is disqualified
        FindNext();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(0);
        }
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
                newCell = (Transform) Instantiate(CellPrefab, posToInstantiate, Quaternion.identity);
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
        Grid[0, 0].GetComponent<Renderer>().material.color = Color.green;
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
                Set[Set.Count - 1].GetComponent<Renderer>().material.color = Color.red;
                // every cell that is not opened is moved up and colored black so they will be walls
                foreach (Transform cell in Grid)
                {
                    if (!Set.Contains(cell))
                    {
                        cell.Translate(Vector3.up);
                        cell.GetComponent<Renderer>().material.color = Color.black;
                        cell.GetComponentInChildren<TextMesh>().text = "";
                    }
                }
                return;
            }
            // if not done, find first element in smallest adjacency sub list
            nextCell = AdjSet[lowestList][0];
            // since we do not want the same cell in both AdjSet and Set,
            // remove this 'next' variable from AdjSet.
            AdjSet[lowestList].Remove(nextCell);
        } while (nextCell.GetComponent<CellScript>().AdjacentsOpened >= 2);
        nextCell.GetComponent<Renderer>().material.color = Color.white;
        nextCell.GetComponentInChildren<TextMesh>().text = "";
        AddToSet(nextCell);
        // Recursively call this function
        Invoke("FindNext", 0);
    }
}
