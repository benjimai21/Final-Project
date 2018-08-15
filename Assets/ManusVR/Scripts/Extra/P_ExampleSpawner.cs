using UnityEngine;

namespace ManusVR.PhysicalInteraction
{
    public class P_ExampleSpawner : MonoBehaviour {
        public GameObject Spawnable;
        public Transform SpawnTransform;

        public void SpawnObject()
        {
            Instantiate(Spawnable, SpawnTransform.position, SpawnTransform.rotation);
        }
    }
}
