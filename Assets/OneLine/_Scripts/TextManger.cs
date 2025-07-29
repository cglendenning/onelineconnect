using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextManger : MonoBehaviour
{
    public GameObject textPrefab;
    public ShopDialog shopUI;
    public RewardedVideoButton rewardedButton;

    private int showHint = 0; //with every incr show 3 hints;

    public void showHints()
    {
        if (PlayerData.instance.NumberOfHints <= 0)
        {
            if (Purchaser.instance.isEnabled)
            {
                shopUI.Show();
            }
            else
            {
                rewardedButton.OnClick();
            }
            return;
        }

        showHint++;

        int maxPoint = showHint * 3;
        int minPoint = maxPoint - 2;

        WaysUI[] allWays = GameObject.FindObjectsByType<WaysUI>(FindObjectsSortMode.None);

        int count = allWays.Length;
        bool isAnyHintShown = false;
        for (int i = 0; i < count; i++)
        {
            WaysUI wayUi = allWays[i];
            Ways way = wayUi.getWayModel();

            string forStart = "";
            string forEnd = "";

            if (way.pathTag > 1)
            {
                string sol = way.solutions;

                string[] allSol = sol.Split(new char[] { ',' });
                int len = allSol.Length;

                for (int j = 0; j < len; j++)
                {
                    string[] allSolRead = allSol[j].Split(new char[] { '_' });

                    int solIndex = int.Parse(allSolRead[0]);
                    if (solIndex >= minPoint && solIndex <= maxPoint)
                    {
                        if (allSolRead[1].Contains("s"))
                        {
                            forStart = solIndex + "";
                            forEnd = (solIndex + 1) + "";
                        }
                        else
                        {
                            forStart = (solIndex + 1) + "";
                            forEnd = (solIndex) + "";
                        }

                        createOrUpdateTextM(way, forStart, forEnd);
                        isAnyHintShown = true;
                    }
                }
            }
            else if (way.solutionPosition >= minPoint && way.solutionPosition <= maxPoint)
            {
                forStart = way.solutionPosition + "";
                forEnd = (way.solutionPosition + 1) + "";
                createOrUpdateTextM(way, forStart, forEnd);
                isAnyHintShown = true;
            }
        }

        if (isAnyHintShown)
        {
            PlayerData.instance.NumberOfHints -= 1;
            PlayerData.instance.SaveData();

            GameObject.FindFirstObjectByType<UIControllerForGame>()?.UpdateHint();
        }

        GameObject.FindFirstObjectByType<AnimationHandler>()?.runAnimations();
    }

    TextM CreatObj(Vector3 pos)
    {
        GameObject go = Instantiate(textPrefab) as GameObject;
        go.transform.position = pos;
        go.transform.parent = transform;

        return go.GetComponent<TextM>();
    }

    public void createOrUpdateTextM(Ways way, string forStart, string forEnd)
    {
        Vector3 startPos = GridManager.GetGridManger().GetPosForGrid(way.startingGridPosition);
        Vector3 endPos = GridManager.GetGridManger().GetPosForGrid(way.endGridPositon);

        startPos.z = -1;
        endPos.z = -1;
        TextM stPosText = findTextMAtPos(startPos);
        TextM endPosText = findTextMAtPos(endPos);

        if (stPosText == null)
        {
            stPosText = CreatObj(startPos);
        }

        if (endPosText == null)
        {
            endPosText = CreatObj(endPos);
        }

        GameObject.FindFirstObjectByType<AnimationHandler>()?.addAnimationToRun(stPosText.transform.position, int.Parse(forStart), stPosText);
        GameObject.FindFirstObjectByType<AnimationHandler>()?.addAnimationToRun(endPosText.transform.position, int.Parse(forEnd), endPosText);
    }

    public TextM findTextMAtPos(Vector3 pos)
    {
        TextM[] allMeshe = GameObject.FindObjectsByType<TextM>(FindObjectsSortMode.None);

        for (int i = 0; i < allMeshe.Length; i++)
        {
            TextM tM = allMeshe[i];
            if (tM.transform.position.Equals(pos))
            {
                return tM;
            }
        }

        return null;
    }
}

