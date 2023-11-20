using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;

using UnityEngine;

using RTFunctions.Functions.Data;
using RTFunctions.Functions.IO;
using RTFunctions.Functions.Managers;

namespace RTFunctions.Patchers
{
    [HarmonyPatch(typeof(BackgroundManager))]
    public class BackgroundManagerPatch : MonoBehaviour
    {
		public static AudioSource Audio => AudioManager.inst.CurrentAudioSource;

		[HarmonyPatch("CreateBackgroundObject")]
		[HarmonyPrefix]
		static bool CreateBackgroundObject(BackgroundManager __instance, ref GameObject __result, DataManager.GameData.BackgroundObject __0)
		{
			var backgroundObject = (BackgroundObject)__0;

			float scaleZ = backgroundObject.zscale;
			int depth = backgroundObject.depth;

			var gameObject = Instantiate(__instance.backgroundPrefab, new Vector3(__0.pos.x, __0.pos.y, (float)(32 + __0.layer * 10)), Quaternion.identity);
			gameObject.name = __0.name;
			gameObject.layer = 9;
			gameObject.transform.SetParent(__instance.backgroundParent);
			gameObject.transform.localScale = new Vector3(__0.scale.x, __0.scale.y, scaleZ);
			gameObject.transform.localRotation = Quaternion.Euler(new Vector3(backgroundObject.rotation.x, backgroundObject.rotation.y, __0.rot));

			gameObject.GetComponent<SelectBackgroundInEditor>().obj = __instance.backgroundObjects.Count;
			__instance.backgroundObjects.Add(gameObject);

				backgroundObject.gameObjects.Clear();
				backgroundObject.transforms.Clear();
				backgroundObject.renderers.Clear();

				backgroundObject.gameObjects.Add(gameObject);
				backgroundObject.transforms.Add(gameObject.transform);
				backgroundObject.renderers.Add(gameObject.GetComponent<Renderer>());

			if (__0.drawFade)
			{
				for (int i = 1; i < depth - __0.layer; i++)
				{
					var gameObject2 = Instantiate(__instance.backgroundFadePrefab, Vector3.zero, Quaternion.identity);
					gameObject2.name = $"{__0.name} Fade [{i}]";

					gameObject2.transform.SetParent(gameObject.transform);
					gameObject2.transform.localPosition = new Vector3(0f, 0f, i);
					gameObject2.transform.localScale = Vector3.one;
					gameObject2.transform.localRotation = Quaternion.Euler(Vector3.zero);
					gameObject2.layer = 9;

					backgroundObject.gameObjects.Add(gameObject2);
					backgroundObject.transforms.Add(gameObject2.transform);
					backgroundObject.renderers.Add(gameObject2.GetComponent<Renderer>());
				}
			}

			backgroundObject.SetShape(backgroundObject.shape.Type, backgroundObject.shape.Option);

			__result = gameObject;
			return false;
		}

		public static Color bgColorToLerp;

		[HarmonyPatch("UpdateBackgroundObjects")]
		[HarmonyPrefix]
		static bool UpdateBackgroundObjects(BackgroundManager __instance)
		{
			if (GameManager.inst.gameState == GameManager.State.Playing)
			{
				Audio?.GetSpectrumData(__instance.samples, 0, FFTWindow.Rectangular);
				__instance.sampleLow = __instance.samples.Skip(0).Take(56).Average((float a) => a) * 1000f;
				__instance.sampleMid = __instance.samples.Skip(56).Take(100).Average((float a) => a) * 3000f;
				__instance.sampleHigh = __instance.samples.Skip(156).Take(100).Average((float a) => a) * 6000f;
				int num = 0;
				foreach (var bg in Objects.backgroundObjects)
                {
					var backgroundObject = bg;

					var beatmapTheme = RTHelpers.BeatmapTheme;

					Color a;

					if (backgroundObject.reactive)
					{
						if (FunctionsPlugin.BGReactiveLerp.Value)
						{
							a = RTMath.Lerp(beatmapTheme.GetBGColor(backgroundObject.color), beatmapTheme.GetBGColor(bg.reactiveCol), __instance.samples[Mathf.Clamp(bg.reactiveColSample, 0, __instance.samples.Length - 1)] * bg.reactiveColIntensity);
						}
						else
						{
							a = beatmapTheme.GetBGColor(backgroundObject.color);
							a += beatmapTheme.GetBGColor(bg.reactiveCol) * __instance.samples[Mathf.Clamp(bg.reactiveColSample, 0, __instance.samples.Length - 1)] * bg.reactiveColIntensity;
						}
					}
					else
                    {
						a = beatmapTheme.GetBGColor(backgroundObject.color);
					}

					a.a = 1f;

					int i = 0;
					foreach (var renderer in bg.renderers)
                    {
						if (i == 0)
                        {
							renderer.material.color = a;
                        }
						else
						{
							int layer = bg.depth - backgroundObject.layer;
							float t = a.a / (float)layer * (float)i;
							Color b = beatmapTheme.GetBGColor(bg.FadeColor);

							if (RTHelpers.ColorMatch(b, beatmapTheme.backgroundColor, 0.05f))
							{
								b = bgColorToLerp;
								b.a = 1f;
								renderer.material.color = Color.Lerp(Color.Lerp(a, b, t), b, t);
							}
							else
							{
								b.a = 1f;
								renderer.material.color = Color.Lerp(Color.Lerp(a, b, t), b, t);
							}
						}

						i++;
                    }

					if (backgroundObject.reactive)
                    {
						switch (backgroundObject.reactiveType)
						{
							case DataManager.GameData.BackgroundObject.ReactiveType.LOW:
								backgroundObject.reactiveSize = new Vector2(__instance.sampleLow, __instance.sampleLow) * backgroundObject.reactiveScale;
								break;
							case DataManager.GameData.BackgroundObject.ReactiveType.MID:
								backgroundObject.reactiveSize = new Vector2(__instance.sampleMid, __instance.sampleMid) * backgroundObject.reactiveScale;
								break;
							case DataManager.GameData.BackgroundObject.ReactiveType.HIGH:
								backgroundObject.reactiveSize = new Vector2(__instance.sampleHigh, __instance.sampleHigh) * backgroundObject.reactiveScale;
								break;
							case (DataManager.GameData.BackgroundObject.ReactiveType)3:
								{
									float x = __instance.samples[Mathf.Clamp(bg.reactiveScaSamples[0], 0, __instance.samples.Length - 1)];
									float y = __instance.samples[Mathf.Clamp(bg.reactiveScaSamples[1], 0, __instance.samples.Length - 1)];

									backgroundObject.reactiveSize = new Vector2(x * bg.reactiveScaIntensity[0], y * bg.reactiveScaIntensity[1]) * backgroundObject.reactiveScale;
									break;
								}
						}
						if (__instance.backgroundObjects.Count > num)
                        {
							float x = __instance.samples[Mathf.Clamp(bg.reactivePosSamples[0], 0, __instance.samples.Length - 1)];
							float y = __instance.samples[Mathf.Clamp(bg.reactivePosSamples[1], 0, __instance.samples.Length - 1)];

							float rot = __instance.samples[Mathf.Clamp(bg.reactiveRotSample, 0, __instance.samples.Length - 1)];

							var gameObject = __instance.backgroundObjects[num];

							float z = __instance.samples[Mathf.Clamp(bg.reactiveZSample, 0, __instance.samples.Length - 1)];

							gameObject.transform.localPosition = new Vector3(backgroundObject.pos.x + x * bg.reactivePosIntensity[0], backgroundObject.pos.y + y * bg.reactivePosIntensity[1], (float)(32 + backgroundObject.layer * 10) + z * bg.reactiveZIntensity);
							gameObject.transform.localScale = new Vector3(backgroundObject.scale.x, backgroundObject.scale.y, bg.zscale) + new Vector3(backgroundObject.reactiveSize.x, backgroundObject.reactiveSize.y, 0f);
							gameObject.transform.localRotation = Quaternion.Euler(new Vector3(bg.rotation.x, bg.rotation.y, backgroundObject.rot + rot * bg.reactiveRotIntensity));
						}
					}
					else
					{
						backgroundObject.reactiveSize = Vector2.zero;
						if (__instance.backgroundObjects.Count > num)
						{
							var gameObject = __instance.backgroundObjects[num];
							gameObject.transform.localPosition = new Vector3(backgroundObject.pos.x, backgroundObject.pos.y, (float)(32 + backgroundObject.layer * 10));
							gameObject.transform.localRotation = Quaternion.Euler(new Vector3(bg.rotation.x, bg.rotation.y, backgroundObject.rot));
							gameObject.transform.localScale = new Vector3(backgroundObject.scale.x, backgroundObject.scale.y, bg.zscale);
						}
					}
					num++;
				}
			}

			return false;
		}
	}
}
