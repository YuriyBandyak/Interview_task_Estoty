using System;

public struct PlayerParametersModifiers {
    public Action<float> FireRateSetter;
    public Action IncreaseHealthAction;
    public Action ReceiveShieldAction;
}