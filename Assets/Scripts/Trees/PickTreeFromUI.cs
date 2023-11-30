using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Class that takes care of the UI borders for the trees, as well as user interaction with them, on top of highlighting picked trees
/// </summary>
public class PickTreeFromUI : MonoBehaviour, IPointerClickHandler
{

    private Image treeBorder;
    public bool isPicked = false;
    [SerializeField] private SimpleTree currentTree;
    public Population currentPopulation;

    PickTreeFromUI(Population myPop)
    {
        currentPopulation = myPop;
    }

    private void Start()
    {
        treeBorder = GetComponent<Image>();
    }

    // Triggers when the UI element is left clicked
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            SelectTree();
        }
    }

    // Gets called in OnPointerClick function
    public void SelectTree()
    {
        if (!isPicked)
        {
            treeBorder.color = Color.red;
            isPicked = true;

            Debug.Log("picked UI tree: " + currentTree.instanceID + ", UI: " + transform.name);
            // update the weight of the assigned tree
            currentTree.UpdateWeight(isPicked, currentPopulation);

        }
        else
        {
            ResetTreePick();

            currentTree.UpdateWeight(isPicked, currentPopulation);
        }

    }

    /// <summary>
    /// turns the colour back to black and unpicks it
    /// </summary>
    public void ResetTreePick()
    {
        treeBorder.color = Color.black;
        isPicked = false;

        Debug.Log("unpicked UI tree: " + currentTree.instanceID + ", UI: " + transform.name);
    }

    /// <summary>
    /// Assign a tree to the spot
    /// </summary>
    /// <param name="tree"></param>
    public void AssignTree(SimpleTree tree)
    {
        currentTree = tree;
    }
}
