using System.Collections;
using System.Collections.Generic;
using SWNetwork;
using UnityEngine;

public class Enemy : MonoBehaviour, IHealth
{

    public bool MoveToRight = true;

    [Header("Detection")]
    public float LeadingBuffer = 0.5f;
    public LayerMask DetectionMask;

    [Header("Edge")]
    public bool EdgeDetectionEnabled = true;
    public float EdgeDetectionRayDistance = 0.2f;

    [Header("Wall")]
    public bool WallDetectionEnabled = true;
    public float WallDetection = 0.2f;

    [Header("Health")]
    public int MaxHealth = 30;
    HealthBar healthBar;
    int currentHealth = 0;


    [Header("Attack")]
    public int Damage = 1;

    Movement movement;

    NetworkID networkID;
    const string HEALTH = "Health";
    RemoteEventAgent remoteEventAgent;
    SyncPropertyAgent syncPropertyAgent;

    // Start is called before the first frame update
    void Start()
    {
        movement = GetComponent<Movement>();
        networkID = GetComponent<NetworkID>();
        remoteEventAgent = GetComponent<RemoteEventAgent>();
        syncPropertyAgent = GetComponent<SyncPropertyAgent>();
        healthBar = GetComponentInChildren<HealthBar>();
    }

    // Update is called once per frame
    void Update()
    {
        if (networkID.IsMine)
        {
            if (EdgeDetectionEnabled && EdgeDetected())
            {
                MoveToRight = !MoveToRight;
            }

            if (WallDetectionEnabled && WallDetected())
            {
                MoveToRight = !MoveToRight;
            }

            if (MoveToRight)
            {
                movement.Move(1);
            }
            else
            {
                movement.Move(-1);
            }
        }
    }

    public void OnHealthSyncPropertyReady()
    {
        int health = syncPropertyAgent.GetPropertyWithName(HEALTH).GetIntValue();
        int version = syncPropertyAgent.GetPropertyWithName(HEALTH).version;

        if (version == 0)
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
             Killed();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        GameObject other = collision.gameObject;
        IHealth health = (IHealth)other.GetComponent(typeof(IHealth));

        if (health != null)
        {
            health.TakeDamage(Damage);
        }
    }

    Vector3 DetectionPosition()
    {
        Vector3 detectionPosition = transform.position;

        if (movement.FaceRight)
        {
            detectionPosition += Vector3.right * LeadingBuffer;
        }
        else
        {
            detectionPosition -= Vector3.right * LeadingBuffer;
        }

        return detectionPosition;
    }

    bool EdgeDetected()
    {
        Debug.DrawRay(DetectionPosition(), Vector2.down * WallDetection, Color.green);
        RaycastHit2D hit2D = Physics2D.Raycast(DetectionPosition(), Vector2.down, EdgeDetectionRayDistance, DetectionMask);

        return hit2D.collider == null;
    }

    bool WallDetected()
    {
        Vector2 forwardDetection;

        if (movement.FaceRight)
        {
            forwardDetection = Vector2.right;
        }
        else
        {
            forwardDetection = Vector2.left;
        }

        Debug.DrawRay(transform.position, forwardDetection * WallDetection, Color.green);
        RaycastHit2D hit2D = Physics2D.Raycast(DetectionPosition(), forwardDetection, WallDetection, DetectionMask);

        bool hitWall = hit2D.collider != null;

        return hitWall;
    }

    public void TakeDamage(int damage)
    {
        int health = syncPropertyAgent.GetPropertyWithName(HEALTH).GetIntValue();
        health = Mathf.Clamp(health - damage, 0, MaxHealth);
        if(NetworkClient.Instance == null || NetworkClient.Instance.IsHost)
        {
            syncPropertyAgent.Modify(HEALTH, health);
            
        }
        
            
           
            
    }

    void Killed()
    {
        if (NetworkClient.Instance != null)
        {
            if (NetworkClient.Instance.IsHost)
            {
                networkID.Destroy();
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void UpdateHealthBar(int currentHealth)
    {
        if (healthBar != null)
        {
            float percentage = (float)currentHealth / (float)MaxHealth;
            healthBar.UpdateHealth(percentage);
        }
    }

    
   
}
