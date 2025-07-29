using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelData  {
	public static int totalLevelsPerWorld = 50;
	public static int prvLevelToCrossToUnLock = 25;
	public static int pressedWorld = 1;

	public static int worldSelected = 1;
    public static int levelSelected = 1;

    // Craig 9/4/2018 - "World" should become Level, and "Level" should become Puzzle.
    public static string[] worldNames = {"Level 1", "Level 2", "Level 3", "Level 4", "Level 5", "Level 6" };

	public static int[] hintGainForWorld = { 3, 5, 7, 19, 21, 25, 25, 25, 25, 25 };


	public static Dictionary<int, List<int>> hintLevel = null;

	public static bool isLevelIsHintLevel(int worldNumber,int level){

		if (hintLevel == null) {
			hintLevel = new Dictionary<int , List<int>> ();

			hintLevel.Add(1, new List<int> (){ 16, 28, 36, 42 });
			hintLevel.Add(2, new List<int> (){ 16, 28, 36, 42 });

			hintLevel.Add(3, new List<int> (){ 16, 28, 36, 42 });

			hintLevel.Add(4, new List<int> (){ 16, 28, 36, 42 });

			hintLevel.Add(5, new List<int> (){ 16, 28, 36, 42 });
			hintLevel.Add(6, new List<int> (){ 16, 28, 36, 42 });

            hintLevel.Add(7, new List<int> (){ 16, 28, 36, 42 });
            hintLevel.Add(8, new List<int> (){ 16, 28, 36, 42 });

            hintLevel.Add(9, new List<int> (){ 16, 28, 36, 42 });
            hintLevel.Add(10, new List<int> (){ 16, 28, 36, 42 });

		}

		if(hintLevel[worldNumber].Contains(level)){

			return true;
		}

		return false;

	}
}
