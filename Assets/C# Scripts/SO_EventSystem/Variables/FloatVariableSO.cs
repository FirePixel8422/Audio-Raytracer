using UnityEngine;

namespace CrowGroup.Utility.Scriptable
{
    [CreateAssetMenu(fileName = "New_FloatVariable", menuName = "ScriptableObjects/EventVariable/FloatVariable", order = -1005)]
    public class FloatVariableSO : VariableSO<float>, INumericalVariable<float>
    {
        public void Add(float value)
        {
            CurrentValue += value;
        }
    }
}