﻿using Assets.CodeBase.Character.Animation;
using Assets.CodeBase.Character.Data.Colliders;
using Assets.CodeBase.Character.Data.Layers;
using Assets.CodeBase.Character.Data.ScriptableObjects;
using Assets.CodeBase.Character.States.Movement;
using Assets.CodeBase.Character.States.Movement.Grounded;
using Assets.CodeBase.Infrastructure.Properties;
using Assets.CodeBase.Infrastructure.Services;
using Assets.CodeBase.Infrastructure.Services.Input;
using UnityEngine;

namespace Assets.CodeBase.Character.Player
{
    public class Player : MonoBehaviour, IAnimationEventUser
    {
        [Header("Movement Data")]
        [SerializeField] private UnitScriptableObject _movementData;

        [Header("Collisions")]
        [SerializeField] private Rigidbody _rigidbody;
        [SerializeField] private UnitCapsuleColliderUtility _colliderUtility;
        [SerializeField] private UnitLayerData _layerData;

        [Header("Animations")]
        [SerializeField] private Animator _animator;
        [SerializeField] private UnitAnimationData _animationData;

        private MovementStateMachine _movementStateMachine;

        public Animator Animator => _animator;

        private void Awake() {
            _colliderUtility.Initialize();
            _colliderUtility.CalculateCapsuleColliderDimensions();

            _animationData.Initialize();

            _movementStateMachine = new MovementStateMachine(
                transform,
                AllServices.Container.Single<IInputService>(),
                _movementData.GroundedData,
                _movementData.AirborneData,
                _rigidbody,
                _colliderUtility,
                _layerData,
                _animator,
                _animationData);
        }

        private void Start() {
            _movementStateMachine.Enter<IdlingState>();
        }

        private void Update() {
            _movementStateMachine.HandleInput();
            _movementStateMachine.Update();
        }

        private void FixedUpdate() {
            _movementStateMachine.PhysicsUpdate();
        }

        private void OnTriggerEnter(Collider collider) => _movementStateMachine.OnTriggerEnter(collider);

        private void OnTriggerExit(Collider collider) => _movementStateMachine.OnTriggerExit(collider);

        private void OnValidate() {
            _colliderUtility.Initialize();
            _colliderUtility.CalculateCapsuleColliderDimensions();
        }

        public void OnAnimationEnterEvent() => _movementStateMachine.OnAnimationEnterEvent();

        public void OnAnimationExitEvent() => _movementStateMachine.OnAnimationExitEvent();

        public void OnAnimationTransitEvent() => _movementStateMachine.OnAnimationTransitEvent();
    }
}
