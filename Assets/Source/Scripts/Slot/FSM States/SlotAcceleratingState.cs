using AxGrid;
using AxGrid.FSM;

namespace SlotMachine
{
    [State("SlotAccelerating")]
    public class SlotAcceleratingState : FSMState
    {
        [Enter]
        private void EnterThis()
        {
            Settings.Model.Set("BtnStartEnable", false);
            Settings.Model.Set("BtnStopEnable", false);
            // ‘орс-invoke нужен Ч Set не стрел€ет событие если значение не изменилось
            Settings.Model.EventManager.Invoke("OnBtnStartEnableChanged");
            Settings.Model.EventManager.Invoke("OnBtnStopEnableChanged");

            Settings.Model.EventManager.Invoke("OnSlotAccelerating");
        }

        [One(3f)]
        private void AllowStop()
        {
            Parent.Change("SlotFullSpeed");
        }

        [Exit]
        private void ExitThis() { }
    }
}