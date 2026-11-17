using UnityEngine;

public enum LevelBlockType
{
    Blue,
    Red,
    Green,
    Yellow,
    MovingYellow,
    MovingBlue,
    RedXO
}

[DisallowMultipleComponent]
public class LevelDesignerScript : MonoBehaviour
{
    public int levelNumber = 1;
    public LevelBlockType blockType = LevelBlockType.Red;
    public Vector3 startPosition = new Vector3(-4f, 2f, 0f);
    public int count = 5;
    public float horizontalSpacing = 1.3f;
    public float verticalSpacing = 0.7f;
    public string mixedPattern = "Red,Green,Blue";
    public int mixedCount = 9;

    public Transform levelRoot;

    [Header("Block Prefabs")]
    public GameObject bluePrefab;
    public GameObject redPrefab;
    public GameObject greenPrefab;
    public GameObject yellowPrefab;
    public GameObject movingYellowPrefab;
    public GameObject movingBluePrefab;
    public GameObject redXOPrefab;

    void Awake()
    {
        if (!Application.isPlaying)
            return;

        if (levelRoot == null)
        {
            var child = transform.Find("Designed Blocks");
            if (child != null)
                levelRoot = child;
        }

        if (levelRoot != null)
        {
            levelRoot.gameObject.SetActive(false);
            Destroy(levelRoot.gameObject);
        }
    }
}
