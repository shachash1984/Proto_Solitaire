using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CardState
{
    drawPile,
    tableau,
    target,
    discard
}
public class CardProspector : Card
{

    public CardState state = CardState.drawPile;
    //the hiddenBy list stores which other cards will keep this one face down
    public List<CardProspector> hiddenBy = new List<CardProspector>();
    //layout ID matches this card to a layout XML id if its a tableau card
    public int layoutID;
    //the SlotDef class stores info pulled in from the layoutXML <slot>
    public SlotsDef slotDef;

    public override void OnMouseUpAsButton()
    {

        //call the CardClicked method on the Prospector singleton
        Prospector.S.CardClicked(this);
        //also call the base class (Card.cs) version
        base.OnMouseUpAsButton();
    }

    public void OnMouseDown()
    {
        PlaySound();
    }

    public void PlaySound()
    {
        GetComponent<AudioSource>().Play();
    }

}
