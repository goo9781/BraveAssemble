[System.Serializable]
public class BAEnemySpawnData : BAGameDataBase
{
    public string StageID;
    public string UnitID;
    public float StartDelay;
    public float SpawnInterval;
    public int InitialPoolSize;
    public int MaxAliveCount;
    public int TotalSpawnCount;
}
