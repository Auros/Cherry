using UnityEngine;

namespace Cherry
{
    public class DiComponent : MonoBehaviour
    {
        public object? Component;

        public DiComponent(object input)
        {
            GameObject go = new GameObject(input.GetType().Name);
            var c = go.AddComponent<DiComponent>();
            c.Component = input;
        }
    }
}