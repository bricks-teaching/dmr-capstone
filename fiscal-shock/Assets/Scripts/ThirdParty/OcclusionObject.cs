#pragma warning disable
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ThirdParty {
	public class OcclusionObject : OcclusionBase
	{
		List<Renderer> renderers { get; set; }
		List<Light> lights { get; set; }
		private bool isVisibleLast { get; set; }
		float PlayerDistance { get; set; }
					GameObject Player { get; set; }

		void Start () {
			if (!enabled/* || !Configs.EnableObjectOcclusion*/) { return; }
									Player = GameObject.FindGameObjectWithTag("Player");
			renderers = GetChildRenderers(gameObject);
			lights = GetChildLights(gameObject);
			Renderer thisRenderer = gameObject.GetComponent<Renderer>();
			if (thisRenderer != null) {
				renderers.Add(thisRenderer);
			}

			if (renderers != null || renderers.Count == 0)
			{
				InvokeRepeating("CheckStatus", Settings.occlusionSampleDelay, Settings.occlusionSampleDelay);
				InvokeRepeating("Hide", Settings.occlusionSampleDelay, Settings.occlusionHideDelay);
			}
			isVisible = true;
		}

		public void Hide()
		{
			if ((DateTime.Now - lastSeen).TotalSeconds > Settings.occlusionHideDelay && PlayerDistance > Settings.minOcclusionDistance)
			{
				isVisible = false;
			}
		}

		List<Light> GetChildLights(GameObject toSearch)
		{
			List<Light> theseLights = toSearch.GetComponentsInChildren<Light>().ToList();

			return theseLights;
		}

		List<Renderer> GetChildRenderers(GameObject toSearch)
		{
			List<Renderer> theseRenderers = new List<Renderer>();
			int ChildCount = toSearch.transform.childCount;

			for (int i = 0; i < ChildCount; i++)
			{
				GameObject thisChild = toSearch.transform.GetChild(i).gameObject;

				if (thisChild.GetComponent<OcclusionObject>() == null)
				{
					Renderer thisRenderer = thisChild.GetComponent<Renderer>();
					if (thisRenderer != null)
					{
						theseRenderers.Add(thisRenderer);
					}

					theseRenderers.AddRange(GetChildRenderers(thisChild));
				}
			}

			return theseRenderers;
		}

		public void Toggle()
		{
			PlayerDistance = Vector3.Distance(Player.transform.position, transform.position);
			bool visible = isVisible || PlayerDistance < Settings.minOcclusionDistance;

			foreach (Renderer thisRenderer in renderers)
			{
				if(thisRenderer.enabled == isVisible) { return; } //they're probably all the same

				thisRenderer.enabled = visible;
			}
			foreach (Light thisLight in lights)
			{
				thisLight.enabled = visible;
			}
		}

		public void CheckStatus()
		{
			Toggle();
		}
	}
}
