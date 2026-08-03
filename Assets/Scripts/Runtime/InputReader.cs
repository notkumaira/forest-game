using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// SYS-10.2 — ScriptableObject wrapper around PlayerInputActions.
///
/// Everything that needs input references this asset and subscribes to its events.
/// Nothing else in the project should touch PlayerInputActions directly.
///
/// Subscribe in OnEnable, unsubscribe in OnDisable. Always. A ScriptableObject outlives
/// the scene objects listening to it, so a missed unsubscribe keeps a destroyed
/// MonoBehaviour alive in the event's invocation list and throws on the next input.
/// </summary>
[CreateAssetMenu(fileName = "InputReader", menuName = "Game/Input Reader")]
public class InputReader : ScriptableObject,
    PlayerInputActions.IPlayerActions,
    PlayerInputActions.IUIActions,
    PlayerInputActions.IBattleActions
{
    // ---- Player -------------------------------------------------------------
    public event Action<Vector2> MoveEvent;
    public event Action InteractEvent;
    public event Action OpenInventoryEvent;
    public event Action PauseEvent;

    // ---- UI -----------------------------------------------------------------
    public event Action<Vector2> UINavigateEvent;
    public event Action UISubmitEvent;
    public event Action UICancelEvent;

    // ---- Battle -------------------------------------------------------------
    public event Action<Vector2> BattleNavigateEvent;
    public event Action BattleConfirmEvent;
    public event Action BattleCancelEvent;

    /// <summary>
    /// Last known Move value. PlayerController reads this in FixedUpdate rather than
    /// caching the event value itself — physics runs on its own clock and input events
    /// arrive on the frame clock.
    /// </summary>
    public Vector2 MoveValue { get; private set; }

    private PlayerInputActions _actions;

    /// <summary>The underlying asset, for anything that needs it (e.g. the UI input module).</summary>
    public InputActionAsset Asset
    {
        get
        {
            EnsureInitialized();
            return _actions.asset;
        }
    }

    private void OnEnable()
    {
        EnsureInitialized();

        // Exploration is the default state on load. GameStateManager (SYS-10.3) takes
        // over map switching once it exists.
        if (Application.isPlaying) EnablePlayer();
    }

    private void OnDisable()
    {
        if (_actions == null) return;
        DisableAll();

        // Deliberately not calling _actions.Dispose(). Dispose() calls Object.Destroy()
        // internally, and OnDisable also runs during editor domain reloads and asset
        // imports, where that throws. Unity reclaims the instance on reload anyway.
    }

    /// <summary>
    /// Creates the backing actions object if it doesn't exist yet.
    ///
    /// Called from everywhere rather than relying on OnEnable, because OnEnable fires
    /// when the asset *loads* — which may have been in edit mode, long before Play was
    /// pressed. With domain reload disabled, it never fires again, and the asset would
    /// sit there looking correctly wired up while silently doing nothing.
    /// </summary>
    private void EnsureInitialized()
    {
        if (_actions != null) return;

        _actions = new PlayerInputActions();
        _actions.Player.SetCallbacks(this);
        _actions.UI.SetCallbacks(this);
        _actions.Battle.SetCallbacks(this);
    }

    // ---- Map switching ------------------------------------------------------
    // Exactly one gameplay map is active at a time. Called by GameStateManager.

    public void EnablePlayer()
    {
        EnsureInitialized();
        _actions.Player.Enable();
        _actions.UI.Disable();
        _actions.Battle.Disable();
    }

    public void EnableUI()
    {
        EnsureInitialized();
        _actions.UI.Enable();
        _actions.Player.Disable();
        _actions.Battle.Disable();

        // Movement is stale the moment the map is disabled — the key-up never arrives,
        // so without this the player keeps walking after the inventory opens.
        ClearMove();
    }

    public void EnableBattle()
    {
        EnsureInitialized();
        _actions.Battle.Enable();
        _actions.Player.Disable();
        _actions.UI.Disable();

        ClearMove();
    }

    public void DisableAll()
    {
        if (_actions == null) return;
        _actions.Player.Disable();
        _actions.UI.Disable();
        _actions.Battle.Disable();
    }

    private void ClearMove()
    {
        MoveValue = Vector2.zero;
        MoveEvent?.Invoke(Vector2.zero);
    }

    // ---- Player callbacks ---------------------------------------------------

    public void OnMove(InputAction.CallbackContext context)
    {
        // Fires on both performed and canceled; canceled delivers Vector2.zero,
        // which is what stops the player.
        MoveValue = context.ReadValue<Vector2>();
        MoveEvent?.Invoke(MoveValue);
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
            InteractEvent?.Invoke();
    }

    public void OnOpenInventory(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
            OpenInventoryEvent?.Invoke();
    }

    public void OnPause(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
            PauseEvent?.Invoke();
    }

    // ---- UI callbacks -------------------------------------------------------
    // EventSystem navigation is handled by InputSystemUIInputModule, not here.
    // These exist for game logic that cares about UI input — closing a panel on
    // Cancel, for instance.

    public void OnNavigate(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
            UINavigateEvent?.Invoke(context.ReadValue<Vector2>());
    }

    public void OnSubmit(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
            UISubmitEvent?.Invoke();
    }

    public void OnCancel(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
            UICancelEvent?.Invoke();
    }

    // Unused UI actions from the template. The interface requires them; leave empty.
    public void OnPoint(InputAction.CallbackContext context) { }
    public void OnClick(InputAction.CallbackContext context) { }
    public void OnRightClick(InputAction.CallbackContext context) { }
    public void OnMiddleClick(InputAction.CallbackContext context) { }
    public void OnScrollWheel(InputAction.CallbackContext context) { }
    public void OnTrackedDevicePosition(InputAction.CallbackContext context) { }
    public void OnTrackedDeviceOrientation(InputAction.CallbackContext context) { }

    // ---- Battle callbacks ---------------------------------------------------

    public void OnBattleNavigate(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
            BattleNavigateEvent?.Invoke(context.ReadValue<Vector2>());
    }

    public void OnConfirm(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
            BattleConfirmEvent?.Invoke();
    }

    public void OnBattleCancel(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
            BattleCancelEvent?.Invoke();
    }
}