using System.Collections;
using System.Collections.Generic;
using PathCreation;
using UnityEngine;

public class Witch : MonoBehaviour
{
    [SerializeField] float speed;
    [SerializeField] PathCreator pathCreator;
    [SerializeField] Animator animator;
    [SerializeField] GameObject fireball;
    [SerializeField] float throwAngle;
    [SerializeField] float throwVelocity;

    [HideInInspector] public bool flying = false;
    float distanceTravelled;
    private bool canMove = false;

    private void Start()
    {
        transform.position = pathCreator.path.GetPointAtDistance(0);
    }

    private void Update()
    {
        if (!canMove) return;
        if (distanceTravelled < pathCreator.path.length)
        {
            distanceTravelled += speed * Time.deltaTime;
            transform.position = pathCreator.path.GetPointAtDistance(distanceTravelled, EndOfPathInstruction.Stop);
            transform.forward = pathCreator.path.GetDirectionAtDistance(distanceTravelled, EndOfPathInstruction.Stop);
        }
        else
        {
            CastSpell();
        }



    }

    public IEnumerator StartMovement()
    {
        flying = true;
        yield return new WaitUntil(() =>
        {
            return animator.GetCurrentAnimatorStateInfo(0).IsTag("Flying");
        });
        canMove = true;
    }

    private void LateUpdate()
    {
        animator.SetBool("Flying", flying);
    }

    private void CastSpell()
    {
        flying = false;
        canMove = false;
        animator.SetTrigger("CastSpell");
    }

    public void ShowFireball()
    {
        fireball.gameObject.SetActive(true);
    }

    public void ThrowFireball()
    {
        fireball.transform.parent = null;
        fireball.GetComponent<Rigidbody>().velocity = Quaternion.AngleAxis(throwAngle, Vector3.right) * Vector3.forward * throwVelocity;
    }
}
