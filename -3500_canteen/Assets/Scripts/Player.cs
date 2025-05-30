using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour, IKithenObjectParent
{
    public static Player Instance { get; private set; }

    public event EventHandler OnPickedSomething;

    public event EventHandler<OnSelectedCounterChangedEventArgs> OnSelectedCounterChanged;

    public class OnSelectedCounterChangedEventArgs : EventArgs
    {
        public BaseCounter selectedCounter;
    }

    [SerializeField]
    private float speed = 7f;

    [SerializeField]
    private GameInput gameInput;

    [SerializeField]
    private LayerMask counterLayerMask;

    [SerializeField]
    private Transform kitchenObjectHoldPoint;

    private bool isWalking;
    private bool canMove;
    private Vector3 lastInteractDir;
    private BaseCounter selectedCounter;
    private KitchenObject kitchenObject;

    
    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("Something Went Wrong there are more than one player");
        }
        Instance = this;
    }

    private void Start()
    {
        gameInput.OnInteractActions += GameInput_OnInteractActions;
        gameInput.OnInteractAlternateActions += GameInput_OnInteractAlternateActions;
    }

    private void GameInput_OnInteractAlternateActions(object sender, EventArgs e)
    {
        if (!KitchenGameManager.Instance.IsGamePlaying())
            return;
        if (selectedCounter != null)
        {
            selectedCounter.InteractAlternate(this);
        }
    }

    private void GameInput_OnInteractActions(object sender, System.EventArgs e)
    {
        if (!KitchenGameManager.Instance.IsGamePlaying())
            return;
        if (selectedCounter != null)
        {
            selectedCounter.Interact(this);
        }
    }

    private void Update()
    {
        HandleMovement();
        HandleInteractions();
    }

    public bool IsWalking()
    {
        return isWalking;
    }

    private void HandleInteractions()
    {
        // 获取主相机
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("主相机未找到！");
            return;
        }

        // 从相机通过鼠标位置发射射线
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        float interactDis = 1000f;

        // 进行射线检测
        if (Physics.Raycast(ray, out RaycastHit raycastHit, interactDis, counterLayerMask))
        {
            
            // 尝试获取击中物体的BaseCounter组件
            if (raycastHit.transform.TryGetComponent(out BaseCounter baseCounter))
            {
                Debug.LogWarning("击中的物体有BaseCounter组件！");
                // 如果BaseCounter组件不为当前选中的
                if (baseCounter != selectedCounter)
                {
                    SetSelectedCounter(baseCounter);
                }
            }
            else
            {
                // 击中的物体没有BaseCounter组件，设置为null
                SetSelectedCounter(null);
                Debug.LogWarning("击中的物体没有BaseCounter组件！");
            }
        }
        else
        {
            // 射线未击中任何物体，设置为null
            SetSelectedCounter(null);
            Debug.LogWarning("射线未击中任何物体！");
        }
    }

    private void HandleMovement()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("主相机未找到！");
            return;
        }
        Debug.LogWarning("yi！");
        Ray Mouse = mainCamera.ScreenPointToRay(Input.mousePosition);
        //Ray mouseRay = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(Mouse, out RaycastHit hitInfo))
        {
            // ------------------- 修改部分 -------------------
            // 获取相机位置
            Vector3 cameraPos = mainCamera.transform.position;
            // 计算相机到碰撞点的向量
            Vector3 direction = hitInfo.point - cameraPos;
            // 设置靠近相机的比例（0.5表示中间位置，0-1之间调整，越小越靠近相机）
            float cameraProximity = 0.9f;
            // 插值计算新位置
            Vector3 newPosition = cameraPos + direction * cameraProximity;
            // ------------------- 修改部分 -------------------

            transform.position = newPosition; // 应用新位置
            isWalking = true;
        }
        else
        {
            isWalking = false;
        }
    }

    private void SetSelectedCounter(BaseCounter selectedCounter)
    {
        this.selectedCounter = selectedCounter;

        OnSelectedCounterChanged?.Invoke(
            this,
            new OnSelectedCounterChangedEventArgs { selectedCounter = selectedCounter }
        );
    }

    // Interfaces functions
    public Transform GetKitchenObjectFollowTransform()
    {
        return kitchenObjectHoldPoint;
    }

    public void SetKitchenObject(KitchenObject kitchenObject)
    {
        this.kitchenObject = kitchenObject;

        if (kitchenObject != null)
        {
            OnPickedSomething?.Invoke(this, EventArgs.Empty);
        }
    }

    public KitchenObject GetKitchenObject()
    {
        return kitchenObject;
    }

    public void ClearKitchenObject()
    {
        kitchenObject = null;
    }

    public bool HasKitchenObject()
    {
        return kitchenObject != null;
    }
}
