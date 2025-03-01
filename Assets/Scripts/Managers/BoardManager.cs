using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BoardManager : MonoBehaviour
{
    [Header("-------------Prefabs-------------")]
    public ExitCellObject ExitCellPrefab;
    public FoodObject[] FoodPrefabs;
    public WallObject[] WallPrefabs;
    public Enemy EnemyPrefab;
    public Enemy_2 Enemy_2_Prefab;

    [Header("----------Food Settings----------")]
    public int FromNumberOfFood;
    public int ToNumberOfFood;
    public int DefaultFromNumberOfFood;
    public int DefaultToNumberOfFood;

    [Header("----------Wall Settings----------")]
    public int FromNumberOfWall;
    public int ToNumberOfWall;
    public int DefaultFromNumberOfWall;
    public int DefaultToNumberOfWall;

    [Header("----------Board Settings---------")]
    public int Width;
    public int Height;
    public int DefaultWidth;
    public int DefaultHeight;
    public Tile[] GroundTiles;
    public Tile[] WallTiles;

    [Header("----------Zombie Checks----------")]
    public bool IsZombie;
    public bool IsZombieUpDate;
    public bool IsZombieUpDate2;

    private CellData[,] m_BoardData;
    private Tilemap m_Tilemap;
    private Grid m_Grid;
    private List<Vector2Int> m_EmptyCellsList;
    private List<Enemy> m_Enemies = new List<Enemy>();
    private List<Enemy_2> m_Enemies_2 = new List<Enemy_2>();
    private List<FoodObject> m_Foods = new List<FoodObject>();
    private List<WallObject> m_Walls = new List<WallObject>();

    public class CellData
    {
        public bool Passable;
        public CellObject ContainedObject;
    }

    public void Init()
    {
        InitializeBoard();
        GenerateInitialObjects();
        HandleLevelSpecificSettings();
    }

    private void InitializeBoard()
    {
        m_Tilemap = GetComponentInChildren<Tilemap>();
        m_Grid = GetComponentInChildren<Grid>();
        m_EmptyCellsList = new List<Vector2Int>();
        m_BoardData = new CellData[Width, Height];

        for (int y = 0; y < Height; ++y)
        {
            for (int x = 0; x < Width; ++x)
            {
                Tile tile;
                m_BoardData[x, y] = new CellData();

                if (IsBorderCell(x, y))
                {
                    tile = WallTiles[Random.Range(0, WallTiles.Length)];
                    m_BoardData[x, y].Passable = false;
                }
                else
                {
                    tile = GroundTiles[Random.Range(0, GroundTiles.Length)];
                    m_BoardData[x, y].Passable = true;
                    m_EmptyCellsList.Add(new Vector2Int(x, y));
                }

                m_Tilemap.SetTile(new Vector3Int(x, y, 0), tile);
            }
        }

        m_EmptyCellsList.Remove(new Vector2Int(1, 1));
    }

    private bool IsBorderCell(int x, int y)
    {
        return x == 0 || y == 0 || x == Width - 1 || y == Height - 1;
    }

    private void GenerateInitialObjects()
    {
        GenerateExitSign();
        GenerateFood();
        GenerateWall();
    }

    private void GenerateExitSign()
    {
        Vector2Int endCoord = new Vector2Int(Width - 2, Height - 2);
        AddObject(Instantiate(ExitCellPrefab), endCoord);
        m_EmptyCellsList.Remove(endCoord);
    }

    private void GenerateFood()
    {
        int foodCount = Random.Range(FromNumberOfFood, ToNumberOfFood);
        for (int i = 0; i < foodCount; ++i)
        {
            Vector2Int coord = GetRandomEmptyCell();
            FoodObject newFood = Instantiate(FoodPrefabs[Random.Range(0, FoodPrefabs.Length)]);
            AddObject(newFood, coord);
            m_Foods.Add(newFood);
        }

        AdjustFoodCountForLevel();
    }

    private void AdjustFoodCountForLevel()
    {
        if (GameManager.Instance.CurrentLevel == 1)
        {
            FromNumberOfFood = DefaultFromNumberOfFood;
            ToNumberOfFood = DefaultToNumberOfFood;
        }

        if (GameManager.Instance.CurrentLevel % 5 == 0)
        {
            FromNumberOfFood++;
            ToNumberOfFood++;
        }
    }

    private void GenerateWall()
    {
        int wallCount = Random.Range(FromNumberOfWall, ToNumberOfWall);
        for (int i = 0; i < wallCount; ++i)
        {
            Vector2Int coord = GetRandomEmptyCell();
            WallObject newWall = Instantiate(WallPrefabs[Random.Range(0, WallPrefabs.Length)]);
            AddObject(newWall, coord);
            m_Walls.Add(newWall);
        }

        AdjustWallCountForLevel();
    }

    private void AdjustWallCountForLevel()
    {
        if (GameManager.Instance.CurrentLevel == 1)
        {
            FromNumberOfWall = DefaultFromNumberOfWall;
            ToNumberOfWall = DefaultToNumberOfWall;
        }

        if (GameManager.Instance.CurrentLevel % 5 == 0)
        {
            FromNumberOfWall += 3;
            ToNumberOfWall += 3;
        }
    }

    private void HandleLevelSpecificSettings()
    {
        int enemyCount = GameManager.Instance.CurrentLevel / 5;
        int enemy2Count = GameManager.Instance.CurrentLevel / 10;

        Debug.Log("Level to update:" + GameManager.Instance.CurrentLevel);
        if (GameManager.Instance.CurrentLevel > 5 && GameManager.Instance.CurrentLevel % 5 == 0)
        {
            EnemyPrefab.Damage++;
            EnemyPrefab.Health++;
        }
        if (GameManager.Instance.CurrentLevel > 10 && GameManager.Instance.CurrentLevel % 10 == 0)
        {
            Enemy_2_Prefab.Damage++;
            Enemy_2_Prefab.Health++;
        }

        for (int i = 0; i < enemyCount; i++)
        {
            AddEnemyXToList(GenerateEnemyX());
            IsZombie = true;
        }

        for (int i = 0; i < enemy2Count; i++)
        {
            AddEnemyJToList(GenerateEnemyJ());
            IsZombie = true;
        }
    }

    private void AddEnemyXToList(Enemy newEnemy)
    {
        if (newEnemy != null)
        {
            m_Enemies.Add(newEnemy);
        }
    }

    private Enemy GenerateEnemyX()
    {
        if (m_EmptyCellsList.Count == 0)
        {
            Debug.LogWarning("No empty cells available to spawn an enemy.");
            return null;
        }

        Vector2Int coord = GetRandomEmptyCell();
        m_EmptyCellsList.Remove(coord);
        Enemy newEnemy = Instantiate(EnemyPrefab);
        AddObject(newEnemy, coord);
        return newEnemy;
    }

    private void AddEnemyJToList(Enemy_2 newEnemy2)
    {
        if (newEnemy2 != null)
        {
            m_Enemies_2.Add(newEnemy2);
        }
    }

    private Enemy_2 GenerateEnemyJ()
    {
        if (m_EmptyCellsList.Count == 0)
        {
            Debug.LogWarning("No empty cells available to spawn an enemy.");
            return null;
        }

        Vector2Int coord = GetRandomEmptyCell();
        m_EmptyCellsList.Remove(coord);
        Enemy_2 newEnemy = Instantiate(Enemy_2_Prefab);
        AddObject(newEnemy, coord);
        return newEnemy;
    }

    private Vector2Int GetRandomEmptyCell()
    {
        int randomIndex = Random.Range(0, m_EmptyCellsList.Count);
        Vector2Int coord = m_EmptyCellsList[randomIndex];
        m_EmptyCellsList.RemoveAt(randomIndex);
        return coord;
    }

    private void AddObject(CellObject obj, Vector2Int coord)
    {
        CellData data = m_BoardData[coord.x, coord.y];
        obj.transform.position = CellToWorld(coord);
        data.ContainedObject = obj;
        obj.Init(coord);
    }

    public Vector3 CellToWorld(Vector2Int cellIndex)
    {
        return m_Grid.GetCellCenterWorld((Vector3Int)cellIndex);
    }

    public CellData GetCellData(Vector2Int cellIndex)
    {
        if (IsCellIndexValid(cellIndex))
        {
            return m_BoardData[cellIndex.x, cellIndex.y];
        }

        return null;
    }

    private bool IsCellIndexValid(Vector2Int cellIndex)
    {
        return cellIndex.x >= 0 && cellIndex.x < Width && cellIndex.y >= 0 && cellIndex.y < Height;
    }

    public Tile GetCellTile(Vector2Int cellIndex)
    {
        return m_Tilemap.GetTile<Tile>(new Vector3Int(cellIndex.x, cellIndex.y, 0));
    }

    public void Clean()
    {
        if (m_BoardData == null) return;

        for (int y = 0; y < Height; ++y)
        {
            for (int x = 0; x < Width; ++x)
            {
                var cellData = m_BoardData[x, y];
                if (cellData.ContainedObject != null)
                {
                    Destroy(cellData.ContainedObject.gameObject);
                }
                SetCellTile(new Vector2Int(x, y), null);
            }
        }

        ResetPlayerAnimator();
    }

    public void SetCellTile(Vector2Int cellIndex, Tile tile)
    {
        m_Tilemap.SetTile(new Vector3Int(cellIndex.x, cellIndex.y, 0), tile);
    }

    private void ResetPlayerAnimator()
    {
        var animator = GameManager.Instance.PlayerController.Animator;
        animator.SetBool("Panic", false);
        animator.SetBool("Moving", false);
        animator.SetBool("Attack", false);
    }

    public void Save(ref SceneEnemyData data)
    {
        data.Walls = SaveWalls();
        data.Foods = SaveFoods();
        data.Enemies = SaveEnemies();
        data.Enemies_2 = SaveEnemies2();
        data.ToNumberOfFood = ToNumberOfFood;
        data.FromNumberOfFood = FromNumberOfFood;
        data.ToNumberOfWall = ToNumberOfWall;
        data.FromNumberOfWall = FromNumberOfWall;
        data.Width = Width;
        data.Height = Height;
    }

    private WallSaveData[] SaveWalls()
    {
        List<WallSaveData> walls = new List<WallSaveData>();
        for (int i = m_Walls.Count - 1; i >= 0; i--)
        {
            if (m_Walls[i] != null)
            {
                WallObject wall = m_Walls[i];
                WallSaveData saveData = new WallSaveData
                {
                    Position = wall.transform.position,
                    Name = wall.name
                };
                walls.Add(saveData);
            }
            else
            {
                m_Walls.RemoveAt(i);
            }
        }
        return walls.ToArray();
    }

    private FoodSaveData[] SaveFoods()
    {
        List<FoodSaveData> foods = new List<FoodSaveData>();
        for (int i = m_Foods.Count - 1; i >= 0; i--)
        {
            if (m_Foods[i] != null)
            {
                FoodObject food = m_Foods[i];
                FoodSaveData saveData = new FoodSaveData
                {
                    Position = food.transform.position,
                    Name = food.name
                };
                foods.Add(saveData);
            }
            else
            {
                m_Foods.RemoveAt(i);
            }
        }
        return foods.ToArray();
    }

    private EnemySaveData[] SaveEnemies()
    {
        List<EnemySaveData> enemies = new List<EnemySaveData>();
        for (int i = m_Enemies.Count - 1; i >= 0; i--)
        {
            if (m_Enemies[i] != null)
            {
                Enemy enemy = m_Enemies[i];
                EnemySaveData saveData = new EnemySaveData
                {
                    Position = enemy.transform.position
                };
                enemies.Add(saveData);
            }
            else
            {
                m_Enemies.RemoveAt(i);
            }
        }
        return enemies.ToArray();
    }

    private EnemySaveData_2[] SaveEnemies2()
    {
        List<EnemySaveData_2> enemies_2 = new List<EnemySaveData_2>();
        for (int i = m_Enemies_2.Count - 1; i >= 0; i--)
        {
            if (m_Enemies_2[i] != null)
            {
                Enemy_2 enemy = m_Enemies_2[i];
                EnemySaveData_2 saveData = new EnemySaveData_2
                {
                    Position = enemy.transform.position
                };
                enemies_2.Add(saveData);
            }
            else
            {
                m_Enemies_2.RemoveAt(i);
            }
        }
        return enemies_2.ToArray();
    }

    public void Load(SceneEnemyData data)
    {
        Clean();
        Width = data.Width;
        Height = data.Height;
        InitializeBoard();
        GenerateExitSign();
        LoadObjects(data);
    }

    private void LoadObjects(SceneEnemyData data)
    {
        LoadEnemies(data.Enemies);
        LoadEnemies2(data.Enemies_2);
        LoadFoods(data.Foods);
        LoadWalls(data.Walls);

        FromNumberOfFood = data.FromNumberOfFood;
        ToNumberOfFood = data.ToNumberOfFood;
        FromNumberOfWall = data.FromNumberOfWall;
        ToNumberOfWall = data.ToNumberOfWall;
    }

    private void LoadEnemies(EnemySaveData[] enemies)
    {
        foreach (var enemyData in enemies)
        {
            if (enemyData.Position != null)
            {
                Vector2Int cellIndex = new Vector2Int(Mathf.FloorToInt(enemyData.Position.x), Mathf.FloorToInt(enemyData.Position.y));
                Vector3 worldPosition = CellToWorld(cellIndex);
                Enemy enemy = Instantiate(EnemyPrefab, worldPosition, Quaternion.identity);
                AddObject(enemy, cellIndex);
            }
        }
    }

    private void LoadEnemies2(EnemySaveData_2[] enemies_2)
    {
        foreach (var enemyData in enemies_2)
        {
            if (enemyData.Position != null)
            {
                Vector2Int cellIndex = new Vector2Int(Mathf.FloorToInt(enemyData.Position.x), Mathf.FloorToInt(enemyData.Position.y));
                Vector3 worldPosition = CellToWorld(cellIndex);
                Enemy_2 enemy = Instantiate(Enemy_2_Prefab, worldPosition, Quaternion.identity);
                AddObject(enemy, cellIndex);
            }
        }
    }

    private void LoadFoods(FoodSaveData[] foods)
    {
        foreach (var foodData in foods)
        {
            if (foodData.Position != null)
            {
                Vector2Int cellIndex = new Vector2Int(Mathf.FloorToInt(foodData.Position.x), Mathf.FloorToInt(foodData.Position.y));
                Vector3 worldPosition = CellToWorld(cellIndex);
                FoodObject foodObject = GetFoodObjectByName(foodData.Name);
                FoodObject food = Instantiate(foodObject, worldPosition, Quaternion.identity);
                AddObject(food, cellIndex);
            }
        }
    }

    private FoodObject GetFoodObjectByName(string name)
    {
        if (name != null)
        {
            if (name.ToLower().Contains("smalldrink"))
            {
                return FoodPrefabs[0];
            }
            else
            {
                return FoodPrefabs[1];
            }
        }
        return null;
    }

    private void LoadWalls(WallSaveData[] walls)
    {
        foreach (var wallData in walls)
        {
            if (wallData.Position != null)
            {
                Vector2Int cellIndex = new Vector2Int(Mathf.FloorToInt(wallData.Position.x), Mathf.FloorToInt(wallData.Position.y));
                Vector3 worldPosition = CellToWorld(cellIndex);
                WallObject wallObject = GetWallObjectByName(wallData.Name);
                WallObject wall = Instantiate(wallObject, worldPosition, Quaternion.identity);
                AddObject(wall, cellIndex);
            }
        }
    }

    private WallObject GetWallObjectByName(string name)
    {
        if (name != null)
        {
            if (name.Contains("1"))
            {
                return WallPrefabs[1];
            }
            else if (name.Contains("2"))
            {
                return WallPrefabs[2];
            }
            else if (name.Contains("3"))
            {
                return WallPrefabs[3];
            }
            else if (name.Contains("6"))
            {
                return WallPrefabs[4];
            }
            else if (name.Contains("7"))
            {
                return WallPrefabs[5];
            }
            else if (name.Contains("8"))
            {
                return WallPrefabs[6];
            }
            else if (name.Contains("9"))
            {
                return WallPrefabs[7];
            }
            else
            {
                return WallPrefabs[0];
            }
        }
        return null;
    }
}

[System.Serializable]
public struct EnemySaveData
{
    public Vector3 Position;
}

[System.Serializable]
public struct EnemySaveData_2
{
    public Vector3 Position;
}

[System.Serializable]
public struct FoodSaveData
{
    public Vector3 Position;
    public string Name;
}

[System.Serializable]
public struct WallSaveData
{
    public Vector3 Position;
    public string Name;
}

[System.Serializable]
public struct SceneEnemyData
{
    public EnemySaveData[] Enemies;
    public EnemySaveData_2[] Enemies_2;
    public FoodSaveData[] Foods;
    public int FromNumberOfFood;
    public int ToNumberOfFood;
    public WallSaveData[] Walls;
    public int FromNumberOfWall;
    public int ToNumberOfWall;
    public int Width;
    public int Height;
}
