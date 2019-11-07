using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card : MonoBehaviour {

    public string suit;
    public int rank;
    public Color color = Color.black; // color to tint pips
    public string colS = "Black"; //or "Red. Name of the color
    //this list holds all of the Decorator GameObjects
    public List<GameObject> decoGOs = new List<GameObject>();
    //this list holds all of the Pip GameObjects
    public List<GameObject> pipGOs = new List<GameObject>();

    public GameObject back; //the GameObject of the back of the card
    public CardDefinition def; //Parsed from DeckXML.xml
    //List of the SpriteRenderer components of this GameObject and its children
    public SpriteRenderer[] spriteRenderers;

    void Start()
    {
        SetSortOrder(0);
    }

    public bool faceUp
    {
        get
        {
            return (!back.activeSelf);
        }
        set
        {
            back.SetActive(!value);
        }
    }
    
    virtual public void OnMouseUpAsButton()
    {
        print(name);
    }

    //if spriteRenderers is not yet defined, this function defines it
    public void PopulateSpriteRenderers()
    {
        //if spriteRenderers is null or empty
        if (spriteRenderers == null || spriteRenderers.Length == 0) 
        {
            //get SpriteRenderer components of this GameObject and its children
            spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        }
    }

    //sets the sortingLayerName on all SpriteRenderer components
    public void SetSortingLayerName(string tSLN)
    {
        PopulateSpriteRenderers();
        foreach (SpriteRenderer tSR in spriteRenderers)
        {
            tSR.sortingLayerName = tSLN;
        }
    }

    //sets the sortingOrder of all SpriteRenderer components
    public void SetSortOrder(int sOrd)
    {
        PopulateSpriteRenderers();
        //the white background of the card is on bottom (sOrd)
        //on top of that are all the pips, decorators, faces, etc (sOrd + 1)
        // the back is on top so when visible it covers the rest (sOrd + 2)

        //iterate through all the spriteRenderers as tSR
        foreach (SpriteRenderer tSR in spriteRenderers)
        {
            if(tSR.gameObject == this.gameObject)
            {
                //if the gameObject is this.gameObject its the background
                tSR.sortingOrder = sOrd;
                continue;
            }
            //each of the children of this GameObject are named switch based on the names
            switch (tSR.gameObject.name)
            {
                case "back":
                    tSR.sortingOrder = sOrd + 2;                    
                    break;
                case "face":
                default:
                    tSR.sortingOrder = sOrd + 1;
                    break;
            }
        }
    }
}

[System.Serializable]
public class Decorator
{
    [Header("Decorator")]
    //this class stores info about each decorator or pip from DeckXML
    public string type; //for card pips, type = pip
    public Vector3 loc; //sprite location on the card
    public bool flip = false;
    public float scale = 1f; //sprite scale
    
}

[System.Serializable]
public class CardDefinition
{
    [Header("CardDefinition")]
    //this class stores information for each rank of card
    public string face;// sprite to use for each face card
    public int rank; //rank of this card
    public List<Decorator> pips = new List<Decorator>(); //pips used because decorators (from XML) are used the same way on every card in the deck.
    //pips only stores info about the pips on numbered cards
}
