using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Tilemaps;
using Unity.Mathematics;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using Unity.Collections;


public class UnitMover : MonoBehaviour
{
    private bool changePath;
    private bool isWalking;

    private NativeList<int2> Path;

    private void Update()
    {
        if (MainGridScript.Clicked)
        {
            int2 startPos = MainGridScript.MainGrid.GetXY(transform.position);
            //Debug.Log(x2 + " " + y2);
            float startTime = Time.realtimeSinceStartup;
            //Path = MainGridScript.FindPath(startPos, MainGridScript.ClickPosition);
            Debug.Log("Time:" + (Time.realtimeSinceStartup - startTime) * 1000);
            if (isWalking)
            {
                changePath = true;
            }
            else
            {
                //StartCoroutine(MoveOnPath());
            }
        }
    }
    //IEnumerator MoveOnPath()
    //{
        //isWalking = true;
        //for (int i = 1; i < Path.Length; i++)
        //{
        //    if (changePath)
        //    {
        //        i = 1;
        //        changePath = false;
        //    }
        //    Guy.transform.position = GetWorldPosition(Path[i]);
        //    yield return new WaitForSeconds(.5f);
        //}
        //isWalking = false;
        //if (changePath)
        //{
        //    StartCoroutine(MoveOnPath());
        //}

    //}
}
