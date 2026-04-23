using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetRandomTexture : MonoBehaviour, ISerializationCallbackReceiver
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
    #region Serialization
    public void OnAfterDeserialize()
    {

    }
    public virtual void OnBeforeSerialize()
    {
        ControlListLengths();
    }
    private void ControlListLengths()
    {
        if (weights.Count < possibleSprites.Count)
        {
            weights.Add(0);
        }
        if (weights.Count > possibleSprites.Count)
        {
            weights.RemoveAt(weights.Count - 1);
        }
    }
    #endregion
}
