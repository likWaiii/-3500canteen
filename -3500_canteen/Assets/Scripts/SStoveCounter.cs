using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using static CuttingCounter;

public class SStoveCounter : BaseCounter, IHasProgress
{
    public event EventHandler<IHasProgress.OnProgressChangedEventArgs> OnProgressChanged;

    public event EventHandler<OnStateChangedEventArgs> OnStateChanged;

    public class OnStateChangedEventArgs : EventArgs
    {
        public State state;
    }

    public enum State
    {
        Idle,
        Frying1,
        Frying2,
        Fried,
        Burned,
    }

    private State state;

    [SerializeField]
    private FryingReciepeSO[] fryingReciepeSOArray;

    [SerializeField]
    private BurningReciepeSO[] burningReciepeSOArray;

    [SerializeField]
    private Frying2ReciepeSO[] frying2ReciepeSOArray;
    private float fryingTimer;
    private float burnedTimer;
    private FryingReciepeSO fryingReciepeSO;
    private BurningReciepeSO burningReciepeSO;
    private Frying2ReciepeSO frying2ReciepeSO;

    // ��Ҫ��ӵ����ֶ�

    // �����������Ʊ���
    private bool isFlipping = false;
    private float flipStartTime;
    private const float FLIP_DURATION = 0.6f; // ��������ʱ��
    private Vector3 originalPosition; // ��¼����ǰ��λ��
    private Quaternion originalRotation; // ��¼����ǰ����ת

    private void Start()
    {
        state = State.Idle;
    }

    private bool hasTurned = false; // ���ڼ�¼�Ƿ��Ѿ�������

    private void Update()
    {
        if (HasKitchenObject())
        {
            // Debug.LogWarning("1��");
            switch (state)
            {
                case State.Idle:
                    break;

                case State.Frying1:
                    fryingTimer += Time.deltaTime;

                    OnProgressChanged?.Invoke(
                        this,
                        new IHasProgress.OnProgressChangedEventArgs
                        {
                            progressNormalized = fryingTimer / fryingReciepeSO.fryingTimerMax,
                        }
                    );

                    if (fryingTimer > fryingReciepeSO.fryingTimerMax)
                    {
                        // Frying2
                        GetKitchenObject().DestroySelf();
                        KitchenObject.SpwanKitchenObject(fryingReciepeSO.output, this);
                        frying2ReciepeSO = GetFrying2ReciepeSOWithInput(
                            GetKitchenObject().GetKitchenObjectOS()
                        );
                        if (!frying2ReciepeSO || frying2ReciepeSO.middle == null )
                        {
                            // ���û���м�״̬��ֱ�ӽ���Fried״̬
                            state = State.Fried;
                            burningReciepeSO = GetBurningSOWithInput(GetKitchenObject().GetKitchenObjectOS());
                            burnedTimer = 0f;
                            OnStateChanged?.Invoke(this, new OnStateChangedEventArgs { state = state });
                        }
                        else
                        {
                            state = State.Frying2;
                            fryingTimer = 0f;
                            OnStateChanged?.Invoke(this, new OnStateChangedEventArgs { state = state });
                            OnProgressChanged?.Invoke(
                                this,
                                new IHasProgress.OnProgressChangedEventArgs { progressNormalized = 0f }
                            );
                        }
                    }
                    break;

                case State.Frying2:

                    if (!isFlipping)
                    {
                        fryingTimer += Time.deltaTime;

                        OnProgressChanged?.Invoke(
                            this,
                            e: new IHasProgress.OnProgressChangedEventArgs
                            {
                                progressNormalized = fryingTimer / frying2ReciepeSO.fryingTimerMax,
                            }
                        );
                    }

                    if (fryingTimer > frying2ReciepeSO.fryingTimerMax)
                    {
                        // Burned
                        GetKitchenObject().DestroySelf();
                        KitchenObject.SpwanKitchenObject(frying2ReciepeSO.output, this);
                        state = State.Burned;

                        OnStateChanged?.Invoke(this, new OnStateChangedEventArgs { state = state });
                        OnProgressChanged?.Invoke(
                            this,
                            new IHasProgress.OnProgressChangedEventArgs { progressNormalized = 0f }
                        );
                    }
                    
                    // �����������Ҽ������߼�
                    if (Input.GetMouseButtonDown(1) && !hasTurned && !isFlipping)
                    {
                        Debug.Log("Get in");
                        hasTurned = true;
                        StartFlipAnimation();
                        //GetKitchenObject().DestroySelf();
                        //KitchenObject.SpwanKitchenObject(frying2ReciepeSO.middle, this);

                        //burnedTimer = 0f;
                        //burningReciepeSO = GetBurningSOWithInput(GetKitchenObject().GetKitchenObjectOS());
                        //state = State.Fried;

                        //OnStateChanged?.Invoke(this, new OnStateChangedEventArgs { state = state });

                        //Debug.Log("ʳ���ѷ��棬����Fried״̬");
                    }

                    // ���·��涯��
                    if (isFlipping)
                    {
                        UpdateFlipAnimation();
                    }

                    break;

                case State.Fried:

                    burnedTimer += Time.deltaTime;

                    OnProgressChanged?.Invoke(
                        this,
                        new IHasProgress.OnProgressChangedEventArgs
                        {
                            progressNormalized = burnedTimer / burningReciepeSO.burningTimerMax,
                        }
                    );

                    if (burnedTimer > burningReciepeSO.burningTimerMax)
                    {
                        // Burned
                        GetKitchenObject().DestroySelf();
                        KitchenObject.SpwanKitchenObject(burningReciepeSO.output, this);
                        state = State.Burned;

                        OnStateChanged?.Invoke(this, new OnStateChangedEventArgs { state = state });

                        OnProgressChanged?.Invoke(
                            this,
                            new IHasProgress.OnProgressChangedEventArgs { progressNormalized = 0f }
                        );
                    }
                    break;

                case State.Burned:
                    break;
            }
        }
    }

    public override void Interact(Player player)
    {
        if (!HasKitchenObject())
        {
            //Theree is no KitchenObject
            if (player.HasKitchenObject())
            {
                //Player is Carrying Something

                if (HasRecipeWithInput(player.GetKitchenObject().GetKitchenObjectOS()))
                {
                    //player carring something that can be fried
                    player.GetKitchenObject().SetKitchenObjectParent(this);

                    fryingReciepeSO = GetFryingSOWithInput(GetKitchenObject().GetKitchenObjectOS());

                    state = State.Frying1;
                    fryingTimer = 0f;

                    OnStateChanged?.Invoke(this, new OnStateChangedEventArgs { state = state });

                    OnProgressChanged?.Invoke(
                        this,
                        new IHasProgress.OnProgressChangedEventArgs
                        {
                            progressNormalized = fryingTimer / fryingReciepeSO.fryingTimerMax,
                        }
                    );
                }
            }
            else
            {
                //Payer not carring nothing
            }
        }
        else
        {
            //Theree is a KitchenObject
            if (player.HasKitchenObject())
            {
                //Player is carring something
                if (
                    player.GetKitchenObject().TryGetPlate(out PlateKitchenObject plateKitchenObject)
                )
                {
                    //Player is holding a plate
                    if (
                        plateKitchenObject.TryAddIngredient(GetKitchenObject().GetKitchenObjectOS())
                    )
                    {
                        GetKitchenObject().DestroySelf();

                        state = State.Idle;

                        OnStateChanged?.Invoke(this, new OnStateChangedEventArgs { state = state });

                        OnProgressChanged?.Invoke(
                            this,
                            new IHasProgress.OnProgressChangedEventArgs { progressNormalized = 0f }
                        );
                    }
                }
            }
            else
            {
                //Player is not carring something
                GetKitchenObject().SetKitchenObjectParent(player);

                state = State.Idle;

                OnStateChanged?.Invoke(this, new OnStateChangedEventArgs { state = state });

                OnProgressChanged?.Invoke(
                    this,
                    new IHasProgress.OnProgressChangedEventArgs { progressNormalized = 0f }
                );
            }
        }
    }

    private bool HasRecipeWithInput(KitchenObjectOS inputKitchenObjectOS)
    {
        FryingReciepeSO fryingReciepeSO = GetFryingSOWithInput(inputKitchenObjectOS);
        return fryingReciepeSO != null;
    }

    private Frying2ReciepeSO GetFrying2ReciepeSOWithInput(KitchenObjectOS inputKitchenObjectOS)
    {
        foreach (Frying2ReciepeSO frying2RecipeSO in frying2ReciepeSOArray)
        {
            if (frying2RecipeSO.input == inputKitchenObjectOS)
            {
                return frying2RecipeSO;
            }
        }

        return null;
    }

    private FryingReciepeSO GetFryingSOWithInput(KitchenObjectOS inputKitchenObjectOS)
    {
        foreach (FryingReciepeSO fryingRecipeSO in fryingReciepeSOArray)
        {
            if (fryingRecipeSO.input == inputKitchenObjectOS)
            {
                return fryingRecipeSO;
            }
        }

        return null;
    }

    private BurningReciepeSO GetBurningSOWithInput(KitchenObjectOS inputKitchenObjectOS)
    {
        Debug.Log("GetBurningSOWithInput called with input: " + inputKitchenObjectOS);
        foreach (BurningReciepeSO burningRecipeSO in burningReciepeSOArray)
        {
            if (burningRecipeSO.input == inputKitchenObjectOS)
            {
                return burningRecipeSO;
            }
        }

        return null;
    }

    public bool IsFried()
    {
        return state == State.Fried;
    }

    public bool IsFrying2()
    {
        return state == State.Frying2;
    }

    public bool HasTurned()
    {
        return hasTurned;
    }

    // ��������ʼ���涯��
    private void StartFlipAnimation()
    {
        Debug.Log("��ʼ���涯��");
        isFlipping = true;
        hasTurned = true;
        flipStartTime = Time.time;

        // ���浱ǰλ�ú���ת
        KitchenObject ko = GetKitchenObject();
        originalPosition = ko.transform.position;
        originalRotation = ko.transform.rotation;
    }

    // ���������·��涯��
    private void UpdateFlipAnimation()
    {
        float elapsed = Time.time - flipStartTime;
        float progress = Mathf.Clamp01(elapsed / FLIP_DURATION);

        // ������Ծ���ߣ����������½�
        float height = Mathf.Sin(progress * Mathf.PI) * 0.6f; // ��Ծ�߶�

        // ������ת��0-180��
        float rotation = Mathf.Lerp(0f, 180f, progress);

        // Ӧ��λ�ú���ת�仯
        KitchenObject ko = GetKitchenObject();
        ko.transform.position = originalPosition + Vector3.up * height;
        ko.transform.rotation = originalRotation * Quaternion.Euler(0, 0, rotation);

        // ����������Ĵ���
        if (progress >= 1f)
        {
            CompleteFlip();
        }
    }

    // ��������ɷ������߼�
    private void CompleteFlip()
    {
        Debug.Log("��ɷ���");
        isFlipping = false;

        // ����λ�ú���ת
        KitchenObject ko = GetKitchenObject();
        ko.transform.position = originalPosition;
        ko.transform.rotation = originalRotation * Quaternion.Euler(180f, 0, 0);

        GetKitchenObject().DestroySelf();
        KitchenObject.SpwanKitchenObject(frying2ReciepeSO.middle, this);

        burnedTimer = 0f;
        burningReciepeSO = GetBurningSOWithInput(GetKitchenObject().GetKitchenObjectOS());
        state = State.Fried;

        OnStateChanged?.Invoke(this, new OnStateChangedEventArgs { state = state });

        flipStartTime = 0f;
        hasTurned = false; // ���÷���״̬
        isFlipping = false; // ���÷��涯��״̬
        Debug.Log("ʳ���ѷ��棬����Fried״̬");
    }
}
