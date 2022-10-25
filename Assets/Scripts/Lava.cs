using UnityEngine;

public class Lava : MonoBehaviour
{
    [SerializeField] private Material material;
    [SerializeField] private Vector3 translationLava;
    [SerializeField] private Vector2 translationMaterial;

    private void FixedUpdate()
    {
        transform.Translate(translationLava);
        material.mainTextureOffset += translationMaterial;
    }
}