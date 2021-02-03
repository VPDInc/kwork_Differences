using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum StatusRewardCard
{
    Opened,
    Closed,
    Waiting
}

public struct DayInformationRewardCard
{
    private int _numberOfDay;
    private StatusRewardCard _status;
}
