using System;
using UnityEngine;

public class DOF : MonoBehaviour
{

    [HideInInspector] private int _dofCount = -1;
    [HideInInspector] private Transform[] _bones = null;
    [HideInInspector] private Transform _wrist = null;
    [HideInInspector] private char[] _dofAxes = null;

    [HideInInspector] private Quaternion[] _originalRotation = null;
    [HideInInspector] private Vector3? _wristOriginalTransform = null;
    [HideInInspector] private Quaternion? _wristOriginalRotation = null;

    //[HideInInspector] private Quaternion[] _currentRotationOrigins = null;

    // aggregate rotation angels
    [SerializeField] private float[] _dofs = null;
    // angels to rotate, reset to 0 after apply
    [SerializeField] public float[] DoFTilts = null;

    // aggregate
    // frist transform then rotation [x, y, z, x, y, z]
    [SerializeField] private Vector3 _wristPosition;
    [SerializeField] public float[] WristTilts = new float[6];


    // eigen
    [HideInInspector] private float[][] _eigenVectors;

    // joint constrants
    [HideInInspector] public float[] JointMinValues { get; private set; }
    [HideInInspector] public float[] JointMaxValues { get; private set; }

    // TODO refactor
    // use predescribed contact points on hand (sphere colliders)

    private bool _is_selectedContact;
    public bool Is_selectedContact { 
        get { return _is_selectedContact; }
        set {
            _is_selectedContact = value;
            UpdateColliders();      
        }
    }
    public bool Is_meshCollider { get; set; }

    private bool _is_manualControl;
    public bool Is_manualControl
    {
        get { return _is_manualControl; }
        set
        {
            _is_manualControl = value;
            UpdateColliders();
        }
    }
    public SphereCollider[] SelectedContactPoints { get; set; }

    // Start is called before the first frame update
    void Start()
    {
        Is_meshCollider = false;
        _is_selectedContact = false; // has priority over Is_meshCollider

        UpdateColliders();

        AttachDofToJoints();
        JointConstrantsInit();
        EigenInit();

    }


    void FixedUpdate()
    {
        //ResetTransform();

        // for see the effect of twicking values in inspector
        //UpdateWrist();
        //UpdateBones();

    }

    //private void OnCollisionStay(Collision collision)
    //{
    //    var rb = GetComponent<Rigidbody>();
    //    if(rb != null)
    //    {
    //        rb.velocity = Vector3.zero;
    //        rb.angularVelocity = Vector3.zero;
    //    }
    //}

    // different colliders are used in different modes.
    void UpdateColliders()
    {
        var colliders = GetComponentsInChildren<Collider>();
        Type collidersToEnable;
        if (Is_meshCollider)
        {
            collidersToEnable = typeof(MeshCollider);
            Destroy(GetComponent<Rigidbody>());
        }
        else
        {
            collidersToEnable = typeof(CapsuleCollider);


            var rb = GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody>();
                rb.useGravity = false;
            }


        }

        if (Is_selectedContact)
        {
            collidersToEnable = typeof(SphereCollider);
            SelectedContactPoints = GetComponentsInChildren<SphereCollider>();
        }

        //if (Is_manualControl)
        //{
        //    collidersToEnable = typeof(CapsuleCollider);
        //    var rb = gameObject.AddComponent<Rigidbody>();
        //    rb.useGravity = false;
        //}
        //else
        //{
        //    Destroy(GetComponent<Rigidbody>());
        //}
        

        foreach (var collider in colliders)
        {
            
            var type = collider.GetType();

            //if(type != typeof(MeshCollider))
            //{
            //    collider.isTrigger = true; 
            //}

            //print(string.Format("{0}, {1}",collider , collider.GetType() == collidersToEnable));
            if (type == collidersToEnable)
            {
                collider.enabled = true;

            }
            else
            {
                collider.enabled = false;
            }
        }

        // colliders to disable
        //var colliders = GetComponentsInChildren(
        //    Is_selectedContact ? typeof(CapsuleCollider) : typeof(SphereCollider));

        //foreach (var collider in colliders)
        //{
        //    ((Collider) collider).enabled = false;
        //}

        //if (Is_selectedContact)
        //{
        //    SelectedContactPoints = GetComponentsInChildren<SphereCollider>();
        //}

        //var rb = GetComponent<Rigidbody>();
        //if (Is_selectedContact)
        //{
            
        //    if(rb == null)
        //    {
        //        rb = gameObject.AddComponent<Rigidbody>();
        //        rb.useGravity = false;
        //        rb.isKinematic = false;

        //        Physics.SyncTransforms();
        //        rb.ResetCenterOfMass();
        //    }
        //}
        //else
        //{
        //    if(rb != null)
        //    {
        //        Destroy(rb);
        //    }
            
        //}

    }

    //private void OnDestroy()
    //{
    //    var rb = gameObject.AddComponent<Rigidbody>();
    //    if(rb != null)
    //    {
    //        Destroy(rb);
    //    }
    //}

    private void JointConstrantsInit()
    {

        // first 6 values for transform do not matter

        JointMinValues = new float[29]
        {
            0, 0, 0,
            0, 0, 0,
            -15, -50, -20, -70, -80,
            0, -20, 0, 0, 0,
            -10, 0, 0, -0.7f, 0,
            -15, 0, 0, -1, 0,
            -5, 0, 0
        };
        JointMaxValues = new float[29]
        {
            0, 0, 0,
            0, 0, 0,
            55, 20, 20, 20, 10,
            90, 10, 90, 90, 90,
            10, 90, 90, 0.7f, 90,
            24, 90, 90, 1, 90,
            25, 90, 90

        };

    }

    private void EigenInit()
    {
        const int PrincipalComponentCount = 6;
        const int JointDofCount = 23;

        _eigenVectors = new float[PrincipalComponentCount][];


        _eigenVectors[0] = new float[JointDofCount]
        {
            0.05174f, -0.12964f,    0,   -0.06365f,    -0.01094f,
            -0.41296f,    0.050409f,    -0.042016f,   -0.00754f,    -0.51469f,
            0,   -0.02456f,    -0.01305f,    0,   -0.53003f,
            -0.04461f,    0.01607f, -0.06576f,    0,
            -0.45099f,    -0.10093f,    -0.17029f,    -0.07539f
        };

        _eigenVectors[1] = new float[JointDofCount]
        {
            -0.20613f,    0.00719f, 0,   -0.15979f,    -0.04356f,
            0.20746f, -0.049675f,   -0.33611f,    -0.20765f,    0.18604f,
            0,   -0.35635f,    -0.33362f,    0,   0.02716f,
            0.09129f, -0.38659f,    -0.26735f,    0,   -0.19259f,
            0.02125f, -0.34394f,    -0.264f

        };

        _eigenVectors[2] = new float[JointDofCount]
        {
            0.07599f, 0.1758f,  0,   -0.58248f,    0.02899f,
            0.14512f, -0.15071f,    -0.17381f,    -0.04835f,    0.16485f,
            0,   -0.06025f,    0.05891f, 0,   -0.11095f,
            0.16019f, 0.1762f,  0.2662f,  0,   -0.40847f,
            0.17407f, 0.39977f, 0.12555f,
        };

        _eigenVectors[3] = new float[JointDofCount]
        {
            -0.01666f,    -0.08333f,    0,   0.48276f, 0.0009f,
            0.37761f, -0.067079f,   0.12263f, 0.09017f, 0.26142f,
            0,   0.25866f, 0.10387f, 0,   -0.14321f,
            0.19362f, 0.08207f, 0.2204f,  0,   -0.50834f,
            0.042f,   -0.24976f,    -0.08701f
        };

        _eigenVectors[4] = new float[JointDofCount]
        {
            -0.10565f,    -0.00662f,    0,   0.39313f, -0.23341f,
            0.0474f,  0.00602f, 0.0459f,  -0.33341f,    -0.05043f,
            0,   -0.14934f,    -0.56798f,    0,   -0.10468f,
            -0.00096f,    0.19486f, -0.05894f,    0,  -0.08053f,
            -0.03538f,    0.43649f, 0.2571f
        };

        _eigenVectors[5] = new float[JointDofCount]
        {
            -0.18844f,    0.12005f, 0,   -0.01788f,    -0.19021f,
            -0.15442f,    -0.14599f,    0.27379f, -0.24099f,    0.07924f,
            0,   -0.12278f,    0.23768f, 0,   0.05421f, 
            0.08843f, 0.51388f, -0.33799f,    0,   -0.01156f,    
            0.24779f, -0.02338f,    -0.4543f
        };
    }


    // constrain the dof according to _minValues and _maxValues
    private float[] ConstrainJointValues(float[] DofValues)
    {
        for (int i = 5; i < DofValues.Length; i++)
        {
            var lowerBound = JointMinValues[i];
            var upperBound = JointMaxValues[i];
            var currentVal = DofValues[i];

            if (currentVal < lowerBound)
            {
                DofValues[i] = lowerBound;
            }else if (currentVal > upperBound)
            {
                DofValues[i] = upperBound;
            }
        }

        return DofValues;
    }

    // TODO consider origin
    // wristAndEigen : float[12] frontmost 6 for wrist and the latter 6 for eigen scalars
    public void ApplyEigen(float[] wristAndEigen, int numberOfValidPC)
    {
        float[] newDofValues = new float[29];
        for (int i = 0; i < 6; i++)
        {
            newDofValues[i] = wristAndEigen[i];
        }

        for (int i = 0; i < numberOfValidPC; i++)
        {
            var eigengraspScalar = wristAndEigen[i + 6];
            var eigengraspCurr = _eigenVectors[i];
            for (int j = 0; j < eigengraspCurr.Length; j++)
            {
                newDofValues[j + 6] += eigengraspCurr[j] * eigengraspScalar;
            }
        }

        ApplyDOFs(ConstrainJointValues(newDofValues));
    }

    public void ApplyEigen(float[] wristAndEigen)
    {
        ApplyEigen(wristAndEigen, wristAndEigen.Length - 6);
    }


    // wrist position: global axes
    // every thing else: local axes
    public void ApplyDOFs(float[] newDofValues)
    {
        ResetTransform();

        for (int i = 0; i < 29 - 6; i++)
        {
            DoFTilts[i] = newDofValues[i + 6];
        }

        // apply wrist transform
        
        _wrist.position = new Vector3(newDofValues[0], newDofValues[1], newDofValues[2]);
        _wrist.Rotate(newDofValues[3], newDofValues[4], newDofValues[5]);

        UpdateBones();
    }

    public void MoveTowards(Vector3 direction, float distance)
    {
        _wrist.transform.Translate(direction.normalized * distance, Space.World);
    }

    private void AttachDofToJoints()
    {
        // config
        const int dofCount = 23;

        const string BaseBoneName = "Armature/wrist";
        string[] boneNames = new string[dofCount]
        {
            "carpal1", "carpal1", "carpal1/MC1/PP1", "carpal1/MC1/PP1", "carpal1/MC1/PP1/DP1",
            "carpal23/MC2/PP2", "carpal23/MC2/PP2", "carpal23/MC2/PP2/MP2", "carpal23/MC2/PP2/MP2/DP2",
            "carpal23/MC3/PP3", "carpal23/MC3/PP3", "carpal23/MC3/PP3/MP3", "carpal23/MC3/PP3/MP3/DP3",
            "carpal4", "carpal4/MC4/PP4", "carpal4/MC4/PP4", "carpal4/MC4/PP4/MP4", "carpal4/MC4/PP4/MP4/DP4",
            "carpal5", "carpal5/MC5/PP5", "carpal5/MC5/PP5", "carpal5/MC5/PP5/MP5", "carpal5/MC5/PP5/MP5/DP5"
        };

        char[] dofAxes = new char[dofCount]
        {
            'x', 'z', 'x', 'z', 'z',
            'x', 'z', 'x', 'x',
            'x', 'z', 'x', 'x',
            'z', 'x', 'z', 'x', 'x',
            'z', 'x', 'z', 'x', 'x'
        };

        // inits
        _dofCount = dofCount;
        _bones = new Transform[_dofCount];
        _originalRotation = new Quaternion[_dofCount];
        //_currentRotationOrigins = new Quaternion[_dofCount];
        _dofs = new float[_dofCount];
        DoFTilts = new float[_dofCount];
        _dofAxes = dofAxes;
        

        // find wrist 
        _wrist = gameObject.transform;
        _wristOriginalTransform = _wrist.position;
        _wristOriginalRotation = _wrist.rotation;


        // find bones from model
        for (int i=0; i< _dofCount; i++)
        {
            string boneFullName = string.Format("{0}/{1}", BaseBoneName, boneNames[i]);
            if ((_bones[i] = transform.Find(boneFullName)) == null)
            {
                print(string.Format("bone {0} not found", boneFullName));
                continue;
            }
            _originalRotation[i] = _bones[i].rotation;
        }

 
    }

    public void ResetTransform()
    {
        // reset wrist
        try
        {
            _wrist.position = _wristOriginalTransform.Value;
            _wrist.rotation = _wristOriginalRotation.Value;
          
        }
        catch (InvalidOperationException)
        {
            print("wrist original value lose, transformation abandoned");
        }

        // reset bones
        for (int i = 0; i < _dofCount; i++)
        {
            _bones[i].rotation = _originalRotation[i];
        }

        var rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        Array.Clear(_dofs, 0, _dofs.Length);
    }

    public void UpdateWrist()
    {
        _wrist.Translate(WristTilts[0], WristTilts[1], WristTilts[2]);
        _wrist.Rotate(WristTilts[3], WristTilts[4], WristTilts[5]);


        for(var i=0; i<6; i++)
        {
            WristTilts[i] = 0;
        }

        _wristPosition = _wrist.transform.position;
        //for (int i = 0; i < _dofCount; i++)
        //{
        //    _currentRotationOrigins[i] = _bones[i].rotation;
        //}

    }

    private bool CheckDoFLimits(int DoFIndex, float addition)
    {
        var newVal = _dofs[DoFIndex] + addition;

        if(newVal > JointMaxValues[DoFIndex + 6] ||
            newVal < JointMinValues[DoFIndex + 6])
        {
            return false;
        }

        return true;
    }

    public void UpdateBones(bool is_constraintsApplied = false)
    {
        for (int i = 0; i < _dofCount; i++)
        {
            Transform boneCurr = _bones[i];

            float angelToRotate = DoFTilts[i];
            // skip if angel equals to 0
            if (Math.Abs(angelToRotate) < float.Epsilon) continue;

            DoFTilts[i] = 0f;


            if (is_constraintsApplied && !CheckDoFLimits(i, angelToRotate))
            {
                Transform bonePrev = boneCurr;
                continue;
            }

            _dofs[i] += angelToRotate;

            switch (_dofAxes[i])
            {
                case 'x':
                    boneCurr.Rotate(angelToRotate, 0, 0, Space.Self);
                    break;
                case 'z':
                    boneCurr.Rotate(0, 0, angelToRotate, Space.Self);
                    break;
                case 'y':
                    boneCurr.Rotate(0, angelToRotate, 0, Space.Self);
                    break;
                default:
                    print("unrecognized dof axis " + angelToRotate);
                    break;
            }
        }
    }

    private Vector3 CurrentWristPosition;
    private Quaternion CurrentWristRotation;
    private Quaternion[] CurrentBonesRotation = null;
    private Vector3 CurrentRbVelocity;
    private Vector3 CurrentRbAngularVelocity;
    private float[] CurrentDofs = null;
    private bool IsTransformSaved = false;

    public void SaveCurrentTransform()
    {
        // save wrist
        CurrentBonesRotation = new Quaternion[_dofCount];
        CurrentDofs = new float[_dofCount];
        try
        {
            CurrentWristPosition = _wrist.position;
            CurrentWristRotation = _wrist.rotation;

        }
        catch (InvalidOperationException)
        {
            print("wrist original value lose, transformation abandoned");
        }

        // save bones
        for (int i = 0; i < _dofCount; i++)
        {
            //_bones[i].rotation = _originalRotation[i];
            CurrentBonesRotation[i] = _bones[i].rotation;
        }

        var rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            CurrentRbVelocity = rb.velocity;
            CurrentRbAngularVelocity = rb.angularVelocity;
        }

        //Array.Clear(_dofs, 0, _dofs.Length);
        Array.Copy(_dofs, CurrentDofs, _dofs.Length);
        IsTransformSaved = true;
    }

    public void LastSavedTransform()
    {
        if (!IsTransformSaved)
        {
            UnityEngine.Debug.Log("Position has not saved!");
            return;
        }
        // recover wrist
        try
        {
            _wrist.position = CurrentWristPosition;
            _wrist.rotation = CurrentWristRotation;

        }
        catch (InvalidOperationException)
        {
            print("wrist original value lose, transformation abandoned");
        }

        // recover bones
        for (int i = 0; i < _dofCount; i++)
        {
            _bones[i].rotation = CurrentBonesRotation[i];
        }

        var rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = CurrentRbVelocity;
            rb.angularVelocity = CurrentRbAngularVelocity;
        }

        Array.Copy(CurrentDofs, _dofs, CurrentDofs.Length);
    }
}
