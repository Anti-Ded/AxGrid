using AxGrid;
using AxGrid.FSM;
using AxGrid.Model;

namespace SlotMachine
{
    [State("SlotIdle")]
    public class SlotIdleState : FSMState
    {
        [Enter]
        private void EnterThis()
        {
            Settings.Model.Set("BtnStartEnable", true);
            Settings.Model.Set("BtnStopEnable", false);
            Settings.Model.EventManager.Invoke("OnBtnStartEnableChanged");
            Settings.Model.EventManager.Invoke("OnBtnStopEnableChanged");
            Settings.Model.EventManager.Invoke("OnSlotIdle");
            Log.Debug("SlotIdle: Enter");
        }

        [Bind("StartSignal")]
        private void OnStart()
        {
            Log.Debug("SlotIdle: StartSignal received!");
            Parent.Change("SlotAccelerating");
        }

        [Exit]
        private void ExitThis()
        {
            // ‘орс-invoke здесь Ч на случай если Set не стрел€ет событие
            Settings.Model.Set("BtnStartEnable", false);
            Settings.Model.EventManager.Invoke("OnBtnStartEnableChanged");
            Log.Debug("SlotIdle: Exit");
        }
    }
}