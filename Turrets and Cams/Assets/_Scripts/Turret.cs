using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Turret : MonoBehaviour{
    public bool IsActivated => isActivated;

    [Header("Turret References")]
    [SerializeField] private Transform[] shootPos;
    [SerializeField] private BulletObject projectilePrefab;

    [Header("Turret Options")]
    [SerializeField] private bool isActivated;

    [Header("Turret Stats")]
    [SerializeField] private float turretCooldown; 
    [SerializeField] private float idleTime = 5f;
    [SerializeField] private float turretBulletLiveTime = 10f;
    [SerializeField] private float bulletSpeed = 5f;
    [SerializeField] private float turretTurnSpeed = 10f;
    [SerializeField] private float turretBloom = 1f;
    [SerializeField] private float turretRange = 25f;

    private Animator turretAnimator;
    private WaitForSeconds turretCooldownWaitForSeconds;
    private WaitForSeconds turretIdleTimer;

    private Transform curretTarget;
    private Collider turretCollider;

    private Coroutine idleCoroutine;

    private Queue<BulletObject> bulletObjectPool = new Queue<BulletObject>();

    private const string TURRET_ACTIVATE = "Turret_Power_On";
    private const string TURRET_DEACTIVATE = "Turret_Power_Off";
    private const string TURRET_SHOOT = "Turret_Shooting";
    private const string TURRET_IDLE = "Turret_Idle";

    private bool onCooldown;

    private void Awake() {
        TryGetComponent(out turretAnimator);
        TryGetComponent(out turretCollider);
    }

    private void Start() {
        turretCooldownWaitForSeconds = new WaitForSeconds(turretCooldown);
        turretIdleTimer = new WaitForSeconds(idleTime);

        PlayPowerAnimation();
    }

    private void Update() {
        if(curretTarget != null){
            DetectTarget();
        }
    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, turretRange);
    }

    public void ActivateTurret(Transform target){
        curretTarget = target;
        isActivated = true;

        PlayPowerAnimation();
    }

    public void PoweredUp(){
        StartCoroutine(TurretFireCoroutine());
    }

    private void PlayPowerAnimation(){
        if(turretAnimator == null) return;

        if(isActivated){
            turretAnimator.Play(TURRET_ACTIVATE);
            return;
        }

        turretAnimator.Play(TURRET_DEACTIVATE);
    }


    private void DetectTarget(){
        float dist = Vector3.Distance(curretTarget.position, transform.position);
        if(dist > turretRange){
            isActivated = false;
            idleCoroutine = StartCoroutine(IdleTimer());
        }
        else if(!isActivated){
            isActivated = true;
            StopCoroutine(idleCoroutine);
            idleCoroutine = null;
        }
    }

    private IEnumerator IdleTimer(){
        turretAnimator.Play(TURRET_IDLE);
        yield return turretIdleTimer;
        curretTarget = null;
        PlayPowerAnimation();
    }

    private IEnumerator TurretCooldownCoroutine(){
        onCooldown = true;
        yield return turretCooldownWaitForSeconds;
        onCooldown = false;
    }

    private IEnumerator TurretFireCoroutine(){
        while(curretTarget != null){
            if(isActivated){
                LookAtTarget();
                FireAtTarget();
            }
            yield return null;
        }
    }

    private void FireAtTarget(){
        if(onCooldown) return;

        BulletObject bullet;

        // Spawn Bullets at fire postions toward target
        foreach(Transform pos in shootPos){
            //Check if there is a bullet in the pool
            if(bulletObjectPool.Count > 0){
                //Reuse the bullet
                bullet = bulletObjectPool.Dequeue();
                bullet.gameObject.SetActive(true);
                bullet.transform.SetPositionAndRotation(pos.position, pos.rotation);
                bullet.TrailRenderer.enabled = true;
            }
            else{
                var spawnedBullet = Instantiate(projectilePrefab, pos.position, pos.rotation);
                spawnedBullet.SetupBulletObj(bulletObjectPool, turretBulletLiveTime, bulletSpeed, turretCollider);
            }
        }

        //Fire animation
        turretAnimator.Play(TURRET_SHOOT, 0);

        // Shoot Cooldown
        StartCoroutine(TurretCooldownCoroutine());
    }
    
    private void LookAtTarget(){
        //Smoothly look at the target
        Vector3 lookPos = curretTarget.position - transform.position;
        lookPos.y = 0;
        Quaternion rotation = Quaternion.LookRotation(lookPos);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, turretTurnSpeed * Time.deltaTime);
    }
}
