using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockWall : MonoBehaviour
{
    public int height, width, depth;
    public GameObject blockPrefab;

    private Rigidbody[] blocks;

    void Start()
    {
        StartCoroutine(GenerateBlocks());
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
            StartCoroutine(GenerateBlocks());
    }

    IEnumerator GenerateBlocks()
    {

        int total = height * width * depth;
        blocks = new Rigidbody[total];
        var col = blockPrefab.GetComponent<Collider>();

        int currentHeight = 0;
        int currentWidth = 0;
        int currentDepth = 0;

        for(int b = 0; b < blocks.Length; b++)
        {
            Vector3 pos = new Vector3(currentWidth, currentHeight, currentDepth);
            pos = Vector3.Scale(pos, col.bounds.size);

            var block = Instantiate(blockPrefab, pos, Quaternion.identity);

            blocks[b] = block.GetComponent<Rigidbody>() ? block.GetComponent<Rigidbody>() : block.AddComponent<Rigidbody>();
            blocks[b].useGravity = true;
            blocks[b].isKinematic = false;            

            if (++currentWidth == width)
            {
                width = 0;

                if (++currentDepth == depth)
                {
                    currentDepth = 0;

                    if (++currentHeight == height)
                        currentDepth = 0;
                }
            }

            yield return null;
        }
    }
}
