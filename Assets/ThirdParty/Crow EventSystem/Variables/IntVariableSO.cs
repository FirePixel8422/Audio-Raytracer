using UnityEngine;

namespace CrowGroup.Utility.Scriptable
{
    [CreateAssetMenu(fileName = "New_IntVariable", menuName = "ScriptableObjects/EventVariable/IntVariable", order = -1005)]
    public class IntVariableSO : VariableSO<int>, INumericalVariable<int>
    {
        public void Add(int value)
        {
            CurrentValue += value;
        }
    }
}
