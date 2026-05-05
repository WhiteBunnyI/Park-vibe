using System;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    //Settings
    [SerializeField] private float _mouseSens;
    [SerializeField] private float _moveSpeed;

    [SerializeField] private Transform _motionRoot;
    [SerializeField] private float _accelerationSpeed;

    //Parameters
    [SerializeField] private float _fatigue;

    //Components
    private CharacterController _controller;
    private Animator _animator;
    private Camera _camera;

    //Input
    private InputAction _move;
    private InputAction _look;
    private InputAction _interact;

    //Variables
    Vector2 _movement;
    Vector2 _cameraLook;
    Collider _lookingAt;
    Bird[] _birds;
    Coroutine _currentCoroutine;

    //Bool
    private bool _isGameOver;
    private bool _isCanMove;
    private bool _isOnBench;

    //Events
    event Action<Collider> _onInteract;

    public static PlayerController Player { get; private set; }

    void Start()
    {
        Player = this;

        _birds = GameObject.FindGameObjectsWithTag("Bird").Select(g => g.GetComponent<Bird>()).ToArray();

        _controller = GetComponent<CharacterController>();
        _animator = GetComponentInChildren<Animator>();
        _camera = Camera.main;

        _move = InputSystem.actions.FindAction("Move");
        _interact = InputSystem.actions.FindAction("Interact");
        _look = InputSystem.actions.FindAction("Look");

        _onInteract += RestOnBench;
        _onInteract += TouchGrass;

        _isGameOver = false;
        _isCanMove = true;
        _isOnBench = false;
    }

    void TouchGrass(Collider collider)
    {
        if (!collider.attachedRigidbody.CompareTag("Grass"))
            return;

        _fatigue -= 2.5f;
        _fatigue = Mathf.Max(0f, _fatigue);
    }

    void RestOnBench(Collider collider)
    {
        IEnumerator AnimCoroutine(Bench bench)
        {
            _isCanMove = false;

            Vector3 targetPos = bench.transform.position + Vector3.up * 0.1f;
            Vector2 targetCamRot = new Vector2(bench.transform.rotation.eulerAngles.y, 0f);

            Vector2 startCamRot = _cameraLook;
            Vector3 startPos = transform.position;

            float t = 0f;
            while (t < 1f)
            {
                transform.position = Vector3.Slerp(startPos, targetPos, t);
                _cameraLook = Vector2.Lerp(startCamRot, targetCamRot, t);

                CameraSystem();

                t += Time.deltaTime;
                yield return null;
            }

            transform.position = targetPos;

            //For BenchCameraSystem
            _cameraLook = Vector2.zero;

            _isOnBench = true;
            bench.IsUsing = true;

            _currentCoroutine = null;

            _isCanMove = true;
        }

        if (!collider.attachedRigidbody.CompareTag("Bench"))
            return;

        _isOnBench = true;
        _currentCoroutine ??= StartCoroutine(AnimCoroutine(collider.attachedRigidbody.GetComponent<Bench>()));
    }

    void StandFromBench()
    {
        IEnumerator AnimCoroutine(Bench bench)
        {
            _isCanMove = false;

            Vector3 targetPos = transform.position + transform.forward * 2f + Vector3.up * .5f;
            Vector2 targetCam = new Vector2(bench.transform.rotation.eulerAngles.y, 0f);

            Vector3 startPos = transform.position;
            Vector2 startCam = new Vector2(_camera.transform.rotation.eulerAngles.y, _camera.transform.rotation.eulerAngles.x);

            float t = 0f;
            while (t < 1f)
            {
                _cameraLook = Vector2.Lerp(startCam, targetCam, t);
                transform.position = Vector3.Slerp(startPos, targetPos, t);

                CameraSystem();

                t += Time.deltaTime;
                yield return null;
            }

            transform.position = targetPos;
            _cameraLook = targetCam;
            _isOnBench = false;
            bench.IsUsing = false;

            _currentCoroutine = null;

            _isCanMove = true;
        }

        _isOnBench = false;
        _currentCoroutine ??= StartCoroutine(AnimCoroutine(_lookingAt.attachedRigidbody.GetComponent<Bench>()));
    }

    void Update()
    {
        if (_isGameOver) return;

        if (_isCanMove)
        {
            if (_isOnBench)
            {
                BenchCycle();
                return;
            }

            StandCycle();
        }

        if (_fatigue >= 100f) GameOver();

        _fatigue += Time.deltaTime / 2f;
    }

    private void LateUpdate()
    {
        AnimationSystem();
        _motionRoot.localPosition = new Vector3(0f, 1f, 0f);
        UISystem();
    }

    void AnimationSystem()
    {
        _animator.SetBool("IsSitting", _isOnBench);
        _animator.SetFloat("X", _movement.x);
        _animator.SetFloat("Y", _movement.y);
    }

    void UISystem()
    {
        UI.SetFatigue(_fatigue);
        UI.SetHelp("");
        if (_lookingAt != null && _lookingAt.attachedRigidbody != null &&
            _lookingAt.attachedRigidbody.TryGetComponent(out IHelper helper))
        {
            UI.SetHelp(helper.Help);
        }
    }

    void StandCycle()
    {
        MovementSystem();
        GetLookingObj();
        InteractionSystem();
        CameraSystem();
        CheckBirds();
    }

    void BenchCycle()
    {
        if (_move.ReadValue<Vector2>().sqrMagnitude != 0f)
        {
            StandFromBench();
            return;
        }
        BenchCameraSystem();
        _fatigue -= Time.deltaTime * 5f;
        _fatigue = Mathf.Max(_fatigue, 0f);
    }

    void CheckBirds()
    {
        var selectedBirds = _birds.Where(b => Vector3.Distance(_camera.transform.position, b.transform.position) <= 3f).Where(b => b.IsActive);
        var frustrum = GeometryUtility.CalculateFrustumPlanes(_camera);
        foreach (var bird in selectedBirds)
        {
            //Проверка, что объект в поле зрения камеры
            if (!GeometryUtility.TestPlanesAABB(frustrum, bird.Renderer.bounds)) continue;

            //Проверка, что объект виден игроку (не находится за другим объектом)
            Ray ray = new Ray(_camera.transform.position, bird.transform.position - _camera.transform.position);
            if (!Physics.Raycast(ray, out var hit, 3f)) continue;
            if (!hit.collider.CompareTag("Bird")) continue;

            _fatigue -= Time.deltaTime * 1.5f;
            _fatigue = Mathf.Max(0f, _fatigue);
            return;
        }
    }

    void GameOver()
    {
        _isCanMove = false;
        _isGameOver = true;
        GameManager.GameOver();
    }

    void MovementSystem()
    {
        Vector2 target = _move.ReadValue<Vector2>();

        Vector2 diff = target - _movement;
        _movement += Time.deltaTime * _accelerationSpeed * diff;

        Vector2 normMovement = _movement;
        if (normMovement.sqrMagnitude > 1f) normMovement.Normalize();

        Vector3 movement = transform.forward * normMovement.y + transform.right * normMovement.x + Physics.gravity;
        movement *= _moveSpeed;
        _controller.Move(movement * Time.deltaTime);

    }

    void BenchCameraSystem()
    {
        CameraDelta();

        _cameraLook.x = Mathf.Clamp(_cameraLook.x, -80f, 80f);
        _cameraLook.y = Mathf.Clamp(_cameraLook.y, -88f, 88f);

        _camera.transform.localRotation = Quaternion.Euler(_cameraLook.y, _cameraLook.x, 0f);
    }

    void CameraSystem()
    {
        CameraDelta();

        _cameraLook.x %= 360f;
        _cameraLook.y = Mathf.Clamp(_cameraLook.y, -88f, 88f);

        _camera.transform.localRotation = Quaternion.Euler(_cameraLook.y, 0f, 0f);
        transform.rotation = Quaternion.Euler(0f, _cameraLook.x, 0f);
    }

    void CameraDelta()
    {
        if (!_isCanMove) return;

        Vector2 look = _look.ReadValue<Vector2>() / 10f * _mouseSens;
        _cameraLook.x += look.x;
        _cameraLook.y -= look.y;
    }

    void GetLookingObj()
    {
        _lookingAt = null;

        Ray ray = new Ray(_camera.transform.position, _camera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, 3f))
        {
            var collider = hit.collider;
            if (collider == null || collider.attachedRigidbody == null) return;
            _lookingAt = collider;
        }
    }

    void InteractionSystem()
    {
        if (_interact.WasPressedThisFrame() && _lookingAt != null)
        {
            _onInteract.Invoke(_lookingAt);
        }
    }

}
