using UnityEngine;

public class MainGridCell
{
    public bool IsWalkable;
    public bool IsOccupied;
    public MainGridCell(bool isWalkable = false, bool isOccupied = false)
    {
        IsWalkable = isWalkable;
        IsOccupied = isOccupied;
    }
    public override string ToString()
    {
        return IsWalkable.ToString();
    }
}
