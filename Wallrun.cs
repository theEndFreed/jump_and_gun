using UnityEngine;

public class Wallrun : MonoBehaviour
{
    public LayerMask wall;
    public LayerMask ground;

    public float wallrunforcem;
    public float maxwallruntime;
    float wallruntimer;

    float horizontalInput;
    float verticalInput;

    public float walljumpupforce;
    public float walljumpdsideforce;

    public float wallcheckdisatce;
    public float minjumpHeight;
    RaycastHit leftwallhit;
    RaycastHit rightwallhit;
    bool wallleft, wallright;

    public KeyCode JumpKey = KeyCode.Space;


    public Transform orientation;
    Movemt_player mp;
    Rigidbody rb;

    public bool useGravity;
    public float gravitycounterforce;

    bool exetingwall;
    public float exitwalltime;
    float exitwalltimer;


    void Start()
    {
        rb = GetComponent<Rigidbody>();
        mp = GetComponent<Movemt_player>();
    }

    void Update()
    {
        checkwall();
        Statemachine();
    }

    void checkwall()
    {
        wallright = Physics.Raycast(transform.position, orientation.right, out rightwallhit, wallcheckdisatce, wall);
        wallleft = Physics.Raycast(transform.position, -orientation.right, out leftwallhit, wallcheckdisatce, wall);
    }

    bool aboveground()
    {
        return !Physics.Raycast(transform.position, Vector3.down, minjumpHeight, ground);
    }

    void Statemachine()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if((wallleft || wallright) && verticalInput != 0 && aboveground() && !exetingwall)
        {
            if (!mp.iswallruning && !mp.sliding)
            {
                Startwallrun();
            }

            if (Input.GetKeyDown(JumpKey))
            {

                Walljump();
            }

            if (wallruntimer > 0)
                wallruntimer -= Time.deltaTime;

            if (wallruntimer <= 0 && mp.iswallruning)
            {
                exetingwall = true;
                exitwalltimer = exitwalltime;
            }

        }

        else if (exetingwall)
        {
            if (mp.iswallruning)
            {
                Stopwallrun();
            }

            if(exitwalltimer > 0)
            {
                exitwalltimer -= Time.deltaTime;
            }

            if(exitwalltimer <= 0)
            {
                exetingwall = false;
            }
        }


        else
        {
            if (mp.iswallruning)
            {
                Stopwallrun();
            }
        }

    }

    private void FixedUpdate()
    {
        if (mp.iswallruning)
        {

            MoveWallRun();
        }
    }

    void Startwallrun()
    {
        mp.iswallruning = true;

        wallruntimer = maxwallruntime;

    }

    void Stopwallrun()
    {
        mp.iswallruning = false;

    }

    void MoveWallRun()
    {
        rb.useGravity = useGravity;


        Vector3 wallNormal = wallright ? rightwallhit.normal : leftwallhit.normal;

        Vector3 wallforward = Vector3.Cross(wallNormal, orientation.up);

        if ((orientation.forward - wallforward).magnitude > (orientation.forward - -wallforward).magnitude)
        {
            wallforward = -wallforward;
        }


        rb.AddForce(wallforward * wallrunforcem, ForceMode.Force);

        if (!(wallleft && horizontalInput > 0) && !(wallright && horizontalInput < 0))
        {
            rb.AddForce(-wallNormal * 200f, ForceMode.Force);
        }

        if (useGravity)
        {
            rb.AddForce(transform.up * gravitycounterforce, ForceMode.Force);    
        }

    }

    void Walljump()
    {
        exetingwall = true;
        exitwalltimer = exitwalltime;

        Vector3 wallNormal = wallright ? rightwallhit.normal : leftwallhit.normal;

        Vector3 forceforJump = transform.up * walljumpupforce + wallNormal * walljumpdsideforce *2f;

        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.y);
        rb.AddForce(forceforJump, ForceMode.Impulse);
    }
}
