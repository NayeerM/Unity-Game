using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerState
{
    walk,
    attack,
    stagger,
    idle
}
public class PlayerMovement : MonoBehaviour
{
    public PlayerState currentState;
    public float speed = 30f;
    private Rigidbody2D rb;
    private Vector3 change;
    private Animator anim;
    public GameObject projectile;
    public VectorValue startingPosition;
    public FloatValue currentHealth;
    public SignalSender playerHealthSignal;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        transform.position = startingPosition.initialValue;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        change = Vector3.zero;
        change.x = Input.GetAxisRaw("Horizontal");
        change.y = Input.GetAxisRaw("Vertical");
        if (Input.GetButtonDown("Fire1") && currentState!=PlayerState.attack
            && currentState!=PlayerState.stagger)
        {
            StartCoroutine(AttackCo());
        }
        else if (currentState == PlayerState.walk || currentState==PlayerState.idle)
        {
            UpdateAnimationAndMove();
        }
        
    }
    private IEnumerator AttackCo()
    {
        anim.SetBool("attacking", true);
        currentState = PlayerState.attack;
        
        yield return null;
        anim.SetBool("attacking", false);
        MakeArrow();
        yield return new WaitForSeconds(.4f);
        currentState = PlayerState.walk;
    }
    private void MakeArrow()
    {
        Vector2 temp = new Vector2(anim.GetFloat("moveX"), anim.GetFloat("moveY"));
        Arrow arrow = Instantiate(projectile, transform.position, Quaternion.identity).GetComponent<Arrow>();
        arrow.Setup(temp, ChooseArrowDirection());
    }
    Vector3 ChooseArrowDirection()
    {
        float temp = Mathf.Atan2(anim.GetFloat("moveY"), anim.GetFloat("moveX"))*Mathf.Rad2Deg;
        return new Vector3(0, 0, temp);
    }
    void UpdateAnimationAndMove()
    {
        if (change != Vector3.zero)
        {
            MoveCharacter();
            anim.SetFloat("moveX", change.x);
            anim.SetFloat("moveY", change.y);
            anim.SetBool("moving", true);
        }
        else
        {
            anim.SetBool("moving", false);
        }
    }
    void MoveCharacter()
    {
        rb.MovePosition(
            transform.position + change * speed * Time.fixedDeltaTime
        );
    }
    public void Knock(float knockTime, float damage)
    {
        currentHealth.RunTimeValue -= damage;
        playerHealthSignal.Raise();
        if (currentHealth.RunTimeValue > 0)
        {
            StartCoroutine(KnockCo(knockTime));
        }
        else
        {
            this.gameObject.SetActive(false);
        }
    }
    private IEnumerator KnockCo(float knockTime)
    {
        if (rb != null)
        {
            yield return new WaitForSeconds(knockTime);
            rb.velocity = Vector2.zero;
            currentState = PlayerState.idle;
            rb.velocity = Vector2.zero;
        }
    }
}
