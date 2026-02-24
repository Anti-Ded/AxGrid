using AxGrid;
using AxGrid.Base;
using AxGrid.FSM;
using AxGrid.Model;
using UnityEngine;

namespace SlotMachine
{
    public class SlotFSM : MonoBehaviourExtBind
    {
        [OnAwake]
        private void AwakeThis()
        {
            Settings.Fsm = new FSM();
            Settings.Fsm.Add(new SlotIdleState());
            Settings.Fsm.Add(new SlotAcceleratingState());
            Settings.Fsm.Add(new SlotFullSpeedState());
            Settings.Fsm.Add(new SlotDeceleratingState());
            Settings.Fsm.Start("SlotIdle");
            Log.Debug("SlotFSM: initialized, state = " + Settings.Fsm.CurrentStateName);
        }

        [OnStart]
        private void StartThis()
        {
            Settings.Model.Set("BtnStartEnable", true);
            Settings.Model.Set("BtnStopEnable", false);
            Settings.Model.EventManager.Invoke("OnBtnStartEnableChanged");
            Settings.Model.EventManager.Invoke("OnBtnStopEnableChanged");
            Log.Debug($"SlotFSM: buttons initialized, BtnStartEnable={Settings.Model.GetBool("BtnStartEnable", true)}");
        }

        [OnUpdate]
        private void UpdateThis()
        {
            Settings.Fsm.Update(Time.deltaTime);
        }

        [Bind("OnStartClick")]
        private void OnStartPressed()
        {
            Log.Debug($"SlotFSM: OnStartClick, state={Settings.Fsm.CurrentStateName}");
            Settings.Fsm.Invoke("StartSignal");
        }

        [Bind("OnStopClick")]
        private void OnStopPressed()
        {
            Log.Debug($"SlotFSM: OnStopClick, state={Settings.Fsm.CurrentStateName}");
            Settings.Fsm.Invoke("StopSignal");
        }
    }
}