using UnityEngine;
using DG.Tweening;

public class InteractableObject : MonoBehaviour
{
    public float Weight = 1f;
    public int Price;
    public bool IsHeld => isHeld;

    private float sellHeight = 10f;
    private string outlineColorProperty = "_Outline_Color";
    private Renderer visualRenderer;
    private Material outlineMaterial;
    private Color defaultColor;
    private Vector3 defaultScale;
    private bool isHovered;
    private bool isHeld;
    private bool isSold;

    private void Awake()
    {
        defaultScale = transform.localScale;
        string expectedChildName = gameObject.name.Replace("(Clone)", "") + "Visual";
        Transform visualChild = transform.Find(expectedChildName);
        if (visualChild != null)
        {
            visualRenderer = visualChild.GetComponent<Renderer>();
            if (visualRenderer != null)
            {
                outlineMaterial = visualRenderer.materials[1];
                defaultColor = outlineMaterial.GetColor(outlineColorProperty);
            }
        }
    }

    private void Update()
    {
        CheckSellCondition();
    }

    private void CheckSellCondition()
    {
        if (isSold) return;

        // Processes the sale automatically when the object reaches the specified height threshold
        if (transform.position.y >= sellHeight)
        {
            isSold = true;
            PlayerStateManager.Instance.AddMoney(Price);
            Destroy(gameObject);
        }
    }

    private void OnMouseEnter()
    {
        isHovered = true;
        UpdateOutlineColor();
    }

    private void OnMouseExit()
    {
        isHovered = false;
        UpdateOutlineColor();
    }

    public void SetHoldState(bool state)
    {
        isHeld = state;
        UpdateOutlineColor();
        if (isHeld)
        {
            transform.DOKill();
            Sequence scaleSequence = DOTween.Sequence();
            scaleSequence.Append(transform.DOScale(defaultScale * 1.1f, 0.1f));
            scaleSequence.Append(transform.DOScale(defaultScale * 0.9f, 0.1f));
            scaleSequence.Append(transform.DOScale(defaultScale, 0.1f));
        }
    }

    private void UpdateOutlineColor()
    {
        if (outlineMaterial == null) return;
        if (isHovered || isHeld) outlineMaterial.SetColor(outlineColorProperty, Color.white);
        else outlineMaterial.SetColor(outlineColorProperty, defaultColor);
    }
}