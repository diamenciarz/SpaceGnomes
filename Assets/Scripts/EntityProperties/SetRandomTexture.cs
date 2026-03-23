using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetRandomTexture : MonoBehaviour
{
    [SerializeField] List<Sprite> possibleSprites;
    [SerializeField] List<float> weights;

    private SpriteRenderer spriteRenderer;
    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        SetRandomSprite();
    }

    private void SetRandomSprite()
    {
        int index = MathUtils.GetWeightedIndex(weights);
        spriteRenderer.sprite = possibleSprites[index];
    }
}
