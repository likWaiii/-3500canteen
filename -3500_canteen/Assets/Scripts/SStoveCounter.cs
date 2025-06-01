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
    private float frying2Timer = 0f;
    private FryingReciepeSO fryingReciepeSO;
    private BurningReciepeSO burningReciepeSO;
    private Frying2ReciepeSO frying2ReciepeSO;

    // 需要添加的新字段


    private void Start()
    {
        state = State.Idle;
    }

    private bool hasTurned = false; // 用于记录是否已经翻过面

    private void Update()
    {
        if (HasKitchenObject())
        {
            // Debug.LogWarning("1！");
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
                            // 如果没有中间状态，直接进入Fried状态
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
                    fryingTimer += Time.deltaTime;

                    OnProgressChanged?.Invoke(
                        this,
                        e: new IHasProgress.OnProgressChangedEventArgs
                        {
                            progressNormalized = fryingTimer / frying2ReciepeSO.fryingTimerMax,
                        }
                    );

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
                    
                    Debug.Log("Button pressed: " + Input.GetMouseButtonDown(1) + ", hasTurned: " + hasTurned);

                    // 新增：处理右键翻面逻辑
                    if (Input.GetMouseButtonDown(1) && !hasTurned)
                    {
                        Debug.Log("Get in");
                        hasTurned = true;
                        GetKitchenObject().DestroySelf();
                        KitchenObject.SpwanKitchenObject(frying2ReciepeSO.middle, this);

                        burnedTimer = 0f;
                        burningReciepeSO = GetBurningSOWithInput(GetKitchenObject().GetKitchenObjectOS());
                        state = State.Fried;

                        OnStateChanged?.Invoke(this, new OnStateChangedEventArgs { state = state });

                        Debug.Log("食物已翻面，进入Fried状态");
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
}
