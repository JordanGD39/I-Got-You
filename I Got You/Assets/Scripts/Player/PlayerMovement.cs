using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private CharacterController characterController;
    private PlayerStats playerStats;

    [SerializeField] private float walkingSpeed = 5;
    [SerializeField] private float runSpeed = 12;
    [SerializeField] private float downSpeed = 2;
    [SerializeField] private float gravity = -9.8f;
    [SerializeField] private float jumpHeight = 10;
    [SerializeField] private Animator anim;
    [SerializeField] private float dampTime = 0.05f;
    [SerializeField] private float outOfBoundsY = 0;
    private float stepOffset = 0.5f;

    private Vector3 velocity = Vector3.zero;
    private Vector3 lastGroundedPosition;

    // Start is called before the first frame update
    void Start()
    {
        characterController = GetComponent<CharacterController>();
        characterController.detectCollisions = false;
        playerStats = GetComponent<PlayerStats>();
        anim = playerStats.ClassModels.GetChild((int)playerStats.CurrentClass).GetComponent<Animator>();
        stepOffset = characterController.stepOffset;
    }

    public void ModifySpeedStats(float multiplier)
    {
        walkingSpeed *= multiplier;
        runSpeed *= multiplier;
        downSpeed *= multiplier;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateMovement();
        MoveByGravity();

        if (!playerStats.IsDown)
        {
            JumpCheck();
        }

        CheckPlayerOutOfBounds();
    }

    private void UpdateMovement()
    {
        Vector3 movement = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        movement.Normalize();

        float speed = 0;

        if (!playerStats.IsDown)
        {
            speed = Input.GetButton("Sprint") ? runSpeed : walkingSpeed;
        }
        else
        {
            speed = downSpeed;
        }

        movement = transform.TransformDirection(movement);
        movement.y = 0;

        anim.SetFloat("Speed", movement.magnitude * speed, dampTime, Time.deltaTime);

        characterController.Move(movement * speed * Time.deltaTime);
    }

    private void MoveByGravity()
    {
        characterController.Move(velocity * Time.deltaTime);

        if (characterController.isGrounded)
        {
            lastGroundedPosition = transform.position;

            if (velocity.y < 0)
            {
                velocity.y = -2;
                characterController.stepOffset = stepOffset;
            }
        }
        else
        {
            characterController.stepOffset = 0;
        }

        velocity.y += gravity * Time.deltaTime;
    }

    private void CheckPlayerOutOfBounds()
    {
        if (transform.position.y < outOfBoundsY)
        {
            transform.position = lastGroundedPosition;
        }
    }

    private void JumpCheck()
    {
        if (Input.GetButtonDown("Jump") && characterController.isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }
}
