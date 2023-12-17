using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletObject : MonoBehaviour{
    private Queue<BulletObject> pool;
    private WaitForSeconds bulletLiveTime;
    private Collider parentCollider;
    public TrailRenderer TrailRenderer;
    private float bulletSpeed;

    public void SetupBulletObj(Queue<BulletObject> particleObjPool, float liveTime, float speed, Collider collider){
        bulletLiveTime = new WaitForSeconds(liveTime);
        bulletSpeed = speed;
        pool = particleObjPool;
        parentCollider = collider;
        StartCoroutine(BulletVisibleTimer());
    }

    private void Update() {
        MoveBullet();
    } 

    public void MoveBullet(){
        transform.Translate(bulletSpeed * Time.deltaTime * transform.forward);
    }

    private IEnumerator BulletVisibleTimer(){
        yield return bulletLiveTime;
        DisableBullet();
    }

    private void DisableBullet(){
        if(gameObject.activeSelf == false) return;
        
        TrailRenderer.enabled = false;
        pool.Enqueue(this);
        gameObject.SetActive(false);
    }

    private void OnCollisionEnter(Collision other){
        //if(other.collider == parentCollider) return;

        if(other.gameObject.CompareTag("Player")){
            Debug.Log("Hit Player!");
        }

        //Disable bullet
        StopAllCoroutines();
        DisableBullet();
    }
}
