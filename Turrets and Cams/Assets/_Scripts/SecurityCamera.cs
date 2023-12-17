using System.Collections;
using UnityEngine;

public class SecurityCamera : MonoBehaviour
{
    [SerializeField] private Turret[] controlledTurrets;
    [SerializeField] private float lookInterval = 0.1f;
    [SerializeField] private float camTurretCooldown = 250f;
    [SerializeField] private float camRange = 30f;
    [Range(30,110)]
    [SerializeField] private float fieldOfView = 75;
    private Transform emitter;
    private GameObject[] playerObj;
    private WaitForSeconds cameraLookInterval;
    private WaitForSeconds cameraTurretCooldown;

    private bool activatedTurrets = false;

    private void Start(){
        cameraLookInterval = new WaitForSeconds(lookInterval);
        cameraTurretCooldown = new WaitForSeconds(camTurretCooldown);
        emitter = transform.GetChild(0);
        playerObj = GameObject.FindGameObjectsWithTag("Player");
        StartCoroutine(CheckForPlayerObj());
    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, camRange);
    }

    IEnumerator CheckForPlayerObj() {
        while(true) {
            yield return cameraLookInterval;

            foreach(GameObject user in playerObj) // check for enemies
            {
                //If player is too far away check next frame if they are close enough
                if(Vector3.Distance(transform.position, user.transform.position) < camRange){
                    if (Physics.Raycast(emitter.position,  user.transform.position - emitter.position, out RaycastHit hit, 100)){
                        if (hit.transform.gameObject.CompareTag("Player")){
                            Vector3 targetDir = user.transform.position - emitter.position;
                            float angle = Vector3.Angle(targetDir, emitter.forward);

                            if (angle < fieldOfView && !activatedTurrets){
                                // See the player
                                foreach(Turret turret in controlledTurrets){
                                    if(!turret.IsActivated){
                                        turret.ActivateTurret(user.transform);
                                    }
                                }
                                StartCoroutine(CamCooldown());
                            }
                        }
                    }
                }

            }
        }
    }

    private IEnumerator CamCooldown(){
        activatedTurrets = true;
        yield return cameraTurretCooldown;
        activatedTurrets = false;
    }
}