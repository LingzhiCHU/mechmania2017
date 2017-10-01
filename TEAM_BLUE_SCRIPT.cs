using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//---------- CHANGE THIS NAME HERE -------
public class TEAM_BLUE_SCRIPT : MonoBehaviour
{
  // private HashSet<Vector3> allVisibleEnemyLocations;
  // private List<Vector3> allAttackedFromLocations;
  private List<Vector3> allDestinations;
  private List<ObjectiveScript> allObjectives;
  private List<ObjectiveScript> targetObjective;
  private List<CharacterScript> characters;
  private ObjectiveScript middleObjective;
  private ObjectiveScript leftObjective;
  private ObjectiveScript rightObjective;
  private team ourTeamColor;
  private zone ourBase;
  private zone enemyBase;
  private Vector3 currentEnemy;

    //---------- CHANGE THIS NAME HERE -------
    public static TEAM_BLUE_SCRIPT AddYourselfTo(GameObject host) {
        //---------- CHANGE THIS NAME HERE -------
        return host.AddComponent<TEAM_BLUE_SCRIPT>();
    }

    /*vvvv DO NOT MODIFY vvvvv*/
    [SerializeField]
    public CharacterScript character1;
    [SerializeField]
    public CharacterScript character2;
    [SerializeField]
    public CharacterScript character3;

    void Start()
    {
        character1 = transform.Find("Character1").gameObject.GetComponent<CharacterScript>();
        character2 = transform.Find("Character2").gameObject.GetComponent<CharacterScript>();
        character3 = transform.Find("Character3").gameObject.GetComponent<CharacterScript>();
        characters = new List<CharacterScript>{character1, character2, character3};
        foreach (CharacterScript character in characters) {
          character.priority = firePriority.LOWHP;
        }
        // allVisibleEnemyLocations = new HashSet<Vector3>();
        // allAttackedFromLocations = new List<Vector3>();
        middleObjective = GameObject.Find("MiddleObjective").GetComponent<ObjectiveScript>();
        leftObjective = GameObject.Find("LeftObjective").GetComponent<ObjectiveScript>();
        rightObjective = GameObject.Find("RightObjective").GetComponent<ObjectiveScript>();
        allObjectives = new List<ObjectiveScript>{middleObjective, leftObjective, rightObjective};
        targetObjective = new List<ObjectiveScript>{null, null, null};
        ourTeamColor = character1.getTeam();
        if (ourTeamColor == team.blue) {
          ourBase = zone.BlueBase;
          enemyBase = zone.RedBase;
        } else {
          ourBase = zone.RedBase;
          enemyBase = zone.BlueBase;
        }
    }
    /*^^^^ DO NOT MODIFY ^^^^*/


    Vector3 FindClosestUncapturedObjectiveLocation(int characterIdx){
      CharacterScript character = characters[characterIdx];
      float dist = Mathf.Infinity;
      Vector3 loc = middleObjective.transform.position;
      foreach (ObjectiveScript objective in allObjectives) {
        if (objective.getControllingTeam() != ourTeamColor) {
          float newDist = Vector3.Distance(character.transform.position, objective.transform.position);
          if (newDist < dist) {
            dist = newDist;
            loc = objective.transform.position;
            targetObjective[characterIdx] = objective;

          }

        }else{

        }
      }
      return loc;
    }

    bool FightBackIfUnderAttack(CharacterScript character){
         //TODO: remove checking visibleEnemyLocations after they fix bug
         int Count = 0;
         int Count2 = 0;
         Count = character.attackedFromLocations.Count ;
         Count2 = character.visibleEnemyLocations.Count;
         if (Count!= 0 || Count2!= 0) {
          
           if (Count!= 0)
            {
                currentEnemy = character.attackedFromLocations[Count-1];
            }
            else
            {
                currentEnemy = character.visibleEnemyLocations[0];
            }
           character.SetFacing(currentEnemy);
           return true;
         } else if (currentEnemy != null){
             float relativeDist = Vector3.Distance(character.transform.position, currentEnemy);
             //Debug.Log("currnt face is : " + currentEnemy);
             character.SetFacing(currentEnemy);
             if (relativeDist < 35f){
               
               fixed_dist(character, 35, currentEnemy);
               return true;
             }
         }
            return false;
         
       }


    Vector3 runAway(Vector3 cur, Vector3 target)
  {
      Vector3 distanceVec = cur - target;
      Vector3 orthDistanceVec = new Vector3(distanceVec.z, distanceVec.y, -distanceVec.x);
      Vector3 moveToVec = 0.2f * distanceVec + 0.8f * orthDistanceVec;
      moveToVec.Normalize();
      return moveToVec;
  }
 
  void fixed_dist(CharacterScript character, double range, Vector3 target)
  {
       character.SetFacing(target); 
       float distance = Vector3.Distance(character.transform.position, target);
       if (distance < range || character.visibleEnemyLocations.Count >= 1)
       {
            character.MoveChar(runAway(character.transform.position, target));
       }
       else
       {
            character.MoveChar(target);
       }  
  }

    GameObject FindClosestItem(HashSet<GameObject> itemSet, CharacterScript character)
    {
        GameObject closestItem = new GameObject();
        float closestDistance = Mathf.Infinity;
        bool found = false;
        foreach (GameObject item in itemSet)
        {
            float dist = Vector3.Distance(item.transform.position, character.transform.position);
            if (dist < closestDistance)
            {
                closestDistance = dist;
                closestItem = item;
                found = true;
            }
        }
        if (!found) {
          closestItem.transform.position = middleObjective.transform.position;
        }
        return closestItem;
    }

    /* Your code below this line */
    // Update() is called every frame
    void Update()
  {
    //Set caracter loadouts, can only happen when the characters are at base.
      if (character1.getZone() == zone.BlueBase || character1.getZone() == zone.RedBase) {
          character1.setLoadout(loadout.LONG);
      }
      if (character2.getZone() == zone.BlueBase || character2.getZone() == zone.RedBase) {
          character2.setLoadout(loadout.LONG);
      }
      if (character3.getZone() == zone.BlueBase || character3.getZone() == zone.RedBase) {
          character3.setLoadout(loadout.LONG);
      }

      List<GameObject> itemList = character1.getItemList();
      HashSet<GameObject> itemSet = new HashSet<GameObject>(itemList);
      double threshold = 1.5;
        for (int characterIdx = 0; characterIdx<3; characterIdx ++) {
          CharacterScript character = characters[characterIdx];
          if (character.getHP()==0) continue;
          //character.FaceClosestWaypoint();
          if (!FightBackIfUnderAttack(character)){
            // stay in objective if not done
            if (character.getZone()==zone.Objective){
              //character.rotateAngle(120);
              //character.FaceClosestWaypoint();
              int Count = character.visibleEnemyLocations.Count;
              int Count2 = character.attackedFromLocations.Count;
              if(Count != 0){
                character.SetFacing(character.visibleEnemyLocations[0]);
              }
              else if(Count2 != 0){
                character.SetFacing(character.attackedFromLocations[Count2-1]);
              }else{
                character.FaceClosestWaypoint();
              }
              if (targetObjective[characterIdx].getControllingTeam() != ourTeamColor){
                continue;
              }

            }
              Vector3 objectiveLoc = FindClosestUncapturedObjectiveLocation(characterIdx);

              float distToObj = Vector3.Distance(objectiveLoc, character.transform.position);

            // if objective captured, go get item if there is one
              if (itemSet.Count > 0){
              GameObject closestItem = FindClosestItem(itemSet, character);
              float distToItem = Vector3.Distance(character.transform.position, closestItem.transform.position);
              float distItemToObj = Vector3.Distance(closestItem.transform.position, objectiveLoc);
              if (distToObj*threshold > distToItem+distItemToObj) {
                  character.MoveChar(closestItem.transform.position);
                  itemSet.Remove(closestItem);
                  continue;
              }
              }

              if (distToObj>50){
              character.FaceClosestWaypoint();
              //Debug.Log("FaceClosestWaypoint " );

              }else {
              character.SetFacing(objectiveLoc);
              //Debug.Log("SetFacing(objectiveLoc)" );

              }

              character.MoveChar(objectiveLoc);
                

            
          }

        }

        character1.attackedFromLocations.Clear();
        character1.visibleEnemyLocations.Clear();
        character2.attackedFromLocations.Clear();
        character2.visibleEnemyLocations.Clear();
        character3.attackedFromLocations.Clear();
        character3.visibleEnemyLocations.Clear();


    }
}