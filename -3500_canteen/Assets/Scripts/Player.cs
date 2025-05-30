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
        // ��ȡ�����
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("�����δ�ҵ���");
            return;
        }

        // �����ͨ�����λ�÷�������
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        float interactDis = 1000f;

        // �������߼��
        if (Physics.Raycast(ray, out RaycastHit raycastHit, interactDis, counterLayerMask))
        {
            
            // ���Ի�ȡ���������BaseCounter���
            if (raycastHit.transform.TryGetComponent(out BaseCounter baseCounter))
            {
                Debug.LogWarning("���е�������BaseCounter�����");
                // ���BaseCounter�����Ϊ��ǰѡ�е�
                if (baseCounter != selectedCounter)
                {
                    SetSelectedCounter(baseCounter);
                }
            }
            else
            {
                // ���е�����û��BaseCounter���������Ϊnull
                SetSelectedCounter(null);
                Debug.LogWarning("���е�����û��BaseCounter�����");
            }
        }
        else
        {
            // ����δ�����κ����壬����Ϊnull
            SetSelectedCounter(null);
            Debug.LogWarning("����δ�����κ����壡");
        }
    }

    private void HandleMovement()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("�����δ�ҵ���");
            return;
        }
        Debug.LogWarning("yi��");
        Ray Mouse = mainCamera.ScreenPointToRay(Input.mousePosition);
        //Ray mouseRay = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(Mouse, out RaycastHit hitInfo))
        {
            // ------------------- �޸Ĳ��� -------------------
            // ��ȡ���λ��
            Vector3 cameraPos = mainCamera.transform.position;
            // �����������ײ�������
            Vector3 direction = hitInfo.point - cameraPos;
            // ���ÿ�������ı�����0.5��ʾ�м�λ�ã�0-1֮�������ԽСԽ���������
            float cameraProximity = 0.9f;
            // ��ֵ������λ��
            Vector3 newPosition = cameraPos + direction * cameraProximity;
            // ------------------- �޸Ĳ��� -------------------

            transform.position = newPosition; // Ӧ����λ��
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
