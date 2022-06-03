using FishNet;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Prediction;
using UnityEngine;

/*
* 
* See TransformPrediction.cs for more detailed notes.
* 
*/

namespace FishNet.Example.Prediction.CharacterControllers
{

    public class Tank2dController : NetworkBehaviour
    {
        #region Types.
        public struct InputData
        {
            public float Horizontal;
            public float Vertical;
            public bool IsShooting;
        }
        public struct ReconcileData
        {
            public Vector3 Position;
            public Quaternion Rotation;
            public Vector2 Velocity;
            public ReconcileData(Vector3 position, Quaternion rotation, Vector2 velocity)
            {
                Position = position;
                Rotation = rotation;
                Velocity = velocity;
            }
        }
        #endregion

        #region Serialized.
        [SerializeField]
        private float _moveRate = 5f;

        [SerializeField]
        private float _turnRate = 5f;

        [SerializeField]
        private float _accelerationSmoothing = .05f;

        [SerializeField]
        private float _angularAccelerationSmoothing = .05f;

        [SerializeField]
        private GameObject _bullet;
        #endregion

        #region Private.
        private Rigidbody2D _rigidbody2D;
        private Vector3 _velocity = Vector3.zero;
        private float _angularVelocity = 0f;
        #endregion


        private void Awake()
        {
            InstanceFinder.TimeManager.OnTick += TimeManager_OnTick;
            _rigidbody2D = GetComponent<Rigidbody2D>();
        }

        public override void OnStartClient()
        {
            base.OnStartClient();            

        }

        private void OnDestroy()
        {
            if (InstanceFinder.TimeManager != null)
            {
                InstanceFinder.TimeManager.OnTick -= TimeManager_OnTick;
            }
        }

        private void TimeManager_OnTick()
        {
            if (base.IsOwner)
            {
                Reconciliation(default, false);
                CheckInput(out InputData id);
                if (id.IsShooting)
                {
                    Shoot(base.LocalConnection);
                }
                Move(id, false);
            }
            if (base.IsServer)
            {
                Move(default, true);
                ReconcileData rd = new ReconcileData(transform.position, transform.rotation, _velocity);
                Reconciliation(rd, true);
            }
        }

        [ServerRpc]
        private void Shoot(NetworkConnection conn)
        {
            GameObject gob = Instantiate(_bullet, transform.position + (transform.forward * 1), transform.rotation);
            Spawn(gob, conn);
        }

        private void CheckInput(out InputData id)
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");

            id = new InputData()
            {
                Horizontal = horizontal,
                Vertical = vertical,
                IsShooting = Input.GetButton("Fire1")
            };
        }

        [Replicate]
        private void Move(InputData md, bool asServer, bool replaying = false)
        {
            Vector3 targetVelocity = transform.up * md.Vertical * _moveRate;

            _rigidbody2D.velocity = Vector3.SmoothDamp(_rigidbody2D.velocity, targetVelocity, ref _velocity, _accelerationSmoothing);

            float targetAngularVelocity = -md.Horizontal * _moveRate * _turnRate;
            _rigidbody2D.angularVelocity = Mathf.SmoothDamp(_rigidbody2D.angularVelocity, targetAngularVelocity, ref _angularVelocity, _angularAccelerationSmoothing);
        }

        [Reconcile]
        private void Reconciliation(ReconcileData rd, bool asServer)
        {
            _rigidbody2D.velocity = rd.Velocity;
            _velocity = rd.Velocity;

            // transform.position = rd.Position;
            // transform.rotation = rd.Rotation;
        }


    }


}