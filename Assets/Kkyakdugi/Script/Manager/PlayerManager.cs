using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 게임 내 Player 인스턴스를 등록·관리하는 싱글턴 매니저.
/// 외부 코드는 이 클래스를 통해 플레이어 상태를 조회·제어합니다.
/// </summary>
public class PlayerManager : MonoBehaviour
{
    // ──────────────────────────────────────────────
    // Singleton
    // ──────────────────────────────────────────────
    public static PlayerManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    // ──────────────────────────────────────────────
    // Internal Storage
    // ──────────────────────────────────────────────
    private readonly Dictionary<string, Player> _players = new();
    private Player _activePlayer;

    // ──────────────────────────────────────────────
    // Events
    // ──────────────────────────────────────────────
    /// <summary>새 Player가 등록될 때 (id, player)</summary>
    public event System.Action<string, Player> OnPlayerRegistered;

    /// <summary>Player가 해제될 때 (id)</summary>
    public event System.Action<string> OnPlayerUnregistered;

    /// <summary>활성 Player가 교체될 때</summary>
    public event System.Action<Player> OnActivePlayerChanged;

    // ──────────────────────────────────────────────
    // Properties
    // ──────────────────────────────────────────────
    /// <summary>현재 활성 플레이어 (읽기 전용)</summary>
    public Player ActivePlayer => _activePlayer;

    /// <summary>등록된 모든 플레이어 목록 (읽기 전용)</summary>
    public IReadOnlyDictionary<string, Player> Players => _players;

    // ──────────────────────────────────────────────
    // Registration
    // ──────────────────────────────────────────────

    /// <summary>
    /// Player를 ID로 등록합니다.
    /// 처음 등록된 Player는 자동으로 ActivePlayer가 됩니다.
    /// </summary>
    public void RegisterPlayer(string id, Player player)
    {
        if (player == null)
        {
            Debug.LogWarning($"[PlayerManager] null Player 등록 시도 (id: {id})");
            return;
        }

        if (_players.ContainsKey(id))
            Debug.LogWarning($"[PlayerManager] 이미 등록된 ID 덮어쓰기: {id}");

        player.Initialize(id);
        _players[id] = player;
        //Debug.Log($"[PlayerManager] Player 등록 완료: {id}");
        OnPlayerRegistered?.Invoke(id, player);

        if (_activePlayer == null)
            SetActivePlayer(id);
    }

    /// <summary>
    /// 등록된 Player를 해제합니다.
    /// 활성 플레이어가 해제되면 남은 목록에서 첫 번째를 활성으로 교체합니다.
    /// </summary>
    public void UnregisterPlayer(string id)
    {
        if (!_players.ContainsKey(id))
        {
            Debug.LogWarning($"[PlayerManager] 등록되지 않은 ID: {id}");
            return;
        }

        bool wasActive = _activePlayer == _players[id];
        _players.Remove(id);
        OnPlayerUnregistered?.Invoke(id);
        //Debug.Log($"[PlayerManager] Player 해제: {id}");

        if (wasActive)
        {
            _activePlayer = null;
            foreach (var kv in _players)
            {
                SetActivePlayer(kv.Key);
                break;
            }
        }
    }

    /// <summary>활성 Player를 ID로 교체합니다.</summary>
    public void SetActivePlayer(string id)
    {
        if (!_players.TryGetValue(id, out var player))
        {
            Debug.LogWarning($"[PlayerManager] ID 없음: {id}");
            return;
        }
        _activePlayer = player;
        //Debug.Log($"[PlayerManager] ActivePlayer 변경: {id}");
        OnActivePlayerChanged?.Invoke(_activePlayer);
    }

    /// <summary>ID로 Player 인스턴스를 반환합니다. 없으면 null.</summary>
    public Player GetPlayer(string id) =>
        _players.TryGetValue(id, out var p) ? p : null;

    // ──────────────────────────────────────────────
    // Active Player — State Query
    // ──────────────────────────────────────────────
    public Player.PlayerState GetState()    => _activePlayer != null ? _activePlayer.State    : Player.PlayerState.Idle;
    public bool               IsGrounded()  => _activePlayer?.IsGrounded  ?? false;
    public Vector2            GetVelocity() => _activePlayer?.Velocity    ?? Vector2.zero;

    // ──────────────────────────────────────────────
    // Active Player — Control
    // ──────────────────────────────────────────────

    /// <summary>활성 플레이어를 지정 위치로 순간이동합니다.</summary>
    public void SetPosition(Vector2 position)        => _activePlayer?.SetPosition(position);

    /// <summary>활성 플레이어의 속도를 직접 지정합니다.</summary>
    public void SetVelocity(Vector2 velocity)        => _activePlayer?.SetVelocity(velocity);

    /// <summary>활성 플레이어를 강제 점프시킵니다.</summary>
    public void ForceJump()                          => _activePlayer?.ForceJump();

    /// <summary>활성 플레이어의 입력을 켜거나 끕니다.</summary>
    public void SetInputEnabled(bool enabled)        => _activePlayer?.SetInputEnabled(enabled);

    /// <summary>활성 플레이어의 이동 속도를 변경합니다.</summary>
    public void SetMoveSpeed(float speed)
    {
        if (_activePlayer != null) _activePlayer.MoveSpeed = speed;
    }

    /// <summary>활성 플레이어의 점프력을 변경합니다.</summary>
    public void SetJumpForce(float force)
    {
        if (_activePlayer != null) _activePlayer.JumpForce = force;
    }

    /// <summary>활성 플레이어의 중력 스케일을 변경합니다.</summary>
    public void SetGravityScale(float scale)         => _activePlayer?.SetGravityScale(scale);

    // ──────────────────────────────────────────────
    // Specific Player — Control  (id 지정 버전)
    // ──────────────────────────────────────────────

    /// <summary>특정 ID의 플레이어 입력을 켜거나 끕니다.</summary>
    public void SetInputEnabled(string id, bool enabled) => GetPlayer(id)?.SetInputEnabled(enabled);

    /// <summary>특정 ID의 플레이어를 이동시킵니다.</summary>
    public void SetPosition(string id, Vector2 position) => GetPlayer(id)?.SetPosition(position);
}
