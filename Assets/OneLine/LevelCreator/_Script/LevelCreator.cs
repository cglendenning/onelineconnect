using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class LevelCreator : MonoBehaviour
{
    public GameObject dot;
    public GameObject gameObjectLine;
    public GameObject redArrow;
    public InputField packageInput, levelInput;

    private List<GameObject> formObjects;
    private List<GameObject> selectedObjects;

    private GameObject startingObj;

    public List<Ways> ways;
    private List<LineRenderer> lineRenderers = new List<LineRenderer>();

    private LineRenderer line;
    public GameObject linePath;

    private List<int> lastTos = new List<int>();
    private int to;
    private int from;
    private int solution;

    private List<SpriteRenderer> dotWayBegins = new List<SpriteRenderer>();
    private List<SpriteRenderer> dotWayEnds = new List<SpriteRenderer>();

    private int packageNumber, levelNumber;

    void Start()
    {
        to = 0;
        from = 0;
        solution = 0;

        line = linePath.GetComponent<LineRenderer>();

        line.startWidth = 0.16f;
        line.endWidth = 0.16f;

        formObjects = new List<GameObject>();
        selectedObjects = new List<GameObject>();

        ways = new List<Ways>();

        for (int i = 1; i < 141; i++)
        {
            GameObject go = Instantiate(dot);
            go.transform.position = GridManager.GetGridManger().GetPosForGrid(i);
            Transform childObject = go.transform.GetChild(0);
            childObject.position = new Vector3(childObject.position.x, childObject.position.y, -1);
            childObject.GetComponent<TextMesh>().text = "" + (i);
            formObjects.Add(go);
        }

        packageInput.text = PlayerPrefs.GetInt("level_editor_package", 1).ToString();
        levelInput.text = PlayerPrefs.GetInt("level_editor_level", 1).ToString();
    }

    public void OnInputValueChanged()
    {
        if (!string.IsNullOrEmpty(packageInput.text))
        {
            int.TryParse(packageInput.text, out packageNumber);
            if (packageNumber < 1) packageInput.text = "1";
        }

        if (!string.IsNullOrEmpty(levelInput.text))
        {
            int.TryParse(levelInput.text, out levelNumber);
            if (levelNumber < 1) levelInput.text = "1";
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            CanStartLine();
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

    void CanStartLine()
    {
        Collider2D[] circles = Physics2D.OverlapCircleAll(Camera.main.ScreenToWorldPoint(Input.mousePosition), 0.1f);

        if (circles != null && circles.Length > 0)
        {
            from = int.Parse(circles[0].transform.GetChild(0).GetComponent<TextMesh>().text);
            if (lastTos.Count != 0 && from != lastTos[lastTos.Count - 1])
            {
                from = -1;
                return;
            }

            startingObj = circles[0].gameObject;
            StartLine(circles[0].transform.position);
        }
    }

    void StartLine(Vector3 posC)
    {
        linePath.SetActive(true);
        Vector3 pos = posC;
        pos.z = 0;
        line.positionCount = 1;
        line.SetPosition(0, pos);
    }

    void MoveLine()
    {
        if (from == -1) return;

        Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        pos.z = 0;
        line.positionCount = 2;
        line.SetPosition(1, pos);
    }

    void EndLine()
    {
        if (from == -1) return;

        linePath.SetActive(false);
        CheckIfColliderCollider(Camera.main.ScreenToWorldPoint(Input.mousePosition));
    }

    void CheckIfColliderCollider(Vector3 pos)
    {
        Collider2D[] circles = Physics2D.OverlapCircleAll(pos, 0.1f);

        if (circles != null && circles.Length > 0)
        {
            to = int.Parse(circles[0].transform.GetChild(0).GetComponent<TextMesh>().text);

            if (to == from)
                return;

            for(int i = 0; i < ways.Count; i++)
            {
                if (ways[i].direction == 2 && ways[i].startingGridPosition == to && ways[i].endGridPositon == from)
                {
                    return;
                }
            }

            lastTos.Add(to);

            var begin = startingObj.GetComponent<SpriteRenderer>();
            var end = circles[0].gameObject.GetComponent<SpriteRenderer>();
            begin.color = Color.yellow;
            end.color = Color.blue;

            dotWayBegins.Add(begin);
            dotWayEnds.Add(end);

            AddWays();
        }
    }

    public void AddWays()
    {
        int pathTag = 1;
        int direction = 1;
        string solutions = "";

        Vector3 startingPos = GridManager.GetGridManger().GetPosForGrid(from);

        Vector3 EndingPos = GridManager.GetGridManger().GetPosForGrid(to);

        selectedObjects.Add(formObjects[from - 1]);
        selectedObjects.Add(formObjects[to - 1]);

        GameObject go = Instantiate(gameObjectLine);

        LineRenderer line = go.GetComponent<LineRenderer>();

        line.positionCount = 2;
        line.startWidth = 0.2f;
        line.endWidth = 0.2f;
        line.SetPosition(0, startingPos);
        line.SetPosition(1, EndingPos);

        lineRenderers.Add(line);

        Ways way = new Ways();
        solution++;

        way.startingGridPosition = from;
        way.endGridPositon = to;
        way.pathTag = pathTag;
        way.solutionPosition = solution;
        way.direction = direction;
        way.solutions = solutions;
        ways.Add(way);
    }

    public void Undo()
    {
        if (ways.Count == 0) return;

        var lastBegin = dotWayBegins[dotWayBegins.Count - 1];
        var lastEnd = dotWayEnds[dotWayEnds.Count - 1];

        ways.RemoveAt(ways.Count - 1);
        Destroy(lineRenderers[lineRenderers.Count - 1].gameObject);

        lineRenderers.RemoveAt(lineRenderers.Count - 1);

        lastTos.RemoveAt(lastTos.Count - 1);
        lastBegin.color = ways.Count > 0 ? Color.blue : Color.white;

        bool stillExists = false;
        for(int i = 0; i < dotWayBegins.Count - 2; i++) 
        {
            if (dotWayBegins[i] == lastEnd)
            {
                stillExists = true;
                break;
            }
        }

        for (int i = 0; i < dotWayEnds.Count - 2; i++)
        {
            if (dotWayEnds[i] == lastEnd)
            {
                stillExists = true;
                break;
            }
        }

        lastEnd.color = stillExists ? Color.yellow : Color.white;

        dotWayBegins.RemoveAt(dotWayBegins.Count - 1);
        dotWayEnds.RemoveAt(dotWayEnds.Count - 1);
    }

    public void SetOneWay()
    {
        if (ways.Count == 0) return;
        var way = ways[ways.Count - 1];
        if (way.direction == 2)
        {
            way.direction = 1;
            Destroy(lineRenderers[lineRenderers.Count - 1].transform.GetChild(0).gameObject);
        }
        else
        {
            bool available = true;
            for(int i = 0; i < ways.Count - 1; i++)
            {
                if (way.startingGridPosition == ways[i].endGridPositon && way.endGridPositon == ways[i].startingGridPosition)
                {
                    available = false;
                    break;
                }
            }

            if (available)
            {
                way.direction = 2;
                GameObject red = Instantiate(redArrow) as GameObject;

                float angle = AngleBetweenVector2(GridManager.GetGridManger().GetPosForGrid(way.startingGridPosition),
                    GridManager.GetGridManger().GetPosForGrid(way.endGridPositon));

                red.transform.rotation = Quaternion.Euler(0, 0, angle);
                red.transform.position = GetPointOnLine(lineRenderers[lineRenderers.Count - 1]);
                red.transform.parent = lineRenderers[lineRenderers.Count - 1].transform;
            }
            else
            {
                Toast.instance.ShowMessage("Can not set one way for this line");
            }
        }
    }

    private float AngleBetweenVector2(Vector2 vec1, Vector2 vec2)
    {
        Vector2 diference = vec2 - vec1;
        float sign = (vec2.y < vec1.y) ? -1.0f : 1.0f;
        return Vector2.Angle(Vector2.right, diference) * sign;
    }

    private Vector3 GetPointOnLine(LineRenderer line)
    {
        Vector3 posStart = line.GetPosition(0);
        Vector3 posEnd = line.GetPosition(1);

        Vector3 direction = (posEnd - posStart).normalized;

        return posStart + direction * 0.25f;
    }

    public void ExportLevel()
    {
        List<Ways> finalWays = new List<Ways>();

        foreach(var way in ways)
        {
            Ways hasWay = null;
            foreach(var fWay in finalWays)
            {
                if (fWay.startingGridPosition == way.startingGridPosition && fWay.endGridPositon == way.endGridPositon ||
                    fWay.startingGridPosition == way.endGridPositon && fWay.endGridPositon == way.startingGridPosition)
                {
                    hasWay = fWay;
                }
            }

            if (hasWay == null)
            {
                finalWays.Add(way);
            }
            else
            {
                string suffix = hasWay.startingGridPosition == way.startingGridPosition ? "_s" : "_o";
                if (hasWay.pathTag == 1)
                {
                    hasWay.solutions += hasWay.solutionPosition + "_s";
                }
                hasWay.pathTag++;
                hasWay.solutions += "," + solution + suffix;
                if (way.direction == 2) hasWay.direction = 2;
            }
        }
        
        string result = "";
        for (int i = 0; i < finalWays.Count; i++)
        {
            result += JsonUtility.ToJson(finalWays[i]);

            if (i != (finalWays.Count - 1))
            {
                result += "=";
            }
        }

        PlayerPrefs.SetInt("level_editor_package", packageNumber);
        PlayerPrefs.SetInt("level_editor_level", levelNumber);

        string path = string.Format("Assets/OneLine/Resources/Package_{0}/level_{1}.json", packageNumber, levelNumber);

        if (!File.Exists(path))
        {
#if UNITY_EDITOR
            string folderPath = "Assets/OneLine/Resources/Package_" + packageNumber;
            if (!AssetDatabase.IsValidFolder(folderPath))
                AssetDatabase.CreateFolder("Assets/OneLine/Resources", "Package_" + packageNumber);

#endif

            StreamWriter writer = new StreamWriter(path, true);
            writer.Write(result);
            writer.Close();

#if UNITY_EDITOR
            UnityEditor.AssetDatabase.Refresh();
#endif

            Toast.instance.ShowMessage("Done");
        }
        else
        {
            Toast.instance.ShowMessage("Level exists. Please delete it before exporting");
        }
    }
}
