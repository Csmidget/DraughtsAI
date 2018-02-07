using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stone : MonoBehaviour {

    [SerializeField]
    [Range(1,2)]
    private int owner;

    [SerializeField]
    private bool king;

    private bool mouseDown;

    Vector3 lastMousePos;
    Vector3 originPoint;

	// Use this for initialization
	void Start () {
        originPoint = transform.position;
	}

    void OnMouseDown()
    {
        if (GameManager.GetActive().GetActivePlayer() == owner)
        {
            mouseDown = true;
            lastMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            BoardManager.GetActive().StonePicked(gameObject);
        }
    }

    void OnMouseUp()
    {
        mouseDown = false;
        lastMousePos = new Vector3();
        transform.position = originPoint;
        BoardManager.GetActive().StoneDropped(gameObject);
    }

    // Update is called once per frame
    void Update () {
		
        if (mouseDown)
        {
            Vector3 newMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            transform.Translate(newMousePos- lastMousePos);
            lastMousePos = newMousePos;
        }
	}
}
