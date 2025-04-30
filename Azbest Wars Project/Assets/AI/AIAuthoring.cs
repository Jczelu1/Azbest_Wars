using UnityEngine;

public class AIAuthoring : MonoBehaviour
{
    [SerializeField]
    float AggressiveChance = .33f;
    [SerializeField]
    float DestDefendChance = .5f;
    [SerializeField]
    float DestRandomness = .25f;
    [SerializeField]
    float ReadjustChance = .01f;
    [SerializeField]
    float ChangeDestChance = .0005f;
    [SerializeField]
    float FormationGrowChance = .5f;
    [SerializeField]
    int FormationMinSize = 3;
    [SerializeField]
    int FormationMaxSize = 6;
    [SerializeField]
    float ConserveChance = .5f;
    [SerializeField]
    int ConserveDuration = 10;
    [SerializeField]
    int StartDelay = 10;
    void Start()
    {
        ArtificialIdiot.AGGRESSIVE_CHANCE = AggressiveChance;
        ArtificialIdiot.DEST_DEFEND_CHANCE = DestDefendChance;
        ArtificialIdiot.DEST_RANDOMNESS = DestRandomness;
        ArtificialIdiot.READJUST_CHANCE = ReadjustChance;
        ArtificialIdiot.CHANGE_DEST_CHANCE = ChangeDestChance;
        ArtificialIdiot.FORMATION_GROW_CHANCE = FormationGrowChance;
        ArtificialIdiot.FORMATION_MIN_SIZE = FormationMinSize;
        ArtificialIdiot.FORMATION_MAX_SIZE = FormationMaxSize;
        ArtificialIdiot.CONSERVE_DURATION = ConserveDuration;
        ArtificialIdiot.CONSERVE_CHANCE = ConserveChance;
        ArtificialIdiot.conserving = 0;
        ArtificialIdiot.startDelay = StartDelay;
    }
}
