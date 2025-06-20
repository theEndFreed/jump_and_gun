using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Movemt_player : MonoBehaviour
{
    public Transform Level_sp;

    [Header("Movment")]
    public float speed;
    public float sprintspeed;
    public float walkspeed;

    public float wallrunspeed;

    public float slidemp;

    public Text speedshower;

    public float groundDrag;

    [Header("Jump")]
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    private bool canjump;
    public float fallmutiplayer;
    public Transform orientation;

    public float airdrag;

    [Header("Slideslope")]
    private float desiredMovespeed;
    private float lastdesiredMovespeed;
    public float slidespeed;
    public bool sliding;
    private bool existingSlope;
    public float speedIncreaseMultiplier;
    public float slopeIncreaseMultiplier;

    [Header("Key Binds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintkey = KeyCode.LeftShift;


    [Header("Slope")]
    public float maxSlpoeAngle;
    private RaycastHit slopehit;


    [Header("Ground")]
    public float playerHeight;
    public LayerMask ground;
    public bool grounded;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;

    public bool iswallruning;

    Rigidbody rb;

    public MovementState state;

    public enum MovementState
    {
        Walking,
        sprinting,
        walllrun,
        sliding,
        air
    }

    float geschwindichkeit;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        canjump = true;

        rb.transform.position = Level_sp.transform.position;
    }

    void Update()
    {

        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, ground);
        SpeedControul();
        MyInput();
        Satethandler();


        if (grounded)
        {
            rb.linearDamping = groundDrag;
        }
        else
        {
            rb.linearDamping = airdrag;
        }

        //geschwindichkeit = rb.linearVelocity.magnitude;
        //speedshower.text = geschwindichkeit.ToString();


    }

    private void FixedUpdate()
    {
        Movment();

        if(rb.linearVelocity.y < 0 && !OnSlope())
        {
            rb.AddForce(Vector3.down * fallmutiplayer, ForceMode.Force);
        }

    }

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if (Input.GetKey(jumpKey) && canjump && grounded)
        {
            canjump = false;

            Jump();

            Invoke(nameof(JumpReady), jumpCooldown);
        }
    }
    private void Satethandler()
    {
        if (iswallruning)
        {
            state = MovementState.walllrun;
            desiredMovespeed = wallrunspeed;

        }

        else if (sliding)
        {
            state = MovementState.sliding;

            if (OnSlope() && rb.linearVelocity.y < 0.1f)
            {
                desiredMovespeed = slidespeed;
            }
            else
            {
                desiredMovespeed = sprintspeed;

            }
        }

        else if (grounded && Input.GetKey(sprintkey))
        {
            state = MovementState.sprinting;
            desiredMovespeed = sprintspeed;
        }
        else if (grounded)
        {
            state = MovementState.Walking;
            desiredMovespeed = walkspeed;
        }
        else
        {
            state = MovementState.air;

        }

        if (Mathf.Abs(desiredMovespeed - lastdesiredMovespeed) > Mathf.Abs(sprintspeed - walkspeed) && speed != 0)
        {
            StopAllCoroutines();
            StartCoroutine(SmoothlyLerpMoveSpeed());

        }
        else
        {
            speed = desiredMovespeed;
        }



        lastdesiredMovespeed = desiredMovespeed;

    }
    private IEnumerator SmoothlyLerpMoveSpeed()
    {
        float time = 0;
        float difference = Mathf.Abs(desiredMovespeed - speed);
        float startValue = speed;

        while (time < difference)
        {
            speed = Mathf.Lerp(startValue, desiredMovespeed, time / difference);

            if (OnSlope())
            {
                float slopeAngle = Vector3.Angle(Vector3.up, slopehit.normal);
                float slopeAngleIncrease = 1 + (slopeAngle / 90f);
                time += Time.deltaTime * speedIncreaseMultiplier * slopeIncreaseMultiplier * slopeAngleIncrease;
            }
            else
                time += Time.deltaTime * speedIncreaseMultiplier;

            yield return null;
        }

        speed = desiredMovespeed;
    }

    private void Movment()
    {
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if (OnSlope() && !existingSlope)
        {
            rb.AddForce(GetSlopeMoveDirection(moveDirection) * speed * 20f, ForceMode.Force);

            if (rb.linearVelocity.y > 0)
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
        }

        else if (grounded)

            rb.AddForce(moveDirection.normalized * speed * 10f, ForceMode.Force);

        else if (!grounded)
        {
            rb.AddForce(moveDirection.normalized * speed * 10f * airMultiplier, ForceMode.Force);
        }

        rb.useGravity = !OnSlope() || !iswallruning;


    }

    private void SpeedControul()
    {
        if (OnSlope() && !existingSlope)
        {
            if (rb.linearVelocity.magnitude > speed)
            {
                rb.linearVelocity = rb.linearVelocity.normalized * speed;
            }
        }

        else
        {
            Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

            if (flatVel.magnitude > speed)
            {
                Vector3 limitedVel = flatVel.normalized * speed;
                rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
            }
        }



    }

    private void Jump()
    {
        existingSlope = true;

        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        if (!sliding)
        {
            rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
        }
        else       
        {
            rb.AddForce(transform.up * jumpForce *slidemp, ForceMode.Impulse);
        }

    }

    private void JumpReady()
    {
        canjump = true;

        existingSlope = false;

    }

    public bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopehit, playerHeight * 0.5f + 0.2f))
        {
            float angle = Vector3.Angle(Vector3.up, slopehit.normal);
            return angle < maxSlpoeAngle && angle != 0;
        }

        return false;
    }

    public Vector3 GetSlopeMoveDirection(Vector3 direction)
    {
        return Vector3.ProjectOnPlane(direction, slopehit.normal).normalized;
    }



}
