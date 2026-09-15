using UnityEngine;
using UnityEngine.UI;


public class ColorSwapper : MonoBehaviour
{
    [SerializeField] private Color colorA;
    [SerializeField] private Color colorB;


    [InspectorButton("Swap Colors", true)]
    private void SwapColors()
    {
        DebugLogger.Log("Swapping colors...");

        Component[] sceneComponents = this.FindObjectsOfType<Component>(true);

        foreach (Component comp in sceneComponents)
        {
            if (comp.TryGetComponent(out Image image))
            {
                if (image.color == colorA)
                {
                    image.color = colorB;
                }
            }
        }
    }

    [InspectorButton("ReverseSwap", true)]
    private void Undo()
    {
        DebugLogger.Log("Swapping colors...");

        Component[] sceneComponents = this.FindObjectsOfType<Component>(true);

        foreach (Component comp in sceneComponents)
        {
            if (comp.TryGetComponent(out Image image))
            {
                if (image.color == colorB)
                {
                    image.color = colorA;
                }
            }
        }
    }
}