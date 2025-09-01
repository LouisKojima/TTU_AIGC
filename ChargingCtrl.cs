using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ChargingCtrl : SerializedMonoBehaviour
{
    [Title("Main")]
    public VehicleStatusData vehicleData;

    public Image progress;

    [Range(0, 100), LabelText("Percent"), LabelWidth(50)]
    [OnValueChanged(nameof(updateProgress))]
    public int progressPercent = 50;

    [LabelText("Progress Color"), ValueDropdown(nameof(progressUseGradientList))]
    [OnValueChanged(nameof(updateProgress))]
    public bool progressUseGradient = false;
    private IEnumerable progressUseGradientList = new ValueDropdownList<bool>()
        {
            { "Simple", false },
            { "Gradient", true }
        };

    [ShowIf("@!" + nameof(progressUseGradient))]
    [HideLabel]
    [OnValueChanged(nameof(updateProgress))]
    public Color progressColor = Color.green;

    [ShowIf("@" + nameof(progressUseGradient))]
    [HideLabel]
    [OnValueChanged(nameof(updateProgress))]
    public Gradient progressGradient = new();

    [LabelText("Min Value")]
    public int lowerFill = 1;

    [LabelText("Max Value")]
    public int upperFill = 100;

    /// <summary>
    /// Update progress according to fill amount.
    /// </summary>
    public void updateProgress()
    {
        if (progress != null)
        {
            progress.fillAmount = Mathf.Lerp(lowerFill, upperFill, progressPercent / 100f) / 100f;

            if (progressUseGradient)
                progress.color = progressGradient.Evaluate(progressPercent / 100f);
            else progress.color = progressColor;

            //if (textStartStop)
            //    textStartStop.text = chargingStatus == ChargingStatus.Charging ? "Stop" : "Start";
        }
    }

    [MinValue(0)]
    public float chargingPower = 10;
    [PropertyRange(0, 100)]
    [LabelText("Charging Target %")]
    public int chargeTargetPercent = 80;

    public enum ChargingStatus { Idle, Ready, Charging, Finished, Cancelled }

    public ChargingStatus pluggedOrIdle
    {
        get
        {
            if (vehicleData && vehicleData.plugged)
                return ChargingStatus.Ready;
            return ChargingStatus.Idle;
        }
    }
    [TitleGroup("States")]
    [EnumToggleButtons]
    [ShowInInspector]
    public ChargingStatus chargingStatus
    {
        get
        {
            return chargingStateContents.FirstOrDefault(x => x.Value.Equals(currentState)).Key;
        }
        set
        {
            currentState = chargingStateContents[value];
        }
    }

    [TitleGroup("States")]
    [ShowInInspector]
    [DisableIf("@true")]
    [ValueDropdown(nameof(valueDropdownList))]
    public ChargingStateContent currentState;

    public struct ChargingStateContent
    {
        //public ChargingStatus chargingStatus;

        public string textStartStop, textTip;

        public bool actionIsStart;
    }

    [TitleGroup("States")]
    [DictionaryDrawerSettings(
        DisplayMode = DictionaryDisplayOptions.ExpandedFoldout,
        KeyLabel = "State",
        ValueLabel = "Contents")]
    [CustomContextMenu("Reset", nameof(ResetChargingStateContents))]
    public Dictionary<ChargingStatus, ChargingStateContent> chargingStateContents = new();

    private void ResetChargingStateContents()
    {
        chargingStateContents.Clear();
        chargingStateContents.Add(ChargingStatus.Idle, new()
        {
            //chargingStatus = ChargingStatus.Idle,
            textStartStop = "Start",
            textTip = "Plug In",
            actionIsStart = true
        });
        chargingStateContents.Add(ChargingStatus.Ready, new()
        {
            //chargingStatus = ChargingStatus.Ready,
            textStartStop = "Start",
            textTip = "Ready",
            actionIsStart = true
        });
        chargingStateContents.Add(ChargingStatus.Charging, new()
        {
            //chargingStatus = ChargingStatus.Charging,
            textStartStop = "Stop Charging",
            textTip = "Charging",
            actionIsStart = false
        });
        chargingStateContents.Add(ChargingStatus.Finished, new()
        {
            //chargingStatus = ChargingStatus.Finished,
            textStartStop = "Start",
            textTip = "Charging Finished",
            actionIsStart = true
        });
        chargingStateContents.Add(ChargingStatus.Cancelled, new()
        {
            //chargingStatus = ChargingStatus.Cancelled,
            textStartStop = "Start",
            textTip = "Charging Cancelled",
            actionIsStart = true
        });
    }

    private ValueDropdownList<ChargingStateContent> valueDropdownList
    {
        get
        {
            ValueDropdownList<ChargingStateContent> result = new();
            result.AddRange(chargingStateContents.Select((x, n) => new ValueDropdownItem<ChargingStateContent>(x.Key.ToString(), x.Value)).ToList());
            return result;
        }
    }

    [Title("Displays")]
    public TextMeshProUGUI textStartStop;
    public TextMeshProUGUI textTip, textTime, textDist;

    public string timePrefix = "Estimated Time: ";
    public int maxDistance = 600;

    public bool TryStartCharging()
    {
        if (!vehicleData.plugged)
            return false;
        chargingStatus = ChargingStatus.Charging;
        //connectorAnim.SetActive(true);
        return true;
    }

    public void StartCharging()
    {
        TryStartCharging();
    }

    public void StopCharging()
    {
        chargingStatus = ChargingStatus.Cancelled;
        //connectorAnim.SetActive(false);
    }

    public void StartOrStop()
    {
        if (currentState.actionIsStart)
            StartCharging();
        else
            StopCharging();
    }

    public void UpdateDisplays()
    {
        if (textStartStop)
            if (textStartStop.text != currentState.textStartStop)
                textStartStop.text = currentState.textStartStop;
        if (textTip)
            if (textTip.text != currentState.textTip)
                textTip.text = currentState.textTip;
        if (textTime)
        {
            float toCharge = vehicleData.batteryCapacity * (chargeTargetPercent - progressPercent) / 100;
            toCharge = Mathf.Max(toCharge, 0);
            float chargeTime = toCharge / chargingPower;
            //Debug.Log(toCharge + " " + chargeTime);
            TimeSpan time = TimeSpan.FromSeconds(chargeTime);
            textTime.text = timePrefix + time.Hours + "h " + time.Minutes + "min";
        }
        if (textDist)
        {
            int remainingDist = Mathf.RoundToInt(maxDistance * progressPercent / 100);
            textDist.text = remainingDist + " km/" + maxDistance + " km";
        }
        connectorAnim.SetActive(chargingStatus == ChargingStatus.Charging);
        connectorReady.SetActive(vehicleData.plugged);
    }

    private void OnValidate()
    {
        vehicleData.batteryPercent = progressPercent;

        if (chargingStatus == ChargingStatus.Idle)
            vehicleData.plugged = false;
        else
            vehicleData.plugged = true;

        UpdateDisplays();
        UpdateOnPlug();
    }

    private bool pluggedPrevious = true;

    private void Start()
    {
        pluggedPrevious = vehicleData.plugged;
    }

    public GameObject connectorReady, connectorAnim;

    private void UpdateOnPlug()
    {
        connectorReady.SetActive(vehicleData.plugged);
        if (!vehicleData.plugged)
            StopCharging();
    }

    public UnityEvent<bool> OnPlugChanged = new();

    private void Update()
    {
        if (pluggedPrevious != vehicleData.plugged)
        {
            OnPlugChanged.Invoke(vehicleData.plugged);
            UpdateOnPlug();
            Init();
            pluggedPrevious = vehicleData.plugged;
        }

        UpdateDisplays();

        if (progressPercent != vehicleData.batteryPercent)
        {
            progressPercent = vehicleData.batteryPercent;
            updateProgress();
        }

        if (chargingStatus == ChargingStatus.Charging)
            Charge();
    }

    private void Charge()
    {
        if (vehicleData.batteryCurrent >= chargeTargetPercent * vehicleData.batteryCapacity / 100f)
        {
            chargingStatus = ChargingStatus.Finished;
        }
        else
        {
            vehicleData.batteryCurrent += Time.deltaTime * chargingPower;
            vehicleData.batteryCurrent = Mathf.Clamp(vehicleData.batteryCurrent, 0, vehicleData.batteryCapacity);
        }
    }

    public void Init()
    {
        if (!vehicleData.plugged)
            chargingStatus = ChargingStatus.Idle;
        else if (chargingStatus != ChargingStatus.Charging)
            chargingStatus = ChargingStatus.Ready;
        UpdateDisplays();
    }
}
