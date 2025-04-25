using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Scenes;

public class LevelController : MonoBehaviour
{
    public void ExitGame()
    {
        Application.Quit();
    }
    public void RestartScene()
    {
        StaticReset();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    public void LoadScene(string sceneName)
    {
        StaticReset();
        SceneManager.LoadScene(sceneName);
    }
    public void StaticReset()
    {
        //SpawnerSystem
        if (SpawnerSystem.unitTypes.IsCreated)
            SpawnerSystem.unitTypes.Dispose();
        SpawnerSystem.unitTypes = new NativeList<UnitTypeData>(Allocator.Persistent);
        SpawnerSystem.unitTypesDescription = new List<DescriptionData>();
        SpawnerSystem.setSelectedQueue = -1;
        SpawnerSystem.setSelectedUnitType = -1;
        SpawnerSystem.started = false;
        //BuildingTypeSystem
        BuildingTypeSystem.started = false;
        BuildingTypeSystem.buildingTypesDescription = new List<DescriptionData>();
        //MapTextureSystem
        MapTextureSystem.mapTexture = null;
        MapTextureSystem.baseTexture = null;
        //TickSystemGroup
        TickSystemGroup.SetTickrate(2);
        //DescriptionController
        DescriptionController.close = false;
        DescriptionController.show = false;
        DescriptionController.updateDesc = false;
        DescriptionController.showBuilding = false;
        DescriptionController.showId = -1;
        //SpawnerInputController
        SpawnerInputController.UIClosedByPlayer = false;
        //UnitStateInput
        UnitStateInput.currentMovementState = 255;
        //ArrowSystem
        if(ArrowSystem.SpawnedArrows.IsCreated) ArrowSystem.SpawnedArrows.Dispose();
        ArrowSystem.SpawnedArrows = new NativeList<Entity>(Allocator.Persistent);
        ArrowSystem.started = false;
        //PathfindSystem.
        if (PathfindSystem.shouldMove.IsCreated) PathfindSystem.shouldMove.Dispose();
        if (PathfindSystem.destinations.IsCreated) PathfindSystem.destinations.Dispose();
        if (PathfindSystem.selectedUnits.IsCreated) PathfindSystem.selectedUnits.Dispose();
        if (PathfindSystem.setMoveState.IsCreated) PathfindSystem.setMoveState.Dispose();
        PathfindSystem.shouldMove = new NativeArray<bool>(4, Allocator.Persistent);
        PathfindSystem.destinations = new NativeArray<int2>(4, Allocator.Persistent);
        PathfindSystem.selectedUnits = new NativeArray<int>(4, Allocator.Persistent);
        PathfindSystem.setMoveState = new NativeArray<byte>(4, Allocator.Persistent);
        //RangedAttackSystem
        if(RangedAttackSystem.SpawnArrows.IsCreated) RangedAttackSystem.SpawnArrows.Dispose();
        RangedAttackSystem.SpawnArrows = new NativeList<Arrow>(Allocator.Persistent);
        //SelectSystem
        SelectSystem.updateSelect = false;
        SelectSystem.resetSelect = true;
        SelectSystem.unitsSelected = 0;
        SelectSystem.unitTypeSelected = -2;
        SelectSystem.movementStateSelected = 255;
        SelectSystem.buildingTypeSelected = -2;
        SelectSystem.buildingsSelected = 0;
        SelectSystem.spawnerSelected = false;
        SelectSystem.selectedEntity = Entity.Null;
        //SetupSystem
        SetupSystem.startDelay = 6;
        //SmoothMoveSystem
        SmoothMoveSystem.enabled = true;
        //ArtificialIdiot
        if(ArtificialIdiot.captureAreas.IsCreated) ArtificialIdiot.captureAreas.Dispose();
        ArtificialIdiot.captureAreas = new NativeList<Entity>(Allocator.Persistent);
        if (ArtificialIdiot.formations.IsCreated) ArtificialIdiot.formations.Dispose();
        ArtificialIdiot.formations = new NativeList<Formation>(Allocator.Persistent);
        //WinConditionSystem
        WinConditionSystem.Ended = false;
        WinConditionSystem.Win = false;
        WinConditionSystem.WinPoints = 0;
        WinConditionSystem.EnemyWinPoints = 0;
        WinConditionSystem.startDelay = 2;
        //CaptureSystem
        CaptureSystem.areaMarked = false;
    }
}
