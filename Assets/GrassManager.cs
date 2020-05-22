using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager : MonoBehaviour
{
    public GameObject grassPrefab;

    public float spawnRate = 1;
    public float reproduceRate = 100;

    public float reproduceRange = 5;
    private float spawnRange;

    private float lastSpawn;
    private float lastReproduce;

    public bool allowReproduce = false;
    public bool allowAutoSpawn= false;

    public bool spawnWhenEaten = true;

    public int maxGrass = 20;

    public Transform spawnArea;

    public Transform spawnPoint;

    [SerializeField] public HashSet<GameObject> allGrass;

    // Start is called before the first frame update
    void Start()
    {
        lastSpawn = Time.time;
        lastReproduce = Time.time;
        allGrass = new HashSet<GameObject>();
        //spawnRange = spawnArea != null ? spawnArea.localScale.x * .5f : 10;
        spawnRange = 20;

        SpawnGrass();
    }

    // Update is called once per frame
    void Update()
    {
        
        if (allGrass != null && allGrass.Count < maxGrass)
        {
            //need to reproduce
            if (allowReproduce && Time.time - lastReproduce > reproduceRate)
            {
                lastReproduce = Time.time;
                var children = new HashSet<GameObject>();

                foreach (GameObject grass in allGrass)
                {
                    reproduceGrass(grass.transform.position, children);
                }

                allGrass.UnionWith(children);

            }

            //need to spawn
            if (allowAutoSpawn && Time.time - lastSpawn > spawnRate)
            {
                lastSpawn = Time.time;
                SpawnGrass();
            }
        }
    }

    public void removeFromAllGrass(GameObject grass)
    {
        if(grass != null && grass.tag == "Grass")
        {
            allGrass.Remove(grass);
            Destroy(grass);
            Debug.Log("GrassEaten!");

            if (spawnWhenEaten)
            {
                SpawnGrass();
            }
        }
    }

    void SpawnGrass()
    {
        Vector3 childPostion = spawnPoint.position + new Vector3(Random.value * spawnRange - spawnRange / 2, 0.5f, Random.value * spawnRange - spawnRange / 2);
        //Vector3 childPostion = spawnPoint.position;
        var newGrass = Instantiate(grassPrefab, childPostion, Quaternion.identity);
        newGrass.transform.parent = transform.parent;

        allGrass.Add(newGrass);
    }

    void reproduceGrass(Vector3 parentPostion, HashSet<GameObject> children)
    {
        Vector3 childPostion = parentPostion + new Vector3(Random.value * reproduceRange - reproduceRange / 2, 0, Random.value * reproduceRange - reproduceRange / 2);
        var newGrass = Instantiate(grassPrefab, childPostion, Quaternion.identity);
        newGrass.transform.parent = transform.parent;

        children.Add(Instantiate(grassPrefab, childPostion, Quaternion.identity));
    }

}
