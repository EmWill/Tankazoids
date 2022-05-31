using FishNet;
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

    public class TankController : NetworkBehaviour
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
            public ReconcileData(Vector3 position, Quaternion rotation)
            {
                Position = position;
                Rotation = rotation;
            }
        }
        #endregion

        #region Serialized.
        [SerializeField]
        private float _moveRate = 5f;

        [SerializeField]
        private float _turnRate = 5f;
        [SerializeField]
        private NetworkObject _bullet;
        #endregion

        #region Private.
        private CharacterController _characterController;
        #endregion


        private void Awake()
        {
            InstanceFinder.TimeManager.OnTick += TimeManager_OnTick;
            _characterController = GetComponent<CharacterController>();
        }

        public override void OnStartClient()
        {
            base.OnStartClient();            
            _characterController.enabled = (base.IsServer || base.IsOwner);
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
                    NetworkObject nob = Instantiate(_bullet, transform.position + (transform.forward * 1), transform.rotation);
                    Shoot(nob);
                }
                Move(id, false);
            }
            if (base.IsServer)
            {
                Move(default, true);
                ReconcileData rd = new ReconcileData(transform.position, transform.rotation);
                Reconciliation(rd, true);
            }
        }

        [ServerRpc]
        private void Shoot(NetworkObject nob)
        {
            ServerManager.Spawn(nob);
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
            Vector3 motion = transform.forward * md.Vertical * _moveRate; // todo there is slowdown when player is facing up or down
            Vector3 move = new Vector3(motion.x, Physics.gravity.y, motion.z); 

            _characterController.Move(move * (float)base.TimeManager.TickDelta);
            transform.Rotate(0, md.Horizontal * _turnRate * (float)base.TimeManager.TickDelta, 0);
        }

        [Reconcile]
        private void Reconciliation(ReconcileData rd, bool asServer)
        {
            transform.position = rd.Position;
            transform.rotation = rd.Rotation;
        }


    }


}