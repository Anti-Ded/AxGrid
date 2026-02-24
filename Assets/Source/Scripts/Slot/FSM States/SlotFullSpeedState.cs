using AxGrid;
using AxGrid.FSM;
using AxGrid.Model;

namespace SlotMachine
{
    [State("SlotFullSpeed")]
    public class SlotFullSpeedState : FSMState
    {
        [Enter]
        private void EnterThis()
        {
            Settings.Model.Set("BtnStartEnable", false);
            Settings.Model.Set("BtnStopEnable", true);

            Settings.Model.EventManager.Invoke("OnSlotFullSpeed");
        }

        [Bind("StopSignal")]
        private void OnStop()
        {
            Parent.Change("SlotDecelerating");
        }

        [Exit]
        private void ExitThis()
        {
            Settings.Model.Set("BtnStopEnable", false);
        }
    }
}