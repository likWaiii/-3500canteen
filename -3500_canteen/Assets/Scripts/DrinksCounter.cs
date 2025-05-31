using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static SStoveCounter;
using static UnityEngine.CullingGroup;

public class DrinksCounter : BaseCounter, IHasProgress
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
        Pouring,
        Overflow,
    }

    private State state;

    [SerializeField]
    private DrinksRecipeSO drinksRecipeSO;

    private float pouringTimer;

    private KitchenObject cup;
    private void Update()
    {
        if (HasKitchenObject())
        {
            switch (state)
            {
                case State.Idle:
                    break;

                case State.Pouring:
                    pouringTimer += Time.deltaTime;

                    OnProgressChanged?.Invoke(
                        this,
                        new IHasProgress.OnProgressChangedEventArgs
                        {
                            progressNormalized = pouringTimer / drinksRecipeSO.pouringTimerMax,
                        }
                    );

                    if (pouringTimer > drinksRecipeSO.pouringTimerMax)
                    {
                        // Overflow
                        GetKitchenObject().DestroySelf();
                        state = State.Overflow;

                        KitchenObject.SpwanKitchenObject(drinksRecipeSO.output, this);

                        OnStateChanged?.Invoke(this, new OnStateChangedEventArgs { state = state });

                    }
                    break;

                case State.Overflow:
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
                    //player carring the cup
                    cup = player.GetKitchenObject();
                    Debug.Log("Player is carrying the cup: " + cup.name);

                    player.GetKitchenObject().SetKitchenObjectParent(this);

                    state = State.Pouring;
                    pouringTimer = 0f;

                    OnStateChanged?.Invoke(this, new OnStateChangedEventArgs { state = state });

                    OnProgressChanged?.Invoke(
                        this,
                        new IHasProgress.OnProgressChangedEventArgs
                        {
                            progressNormalized = pouringTimer / drinksRecipeSO.pouringTimerMax,
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
                //Can not interact with the counter if player is carrying something
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

    public KitchenObject getCup()
    {
        return cup;
    }

    private bool HasRecipeWithInput(KitchenObjectOS inputKitchenObjectOS)
    {
        return inputKitchenObjectOS == drinksRecipeSO.input;
    }

    public bool IsOverflow()
    {
        return state == State.Overflow;
    }
    public bool IsPouring()
    {
        return state == State.Pouring;
    }


}
