using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static SStoveCounter;
using static UnityEngine.CullingGroup;

public class PotCounter : BaseCounter, IHasProgress
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
        Boiling,
        Boiled,
        Burned,
    }

    private State state;

    [SerializeField]
    private BoilingReciepeSO[] boilKitchenObjectOSArray;

    [SerializeField]
    private BurningReciepeSO[] burningReciepeSOArray;

    private int cnt = 0;
    private KitchenObjectOS[] inputKitchenObjectSOList;
    private BoilingReciepeSO boilingReciepeSO;
    private BurningReciepeSO burningReciepeSO;

    private Dictionary<BoilingReciepeSO, Dictionary<KitchenObjectOS, bool>> isObjGiven;

    private float boilingTimer;
    private float burnedTimer;

    private void Awake()
    {
        Clear();
    }

    private void Clear()
    {
        boilingReciepeSO = null;
        inputKitchenObjectSOList = null;
        cnt = 0;
        isObjGiven = null;
        inputKitchenObjectSOList = new KitchenObjectOS[0];
        foreach (BoilingReciepeSO _boilingReciepeSO in boilKitchenObjectOSArray)
        {
            foreach (KitchenObjectOS kitchenObjectOS in _boilingReciepeSO.input)
            {
                if (isObjGiven == null)
                {
                    isObjGiven =
                        new Dictionary<BoilingReciepeSO, Dictionary<KitchenObjectOS, bool>>();
                }
                if (!isObjGiven.ContainsKey(_boilingReciepeSO))
                {
                    isObjGiven.Add(_boilingReciepeSO, new Dictionary<KitchenObjectOS, bool>());
                }
                if (!isObjGiven[_boilingReciepeSO].ContainsKey(kitchenObjectOS))
                {
                    isObjGiven[_boilingReciepeSO].Add(kitchenObjectOS, false);
                }
            }
        }
    }

    private void Update()
    {
        if (HasKitchenObject())
        {
            switch (state)
            {
                case State.Idle:
                    break;

                case State.Boiling:
                    Debug.LogWarning("1！");
                    boilingTimer += Time.deltaTime;

                    OnProgressChanged?.Invoke(
                        this,
                        new IHasProgress.OnProgressChangedEventArgs
                        {
                            progressNormalized = boilingTimer / boilingReciepeSO.boilingTimerMax,
                        }
                    );

                    if (boilingTimer > boilingReciepeSO.boilingTimerMax)
                    {
                        // Boiled
                        GetKitchenObject().DestroySelf();
                        KitchenObject.SpwanKitchenObject(boilingReciepeSO.output, this);
                        state = State.Boiled;
                        burnedTimer = 0f;
                        burningReciepeSO = GetBurningSOWithInput(
                            GetKitchenObject().GetKitchenObjectOS()
                        );

                        OnStateChanged?.Invoke(this, new OnStateChangedEventArgs { state = state });

                        Clear();
                    }
                    break;

                case State.Boiled:
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
        if (state == State.Idle)
        {
            //Theree is no KitchenObject
            if (player.HasKitchenObject())
            {
                //Player is Carrying Something
                Debug.Log(player.GetKitchenObject().name);
                if (HasRecipeWithInput(player.GetKitchenObject().GetKitchenObjectOS()))
                {
                    //player carring something that can be boil
                    player.GetKitchenObject().SetKitchenObjectParent(this);

                    BoilingReciepeSO _boilingReciepeSO = GetBoilingSOWithInput(
                        GetKitchenObject().GetKitchenObjectOS()
                    );

                    if (boilingReciepeSO == null || cnt < boilingReciepeSO.input.Length)
                    {
                        GetKitchenObject().DestroySelf();
                        return;
                    }

                    state = State.Boiling;
                    boilingTimer = 0f;

                    OnStateChanged?.Invoke(this, new OnStateChangedEventArgs { state = state });

                    OnProgressChanged?.Invoke(
                        this,
                        new IHasProgress.OnProgressChangedEventArgs
                        {
                            progressNormalized = boilingTimer / boilingReciepeSO.boilingTimerMax,
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
            //There is a KitchenObject
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
        return GetBoilingSOWithInput(inputKitchenObjectOS) != null;
    }

    private KitchenObjectOS GetOutputForInput(KitchenObjectOS inputKitchenObjectOS)
    {
        BoilingReciepeSO boilingReciepeSO = GetBoilingSOWithInput(inputKitchenObjectOS);
        if (boilingReciepeSO != null)
        {
            return boilingReciepeSO.output;
        }
        else
        {
            return null;
        }
    }

    private BoilingReciepeSO GetBoilingSOWithInput(KitchenObjectOS inputKitchenObjectOS)
    {
        // 已有唯一菜谱，判断加入食材是否存在并添加
        if (boilingReciepeSO != null)
        {
            if (
                isObjGiven[boilingReciepeSO].ContainsKey(inputKitchenObjectOS)
                && isObjGiven[boilingReciepeSO][inputKitchenObjectOS] == false
            )
            {
                isObjGiven[boilingReciepeSO][inputKitchenObjectOS] = true;
                cnt++;
                inputKitchenObjectSOList.Append(inputKitchenObjectOS);
            }
            if (!isObjGiven[boilingReciepeSO].ContainsKey(inputKitchenObjectOS))
            {
                return null;
            }
            return boilingReciepeSO;
        }
        // 没有唯一菜谱，判断此次加入食材是否能与已有食材组成菜谱
        int candidateCount = 0;
        foreach (BoilingReciepeSO _boilingReciepeSO in boilKitchenObjectOSArray)
        {
            bool flag = true;
            foreach (KitchenObjectOS inputKitchenObjectOS_Aready in inputKitchenObjectSOList)
            {
                Debug.Log(inputKitchenObjectOS_Aready.name);
                if (!isObjGiven[_boilingReciepeSO].ContainsKey(inputKitchenObjectOS_Aready))
                {
                    flag = false;
                    break;
                }
            }
            if (flag && isObjGiven[_boilingReciepeSO].ContainsKey(inputKitchenObjectOS))
            {
                isObjGiven[_boilingReciepeSO][inputKitchenObjectOS] = true;
                cnt++;
                inputKitchenObjectSOList.Append(inputKitchenObjectOS);
                boilingReciepeSO = _boilingReciepeSO;
                candidateCount++;
            }
        }
        if (candidateCount >= 1)
        {
            foreach (BoilingReciepeSO _boilingReciepeSO in boilKitchenObjectOSArray)
            {
                if (isObjGiven[_boilingReciepeSO].ContainsKey(inputKitchenObjectOS))
                {
                    isObjGiven[_boilingReciepeSO][inputKitchenObjectOS] = true;
                }
            }
            BoilingReciepeSO tmp = boilingReciepeSO;
            if (candidateCount > 1)
            {
                Debug.Log("有多个菜谱符合条件");
                boilingReciepeSO = null;
            }
            else
            {
                Debug.Log("找到唯一菜谱符合条件");
            }
            return tmp;
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

    public bool IsBoiled()
    {
        return state == State.Boiled;
    }
}
