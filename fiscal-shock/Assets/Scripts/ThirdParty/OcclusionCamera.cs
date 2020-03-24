#pragma warning disable
using System;
using UnityEngine;

namespace ThirdParty {
	public class OcclusionCamera : MonoBehaviour {
		void Start () {
			Camera camera = gameObject.GetComponent<Camera>();
			camera.farClipPlane = Settings.viewDistance;
			InvokeRepeating("CheckView", 0, Settings.occlusionSampleDelay);
		}

		void CheckView () {
			int step = Math.Max(Screen.width, Screen.height) / Settings.occlusionSamples;

			for (int x = 0; x < Screen.width; x += step)
			{
				for (int y = 0; y < Screen.height; y += step)
				{
					Ray SampleRay = Camera.main.ScreenPointToRay(new Vector3(x, y, 0f));
					RaycastHit hit;
					if (Physics.Raycast(SampleRay, out hit, Settings.viewDistance))
					{
						//Debug.DrawRay(SampleRay.origin, SampleRay.direction, Color.red, 1);
						GameObject target = hit.transform.gameObject;

						OcclusionBase thisTargetScript = target.GetComponent<OcclusionBase>();

						while(thisTargetScript == null && target.transform.parent != null)
						{
							if(target.transform.parent != null)
							{
								target = target.transform.parent.gameObject;
							}

							thisTargetScript = target.GetComponent<OcclusionBase>();

						};

						if (thisTargetScript != null)
						{
							thisTargetScript.lastSeen = DateTime.Now;
							thisTargetScript.isVisible = true;
						}
					}
				}
			}

		}
	}
}
