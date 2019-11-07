using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;



//An enum to handle all the possible scoring events
public enum ScoreEvent { draw, mine, mineGold, gameWin, gameLoss}

public class Prospector : MonoBehaviour {

    static public Prospector S;
    static public int SCORE_FROM_PREV_ROUND = 0;
    static public int HIGH_SCORE = 0;
    public float reloadDelay = 3f; // The delay between rounds
    public Vector3 fsPosMid = new Vector3(0.5f, 0.9f, 0);
    public Vector3 fsPosRun = new Vector3(0.5f, 0.75f, 0);
    public Vector3 fsPosMid2 = new Vector3(0.5f, 0.5f, 0);
    public Vector3 fsPosEnd = new Vector3(1.0f, 0.65f, 0);

    public Deck deck;
    public TextAsset deckXML;
    public Layout layout;
    public TextAsset layoutXML;
    public Vector3 layoutCenter;
    public float xOffset = 3;
    public float yOffset = -2.5f;
    public Transform layoutAnchor;
    public CardProspector target;
    public List<CardProspector> tableau;
    public List<CardProspector> discardPile;
    public List<CardProspector> drawPile;

    //fields to track score info
    public int chain = 0; // of cards in this run
    public int scoreRun = 0;
    public int score = 0;
    public FloatingScore fsRun;
    public GUIText GTGameOver;
    public GUIText GTRoundResult;

    void Awake()
    {
        S = this;        
        //check for a high score in PlayerPrefs
        if (PlayerPrefs.HasKey("ProspectorHighScore"))
        {
            HIGH_SCORE = PlayerPrefs.GetInt("ProspectorHighScore");
        }
        //Add the score from last round
        score += SCORE_FROM_PREV_ROUND;
        // and reset the SCORE_FROM_PREV_ROUND
        SCORE_FROM_PREV_ROUND = 0;

        //Set uo the GUITexts that show at the end of the round
        //Get the GUIText Components
        GameObject go = GameObject.Find("GameOver");
        if (go != null)
            GTGameOver = go.GetComponent<GUIText>();

        go = GameObject.Find("RoundResult");
        if (go != null)
            GTRoundResult = go.GetComponent<GUIText>();

        //Make them invisible
        ShowResultGTs(false);

        go = GameObject.Find("HighScore");
        string hScore = "High Score: " + Utils.AddCommasToNumber(HIGH_SCORE);
        go.GetComponent<GUIText>().text = hScore;
    }

    void ShowResultGTs(bool show)
    {
        GTGameOver.gameObject.SetActive(show);
        GTRoundResult.gameObject.SetActive(show);
    }

    
    void Start()
    {
        Scoreboard.S.score = score;
        deck = GetComponent<Deck>();
        deck.InitDeck(deckXML.text);
        Deck.Shuffle(ref deck.cards);

        layout = GetComponent<Layout>();
        layout.ReadLayout(layoutXML.text);

        drawPile = ConvertListCardsToListCardProspectors(deck.cards);
        LayoutGame();
    }

    //the Draw function will pull a single card from the drawPile and return it
    CardProspector Draw()
    {
        CardProspector cd = drawPile[0]; //pull the 0th CardProspector
        drawPile.RemoveAt(0); // then remove it from list<> drawPile
        return cd; 
    }

    //convert from the layoutID int to the CardProspector with that ID
    CardProspector FindCardByLayoutID(int layoutID)
    {
        foreach (CardProspector tCP in tableau)
        {
            //search through all cards in the tableau list
            if (tCP.layoutID == layoutID) //if the card has the same ID return it
                return tCP;
        }
        //if its not found return null
        return null;
    }

    //LayoutGame() positions the initial tableau of cards, aka the "mine"
    void LayoutGame()
    {
        //empty GameObject serves as an anchor for the tableau
        if(layoutAnchor == null)
        {
            GameObject tGo = new GameObject("_LayoutAnchor");
            layoutAnchor = tGo.transform;
            layoutAnchor.transform.position = layoutCenter;
        }

        CardProspector cp;
        //follow the layout
        foreach (SlotsDef tSD in layout.slotDefs)
        {
            //iterate through all the SlotDefs in the layout.slotDefs as tSD
            cp = Draw(); //pull a card from the top (beginning) of the drawPile
            cp.faceUp = tSD.faceUp; //set its faceUp
            cp.transform.parent = layoutAnchor; //this replaces the previous parent: deck.deckAnchor which appears as _Deck in the Hierarchy
            cp.transform.localPosition = new Vector3
                (
                    layout.multiplier.x * tSD.x,
                    layout.multiplier.y * tSD.y,
                    -tSD.layerID
                );

            //set the localPosition of the card based on slotDef
            cp.layoutID = tSD.id;
            cp.slotDef = tSD;
            cp.state = CardState.tableau;
            cp.SetSortingLayerName(tSD.layerName);
            tableau.Add(cp);
        }

        //set which cards are hiding others
        foreach (CardProspector tCP in tableau)
        {
            foreach (int hid in tCP.slotDef.hiddenBy)
            {
                cp = FindCardByLayoutID(hid);
                tCP.hiddenBy.Add(cp);
            }
        }

        //set up the initial taget card
        MoveToTarget(Draw());
        //set up the Draw pile
        UpdateDrawPile();
    }

    //moves the curren target to the discardPile
    void MoveToDiscard(CardProspector cd)
    {
        //set the state of the card to discard
        cd.state = CardState.discard;
        discardPile.Add(cd);
        cd.transform.parent = layoutAnchor;
        cd.transform.localPosition = Vector3.Lerp(cd.transform.localPosition, new Vector3
            (
                layout.multiplier.x * layout.discardPile.x,
                layout.multiplier.y * layout.discardPile.y,
                -layout.discardPile.layerID + 0.5f
            ), 0.25f);
        //position it on the discard pile
        cd.faceUp = true;
        //place it on top of the pile for depth sorting
        cd.SetSortingLayerName(layout.discardPile.layerName);
        cd.SetSortOrder(-100 + discardPile.Count);
    }

    //make cd the new target card
    void MoveToTarget(CardProspector cd)
    {
        //if there is currently a target card, move it to discardPile
        if (target != null)
            MoveToDiscard(target);
        target = cd; // cd is the new target
        cd.state = CardState.target;
        cd.transform.parent = layoutAnchor;
        //move to the target position
        cd.transform.localPosition = new Vector3
            (
                 layout.multiplier.x * layout.discardPile.x,
                 layout.multiplier.y * layout.discardPile.y,
                -layout.discardPile.layerID
            );
        cd.faceUp = true;
        //set the depth sorting
        cd.SetSortingLayerName(layout.discardPile.layerName);
        cd.SetSortOrder(0);
    }

    //arrange all the cards of the drawPile to show how many are left
    void UpdateDrawPile()
    {
        CardProspector cd;
        //go through all the cards of the drawPile
        for (int i = 0; i < drawPile.Count; i++)
        {
            cd = drawPile[i];
            cd.transform.parent = layoutAnchor;
            //position it correctly with the layout.drawPile.stagger
            Vector2 dpStagger = layout.drawPile.stagger;
            cd.transform.localPosition = new Vector3
                (
                    layout.multiplier.x * (layout.drawPile.x + i * dpStagger.x),
                    layout.multiplier.y * (layout.drawPile.y + i * dpStagger.y),
                    -layout.discardPile.layerID + 0.1f * i
                );
            cd.faceUp = false; //make them all face down
            cd.state = CardState.drawPile;
            //set depth sorting
            cd.SetSortingLayerName(layout.drawPile.layerName);
            cd.SetSortOrder(-10 * i);
        }
    }

    List<CardProspector> ConvertListCardsToListCardProspectors(List<Card> lCD)
    {
        List<CardProspector> lCP = new List<CardProspector>();
        CardProspector tCP;
        foreach (Card tCD in lCD)
        {
            tCP = tCD as CardProspector;
            lCP.Add(tCP);
        }
        return lCP;
    }

    public void CardClicked(CardProspector cd)
    {
        //reaction is determined by the state of the clicked card
        switch (cd.state)
        {
            case CardState.drawPile:
                //clicking any card in the drawPile will draw the next card
                MoveToDiscard(target); // moves the target to the discardPile
                MoveToTarget(Draw()); // moves the next drawn card to the target
                UpdateDrawPile(); // restacks the drawPile
                ScoreManager(ScoreEvent.draw);
                break;
            case CardState.tableau:
                //clicking a card in the tableau  will check if its a valid play
                bool validMatch = true;
                if (!cd.faceUp) // if the card is face down its not valid
                    validMatch = false;                
                if (!AdjacentRank(cd, target)) //if its not an adjacent rank its not valid
                    validMatch = false;
                if (!validMatch) return; // return if not valid
                //if we are here it means the card is valid
                tableau.Remove(cd); //remove it from the tableau list
                MoveToTarget(cd);
                SetTableauFaces(); // update tableau card face-ups
                ScoreManager(ScoreEvent.mine);
                break;
            case CardState.target:
                //clicking the target card does nothing
                break;
            case CardState.discard:
                break;
            default:
                break;
        }
        //check to see whether the game is over
        CheckForGameOver();
    }

    //this turns cards in the Mine face-up or face-down
    void SetTableauFaces()
    {
        foreach (CardProspector cd in tableau)
        {
            bool fup = true; // assume the card will be face-up
            foreach (CardProspector cover in cd.hiddenBy)
            {
                //if either of the covering cards are in the tableau
                if (cover.state == CardState.tableau)
                    fup = false; // then this card is face-down
            }
            cd.faceUp = fup; // set the value to the card
        }
    }

    //return true if the two cards are adjacent in rank (A & K wrap around)
    public bool AdjacentRank(CardProspector c0, CardProspector c1)
    {
        //if either card is face-down , its not adjacent
        if (!c0.faceUp || !c1.faceUp) return false;

        //if they are 1 apart they are adjacent
        if (Mathf.Abs(c0.rank - c1.rank) == 1) return true;

        //if one is A and the other King, they're adjacent
        if (c0.rank == 1 && c1.rank == 13) return true;
        if (c1.rank == 1 && c0.rank == 13) return true;

        //otherwise
        return false;
    }

    //test whether the game is over
    void CheckForGameOver()
    {
        if (tableau.Count == 0)
        {
            //call GameOver() with a win
            GameOver(true);
            return;
        }

        //if there are still cards in the draw pile, the game's not over
        if (drawPile.Count > 0)
            return;
        //Check for remaining valid plays
        foreach (CardProspector cd in tableau)
        {
            if (AdjacentRank(cd, target)) {
                //if there is a valid play the game's not over
                return;
            }
        }

        //since there are no valid plays. the game is over.
        //call GameOver with a loss
        GameOver(false);
    }

    //called when the game is over. simple for now but expandable
    void GameOver(bool won)
    {
        if (won)
            ScoreManager(ScoreEvent.gameWin);
        else
            ScoreManager(ScoreEvent.gameLoss);
        //Reload the scene in reloadDelay seconds
        //This will give the score a moment to travel
        Invoke("ReloadLevel", reloadDelay);

    }

    void ReloadLevel()
    {
        //Reload the scene, resetting the game
        SceneManager.LoadScene(0);
    }

    //ScoreManager handles all of the scoring
    void ScoreManager(ScoreEvent sEvent)
    {
        List<Vector3> fsPts;
        switch (sEvent)
        {
            //same things need to happen whether it's a draw, a win , or a loss
            case ScoreEvent.draw: // drawing a card
            case ScoreEvent.gameWin: //won the round                
            case ScoreEvent.gameLoss: // lost the round
                chain = 0; //resets the score chain
                score += scoreRun;// add scoreRun to total score
                scoreRun = 0; //reset scoreRun
                //Add fsRun to the _Scoreboard score
                if(fsRun != null)
                {
                    fsPts = new List<Vector3>();
                    fsPts.Add(fsPosRun);
                    fsPts.Add(fsPosMid2);
                    fsPts.Add(fsPosEnd);
                    fsRun.reportFinishTo = Scoreboard.S.gameObject;
                    fsRun.Init(fsPts, 0, 1);
                    //Also adjust the fontSize
                    fsRun.fontSizes = new List<float>(new float[] { 28, 36, 4 });
                    fsRun = null; // Clear fsRun so it's created again
                }
                break;
                case ScoreEvent.mine://remove a mine card
                chain++;//increase the score chain
                scoreRun += chain; //add score for this card to run
                //Create a FloatingScore for this score
                FloatingScore fs;
                //Move it from the mousePosition to fsPosRun
                Vector3 p0 = Input.mousePosition;
                p0.x /= Screen.width;
                p0.y /= Screen.height;
                fsPts = new List<Vector3>();
                fsPts.Add(p0);
                fsPts.Add(fsPosMid);
                fsPts.Add(fsPosRun);
                fs = Scoreboard.S.CreateFloatingScore(chain, fsPts);
                fs.fontSizes = new List<float>(new float[] { 4, 50, 28 });
                if(fsRun == null)
                {
                    fsRun = fs;
                    fsRun.reportFinishTo = null;
                }
                else
                {
                    fs.reportFinishTo = fsRun.gameObject;
                }
                break;
        }

        //This second switch statement handles round wins and losses
        switch (sEvent)
        {
            case ScoreEvent.gameWin:
                GTGameOver.text = "Round Over";                
                //if its a win, add the score to the next round
                // static fields are NOT reset by SceneManager.LoadScene();
                Prospector.SCORE_FROM_PREV_ROUND = score;
                print("You won this round! Round score: " + score);
                GTRoundResult.text = "You won this round\nRound Score: " + score;
                ShowResultGTs(true);
                break;
            case ScoreEvent.gameLoss:
                //if its a loss check against the high score
                if (Prospector.HIGH_SCORE <= score)
                {
                    print("You got the high score! High score: " + score);
                    string sRR = "You got the high score!\nHigh score: " + score;
                    GTRoundResult.text = sRR;
                    Prospector.HIGH_SCORE = score;
                    PlayerPrefs.SetInt("ProspectorHighScore", score);
                }
                else
                {
                    print("Your final score for the game was: " + score);
                    GTRoundResult.text = "Your final score was: " + score;
                }
                ShowResultGTs(true);
                break;
            default:
                print("score:" + score + " scoreRun:" + scoreRun + " chain:" + chain);
                break;


        }
    }
}
