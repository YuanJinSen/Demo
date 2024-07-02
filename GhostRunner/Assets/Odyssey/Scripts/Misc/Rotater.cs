using UnityEngine;

namespace Odyssey
{
    public class Rotater : MonoBehaviour
    {
		public Space space;
		public Vector3 eulers = new Vector3(0, -180, 0);

		protected void LateUpdate()
		{
			transform.Rotate(eulers * Time.deltaTime, space);
		}
	}
}