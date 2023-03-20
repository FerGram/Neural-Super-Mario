using System.Collections.Generic;
using UnityEngine;
using Unity.Barracuda;
using System.Linq;

/* ******NOTE********
 * 
 * The important part of this exercise is done on functions:
 * 
 * - FitnessAdaptative()
 * - GetLevelRating()
 * - GetNewLevel()
 * 
 * We still use the PCG algorithm to get the first level in order to have a starting point,
 * but then we use a modified fitness function that will adapt to the player feedback after
 * 3 lives.
 * For the moment it will only increase the gap size and enemies if the feedback
 * isn't positive. For better future behaviour, the historical data of the "ratings" dictionary
 * could be analyzed to determine if we should increase/decrease enemies and gap size based
 * on it's tendency
 */

public class LevelGenerator : MonoBehaviour
{
    public static LevelGenerator Instance { get; private set; }

    public ColumnInfo[] columns;
    public GameObject playerPrefab;

    [Space]
    public NNModel modelChallenge;
    public NNModel modelEngagement;
    public NNModel modelFrustration;

    //User demographics data
    private float _playedBefore;
    private float _timePlaying;
    private float _playGames;
    private float _age;
    private float _sex;

    private float columnWidth = 0.625f;
    private int levelLength = 100;
    private Vector3 startingPosition = new Vector3(-8, 0, 0);

    //Genetic algorithm for PCG
    private int populationSize = 500;
    private int eliteSize = 100;
    private float crossoverRate = 0.5f;
    private float mutationRate = 0.1f;

    private float adaptativeMutationRate = 0.05f;
    private float adaptativeEnemyModifier= 0.5f;
    private float adaptativeGapModifier= 0.2f;

    const float THRESHOLD = 0.99f;
    const float MAX_GENERATIONS = 200;
    private GameObject levelGO;

    private Dictionary<Level, LevelData> ratings;
    private Level previousLevel;
    private LevelData previousLevelData;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(gameObject);
        }

        ratings = new Dictionary<Level, LevelData>();
    }

    void Start()
    {
        SetUserDemographics();

        List<Level> population = GetRandomLevels();

        int steps = 0;
        while (FitnessBest(population) < THRESHOLD && steps < MAX_GENERATIONS)
        {
            //Crossover
            population = PopulationCrossover(population);

            //Keep the best <eliteSize>
            population.Sort((x, y) => Fitness(y).CompareTo(Fitness(x)));
            population.RemoveRange(eliteSize, population.Count - eliteSize);

            //Fill with copies of best up to <populationSize>
            population = FillPopulation(population);
            //printFitness(population);

            //Debug.Log("Average fitness: " + FitnessAverage(population) + " in step " + steps);
            //Debug.Log("Best fitness: " + FitnessBest(population) + " in step " + steps);

            steps++;
        }

        Level finalLevel = population[0];

        ExtractLevelData(finalLevel);

        //Visualize level
        for (int i = 0; i < levelLength; i++)
        {
            GameObject gameObject = Instantiate(columns[finalLevel.levelLayout[i]].gameObject, transform);
            gameObject.transform.position = startingPosition + Vector3.right * i * columnWidth;
        }
        Instantiate(playerPrefab, startingPosition, Quaternion.identity);
    }

    private void ExtractLevelData(Level finalLevel)
    {
        previousLevel = finalLevel;
        previousLevelData = new LevelData();

        List<int> gapWidths = new List<int>();
        int gapWidth = 0;

        for (int i = 0; i < finalLevel.levelLayout.Length; i++)
        {
            if (columns[finalLevel.levelLayout[i]].height == 0)
            {
                gapWidth++;
            }
            else if (gapWidth != 0)
            {
                gapWidths.Add(gapWidth);
                gapWidth = 0;
            }

            if (columns[finalLevel.levelLayout[i]].hasEnemy)
            {
                previousLevelData.enemyCount++;
            }
        }

        //Average gap width only
        float avgGap = 0;
        foreach (var gap in gapWidths)
        {
            //Debug.Log(gap);
            avgGap += gap;
        }
        avgGap /= gapWidths.Count;
        previousLevelData.avgGapWidth = avgGap;
    }
    //private void printFitness(List<Level> population)
    //{
    //    for (int i = 0; i < population.Count; i++)
    //    {
    //        Debug.Log("Fitness for idx: " + i + ", " + Fitness(population[i]));
    //    }
    //}

    private List<Level> FillPopulation(List<Level> population)
    {
        while(population.Count < populationSize)
        {
            if (population.Count >= eliteSize) population.Add(new Level(population[Random.Range(0, eliteSize)].levelLayout));
            else population.Add(new Level(population[Random.Range(0, population.Count)].levelLayout));
        }
        return population;
    }

    private List<Level> PopulationCrossover(List<Level> population)
    {
        population = ShuffleLevelList(population);
        int count = population.Count;

        for (int i = 0; i < count; i += 2)
        {
            int[] crossoverChildren = new int[population[i].levelLayout.Length];

            for (int columnIdx = 0; columnIdx < population[i].levelLayout.Length; columnIdx++)
            {
                if (Random.value < crossoverRate) crossoverChildren[columnIdx] = population[i].levelLayout[columnIdx];
                else crossoverChildren[columnIdx] = population[i + 1].levelLayout[columnIdx];

                //Mutation
                if (Random.value < mutationRate)
                {
                    crossoverChildren[columnIdx] = Random.Range(0, columns.Length);
                }
            }
            population.Add(new Level(crossoverChildren));
        }
        return population;
    }

    private float FitnessBest(List<Level> population)
    {
        float best = -Mathf.Infinity;
        int bestIdx = 0;
        int i = 0;
        foreach (var level in population)
        {
            float fitness = Fitness(level);
            if (fitness > best)
            {
                best = fitness;
                bestIdx = i;
            }
            i++;
        }
        return best;
    }

    public float Fitness(Level level)
    {
        float gaps = 0;
        float enemies = 0;
        float platforms = 0;

        float scoreModifier = 0;

        for (int i = 0; i < level.levelLayout.Length; i++)
        {
            if (columns[level.levelLayout[i]].height == 0)
            {
                gaps += 1;
            }
            if (columns[level.levelLayout[i]].hasEnemy)
            {
                enemies += 1f;
            }

            if (columns[level.levelLayout[i]].hasPlatform)
            {
                platforms += 1f;
            }

            if (i < 3 && (columns[level.levelLayout[i]].height == 0 || columns[level.levelLayout[i]].hasEnemy))
            {
                scoreModifier -= 0.1f;
            }
        }

        float score = 1.0f - Mathf.Abs(gaps - (float)levelLength / 3f) / (float)levelLength;
        score += 1.0f - Mathf.Abs(enemies - (float)levelLength / 10f) / (float)levelLength;
        score += 1.0f - Mathf.Abs(platforms - (float)levelLength / 10f) / (float)levelLength;

        score /= 3f;
        score += scoreModifier;

        return score;
    }
    public float FitnessAdaptative(Level level, float[] predictions)
    {
        float gaps = 0;
        float gapWidth = 0;
        List<float> gapWidths = new List<float>();

        float enemies = 0;
        float platforms = 0;

        float scoreModifier = 0;

        for (int i = 0; i < level.levelLayout.Length; i++)
        {
            if (columns[level.levelLayout[i]].height == 0)
            {
                gaps += 1;
                gapWidth++;
            }
            else if (gapWidth != 0)
            {
                gapWidths.Add(gapWidth);
                gapWidth = 0;
            }

            if (columns[level.levelLayout[i]].hasEnemy)
            {
                enemies += 1f;
            }

            if (columns[level.levelLayout[i]].hasPlatform)
            {
                platforms += 1f;
            }

            if (i < 3 && (columns[level.levelLayout[i]].height == 0 || columns[level.levelLayout[i]].hasEnemy))
            {
                scoreModifier -= 0.1f;
            }
        }

        float avgGap = 0;
        foreach (var gap in gapWidths)
        {
            avgGap += gap;
        }
        avgGap /= gapWidths.Count;
        

        float score = 1.0f - Mathf.Abs(gaps - (float)levelLength / 3f) / (float)levelLength;
        score += 1.0f - Mathf.Abs(platforms - (float)levelLength / 10f) / (float)levelLength;

        //Encourage more or less enemies if engagement is low (more engagement is lower number)
        //We'll use the challenge prediction to increase/decrease the enemy multiplier
        //The challenge range is [0, 4] and we need to do some transformations to get it to be [-1, 1]. The goal is to get 
        //to a level in which the model predicts a 2 in challenge, which means moderate challenge

        float enemyMultiplier = predictions[0] / 2 - 1;
        float enemyObjective = previousLevelData.enemyCount + Mathf.RoundToInt(adaptativeEnemyModifier * previousLevelData.engagementLevel * enemyMultiplier);
        score += 1.0f - Mathf.Abs((enemies - enemyObjective)) / enemyObjective;

        //Encourage bigger or smaller gap difference if engagement is low (more engagement is lower number)
        //We'll use the frustration prediction to increase/decrease the gap multiplier
        //The challenge range is [0, 4] and we need to do some transformations to get it to be [-1, 1]. The goal is to get
        //to a level in which the model predicts a 2 in frustration, which means moderate frustration

        float bigGapMultiplier = predictions[2] / 2 - 1;
        float avgGapObjective = previousLevelData.avgGapWidth + adaptativeGapModifier * previousLevelData.engagementLevel * bigGapMultiplier;
        score += 1.0f - Mathf.Abs((avgGap - avgGapObjective)) / avgGapObjective;

        score /= 4f;
        score += scoreModifier;

        return score;
    }

    private List<Level> GetRandomLevels()
    {
        List<Level> levels = new List<Level>();
        for (int i = 0; i < populationSize; i++)
        {
            levels.Add(GetRandomLevel());
        }
        return levels;
    }

    private Level GetRandomLevel()
    {
        int[] levelLayout = new int[levelLength];

        for (int i = 0; i < levelLayout.Length; i++)
        {
            levelLayout[i] = Random.Range(0, columns.Length);
        }

        return new Level(levelLayout);
    }

    private List<Level> ShuffleLevelList(List<Level> levels)
    {
        for (int i = 0; i < levels.Count; i++)
        {
            Level temp = levels[i];
            int randomIndex = Random.Range(i, levels.Count);
            levels[i] = levels[randomIndex];
            levels[randomIndex] = temp;
        }
        return levels;
    }

    public void GetLevelRating()
    {
        float[] predictions = MakeLevelPredictions();

        //The engagement prediction is the main sorting prediction. The rest are for calculating gap size and enemy count
        previousLevelData.engagementLevel = predictions[1];
        ratings.Add(new Level(previousLevel.levelLayout), previousLevelData);

        float enthusiasmAvg = 0;
        foreach (var pair in ratings)
        {
            enthusiasmAvg += pair.Value.engagementLevel;
        }
        enthusiasmAvg /= ratings.Count;
        Debug.Log("Average engagement: " + enthusiasmAvg);

        GetNewLevel(predictions);
    }

    private void SetUserDemographics()
    {
        //Since the player gets respawned every 3 lives and this script is persistent, we'll store 
        //the initial random demographics information so that we do not get different user configuration
        //after those 3 lives instead of doing it in the DataGatherer.cs

        _playedBefore = Mathf.Round(Random.value);
        _timePlaying = Mathf.Round(Random.Range(0, 3));
        _playGames = Mathf.Round(Random.value);
        _age = Mathf.Round(Random.Range(20, 50));
        _sex = Mathf.Round(Random.value);

        Debug.Log("Player stats");
        Debug.Log("Played videogames before: " + _playedBefore);
        Debug.Log("Time spent playing: " + _timePlaying);
        Debug.Log("Plays games: " + _playGames);
        Debug.Log("Age: " + _age);
        Debug.Log("Sex: " + _sex);
    }

    private float[] MakeLevelPredictions()
    {
        //Get the data gathered by the runs
        DataGatherer dataGatherer = FindObjectOfType<DataGatherer>();
        float[] myArray = dataGatherer.GetData();

        //Set the persistent demographics
        myArray[0] = _playedBefore;
        myArray[1] = _timePlaying;
        myArray[2] = _playGames;
        myArray[3] = _age;
        myArray[4] = _sex;

        //float[] myArray = new float[26] { 1, 0, 1, 39, 1, 1, 1, 2, 0, 1, 2, 4, 0, 2, 1, 1, 2, 1, 2, 0, 0, 1, 0, 0, 0, 1 };

        //Input data to tensor
        var inputTensor = new Tensor(1, 26, myArray);
        Debug.Log(inputTensor.DataToString());

        //Get predictions
        float challengePrediction = LoadAndPredict(modelChallenge, inputTensor, "Challenge");
        float engagementPrediction = LoadAndPredict(modelEngagement, inputTensor, "Engagement");
        float frustrationPrediction = LoadAndPredict(modelFrustration, inputTensor, "Frustration");

        //Return the predictions
        float[] predictions = new float[3] { Mathf.Round(challengePrediction),
                                         Mathf.Round(engagementPrediction),
                                         Mathf.Round(frustrationPrediction) };

        inputTensor.Dispose();
        return predictions;
    }

    private float LoadAndPredict(NNModel model, Tensor modelInput, string prediction)
    {
        //Load model and create worker
        var runtimeModel = ModelLoader.Load(model);
        var worker = WorkerFactory.CreateWorker(WorkerFactory.Type.ComputePrecompiled, runtimeModel);

        //Call the model
        worker.Execute(modelInput);

        //Get output and print it
        float[] output = worker.PeekOutput().ToReadOnlyArray();

        Debug.Log($"Predicted {prediction}: " + output[0]);

        worker.Dispose();

        return output[0];
    }

    private void GetNewLevel(float[] predictions)
    {
        Level fillLevel = new Level(ratings.ElementAt(ratings.Count - 1).Key.levelLayout);
        List<Level> population = new List<Level>() { fillLevel };
        population = FillPopulation(population);

        //Increase mutation rate if lower engagement (more engagement is lower number)
        float mutation = mutationRate + adaptativeMutationRate * previousLevelData.engagementLevel;
        do
        {
            for (int i = 0; i < population.Count; i++)
            {
                for (int columnIdx = 0; columnIdx < population[i].levelLayout.Length; columnIdx++)
                {
                    //Mutation
                    if (Random.value < mutation)
                    {
                        population[i].levelLayout[columnIdx] = Random.Range(0, columns.Length);
                    }
                }
            }
            population.Sort((x, y) => FitnessAdaptative(y, predictions).CompareTo(FitnessAdaptative(x, predictions)));
        }
        while (!ratings.ContainsKey(population[0]));

        Level finalLevel = population[0];

        ExtractLevelData(finalLevel);

        //Visualize level
        for (int i = 0; i < levelLength; i++)
        {
            Destroy(transform.GetChild(i).gameObject);
            GameObject gameObject = Instantiate(columns[finalLevel.levelLayout[i]].gameObject, transform);
            gameObject.transform.position = startingPosition + Vector3.right * i * columnWidth;
        }
        Destroy(FindObjectOfType<PlayerMover>().gameObject);
        Instantiate(playerPrefab, startingPosition, Quaternion.identity);
    }

    private void OnDestroy()
    {
        //// Create folder Prefabs and set the path as within the Prefabs folder,
        //// and name it as the GameObject's name with the .Prefab format
        //if (!Directory.Exists("Assets/StoredLevels"))
        //    AssetDatabase.CreateFolder("Assets", "StoredLevels");
        //string localPath = "Assets/StoredLevels/" + gameObject.name + ".prefab";

        //// Make sure the file name is unique, in case an existing Prefab has the same name.
        //localPath = AssetDatabase.GenerateUniqueAssetPath(localPath);

        //// Create the new Prefab and log whether Prefab was saved successfully.
        //bool prefabSuccess;
        //PrefabUtility.SaveAsPrefabAssetAndConnect(gameObject, localPath, InteractionMode.UserAction, out prefabSuccess);
        //if (prefabSuccess == true)
        //    Debug.Log("Prefab was saved successfully");
        //else
        //    Debug.Log("Prefab failed to save" + prefabSuccess);
    }
}

public struct LevelData
{
    public float engagementLevel;
    public int enemyCount;
    public float avgGapWidth;
}
