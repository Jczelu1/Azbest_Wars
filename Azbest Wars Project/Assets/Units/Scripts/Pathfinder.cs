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
    public static void FindPath(in int2 start, in int2 end, in int2 gridSize, in NativeArray<bool> isWalkable, ref NativeList<int2> path)
    {
        NativeArray<PathNode> pathNodeArray = new NativeArray<PathNode>(gridSize.x * gridSize.y, Allocator.Temp);

        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                int nodeIndex = GetIndex(new int2(x, y), gridSize.x);
                PathNode node = new PathNode();
                //node.x = x;
                //node.y = y;
                //node.index = GetIndex(new int2(x, y), gridSize.x);

                node.gCost = int.MaxValue;
                node.fCost = int.MaxValue;
                node.previousNodeIndex = -1;

                pathNodeArray[nodeIndex] = node;
            }
        }


        int startIndex = GetIndex(start, gridSize.x);
        PathNode startNode = pathNodeArray[startIndex];
        int endIndex = GetIndex(end, gridSize.x);
        PathNode endNode = pathNodeArray[endIndex];
        if (!isWalkable[endIndex])
        {
            pathNodeArray.Dispose();
            path = new NativeList<int2>(Allocator.Temp);
            return;
        }
        startNode.gCost = 0;
        startNode.fCost = GetDistanceCost(start, end);
        //set value on array because value type
        pathNodeArray[startIndex] = startNode;

        NativeList<int> openList = new NativeList<int>(Allocator.Temp);
        NativeArray<bool> closedSet = new NativeArray<bool>(gridSize.x * gridSize.y, Allocator.Temp, NativeArrayOptions.ClearMemory);


        openList.Add(startIndex);

        while (openList.Length > 0)
        {
            int currentIndex = GetLowestFCostNodeIndex(openList, pathNodeArray);


            if (currentIndex == GetIndex(end, gridSize.x))
            {
                //end
                path.AddRange(GetPath(pathNodeArray, endIndex, gridSize.x).AsArray());
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
            int2 currentPosition = GetPosition(currentIndex, gridSize.x);
            for (int i = 0; i < directions.Length; i++)
            {
                int2 offset = directions[i];
                int2 neighbourPosition = new int2(currentPosition.x + offset.x, currentPosition.y + offset.y);

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
                int tentativeGCost = currentNode.gCost + GetDistanceCost(currentPosition, neighbourPosition);
                if (tentativeGCost < neighbourNode.gCost)
                {
                    neighbourNode.previousNodeIndex = currentIndex;
                    neighbourNode.gCost = tentativeGCost;
                    neighbourNode.fCost = tentativeGCost+GetDistanceCost(neighbourPosition, end);
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
    private static int2 GetPosition(int index, int width)
    {
        return new int2(index % width, index / width);
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
        int lowestIndex = openList[0];
        PathNode lowestNode = pathNodeArray[lowestIndex];
        for (int i = 1; i < openList.Length; i++)
        {
            PathNode checkNode = pathNodeArray[openList[i]];
            if (checkNode.fCost < lowestNode.fCost)
            {
                lowestNode = checkNode;
                lowestIndex = openList[i];
            }
        }
        return lowestIndex;
    }
    private static bool IsInGrid(int2 pos, int2 gridSize)
    {
        return
            pos.x >= 0 && pos.y >= 0 &&
            pos.x < gridSize.x && pos.y < gridSize.y;
    }
    private static  NativeList<int2> GetPath(NativeArray<PathNode> pathNodeArray, int endIndex, int width)
    {
        PathNode endNode = pathNodeArray[endIndex];
        if (endNode.previousNodeIndex == -1)
        {
            //no path
            return new NativeList<int2>(Allocator.Temp);
        }
        else
        {
            //path
            NativeList<int2> path = new NativeList<int2>(Allocator.Temp);
            path.Add(GetPosition(endIndex, width));
            int currentIndex = endNode.previousNodeIndex;
            while (currentIndex != -1)
            {
                PathNode currentNode = pathNodeArray[currentIndex];
                path.Add(GetPosition(currentIndex, width));
                currentIndex = currentNode.previousNodeIndex;
            }
            return path;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct PathNode
    {
        public int gCost;
        public int fCost;
        public int previousNodeIndex;
    }
}
