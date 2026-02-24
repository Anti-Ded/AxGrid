using AxGrid;
using AxGrid.Base;
using AxGrid.Model;
using UnityEngine;
using UnityEngine.UI;

namespace SlotMachine
{
    // if defaultEnable is true then it cant be disabled by Settings.Model.Set("BtnStartEnable", false); 
    // dont know why
    // So im using this script for Start Button
    public class StartButtonDebugBinder : MonoBehaviourExtBind
    {
        private Button _button;

        [OnStart]
        private void StartThis()
        {
            _button = GetComponent<Button>();
        }

        [Bind("OnBtnStartEnableChanged")]
        private void OnChanged()
        {
            bool val = Settings.Model.GetBool("BtnStartEnable", true);
            Log.Debug($"StartButtonDebugBinder: OnBtnStartEnableChanged → {val}");
            _button.interactable = val;
        }
    }
}