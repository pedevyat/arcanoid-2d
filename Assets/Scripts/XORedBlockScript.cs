using System.Collections.Generic;
using UnityEngine;

public class XORedBlockScript : BlockScript
{
    static readonly List<XORedBlockScript> allBlocks = new List<XORedBlockScript>();

    bool markedX;

    void OnEnable()
    {
        allBlocks.RemoveAll(block => block == null);
        if (!allBlocks.Contains(this))
            allBlocks.Add(this);
    }

    void OnDestroy()
    {
        allBlocks.Remove(this);
    }

    protected override void Start()
    {
        base.Start();
    }

    protected override void OnCollisionEnter2D(Collision2D collision)
    {
        base.OnCollisionEnter2D(collision);
    }

    protected override string GetLabel()
    {
        return markedX ? "X" : "O";
    }

    public override void ApplyHit(int damage)
    {
        markedX = !markedX;
        RefreshLabel();

        if (AllBlocksMarkedX())
            DestroyAllBlocks();
    }

    bool AllBlocksMarkedX()
    {
        allBlocks.RemoveAll(block => block == null);

        if (allBlocks.Count == 0)
            return false;

        foreach (var block in allBlocks)
        {
            if (!block.markedX)
                return false;
        }

        return true;
    }

    void DestroyAllBlocks()
    {
        var blocks = new List<XORedBlockScript>(allBlocks);
        allBlocks.Clear();

        foreach (var block in blocks)
        {
            if (block != null)
                block.DestroyBlock();
        }
    }
}
