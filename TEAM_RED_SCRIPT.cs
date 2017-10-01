using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//---------- CHANGE THIS NAME HERE -------
public class TEAM_BLUE_SCRIPT : MonoBehaviour
{
  private HashSet<Vector3> allVisibleEnemyLocations;
  private List<Vector3> allAttackedFromLocations;
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
  private int range;

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

    //
    // void ClearEnemyLocationList(){
    //   foreach (CharacterScript character in characters) {
    //     if (character.getZone() == zone.BlueBase || character.getZone() == zone.RedBase) {
    //       character.visibleEnemyLocations.Clear();
    //     }
    //   }
    // }

    void Start()
    {
        character1 = transform.Find("Character1").gameObject.GetComponent<CharacterScript>();
        character2 = transform.Find("Character2").gameObject.GetComponent<CharacterScript>();
        character3 = transform.Find("Character3").gameObject.GetComponent<CharacterScript>();
        characters = new List<CharacterScript>{character1, character2, character3};
        foreach (CharacterScript character in characters) {
          character.priority = firePriority.LOWHP;
        }
        allVisibleEnemyLocations = new HashSet<Vector3>();
        allAttackedFromLocations = new List<Vector3>();
        middleObjective = GameObject.Find("MiddleObjective").GetComponent<ObjectiveScript>();
        leftObjective = GameObject.Find("LeftObjective").GetComponent<ObjectiveScript>();
        rightObjective = GameObject.Find("RightObjective").GetComponent<ObjectiveScript>();
        allObjectives = new List<ObjectiveScript>{middleObjective, rightObjective, leftObjective};

        targetObjective = new List<ObjectiveScript>{null, null, null};
        ourTeamColor = character1.getTeam();
        if (ourTeamColor == team.blue) {
          ourBase = zone.BlueBase;
          enemyBase = zone.RedBase;
          // allObjectives = new List<ObjectiveScript>{middleObjective, rightObjective};
        } else {
          ourBase = zone.RedBase;
          enemyBase = zone.BlueBase;
          // allObjectives = new List<ObjectiveScript>{middleObjective, leftObjective};
        }
        range = 15;
    }
    /*^^^^ DO NOT MODIFY ^^^^*/


    Vector3 FindClosestUncapturedObjectiveLocation(int characterIdx){
      CharacterScript character = characters[characterIdx];
      float dist = Mathf.Infinity;
      Vector3 loc = new Vector3(8.0f, 1.5f, -15.0f);
      // Debug.Log("character position: "+character.transform.position);
      foreach (ObjectiveScript objective in allObjectives) {
        // Debug.Log("objective " + objective.transform.position + " is our team? "+(objective.getControllingTeam() == ourTeamColor));
        if (objective.getControllingTeam() != ourTeamColor) {
          float newDist = Vector3.Distance(character.transform.position, objective.transform.position);
          if (newDist < dist) {
            dist = newDist;
            loc = objective.transform.position;
            targetObjective[characterIdx] = objective;
          }
        }
      }
      // Debug.Log("chosen position: "+loc + " -------------------------------------------- ");

      return loc;
    }

    // Vector3 TowardEnemyStillOnObj(int characterIdx, Vector3 enemyPosition, ObjectiveScript objective) {
    //   CharacterScript character = characters[characterIdx];
    //   float distToEnemy = Vector3.Distance(character.transform.position, enemyPosition);
    //   Vector3 objLoc = FindClosestUncapturedObjectiveLocation(characterIdx);
    //   float distToObj = Vector3.Distance(character.transform.position, objLoc);
    //   if (distToObj < 11) { // on obj
    //
    //   }
    // }

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

    bool FightBackIfUnderAttack(int characterIdx){
      CharacterScript character = characters[characterIdx];
         //TODO: remove checking visibleEnemyLocations after they fix bug
         int attackCount = character.attackedFromLocations.Count;
         int visibleCount = character.visibleEnemyLocations.Count;
         if (attackCount != 0 || visibleCount != 0) {
          //  Debug.Log("attackedFromLocations size: " + character.attackedFromLocations.Count );
          //  Debug.Log("visibleEnemyLocations size: " + character.visibleEnemyLocations.Count );
           if (attackCount != 0)
            {
                currentEnemy = character.attackedFromLocations[attackCount - 1];
            }
            else
            {
                currentEnemy = character.visibleEnemyLocations[visibleCount - 1];
            }
            //
            // if (character.getHP() < 35) {
            //   character.MoveChar(character.FindClosestCover(currentEnemy));
            // } else {
             character.SetFacing(currentEnemy);
            //  float dist = Vector3.Distance(character.transform.position, currentEnemy);
            //  if (dist>range && targetObjective[characterIdx].getControllingTeam() == ourTeamColor) {
            fixed_dist(character, 34, currentEnemy);

            //  }
          //  }
           return true;
         } else if (currentEnemy != null) {
           float dist = Vector3.Distance(character.transform.position, currentEnemy);
             if (dist < 36f && targetObjective[characterIdx].getControllingTeam() == ourTeamColor) {
              //  if (character.getHP() < 35) {
              //    character.MoveChar(character.FindClosestCover(currentEnemy));
              //  } else {
                 character.SetFacing(currentEnemy);
                 fixed_dist(character, 34, currentEnemy);

       //  }
               return true;
             }
         }
         return false;
       }

    GameObject FindClosestItem(HashSet<GameObject> itemSet, CharacterScript character)
    {
        GameObject closestItem = new GameObject();
        float closestDistance = Mathf.Infinity;
        bool found = false;
        foreach (GameObject item in itemSet)
        {
            float dist = Vector3.Distance(item.transform.position, character.transform.position) - 11;
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
        // Debug.Log(character1.name + " " + character1.);
        //character1.FaceClosestWaypoint();
        //character1.SetFacing(new Vector3(-8f, 0, 8f));
        //character2.FaceClosestWaypoint();
        //character3.FaceClosestWaypoint();

        // character1.rotateAngle(500);
        // character2.rotateAngle(500);
        // character3.rotateAngle(500);


        List<GameObject> itemList = character1.getItemList();
        HashSet<GameObject> itemSet = new HashSet<GameObject>(itemList);

        double threshold = 1.5;
        for (int characterIdx = 0; characterIdx<3; characterIdx ++) {
          CharacterScript character = characters[characterIdx];
          if (character.getHP()==0) continue;
          if (!FightBackIfUnderAttack(characterIdx)){
            // stay in objective if not done
            if (character.getZone()==zone.Objective) {
              if (targetObjective[characterIdx].getControllingTeam() != ourTeamColor) {
                character.rotateAngle(500);
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
              if (distToObj*threshold > distToItem+distItemToObj || targetObjective[characterIdx].getControllingTeam() == ourTeamColor) {
                  character.MoveChar(closestItem.transform.position);
                  itemSet.Remove(closestItem);
                  continue;
              }
            }

            //
            // if (distToObj>35) {
            //   character.FaceClosestWaypoint();
            // } else if (distToObj<11){
            //   character.rotateAngle(500);
            // } else
            //  {
            if (objectiveLoc!=new Vector3(8.0f, 1.5f, -15.0f)){
              character.SetFacing(objectiveLoc);
            } else {
              character.SetFacing(middleObjective.transform.position);
            }
            // }
            fixed_dist(character, 10, objectiveLoc);
            // character.MoveChar(objectiveLoc);
          }
          // character.FaceClosestWaypoint();
        }

        character1.attackedFromLocations.Clear();
               character1.visibleEnemyLocations.Clear();
            character2.attackedFromLocations.Clear();
               character2.visibleEnemyLocations.Clear();
            character3.attackedFromLocations.Clear();
               character3.visibleEnemyLocations.Clear();


        // character1.MoveChar(new Vector3());
      //   GameObject closestItem = character1.FindClosestItem();
      //  character1.MoveChar(closestItem.transform.position);
        // character2.MoveChar(new Vector3(40.0f, 1.5f, 24.0f));
        // character3.MoveChar(new Vector3(-40.0f, 1.5f, -24.0f));

    }
}