using FishNet.Connection;
using FishNet.Object;
using UnityEngine;
using static PatientInteractions;

// Fills nearby players' screens with pop-up ads. Spams faster as it loses patience, and floods everyone when it escapes.
public class PatientSpam : Patient
{
    [Header("Spam")]
    public float adRange = 12f;                            // only players this close get ads (escape burst ignores this)
    public Vector2 adIntervalRange = new Vector2(4f, 15f); // seconds between ads at 0 patience (x) and full patience (y)
    public float escapedAdInterval = 1.5f;
    public int escapeBurst = 8;

    private float adTimer;

    public override void OnStartServer()
    {
        base.OnStartServer();
        adTimer = adIntervalRange.y; // don't spam the moment the day starts
    }

    protected override void ServerPreUpdate()
    {
        adTimer -= Time.deltaTime;
        if (adTimer > 0f) return;

        SpawnAdsInRange(1);
        adTimer = IsEscaped
            ? escapedAdInterval
            : Mathf.Lerp(adIntervalRange.x, adIntervalRange.y, patience.Value / maxPatience);
    }

    protected override void OnEscaped()
    {
        SpawnAdsForAll(escapeBurst); // the one time range doesn't matter
    }

    // ─────────── sending ads ───────────

    [Server]
    private void SpawnAdsInRange(int count)
    {
        foreach (PlayerMovement p in GameManager.Instance.GetPlayers())
        {
            if (p.stats.isDead.Value) continue;
            if (Vector2.Distance(p.transform.position, transform.position) > adRange) continue;
            SpawnAdsTarget(p.Owner, count);
        }
    }

    [TargetRpc]
    private void SpawnAdsTarget(NetworkConnection conn, int count)
    {
        if (AdOverlay.Instance) AdOverlay.Instance.SpawnAds(count);
    }

    [ObserversRpc]
    private void SpawnAdsForAll(int count)
    {
        if (AdOverlay.Instance) AdOverlay.Instance.SpawnAds(count);
    }

    [ObserversRpc]
    private void ClearAdsForAll()
    {
        if (AdOverlay.Instance) AdOverlay.Instance.ClearAll();
    }

    // ─────────── outcomes ───────────

    // Numbers(...) plus ads for everyone in range
    private RollOutcome AdBurst(int ads, int credits, float patienceChange, int playerHealth) =>
        player =>
        {
            ApplyNumbers(player, credits, patienceChange, playerHealth);
            if (ads > 0) SpawnAdsInRange(ads);
        };

    // Best outcome: wipes every player's ads
    private RollOutcome ClearAndCalm(int credits, float patienceChange) =>
        player =>
        {
            ApplyNumbers(player, credits, patienceChange, 0);
            ClearAdsForAll();
        };

    protected override void DefineInteractions()
    {
        // AdBurst(ads, credits, patience, playerHealth), rolls 1 through 6
        DefineInteraction(Observe,       AdBurst(6, 10, -20, -10),  AdBurst(4, 10, -10, 0),   AdBurst(2, 10, -5, 0),    AdBurst(1, 15, 0, 0),   AdBurst(0, 15, 5, 0),  AdBurst(0, 20, 10, 0));
        DefineInteraction(Therapy,       AdBurst(5, 10, -15, -10),  AdBurst(3, 10, -5, 0),    AdBurst(1, 10, 5, 0),     AdBurst(0, 15, 15, 0),  AdBurst(0, 20, 20, 0), ClearAndCalm(25, 30));
        DefineInteraction(ElectricChair, AdBurst(10, 10, -40, -30), AdBurst(6, 15, -25, -20), AdBurst(4, 20, -15, -10), AdBurst(2, 20, -10, 0), AdBurst(0, 25, -5, 0), AdBurst(0, 35, -5, 0));
        DefineInteraction(Lobotomy,      AdBurst(8, 5, -40, -20),   AdBurst(5, 10, -20, -10), AdBurst(2, 10, 0, 0),     AdBurst(0, 15, 20, 0),  AdBurst(0, 20, 35, 0), ClearAndCalm(25, 50));
    }
}
