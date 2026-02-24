using AxGrid;
using AxGrid.FSM;
using AxGrid.Model;

namespace SlotMachine
{
    [State("SlotDecelerating")]
    public class SlotDeceleratingState : FSMState
    {
        [Enter]
        private void EnterThis()
        {
            Settings.Model.Set("BtnStartEnable", false);
            Settings.Model.Set("BtnStopEnable", false);

            Settings.Model.EventManager.Invoke("OnSlotDecelerating");
        }

        [Bind("OnSpinAligned")]
        private void OnAligned()
        {
            Parent.Change("SlotIdle");
        }

        [Exit]
        private void ExitThis() { }
    }
}