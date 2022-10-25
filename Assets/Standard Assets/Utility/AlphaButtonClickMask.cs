using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class AlphaButtonClickMask : MonoBehaviour, ICanvasRaycastFilter 
{
    protected Image Image;

    public void Start()
    {
        Image = GetComponent<Image>();

        Texture2D tex = Image.sprite.texture as Texture2D;

        bool isInvalid = false;
        if (tex != null)
        {
            try
            {
                tex.GetPixels32();
            }
            catch (UnityException e)
            {
                Debug.LogError(e.Message);
                isInvalid = true;
            }
        }
        else
        {
            isInvalid = true;
        }

        if (isInvalid)
        {
            Debug.LogError("This script need an Image with a readbale Texture2D to work.");
        }
    }

    public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
    {
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(Image.rectTransform, sp, eventCamera, out localPoint);

		Vector2 pivot = Image.rectTransform.pivot;
		Vector2 normalizedLocal = new Vector2(pivot.x + localPoint.x / Image.rectTransform.rect.width, pivot.y + localPoint.y / Image.rectTransform.rect.height);
        Vector2 uv = new Vector2(
            Image.sprite.rect.x + normalizedLocal.x * Image.sprite.rect.width, 
            Image.sprite.rect.y + normalizedLocal.y * Image.sprite.rect.height );

        uv.x /= Image.sprite.texture.width;
        uv.y /= Image.sprite.texture.height;

        //uv are inversed, as 0,0 or the rect transform seem to be upper right, then going negativ toward lower left...
        Color c = Image.sprite.texture.GetPixelBilinear(uv.x, uv.y);

        return c.a> 0.1f;
    }
}
