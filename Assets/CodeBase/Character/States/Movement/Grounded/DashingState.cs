﻿using Assets.CodeBase.Character.Data.States.Grounded;
using Assets.CodeBase.Character.States.Movement.Grounded.Moving;
using Assets.CodeBase.Character.States.Movement.Grounded.Stopping;
using System;
using UnityEngine;

namespace Assets.CodeBase.Character.States.Movement.Grounded
{
    public class DashingState : GroundedState
    {
        private UnitDashData _dashData;

        private float _startTime;
        private int _consecutiveDashesUsed;

        private bool _shouldKeepRotating;

        public DashingState(MovementStateMachine stateMachine) : base(stateMachine) {
            _dashData = _stateMachine.Player.Data.GroundedData.DashData;
        }

        public override void Enter() {
            base.Enter();

            _stateMachine.ReusableData.MovementSpeedModifier = _dashData.SpeedModifier;
            _stateMachine.ReusableData.RotationData = _dashData.RotationData;

            AddForceOnTransitionFromIdleState();
            UpdateConsecutiveDashes();

            _shouldKeepRotating = _stateMachine.ReusableData.MovementInput != Vector2.zero;

            _startTime = Time.time;
        }

        public override void PhysicsUpdate() {
            base.PhysicsUpdate();

            if (!_shouldKeepRotating)
                return;

            RotateTowardsTargetRotation();
        }

        public override void Exit() {
            base.Exit();

            SetBaseRotationData();
        }

        protected override void AddInputActionsCallbacks() {
            base.AddInputActionsCallbacks();

            _stateMachine.Player.InputService.MovementPerformed += OnMovementStarted;
        }

        protected override void RemoveInputActionsCallbacks() {
            base.RemoveInputActionsCallbacks();

            _stateMachine.Player.InputService.MovementPerformed -= OnMovementStarted;
        }

        private void OnMovementStarted() {
            _shouldKeepRotating = true;
        }

        public override void OnAnimationTransitEvent() {
            if (_stateMachine.ReusableData.MovementInput == Vector2.zero)
                _stateMachine.Enter<HardStoppingState>();
            else _stateMachine.Enter<SprintingState>();
        }

        protected override void OnMovementCancelled() { }
        protected override void OnDashStarted() { }

        private void AddForceOnTransitionFromIdleState() {
            if (_stateMachine.ReusableData.MovementInput != Vector2.zero)
                return;

            Vector3 characterRotationDirection = _stateMachine.Player.transform.forward;

            characterRotationDirection.y = 0f;

            UpdateTargetRotation(characterRotationDirection, false);

            _stateMachine.Player.Rigidbody.velocity = characterRotationDirection * GetMovementSpeed();
        }

        private void UpdateConsecutiveDashes() {
            if (!IsConsecutive())
                _consecutiveDashesUsed = 0;

            _consecutiveDashesUsed++;
            if (_consecutiveDashesUsed == _dashData.ConsecutiveDashesLimitAmount) {
                _consecutiveDashesUsed = 0;

                _stateMachine.Player.InputService.DisableDashFor(_dashData.DashLimitReachedCooldown);
            }
        }

        private bool IsConsecutive() =>
            Time.time < _startTime + _dashData.ConsecutiveTime;
    }
}
