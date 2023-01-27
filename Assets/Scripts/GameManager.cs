using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    [Header("List of all object")]
    [Tooltip("Drop All Prefab Object Here !")]public GameObject[] allObject;
    [Tooltip("Drop All Mat Here !")]public Material[] allMat;
    private int matIndex;

    private List<GameObject> placedObject = new();

    [Header("Zone")]
    [SerializeField]private GameObject buildingZone;
    private GameObject ground;

    
    private GameObject currentObject;
    private int currentIndex;
    private DroppableObject droppableObject;
    [SerializeField]private bool selected;
    private bool isObjectRotationChanged;
    private bool isGroundRotated;

    [System.Serializable]
    public struct AlreadyPlacedObject
    {
        public bool wasAlreadyPlaced;
        public Vector3 initialPos;
        public Quaternion initialRot;
    }
    private AlreadyPlacedObject alreadyPlacedObject;

    public Vector3 mouse;
    public Vector3 inputMouse;
    // Start is called before the first frame update
    void Start()
    {
        ground = GameObject.FindGameObjectWithTag("Ground");
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        
    }

    #region INPUT


    //Create new object
    public void OnCreateObject(InputAction.CallbackContext ctx)
    {
        if(ctx.performed && !selected)
        {
            //read name of input and convert into index
            int index = int.Parse(ctx.control.name);
            SelectObject(index);
        }
        
    }


    public void OnLeftClickInteraction(InputAction.CallbackContext ctx)
    {
        if(ctx.performed)
        {
            //Reselect object
            if(!selected)
            {
                //Make new ray to get the object
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if(Physics.Raycast(ray, out hit) && hit.transform.tag != "Ground")
                {
                    GameObject hittedObject = hit.transform.parent.gameObject;
                    if(!placedObject.Contains(hittedObject))
                    {
                        Debug.Log($"Don't : {hittedObject.name}");
                        return;
                    }
                    currentObject = placedObject.Find(x => x == hittedObject);
                    selected = true;
                    SetAlreadyPlacedObject(currentObject.transform.position, currentObject.transform.rotation, true);
                    droppableObject = currentObject.GetComponentInChildren<DroppableObject>(true);
                    droppableObject.enabled = true;
                    droppableObject.gameObject.layer = 2;
                    
                    MeshRenderer currentMeshRenderer = droppableObject.GetComponent<MeshRenderer>();
                    for(int i = 0; i < allMat.Length; i++)
                    {
                        if(currentMeshRenderer.material.name == allMat[i].name)matIndex = i + 1;
                    }
                    
                    droppableObject.Droppable(true, Color.green);
                    StartCoroutine(MoveObject());
                }
                else return;

            }
            //if object already selected, place it
            else
            {
                if(!droppableObject.IsDroppable)return;
                //currentMesh.material.color = currentObjColor;
                droppableObject.gameObject.layer = 0;
                matIndex = 0;
                ResetDroppable();
                if(alreadyPlacedObject.wasAlreadyPlaced)ResetAlreadyPlacedObject();
                ResetVariable();
            }
        }
    }

    public void OnRightClickInteraction(InputAction.CallbackContext ctx)
    {
        if(ctx.performed)
        {
            if(!selected || currentObject == null)return;
            MeshRenderer currentMeshRenderer = currentObject.GetComponentInChildren<MeshRenderer>();
            if(matIndex >= allMat.Length)matIndex = 0;
            currentMeshRenderer.material = allMat[matIndex];
            currentMeshRenderer.material.name = allMat[matIndex].name;
            droppableObject.originalColor = allMat[matIndex].color;
            matIndex += 1;
        }
    }


    //delete selected object
    public void OnDeleteObject(InputAction.CallbackContext ctx)
    {
        if(ctx.performed)
        {
            if(!selected || currentObject == null)return;
            DeleteObject();
        }
    }

    
    public void OnRotateObject(InputAction.CallbackContext ctx)
    {
        if(ctx.performed)
        {
            if(!selected || currentObject == null)return;
            float scrollDir = ctx.ReadValue<float>();
            Vector3 positiveRotate;
            Vector3 negativeRotate;
            switch(isObjectRotationChanged)
            {
                case true :
                    positiveRotate = Vector3.up;
                    negativeRotate = Vector3.down;
                break;
                case false :
                    positiveRotate = Vector3.right;
                    negativeRotate = Vector3.left;
                break;
            }

            if(scrollDir > 0)currentObject.transform.RotateAround(currentObject.transform.position, positiveRotate, 15);
            else currentObject.transform.RotateAround(currentObject.transform.position, negativeRotate, 15);

        }
    }

    public void OnRotateBuildingZone(InputAction.CallbackContext ctx)
    {
        if(ctx.performed)
        {
            Vector2 rotateDir = ctx.ReadValue<Vector2>();
            isGroundRotated = true;
            StartCoroutine(RotateGround(rotateDir));
        }
        if(ctx.canceled)
        {
            isGroundRotated = false;
            StopCoroutine("RotateGround");
        }
    }

    //Change type of rotation (Left/Right to Up/Down Rotation)
    public void OnChangeRotationType(InputAction.CallbackContext ctx)
    {
        if(ctx.performed)
        {
            if(!selected || currentObject == null)return;
            isObjectRotationChanged = !isObjectRotationChanged;
        }
    }
    
    //Cancel selection of object
    public void OnCancelSelection(InputAction.CallbackContext ctx)
    {
        if(ctx.performed)
        {
            if(!selected || currentObject == null)return;
            
            //return current object to it initial place
            if(alreadyPlacedObject.wasAlreadyPlaced)
            {
                currentObject.transform.position = alreadyPlacedObject.initialPos;
                currentObject.transform.rotation = alreadyPlacedObject.initialRot;

                //droppableObject.GetComponent<Collider>().enabled = true;
                
                StopCoroutine(MoveObject());
                ResetAlreadyPlacedObject();
                ResetDroppable();
                ResetVariable();
            }
            else
            {
                //delete object if it was created
                DeleteObject();
            }
        }
    }

    public void OnMovingObjectUp(InputAction.CallbackContext ctx)
    {
        if(ctx.performed)
        {
            Collider collider = droppableObject?.GetComponent<Collider>();
            if(collider == null)return;
            if(droppableObject.IsDroppable)currentObject.transform.position += new Vector3(0f, collider.bounds.size.y, 0f);
            else currentObject.transform.position += new Vector3(0f, droppableObject.allObjectIn[0].GetComponent<Collider>().bounds.size.y +.0001f, 0f);
        }
    }

    #endregion

    #region ACTION
    //Select new object created
    private void SelectObject(int key)
    {
        currentIndex = key - 1;
        //spawn object
        currentObject = Instantiate(allObject[currentIndex], buildingZone.transform);
        //add current object to placed object
        placedObject.Add(currentObject);
        selected = true;
        droppableObject = currentObject.GetComponentInChildren<DroppableObject>();
        currentObject.transform.position += new Vector3(0f, droppableObject.GetComponent<Collider>().bounds.extents.y, 0f);
        droppableObject.Droppable(true, Color.green);
        matIndex = 0;
        StartCoroutine(MoveObject());
    }

    //Make object move with mouse position
    private IEnumerator MoveObject()
    {
        while(selected)
        {
            mouse = GetMousePosition();
            inputMouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            Vector3 mousePos = GetMousePosition();
            currentObject.transform.position = new Vector3(mousePos.x, currentObject.transform.position.y, mousePos.z);
            
            yield return null;
        }
    }

    private IEnumerator RotateGround(Vector2 dir)
    {
        while(isGroundRotated)
        {
            ground.transform.RotateAround(ground.transform.position, dir, 15 * Time.deltaTime);
            yield return null;
        }
    }

    private void DeleteObject()
    {
        placedObject.Remove(currentObject);
        Destroy(currentObject);
        ResetVariable();
        if(alreadyPlacedObject.wasAlreadyPlaced)ResetAlreadyPlacedObject();
        StopCoroutine(MoveObject());
    }
    //check mouse position
    private Vector3 GetMousePosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        //Debug.DrawRay(ray.origin, ray.direction * 100, Color.blue);
        
        IsObjectDroppable(ray);

        //Ray for returning mouse position
        Plane plane = new Plane(Vector3.up, ground.transform.position);
        float distance;
        plane.Raycast(ray, out distance);
        return ray.GetPoint(distance);
    }

    //Check if object is droppable
    private void IsObjectDroppable(Ray ray)
    {
        //all object hitted by raycast
        RaycastHit[] hits = Physics.RaycastAll(ray, Mathf.Infinity);
        
        //Undroppable when raycast void
        if(hits.Length <= 0)droppableObject.Droppable(false, Color.red);
        else
        {
            foreach(RaycastHit hit in hits)
            {
                //if hitted object is ground, make object droppable
                if(hit.transform.tag == "Ground")droppableObject.Droppable(true, Color.green);
                //if hitted object isn't ground (exemple : another object), make object undroppable
                else if(hit.transform.tag != "Ground" && droppableObject.allObjectIn.Count > 0)droppableObject.Droppable(false, Color.red);
                //Debug.Log(hit.transform.name);
            }

        }
        //Debug.Log(hit.transform.name);
    }

    //Reset variables
    private void ResetVariable()
    {
        droppableObject = null;
        currentObject = null;
        currentIndex = 0;
        if(selected)selected = false;
    }

    private void ResetDroppable()
    {
        droppableObject.ReturnOriginalColor();
        droppableObject.gameObject.layer = 0;
        droppableObject.enabled = false;
    }

    private void SetAlreadyPlacedObject(Vector3 pos, Quaternion rot, bool wasAlreadyPlaced)
    {
        alreadyPlacedObject.initialPos = pos;
        alreadyPlacedObject.initialRot = rot;
        alreadyPlacedObject.wasAlreadyPlaced = wasAlreadyPlaced;
    }

    private void ResetAlreadyPlacedObject()
    {
        alreadyPlacedObject.initialPos = Vector3.zero;
        alreadyPlacedObject.initialRot = Quaternion.Euler(Vector3.zero);
        alreadyPlacedObject.wasAlreadyPlaced = false;
    }
    #endregion

}

