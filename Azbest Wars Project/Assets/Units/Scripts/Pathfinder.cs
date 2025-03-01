using Unity.Collections;
using UnityEngine;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Burst;
using System.Runtime.InteropServices;
using Unity.VisualScripting;
using NUnit;
using System.IO;
using System;

[BurstCompile]
public struct Pathfinder
{
    const int STRAIGHT_COST = 5;
    const int DIAGONAL_COST = 7;
    private static readonly int2[] directions = new int2[8]
    {
        new int2(0, 1),   new int2(0, -1),
        new int2(-1, 0),  new int2(1, 0),
        new int2(-1, 1),  new int2(1, 1),
        new int2(-1, -1), new int2(1, -1)
    };

    [BurstCompile]
    public static void FindPath(int2 start, int2 end, int2 gridSize, NativeArray<bool> isWalkable, ref NativeList<int2> path)
    {

        NativeArray<PathNode> pathNodeArray = new NativeArray<PathNode>(gridSize.x * gridSize.y, Allocator.Temp);

        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                
                PathNode node = new PathNode();
                node.x = x;
                node.y = y;
                node.index = GetIndex(new int2(x, y), gridSize.x);

                node.gCost = int.MaxValue;
                node.hCost = GetDistanceCost(new int2(x, y), end);
                node.CalculateFCost();
                node.isWalkable = true;
                node.previousNodeIndex = -1;

                pathNodeArray[node.index] = node;
            }
        }



        PathNode startNode = pathNodeArray[GetIndex(start, gridSize.x)];
        PathNode endNode = pathNodeArray[GetIndex(end, gridSize.x)];
        if (!endNode.isWalkable)
        {
            pathNodeArray.Dispose();
            path = new NativeList<int2>(Allocator.Temp);
            return;
        }
        startNode.gCost = 0;
        startNode.CalculateFCost();
        //set value on array because value type
        pathNodeArray[GetIndex(start, gridSize.x)] = startNode;

        NativeList<int> openList = new NativeList<int>(Allocator.Temp);
        NativeArray<bool> closedSet = new NativeArray<bool>(gridSize.x * gridSize.y, Allocator.Temp, NativeArrayOptions.ClearMemory);


        openList.Add(startNode.index);

        while (openList.Length > 0)
        {
            int currentIndex = GetLowestFCostNodeIndex(openList, pathNodeArray);


            if (currentIndex == GetIndex(end, gridSize.x))
            {
                //end
                path.AddRange(GetPath(pathNodeArray, pathNodeArray[GetIndex(end, gridSize.x)]).AsArray());
                break;
            }

            for (int i = 0; i < openList.Length; i++)
            {
                if (openList[i] == currentIndex)
                {
                    openList.RemoveAtSwapBack(i);
                    break;
                }
            }
            closedSet[currentIndex] = true;
            //set value on array because value type
            PathNode currentNode = pathNodeArray[currentIndex];
            for (int i = 0; i < directions.Length; i++)
            {
                int2 offset = directions[i];
                int2 neighbourPosition = new int2(currentNode.x + offset.x, currentNode.y + offset.y);

                if (!IsInGrid(neighbourPosition, gridSize.x))
                {
                    //invalid position
                    continue;
                }

                int neighbourIndex = GetIndex(neighbourPosition, gridSize.x);
                if (closedSet[neighbourIndex])
                {
                    //already searched
                    continue;
                }

                PathNode neighbourNode = pathNodeArray[neighbourIndex];
                if (!isWalkable[neighbourIndex])
                {
                    //unwalkable
                    continue;
                }
                int2 currentPosition = new int2(currentNode.x, currentNode.y);
                int tentativeGCost = currentNode.gCost + GetDistanceCost(currentPosition, neighbourPosition);
                if (tentativeGCost < neighbourNode.gCost)
                {
                    neighbourNode.previousNodeIndex = currentIndex;
                    neighbourNode.gCost = tentativeGCost;
                    neighbourNode.hCost = GetDistanceCost(neighbourPosition, end);
                    neighbourNode.CalculateFCost();
                    pathNodeArray[neighbourIndex] = neighbourNode;

                    if (!openList.Contains(neighbourIndex)) openList.Add(neighbourIndex);
                }
            }
        }

        openList.Dispose();
        closedSet.Dispose();
        pathNodeArray.Dispose();
        return;
    }
    private static int GetIndex(int2 pos, int width)
    {
        return pos.x + (pos.y * width);
    }
    private static int GetDistanceCost(int2 a, int2 b)
    {
        int xDistance = math.abs(a.x - b.x);
        int yDistance = math.abs(a.y - b.y);
        int remaining = math.abs(xDistance - yDistance);
        return DIAGONAL_COST * math.min(xDistance, yDistance) + STRAIGHT_COST * remaining;
    }
    private static int GetLowestFCostNodeIndex(NativeList<int> openList, NativeArray<PathNode> pathNodeArray)
    {
        PathNode lowestNode = pathNodeArray[openList[0]];
        for (int i = 1; i < openList.Length; i++)
        {
            PathNode checkNode = pathNodeArray[openList[i]];
            if (checkNode.fCost < lowestNode.fCost)
            {
                lowestNode = checkNode;
            }
        }
        return lowestNode.index;
    }
    private static bool IsInGrid(int2 pos, int2 gridSize)
    {
        return
            pos.x >= 0 && pos.y >= 0 &&
            pos.x < gridSize.x && pos.y < gridSize.y;
    }
    private static  NativeList<int2> GetPath(NativeArray<PathNode> pathNodeArray, PathNode endNode)
    {
        if (endNode.previousNodeIndex == -1)
        {
            //no path
            return new NativeList<int2>(Allocator.Temp);
        }
        else
        {
            //path
            NativeList<int2> path = new NativeList<int2>(Allocator.Temp);
            path.Add(new int2(endNode.x, endNode.y));
            int currentIndex = endNode.previousNodeIndex;
            while (currentIndex != -1)
            {
                PathNode currentNode = pathNodeArray[currentIndex];
                path.Add(new int2(currentNode.x, currentNode.y));
                //log
                currentIndex = currentNode.previousNodeIndex;
            }
            return path;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct PathNode
    {
        public int x;
        public int y;

        public int index;

        public int gCost;
        public int hCost;
        public int fCost;

        public bool isWalkable;

        public int previousNodeIndex;

        public void CalculateFCost()
        {
            fCost = gCost + hCost;
        }
    }
}
