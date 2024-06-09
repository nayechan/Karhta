using UnityEngine;
using UnityEngine.Rendering;

namespace InGame.Chunk
{
    public class WaterVolumeTrigger : MonoBehaviour
    {
        public Volume localVolume;

        private void Awake()
        {
            localVolume = GetComponent<Volume>();
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("MainCamera"))
            {
                //RenderSettings.fog = true;
                localVolume.weight = 1f;
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("MainCamera"))
            {
                //RenderSettings.fog = false;
                localVolume.weight = 0f;
            }
        }
    }
}