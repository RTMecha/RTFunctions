using UnityEngine;

namespace RTFunctions.Functions.Components
{
    public class RTDelayTracker : MonoBehaviour
	{
		public Transform self;

		public Transform leader;

		public bool move = true;
		public float moveDelay = 0.1f;
		public bool includeZ = false;

		public bool rotate = false;
		public float rotateDelay = 0.1f;

		public Vector3 moveOffset;

		public Vector3 target;

		public RotateType rotateType = RotateType.Up;

		public enum RotateType
		{
			Up,
			Right
		}

		void LateUpdate()
        {
			if (leader && self)
			{
				float pitch = AudioManager.inst.CurrentAudioSource.pitch;
				if (pitch < 0f)
					pitch = -pitch;
				if (pitch == 0f)
					pitch = 0.001f;

				target = leader.position + moveOffset;

				float p = Time.deltaTime * 60f * pitch;
				float md = 1f - Mathf.Pow(1f - moveDelay, p);
				float rd = 1f - Mathf.Pow(1f - rotateDelay, p);

				if (move)
                {
					if (includeZ)
						self.localPosition += (target - self.position) * md;
					else
						self.localPosition += (new Vector3(target.x, target.y, 0f) - new Vector3(self.position.x, self.position.y, 0f)) * md;
                }

				if (rotate)
				{
					switch (rotateType)
                    {
						case RotateType.Up:
							{
								self.up = (target - self.position) * rd;
								break;
                            }
                        case RotateType.Right:
							{
								self.right = (target - self.position) * rd;
								break;
                            }
                    }
				}
			}
		}
	}
}
