using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SlotsDef
{
    public float x;
    public float y;
    public bool faceUp = false;
    public string layerName = "Default";
    public int layerID = 0;
    public int id;
    public List<int> hiddenBy = new List<int>();
    public string type = "slot";
    public Vector2 stagger;
}

public class Layout : MonoBehaviour {

    public PT_XMLReader xmlr;
    public PT_XMLHashtable xml;
    public Vector2 multiplier; //sets the spacing of the tableau

    [Header("SlotDef References")]
    public List<SlotsDef> slotDefs; //all the slotDefs for Row0-Row3
    public SlotsDef drawPile;
    public SlotsDef discardPile;
    //this holds all of the possible names for the layers set by layersID
    public string[] sortingLayerNames = new string[]
    {
        "Row0", "Row1", "Row2", "Row3", "Discard", "Draw"
    };

    //this function is called to read in the LayoutXML.xml file
    public void ReadLayout(string xmlText)
    {
        xmlr = new PT_XMLReader();
        xmlr.Parse(xmlText);
        xml = xmlr.xml["xml"][0];

        //read in the multiplier, which sets card spacing
        multiplier.x = float.Parse(xml["multiplier"][0].att("x"));
        multiplier.y = float.Parse(xml["multiplier"][0].att("y"));

        //read in the slots
        SlotsDef tSD;
        //slotsX is used as a shortcut to all the <slot>s
        PT_XMLHashList slotsX = xml["slot"];
        
        for (int i = 0; i < slotsX.Count; i++)
        {
            tSD = new SlotsDef();
            if (slotsX[i].HasAtt("type")) //if this <slot> has a type attribute parse it
            {
                tSD.type = slotsX[i].att("type");                
            }
                
            else //if not, set its type to "slot"; its a tableau card
                tSD.type = "slot";

            //various attributes are parsed into numerical values
            tSD.x = float.Parse(slotsX[i].att("x"));
            tSD.y = float.Parse(slotsX[i].att("y"));
            tSD.layerID = int.Parse(slotsX[i].att("layer"));

            //this converts the number of the layerID into a text layerName
            tSD.layerName = sortingLayerNames[tSD.layerID];
            //the layers are used to make sure that the correct cards are on top of the others

            switch (tSD.type)
            {
                //pull additional attributes based on the type of this <slot>
                case "slot":
                    tSD.faceUp = (slotsX[i].att("faceup") == "1");
                    tSD.id = int.Parse(slotsX[i].att("id"));
                    if (slotsX[i].HasAtt("hiddenby"))
                    {
                        string[] hiding = slotsX[i].att("hiddenby").Split(',');
                        foreach (string s in hiding)
                        {
                            tSD.hiddenBy.Add(int.Parse(s));
                        }
                    }
                    slotDefs.Add(tSD);
                    break;
                case "drawpile":
                    tSD.stagger.x = float.Parse(slotsX[i].att("xstagger"));
                    drawPile = tSD;
                    break;
                case "discardpile":
                    discardPile = tSD;
                    break;
            }

        }

    }
}
