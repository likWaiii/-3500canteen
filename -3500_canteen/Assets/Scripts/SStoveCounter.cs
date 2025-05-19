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
    private Frying2ReciepeSO[] Frying2ReciepeSOArray;
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

    private bool HasTurned = false; // 用于记录是否已经翻过面

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
                        // Fried
                        GetKitchenObject().DestroySelf();
                        KitchenObject.SpwanKitchenObject(fryingReciepeSO.output, this);
                        state = State.Fried;
                        burnedTimer = 0f;
                        burningReciepeSO = GetBurningSOWithInput(
                            GetKitchenObject().GetKitchenObjectOS()
                        );

                        OnStateChanged?.Invoke(this, new OnStateChangedEventArgs { state = state });
                    }
                    break;

                case State.Frying2:
                    frying2Timer += Time.deltaTime;

                    OnProgressChanged?.Invoke(
                        this,
                        e: new IHasProgress.OnProgressChangedEventArgs
                        {
                            progressNormalized = frying2Timer / frying2ReciepeSO.fryingTimerMax,
                        }
                    );

                    if (frying2Timer > frying2ReciepeSO.fryingTimerMax)
                    {
                        // Fried after turning
                        GetKitchenObject().DestroySelf();
                        KitchenObject.SpwanKitchenObject(frying2ReciepeSO.output, this);
                        state = State.Fried;
                        burnedTimer = 0f;
                        burningReciepeSO = GetBurningSOWithInput(
                            GetKitchenObject().GetKitchenObjectOS()
                        );

                        OnStateChanged?.Invoke(this, new OnStateChangedEventArgs { state = state });
                    }
                    break;

                case State.Fried:
                    // 新增：处理右键翻面逻辑
                    if (Input.GetMouseButtonDown(1) && !HasTurned)
                    {
                        // 获取翻面后的食谱数据（从Frying2ReciepeSOArray中）
                        KitchenObject currentObject = GetKitchenObject();
                        if (currentObject != null)
                        {
                            KitchenObjectOS currentObjectOS = currentObject.GetKitchenObjectOS();
                            frying2ReciepeSO = GetFrying2ReciepeSOWithInput(currentObjectOS);

                            if (frying2ReciepeSO != null)
                            {
                                // 正确使用frying2ReciepeSO
                                HasTurned = true;
                                frying2Timer = 0f;

                                // 销毁当前食物并生成翻面后的食物
                                GetKitchenObject().DestroySelf();
                                KitchenObject.SpwanKitchenObject(frying2ReciepeSO.output, this);

                                state = State.Frying2;

                                OnStateChanged?.Invoke(
                                    this,
                                    new OnStateChangedEventArgs { state = state }
                                );

                                OnProgressChanged?.Invoke(
                                    this,
                                    new IHasProgress.OnProgressChangedEventArgs
                                    {
                                        progressNormalized = 0f,
                                    }
                                );

                                Debug.Log("食物已翻面，进入Frying2状态");
                            }
                            else
                            {
                                Debug.LogError("找不到该食物的翻面食谱！");
                            }
                        }
                    }

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

    private KitchenObjectOS GetOutputForInput(KitchenObjectOS inputKitchenObjectOS)
    {
        FryingReciepeSO fryingReciepeSO = GetFryingSOWithInput(inputKitchenObjectOS);
        fryingReciepeSO = GetFryingSOWithInput(inputKitchenObjectOS);
        if (fryingReciepeSO != null)
        {
            return fryingReciepeSO.output;
        }
        else
        {
            return null;
        }
    }

    private KitchenObjectOS GetOutputForInput2(KitchenObjectOS inputKitchenObjectOS)
    {
        FryingReciepeSO fryingReciepeSO = GetFrying2SOWithInput(inputKitchenObjectOS);
        fryingReciepeSO = GetFrying2SOWithInput(inputKitchenObjectOS);
        if (frying2ReciepeSO != null)
        {
            return frying2ReciepeSO.output;
        }
        else
        {
            return null;
        }
    }
    private Frying2ReciepeSO GetFrying2ReciepeSOWithInput(KitchenObjectOS inputKitchenObjectOS)
    {
        foreach (Frying2ReciepeSO frying2RecipeSO in Frying2ReciepeSOArray)
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
    private FryingReciepeSO GetFrying2SOWithInput(KitchenObjectOS inputKitchenObjectOS)
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
}
