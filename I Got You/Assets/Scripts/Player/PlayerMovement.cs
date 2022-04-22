using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private CharacterController characterController;

    [SerializeField] private float walkingSpeed = 5;
    [SerializeField] private float runSpeed = 12;
    [SerializeField] private float gravity = -9.8f;
    [SerializeField] private float jumpHeight = 10;
    [SerializeField] private float dashSpeed;
    [SerializeField] private float dashTime;
    [SerializeField] private Vector3 movement;
    private float stepOffset = 0.5f;

    private Vector3 velocity = Vector3.zero;

    // Start is called before the first frame update
    void Start()
    {
        characterController = GetComponent<CharacterController>();
        stepOffset = characterController.stepOffset;
    }

    // Update is called once per frame
    void Update()
    {
        //if (DashCheck =)
        //{
        //    UpdateMovement();
        //    als de dashcheck aan staat moet updatemovement uit.
        //    
        //}
        UpdateMovement();
        MoveByGravity();
        JumpCheck();
        DashCheck();
    }

    private void UpdateMovement()
    {
        movement = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        movement.Normalize();
        movement = transform.TransformDirection(movement);
        movement.y = 0;

        float speed = Input.GetButton("Sprint") ? runSpeed : walkingSpeed;

        characterController.Move(movement * speed * Time.deltaTime);
    }

    private void MoveByGravity()
    {
        characterController.Move(velocity * Time.deltaTime);

        if (characterController.isGrounded && velocity.y < 0)
        {
            velocity.y = -2;
            characterController.stepOffset = stepOffset;
        }
        else
        {
            characterController.stepOffset = 0;
        }

        velocity.y += gravity * Time.deltaTime;
    }

    private void JumpCheck()
    {
        if (Input.GetButtonDown("Jump") && characterController.isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }

    private void DashCheck()
    {
        if (Input.GetButtonDown("Dash") && characterController.isGrounded)
        {
            StartCoroutine(Dash());
            Debug.Log("Dash");
        }
    }

    IEnumerator Dash()
    {
        float startTime = Time.time;

        while (Time.time < startTime + dashTime)
        {
            characterController.Move(movement * dashSpeed * Time.deltaTime);

            yield return null;
        }
    }
}
