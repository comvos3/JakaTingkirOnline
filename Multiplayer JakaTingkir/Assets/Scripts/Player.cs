using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SWNetwork;
using UnityEngine.SceneManagement;


public class Player : MonoBehaviour, IHealth
{
    public float FiringSpeed = 1.0f;

    Movement movement;
    Shoot shoot;
    Animator anim;

    NetworkID networkID;

    const string FIRE = "fire";
    const string HEALTH = "Health";
    public int MaxHealth = 5;
    int currentHealth = 0;
    RemoteEventAgent remoteEventAgent;
    SyncPropertyAgent syncPropertyAgent;
    HealthBar healthBar;
   


    public void TakeDamage(int damage)
    {
        int health = syncPropertyAgent.GetPropertyWithName(HEALTH).GetIntValue();
        health = Mathf.Clamp(health - damage, 0, MaxHealth);
        if(NetworkClient.Instance == null || NetworkClient.Instance.IsHost)
        {
            syncPropertyAgent.Modify(HEALTH, health);
            
        }

       
    }

    void Start()
    {
        anim = GetComponentInChildren<Animator>();
        movement = GetComponent<Movement>();
        shoot = GetComponent<Shoot>();
        networkID = GetComponent<NetworkID>();
        remoteEventAgent = GetComponent<RemoteEventAgent>();
        syncPropertyAgent = GetComponent<SyncPropertyAgent>();

        healthBar = GetComponentInChildren<HealthBar>();

         if (networkID.IsMine)
         {
            CameraFollow cameraFollow = Camera.main.GetComponent<CameraFollow>();
            cameraFollow.Target = transform;
        
		 }
            
       
    }

    public void OnHealthSyncPropertyReady()
    {
        int health = syncPropertyAgent.GetPropertyWithName(HEALTH).GetIntValue();
        int version = syncPropertyAgent.GetPropertyWithName(HEALTH).version;

        if(version == 0)
        {
            syncPropertyAgent.Modify(HEALTH, MaxHealth);
            health = MaxHealth;
        }

        UpdateHealthBar(health);
    }

    public void OnHealthSyncPropertyChanged()
    {
        int health = syncPropertyAgent.GetPropertyWithName(HEALTH).GetIntValue();
        UpdateHealthBar(health);

        if (health == 0)
        {
            Die();
            SceneManager.LoadScene(8);
        }
    }

void Update()
    {
        if (networkID.IsMine)
        {
            // movement
            if (movement.Ground && Input.GetKeyDown(KeyCode.W))
            {
                movement.Jump();
            }

            float move = Input.GetAxis("Horizontal");

            movement.Move(move);

            anim.SetBool("air", !movement.Ground);
            anim.SetFloat("speed", Mathf.Abs(move));

            // shoot
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (!IsInvoking("Fire"))
                {
                    InvokeRepeating("Fire", 0f, FiringSpeed);
                }
            }

            if (Input.GetKeyUp(KeyCode.Space))
            {
                CancelInvoke("Fire");
            }
        }
    }

    void Fire()
    {
        shoot.FireBullet(movement.FaceRight);

        SWNetworkMessage message = new SWNetworkMessage();
        message.Push(movement.FaceRight);
        remoteEventAgent.Invoke(FIRE, message);
    }

    void UpdateHealthBar(int currentHealth)
    {
        if (healthBar != null)
        {
            float percentage = (float)currentHealth / (float)MaxHealth;
            healthBar.UpdateHealth(percentage);
        }
    }

    public void RemoteFired(SWNetworkMessage message)
    {
        bool faceRight = message.PopBool();
        shoot.FireBullet(faceRight);
    }


    void Die()
    {
        int health = syncPropertyAgent.GetPropertyWithName(HEALTH).GetIntValue();
        UpdateHealthBar(health);

        if (NetworkClient.Instance != null)
        {
            if (NetworkClient.Instance.IsHost)
            {
               Destroy(gameObject);
             
            }

            if (health == 0) 
            {
                syncPropertyAgent.Modify(HEALTH, MaxHealth);
                health = MaxHealth;
			}
        }

       
        
    }


   
}
