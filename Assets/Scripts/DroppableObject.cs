using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroppableObject : MonoBehaviour
{
    [SerializeField]private bool isDroppable = true;
    [SerializeField]private MeshRenderer meshRenderer;
    public Color originalColor;

    public List<GameObject> allObjectIn;
    
    public bool IsDroppable
    {
        get{return isDroppable;}
        private set{;}
    }
    // Start is called before the first frame update
    void Awake()
    {
        meshRenderer = this.GetComponent<MeshRenderer>();
        originalColor = meshRenderer.material.color;
    }

    //When selected object collide with already place other object,
    //can't place it
    private void OnTriggerEnter(Collider collider)
    {
        if(!this.enabled)return;
        if(!collider.CompareTag("Ground"))
        {
            //Droppable(false, Color.red);
            allObjectIn.Add(collider.gameObject);
        }
    }

    /*private void OnTriggerStay(Collider collider)
    {
        if(!this.enabled)return;
        if(allObjectIn.Count > 0)Droppable(false, Color.red);
    }*/

    //return to normal if ALL object aren't colliding with current one anymore
    private void OnTriggerExit(Collider collider)
    {
        if(!this.enabled)return;
        if(!collider.CompareTag("Ground"))
        {
            allObjectIn.Remove(collider.gameObject);
            //if(allObjectIn.Count > 0)return;
            //Droppable(true, Color.green);
        }
    }

    //make object droppable or not
    public void Droppable(bool droppable, Color color)
    {
        isDroppable = droppable;
        meshRenderer.material.color = color;
    }

    //return object to original color
    public void ReturnOriginalColor()
    {
        meshRenderer.material.color = originalColor;
    }
}
