using UnityEngine;

public class ConnectWires : MinigameBase
{
    public static ConnectWires Instance;

    void Awake()
    {
        Instance = this;
        gameObject.SetActive(false);
    }
    public override void Open(PatientInteractionInfo info)
    {
        base.Open(info);
    }

    public void OnWireResult(int result)
    {
        Finish(result);
    }

    public void TestReallyGood()
    {
        Finish(6);
    }

    public void TestReallyBad()
    {
        Finish(1);
    }
    public void TestGood()
    {
        Finish(4);
    }

    public void TestBad()
    {
        Finish(3);
    }
}