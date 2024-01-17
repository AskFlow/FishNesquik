using FishNet.Object;
using FishNet.Object.Prediction;
using FishNet.Transporting;
using UnityEngine;

namespace FishNet.Tutorial.Prediction
{
    public class PredictionMotor : NetworkBehaviour
    {
        #region Types
        public struct MoveData : IReplicateData
        {
            public float Horizontal;
            public float Vertical;

            private uint _tick;

            public MoveData(float horizontal, float vertical)
            {
                Horizontal = horizontal;
                Vertical = vertical;
                _tick = 0;
            }

            public void Dispose() { }

            public uint GetTick() => _tick;

            public void SetTick(uint value) => _tick = value;
        }

        public struct ReconcileData : IReconcileData
        {
            public Vector3 Position;
            public Quaternion Rotation;
            public Vector3 Velocity;
            public Vector3 AngularVelocity;

            private uint _tick;

            public ReconcileData(Vector3 position, Quaternion rotation, Vector3 velocity, Vector3 angularVelocity)
            {
                Position = position;
                Rotation = rotation;
                Velocity = velocity;
                AngularVelocity = angularVelocity;
                _tick = 0;
            }

            public void Dispose() { }

            public uint GetTick() => _tick;

            public void SetTick(uint value) => _tick = value;
        }
        #endregion

        #region Misc
        public float MoveRate = 30f;
        private Rigidbody _rigidbody;
        private bool _subscribed = false;
        #endregion

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
        }

        private void SubscribeToTimeManager(bool subscribe)
        {
            if (base.TimeManager == null)
                return;

            if (subscribe == _subscribed)
                return;
            _subscribed = subscribe;

            if (subscribe)
            {
                base.TimeManager.OnTick += TimeManager_OnTick;
                base.TimeManager.OnPostTick += TimeManager_OnPostTick;
            }
            else
            {
                base.TimeManager.OnTick -= TimeManager_OnTick;
                base.TimeManager.OnPostTick -= TimeManager_OnPostTick;
            }
        }

        private void OnDestroy()
        {
            SubscribeToTimeManager(false);
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            SubscribeToTimeManager(true);
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            SubscribeToTimeManager(true);
        }

        private void TimeManager_OnTick()
        {
            if (base.IsOwner)
            {
                Reconciliation(default, true, Channel.Unreliable);
                MoveData data;
                GatherInputs(out data);
                Move(data, true, Channel.Unreliable, false);
            }

            if (base.IsServer)
            {
                Move(default, true);
            }
        }

        private void TimeManager_OnPostTick()
        {
            ReconcileData data = new ReconcileData(
                transform.position, transform.rotation, _rigidbody.velocity, _rigidbody.angularVelocity
            );
            Reconciliation(data, true);
        }

        private void GatherInputs(out MoveData data)
        {
            data = default;

            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");
            if (horizontal == 0f && vertical == 0f)
                return;

            data = new MoveData(horizontal, vertical);
        }

        [Replicate]
        private void Move(MoveData data, bool asServer, Channel channel = Channel.Unreliable, bool replaying = false)
        {
            Vector3 force = new Vector3(data.Horizontal, 0f, data.Vertical) * MoveRate;
            _rigidbody.AddForce(force);
        }

        [Reconcile]
        private void Reconciliation(ReconcileData data, bool asServer, Channel channel = Channel.Unreliable)
        {
            transform.position = data.Position;
            transform.rotation = data.Rotation;
            _rigidbody.velocity = data.Velocity;
            _rigidbody.angularVelocity = data.AngularVelocity;
        }
    }
}
