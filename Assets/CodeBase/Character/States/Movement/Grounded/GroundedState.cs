﻿using Assets.CodeBase.Character.States.Movement.Grounded.Moving;
using Assets.CodeBase.Utility.Colliders;
using UnityEngine;

namespace Assets.CodeBase.Character.States.Movement.Grounded
{
    public class GroundedState : MovementState
    {
        private SlopeData _slopeData;

        public GroundedState(MovementStateMachine stateMachine) : base(stateMachine) {
            _slopeData = stateMachine.Player.ColliderUtility.SlopeData;
        }

        public override void PhysicsUpdate() {
            base.PhysicsUpdate();

            FloatCapsule();
        }

        protected override void AddInputActionsCallbacks() {
            base.AddInputActionsCallbacks();

            _stateMachine.Player.InputService.MovementCancelled += OnMovementCancelled;
            _stateMachine.Player.InputService.DashStarted += OnDashStarted;
        }

        protected override void RemoveInputActionsCallbacks() {
            base.RemoveInputActionsCallbacks();

            _stateMachine.Player.InputService.MovementCancelled -= OnMovementCancelled;
            _stateMachine.Player.InputService.DashStarted -= OnDashStarted;
        }

        protected virtual void OnMovementCancelled() => 
            _stateMachine.Enter<IdlingState>();

        protected virtual void OnDashStarted() => 
            _stateMachine.Enter<DashingState>();

        protected virtual void OnMove() {
            if (_stateMachine.ReusableData.IsWalking) {
                _stateMachine.Enter<WalkingState>();
            } else {
                _stateMachine.Enter<RunningState>();
            }
        }

        private void FloatCapsule() {
            Vector3 capsuleColliderCenter = _stateMachine.Player.ColliderUtility.CapsuleColliderData.Collider.bounds.center;

            Ray downwardsFromCapsuleCenter = new Ray(capsuleColliderCenter, Vector3.down);

            if (Physics.Raycast(
                downwardsFromCapsuleCenter, 
                out RaycastHit hit, 
                _slopeData.FloatRayDistance, 
                _stateMachine.Player.LayerData.GroundLayer,
                QueryTriggerInteraction.Ignore)) {

                float groundAngle = Vector3.Angle(hit.normal, -downwardsFromCapsuleCenter.direction);

                float slopeSpeedModifier = SetSlopeModifierOnAngle(groundAngle);
                if (slopeSpeedModifier == 0f)
                    return;

                float distanceToFloatingPoint = 
                    _stateMachine.Player.ColliderUtility.CapsuleColliderData.ColliderCenterInLocalSpace.y * 
                    _stateMachine.Player.transform.localScale.y - hit.distance;

                if (distanceToFloatingPoint == 0f)
                    return;

                float amountToLift = distanceToFloatingPoint * _slopeData.StepReachForce - GetPlayerVerticalVelocity().y;

                Vector3 liftForce = new Vector3(0f, amountToLift, 0f);

                _stateMachine.Player.Rigidbody.AddForce(liftForce, ForceMode.VelocityChange);
            }
        }

        private float SetSlopeModifierOnAngle(float groundAngle) {
            float slopeSpeedModifier = _stateMachine.Player.Data.GroundedData.SlopeSpeedAngles.Evaluate(groundAngle);

            _stateMachine.ReusableData.MovementOnSlopesSpeedModifier = slopeSpeedModifier;

            return slopeSpeedModifier;
        }
    }
}
