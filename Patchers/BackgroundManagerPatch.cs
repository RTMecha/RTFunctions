using System.Linq;

using HarmonyLib;

using UnityEngine;

using RTFunctions.Functions.Data;
using RTFunctions.Functions.IO;

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

			var gameObject = Instantiate(__instance.backgroundPrefab, new Vector3(__0.pos.x, __0.pos.y, 32f + backgroundObject.layer * 10f), Quaternion.identity);
			gameObject.name = __0.name;
			gameObject.layer = 9;
			gameObject.transform.SetParent(__instance.backgroundParent);
			gameObject.transform.localScale = new Vector3(__0.scale.x, __0.scale.y, scaleZ);
			gameObject.transform.localRotation = Quaternion.Euler(new Vector3(backgroundObject.rotation.x, backgroundObject.rotation.y, __0.rot));

			// For now, removing selecting backgrounds.
			Destroy(gameObject.GetComponent<SelectBackgroundInEditor>());
			//gameObject.GetComponent<SelectBackgroundInEditor>().obj = __instance.backgroundObjects.Count;
			__instance.backgroundObjects.Add(gameObject);

				backgroundObject.gameObjects.Clear();
				backgroundObject.transforms.Clear();
				backgroundObject.renderers.Clear();

				backgroundObject.gameObjects.Add(gameObject);
				backgroundObject.transforms.Add(gameObject.transform);
				backgroundObject.renderers.Add(gameObject.GetComponent<Renderer>());

			if (__0.drawFade)
			{
				for (int i = 1; i < depth - backgroundObject.layer; i++)
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
				foreach (var bg in DataManager.inst.gameData.backgroundObjects)
                {
					var backgroundObject = (BackgroundObject)bg;

					var beatmapTheme = RTHelpers.BeatmapTheme;

					Color a;

					if (backgroundObject.reactive)
					{
						if (FunctionsPlugin.BGReactiveLerp.Value)
						{
							a = RTMath.Lerp(beatmapTheme.GetBGColor(backgroundObject.color), beatmapTheme.GetBGColor(backgroundObject.reactiveCol), __instance.samples[Mathf.Clamp(backgroundObject.reactiveColSample, 0, __instance.samples.Length - 1)] * backgroundObject.reactiveColIntensity);
						}
						else
						{
							a = beatmapTheme.GetBGColor(backgroundObject.color);
							a += beatmapTheme.GetBGColor(backgroundObject.reactiveCol) * __instance.samples[Mathf.Clamp(backgroundObject.reactiveColSample, 0, __instance.samples.Length - 1)] * backgroundObject.reactiveColIntensity;
						}
					}
					else
					{
						a = beatmapTheme.GetBGColor(backgroundObject.color);
					}

					a.a = 1f;

					int i = 0;
					foreach (var renderer in backgroundObject.renderers)
					{
						if (i == 0)
						{
							renderer.material.color = a;
						}
						else
						{
							int layer = backgroundObject.depth - backgroundObject.layer;
							float t = a.a / (float)layer * (float)i;
							Color b = beatmapTheme.GetBGColor(backgroundObject.FadeColor);

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
									float x = __instance.samples[Mathf.Clamp(backgroundObject.reactiveScaSamples[0], 0, __instance.samples.Length - 1)];
									float y = __instance.samples[Mathf.Clamp(backgroundObject.reactiveScaSamples[1], 0, __instance.samples.Length - 1)];

									backgroundObject.reactiveSize =
										new Vector2(x * backgroundObject.reactiveScaIntensity[0], y * backgroundObject.reactiveScaIntensity[1]) * backgroundObject.reactiveScale;
									break;
								}
						}
						if (__instance.backgroundObjects.Count > num)
                        {
							float x = __instance.samples[Mathf.Clamp(backgroundObject.reactivePosSamples[0], 0, __instance.samples.Length - 1)];
							float y = __instance.samples[Mathf.Clamp(backgroundObject.reactivePosSamples[1], 0, __instance.samples.Length - 1)];

							float rot = __instance.samples[Mathf.Clamp(backgroundObject.reactiveRotSample, 0, __instance.samples.Length - 1)];

							var gameObject = __instance.backgroundObjects[num];

							float z = __instance.samples[Mathf.Clamp(backgroundObject.reactiveZSample, 0, __instance.samples.Length - 1)];

							gameObject.transform.localPosition =
								new Vector3(backgroundObject.pos.x + x * backgroundObject.reactivePosIntensity[0],
								backgroundObject.pos.y + y * backgroundObject.reactivePosIntensity[1],
								32f + backgroundObject.layer * 10f + z * backgroundObject.reactiveZIntensity);
							gameObject.transform.localScale =
								new Vector3(backgroundObject.scale.x, backgroundObject.scale.y, backgroundObject.zscale) +
								new Vector3(backgroundObject.reactiveSize.x, backgroundObject.reactiveSize.y, 0f);
							gameObject.transform.localRotation = Quaternion.Euler(
								new Vector3(backgroundObject.rotation.x, backgroundObject.rotation.y,
								backgroundObject.rot + rot * backgroundObject.reactiveRotIntensity));
						}
					}
					else
					{
						backgroundObject.reactiveSize = Vector2.zero;
						if (__instance.backgroundObjects.Count > num)
						{
							var gameObject = __instance.backgroundObjects[num];
							gameObject.transform.localPosition = new Vector3(backgroundObject.pos.x, backgroundObject.pos.y, 32f + backgroundObject.layer * 10f);
							gameObject.transform.localRotation = Quaternion.Euler(new Vector3(backgroundObject.rotation.x, backgroundObject.rotation.y, backgroundObject.rot));
							gameObject.transform.localScale = new Vector3(backgroundObject.scale.x, backgroundObject.scale.y, backgroundObject.zscale);
						}
					}
					num++;
				}
			}

			return false;
		}
	}
}
