using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    public GameObject cube;
    private string pId;
    private Vector4 pColour;
    public Vector3 positVec;
    private bool isPlayerC;
    public void PlayerStart(string id, float r, float g, float b, bool isPlayer)
    {
        pId = id;
        pColour = new Vector4(r, g, b, 1);
        isPlayerC = isPlayer;
    }
    public void PlayerUpdate(string id, float r, float g, float b, float x, float y, float z)
    {
        pId = id;
        pColour = new Vector4(r, g, b, 1);
        if (!isPlayerC)
            positVec = new Vector3(x,y,z);
    }
    // Update is called once per frame
    void Update()
    {
        if (isPlayerC)
        {
            if(Input.GetKeyDown("a"))
        {
                positVec += Vector3.left;
            }
            if (Input.GetKeyDown("w"))
            {
                positVec += Vector3.forward;
            }
            if (Input.GetKeyDown("s"))
            {
                positVec += Vector3.back;
            }
            if (Input.GetKeyDown("d"))
            {
                positVec += Vector3.right;
            }
        }
        cube.GetComponent<MeshRenderer>().material.SetColor("_Color", pColour);
        transform.position = positVec;
        
    }
}
