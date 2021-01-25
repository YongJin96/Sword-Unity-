using UnityEngine;

public class Sneak : MonoBehaviour
{
    public EasySurvivalScripts.PlayerMovement PlayerInfo;

    private void OnTriggerStay(Collider coll)
    {
        if (coll.gameObject.CompareTag("Player"))
        {
            if (PlayerInfo.isCrouch == true)
            {
                PlayerInfo.tag = "Untagged"; // 부쉬에서 웅크리고 있으면 태그 변경 (적이 Player 태그를 감지하니까)
            }
        }
        //else
        //{
        //    PlayerInfo.tag = "Player";
        //}
    }

    private void OnTriggerExit(Collider coll)
    {
        PlayerInfo.tag = "Player";
    }
}
