using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class PathReader : MonoBehaviour
{
    public GameObject wayUI;
    public GameObject redArrow;

    public GameObject linePath;

    private LineRenderer line;

    private List<Ways> ways;
    private List<GameObject> targetColliders;
    private Stack<GameObject> connetedPaths;

    private bool isStarted = false;

    private bool isLineStarted = false;

    // Use this for initialization
    void Start()
    {

        targetColliders = new List<GameObject>();
        line = linePath.GetComponent<LineRenderer>();
        connetedPaths = new Stack<GameObject>();

        Color c = ThemeChanger.current.dotColor;

        line.startColor = c;
        line.endColor = c;

        line.positionCount = 2;

        readJson();
        createWay();

    }

    bool iSStatingFromRightPosition(Vector2 pos)
    {
        if (isStarted)
        {

            Vector2 pos1 = new Vector2(GameObject.FindFirstObjectByType<DotAnimation>().transform.position.x, GameObject.FindFirstObjectByType<DotAnimation>().transform.position.y);
            Vector2 pos2 = pos;

            if (Vector2.Distance(pos2, pos1) > 0.3f)
            {
                return false;
            }
        }

        return true;
    }

    void canStartLine(Vector3 pos)
    {

        if (pos == Vector3.zero)
        {
            pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
        Collider2D[] circles = Physics2D.OverlapCircleAll(pos, 0.1f);

        if (circles != null && circles.Length > 0 && iSStatingFromRightPosition(new Vector2(pos.x, pos.y)))
        {

            targetColliders.Clear();
            StartLine(circles[0].transform.position);
            for (int i = 0; i < circles.Length; i++)
            {
                circles[i].transform.parent.gameObject.SendMessage("childCount", circles[i].gameObject);
            }

        }
    }

    public void touchRightPosition()
    {

    }

    void StartLine(Vector3 posC)
    {
        isLineStarted = true;
        Vector3 pos = posC;
        pos.z = 0;
        line.SetPosition(0, pos);
    }

    void MoveLine()
    {
        if (isLineStarted)
        {
            Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            pos.z = 0;
            line.SetPosition(1, pos);

            if (!linePath.activeSelf)
            {
                linePath.SetActive(true);
            }
            checkIfColliderCollider(pos);
        }
    }


    bool checkIfColliderCollider(Vector3 pos)
    {

        Collider2D[] circles = Physics2D.OverlapCircleAll(pos, 0.1f);

        if (circles != null && circles.Length > 0)
        {
            for (int i = 0; i < circles.Length; i++)
            {



                if (targetColliders.Contains(circles[i].gameObject))
                {

                    EndLine();
                    circles[i].gameObject.transform.parent.gameObject.SendMessage("chageColor", circles[i].gameObject);
                    isStarted = true;

                    canStartLine(Vector3.zero);
                    return true;
                }
            }
        }

        return false;
    }


    void EndLine()
    {
        if (isLineStarted)
        {
            linePath.SetActive(false);
        }

        isLineStarted = false;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {

            Vector3 pos = new Vector2(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y);
            pos.z = 0;

            bool iscanStart = !checkIfColliderCollider(pos);
            if (iscanStart && iSStatingFromRightPosition(new Vector2(pos.x, pos.y)))
            {
                canStartLine(Vector3.zero);
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            EndLine();
        }
        else if (Input.GetMouseButton(0))
        {
            MoveLine();
        }
    }

    void createWay()
    {
        for (int i = 0; i < ways.Count; i++)
        {
            GameObject go = Instantiate(wayUI) as GameObject;
            Ways way = ways[i];

            go.GetComponent<WaysUI>().setWayModel(way);
            go.GetComponent<WaysUI>().createUI();

            if (way.direction > 1)
            {
                GameObject red = Instantiate(redArrow) as GameObject;

                float angle = AngleBetweenVector2(GridManager.GetGridManger().GetPosForGrid(way.startingGridPosition),
                    GridManager.GetGridManger().GetPosForGrid(way.endGridPositon));

                red.transform.rotation = Quaternion.Euler(0, 0, angle);
                red.transform.position = go.GetComponent<WaysUI>().pointOnLine();
                red.transform.parent = transform;
            }

            go.transform.parent = transform;
        }
    }

    private float AngleBetweenVector2(Vector2 vec1, Vector2 vec2)
    {
        Vector2 diference = vec2 - vec1;
        float sign = (vec2.y < vec1.y) ? -1.0f : 1.0f;
        return Vector2.Angle(Vector2.right, diference) * sign;
    }

    void readJson()
    {
        ways = new List<Ways>();
        string levelPath = string.Format("Package_{0}/level_{1}", LevelData.worldSelected, LevelData.levelSelected);

        TextAsset file = Resources.Load(levelPath) as TextAsset;
        string dataAsJson = file.ToString();

        string[] jsonObjects = dataAsJson.Trim().Split(new char[] { '=' });

        for (int i = 0; i < jsonObjects.Length; i++)
        {
            Ways way = JsonUtility.FromJson<Ways>(jsonObjects[i]);
            ways.Add(way);
        }
    }

    public void readOtherChild(Object obj)
    {
        if (isLineStarted)
        {
            targetColliders.Add((GameObject)obj);
        }
    }

    public void pushConnectedPath(GameObject go)
    {
        connetedPaths.Push(go);
    }

    public void revertConnection()
    {
        if (connetedPaths.Count > 0)
        {
            isLineStarted = false;

            GameObject go = connetedPaths.Pop();
            go.GetComponent<WaysUI>().moveBack();

            targetColliders.Clear();

            if (connetedPaths.Count == 0)
            {
                isStarted = false;
                GameObject.FindFirstObjectByType<DotAnimation>().setEnableAtPosition(false, new Vector3(0, 0, 0));
            }
            canStartLine(GameObject.FindFirstObjectByType<DotAnimation>().transform.position);

            EndLine();
        }
    }

    public void resetGame()
    {
        WaysUI.LoadSceneAagin();
    }
    
    public void ResetPathReaderState()
    {
        Debug.Log("🔄 Resetting PathReader state...");
        
        // Reset state variables
        isStarted = false;
        isLineStarted = false;
        
        // Clear collections
        targetColliders.Clear();
        connetedPaths.Clear();
        
        // Reset line renderer
        if (line != null)
        {
            line.positionCount = 2;
            Color c = ThemeChanger.current.dotColor;
            line.startColor = c;
            line.endColor = c;
            
            if (linePath != null)
            {
                linePath.SetActive(false);
            }
        }
        
        Debug.Log("🔄 PathReader state reset complete");
    }
    
    public void RecreateWays()
    {
        Debug.Log("🔄 Recreating ways...");
        
        // Clear existing ways list
        ways.Clear();
        
        // Re-read JSON and recreate ways
        readJson();
        createWay();
        
        Debug.Log("🔄 Ways recreation complete");
    }
}
